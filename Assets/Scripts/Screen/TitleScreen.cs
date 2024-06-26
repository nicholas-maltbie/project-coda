// Copyright (C) 2024 Nicholas Maltbie, Sam Scherer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Eflatun.SceneReference;
using ProjectCoda.Network;
using ProjectCoda.Player;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Unity.Netcode.Transports.UTP.UnityTransport;

namespace ProjectCoda.Screen
{
    public class TitleScreen : MonoBehaviour
    {
        [SerializeField]
        private SceneReference connectingScreen;

        [SerializeField]
        private AudioMixer mixer;

        private ConnectionAddressData connectionData;
        private Button hostButton;
        private Button joinButton;
        private Button quitButton;
        private Button offlinePlayButton;
        private TextField serverAddressField;
        private TextField playerNameField;
        private IntegerField serverPortField;
        private Slider master, music, sfx;
        public void Start()
        {
            FocusFirstElement();
        }

        public void FocusFirstElement()
        {
            GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("host-game").Focus();
        }

        public void OnEnable()
        {
            // The UXML is already instantiated by the UIDocument component
            UIDocument uiDocument = GetComponent<UIDocument>();

            playerNameField = uiDocument.rootVisualElement.Q("player-name") as TextField;
            hostButton = uiDocument.rootVisualElement.Q("host-game") as Button;
            joinButton = uiDocument.rootVisualElement.Q("join-game") as Button;
            quitButton = uiDocument.rootVisualElement.Q("quit-game") as Button;
            offlinePlayButton = uiDocument.rootVisualElement.Q("play-offline") as Button;
            serverAddressField = uiDocument.rootVisualElement.Q("server-address") as TextField;
            serverPortField = uiDocument.rootVisualElement.Q("server-port") as IntegerField;
            master = uiDocument.rootVisualElement.Q("master-audio") as Slider;
            music = uiDocument.rootVisualElement.Q("music-audio") as Slider;
            sfx = uiDocument.rootVisualElement.Q("sfx-audio") as Slider;


            // Populate default values.
            var networkTransport = NetworkManager.Singleton?.NetworkConfig?.NetworkTransport as UnityTransport;
            if (networkTransport != null)
            {
                connectionData = networkTransport.ConnectionData;
            }

            if (string.IsNullOrEmpty(PlayerInfo.PlayerName))
            {
                PlayerInfo.PlayerName = playerNameField.value;
            }
            else
            {
                playerNameField.value = PlayerInfo.PlayerName;
            }

            hostButton.RegisterCallback<NavigationSubmitEvent>(HostGame);
            joinButton.RegisterCallback<NavigationSubmitEvent>(JoinGame);
            quitButton.RegisterCallback<NavigationSubmitEvent>(QuitGame);
            offlinePlayButton.RegisterCallback<NavigationSubmitEvent>(StartInOfflineMode);
            hostButton.RegisterCallback<ClickEvent>(HostGame);
            joinButton.RegisterCallback<ClickEvent>(JoinGame);
            quitButton.RegisterCallback<ClickEvent>(QuitGame);
            offlinePlayButton.RegisterCallback<ClickEvent>(StartInOfflineMode);
            playerNameField.RegisterValueChangedCallback(OnPlayerNameChange);
            serverAddressField.RegisterValueChangedCallback(OnServerAddressChange);
            serverPortField.RegisterValueChangedCallback(OnServerPortChange);

            master.RegisterValueChangedCallback(ChangeMasterVolume);
            music.RegisterValueChangedCallback(ChangeMusicVolume);
            sfx.RegisterValueChangedCallback(ChangeSfxVolume);

            // Disable quit game button on web platform.
            bool notWebGl = Application.platform != RuntimePlatform.WebGLPlayer;
            hostButton.visible = notWebGl;
            quitButton.visible = notWebGl;
        }

        public void OnDisable()
        {
            hostButton.UnregisterCallback<NavigationSubmitEvent>(HostGame);
            joinButton.UnregisterCallback<NavigationSubmitEvent>(JoinGame);
            quitButton.UnregisterCallback<NavigationSubmitEvent>(QuitGame);
            offlinePlayButton.UnregisterCallback<NavigationSubmitEvent>(StartInOfflineMode);
            hostButton.UnregisterCallback<ClickEvent>(HostGame);
            joinButton.UnregisterCallback<ClickEvent>(JoinGame);
            quitButton.UnregisterCallback<ClickEvent>(QuitGame);
            offlinePlayButton.UnregisterCallback<ClickEvent>(StartInOfflineMode);
            serverAddressField.UnregisterValueChangedCallback(OnServerAddressChange);
            serverPortField.UnregisterValueChangedCallback(OnServerPortChange);
            playerNameField.UnregisterValueChangedCallback(OnPlayerNameChange);
            master.UnregisterValueChangedCallback(ChangeMasterVolume);
            music.UnregisterValueChangedCallback(ChangeMusicVolume);
            sfx.UnregisterValueChangedCallback(ChangeSfxVolume);
        }

        private void HostGame(ClickEvent evt) => HostGame();
        private void HostGame(NavigationSubmitEvent evt) => HostGame();
        private void JoinGame(ClickEvent evt) => JoinGame();
        private void JoinGame(NavigationSubmitEvent evt) => JoinGame();
        private void QuitGame(ClickEvent evt) => QuitGame();
        private void QuitGame(NavigationSubmitEvent evt) => QuitGame();
        private void StartInOfflineMode(ClickEvent evt) => StartInOfflineMode();
        private void StartInOfflineMode(NavigationSubmitEvent evt) => StartInOfflineMode();

        private void ChangeMasterVolume(ChangeEvent<float> evt) => ChangeVolume("Master", evt.newValue);
        private void ChangeMusicVolume(ChangeEvent<float> evt) => ChangeVolume("Music", evt.newValue);
        private void ChangeSfxVolume(ChangeEvent<float> evt) => ChangeVolume("SFX", evt.newValue);

        private void ChangeVolume(string controller, float newVol)
        {
            mixer.SetFloat(controller, Mathf.Log10(newVol) * 20);
        }

        private void HostGame()
        {
            SwapToUnityTransport();
            NetworkManager.Singleton.StartHost();
        }

        private void JoinGame()
        {
            SwapToUnityTransport();
            SceneManager.LoadScene(connectingScreen.Name, LoadSceneMode.Single);
            NetworkManager.Singleton.StartClient();
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnPlayerNameChange(ChangeEvent<string> evt)
        {
            PlayerInfo.PlayerName = evt.newValue;
        }

        private void OnServerAddressChange(ChangeEvent<string> evt)
        {
            connectionData.Address = evt.newValue;
            UpdateNetworkAddressData();
        }

        private void OnServerPortChange(ChangeEvent<int> evt)
        {
            connectionData.Port = (ushort)evt.newValue;
            UpdateNetworkAddressData();
        }

        private void SwapToUnityTransport()
        {
            if (NetworkManager.Singleton.NetworkConfig.NetworkTransport is not UnityTransport)
            {
                GameObject networkManagerGo = NetworkManager.Singleton.gameObject;
                NetworkManager.Singleton.NetworkConfig.NetworkTransport = networkManagerGo.GetComponent<UnityTransport>();
            }
        }

        private void StartInOfflineMode()
        {
            GameObject networkManagerGo = NetworkManager.Singleton.gameObject;
            if (!networkManagerGo.TryGetComponent(out OfflineNetworkTransport offlineTransport))
            {
                offlineTransport = networkManagerGo.AddComponent<OfflineNetworkTransport>();
            }

            NetworkManager.Singleton.NetworkConfig.NetworkTransport = offlineTransport;
            NetworkManager.Singleton.StartHost();
        }

        private void UpdateNetworkAddressData()
        {
            if (NetworkManager.Singleton.NetworkConfig.NetworkTransport is UnityTransport networkTransport)
            {
                networkTransport.ConnectionData = connectionData;
            }
        }
    }
}
