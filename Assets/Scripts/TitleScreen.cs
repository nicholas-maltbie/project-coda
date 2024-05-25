// Copyright (C) 2024 Nicholas Maltbie
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

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.Netcode.Transports.UTP.UnityTransport;

namespace nickmaltbie.ProjectCoda
{
    public class TitleScreen : MonoBehaviour
    {
        [SerializeField]
        private NetworkManager networkManager;

        [SerializeField]
        private UnityTransport networkTransport;

        private Button hostButton;
        private Button joinButton;
        private Button quitButton;
        private TextField serverAddressField;
        private IntegerField serverPortField;

        public void OnEnable()
        {
            // The UXML is already instantiated by the UIDocument component
            UIDocument uiDocument = GetComponent<UIDocument>();

            hostButton = uiDocument.rootVisualElement.Q("host-game") as Button;
            joinButton = uiDocument.rootVisualElement.Q("join-game") as Button;
            quitButton = uiDocument.rootVisualElement.Q("quit-game") as Button;
            serverAddressField = uiDocument.rootVisualElement.Q("server-address") as TextField;
            serverPortField = uiDocument.rootVisualElement.Q("server-port") as IntegerField;

            hostButton.RegisterCallback<ClickEvent>(HostGame);
            joinButton.RegisterCallback<ClickEvent>(JoinGame);
            quitButton.RegisterCallback<ClickEvent>(QuitGame);
            serverAddressField.RegisterValueChangedCallback(OnServerAddressChange);
            serverPortField.RegisterValueChangedCallback(OnServerPortChange);

            ConnectionAddressData data = networkTransport.ConnectionData;
            data.Address = serverAddressField.value;
            data.Port = (ushort) serverPortField.value;
            networkTransport.ConnectionData = data;
        }

        public void OnDisable()
        {
            hostButton.UnregisterCallback<ClickEvent>(HostGame);
            joinButton.UnregisterCallback<ClickEvent>(JoinGame);
            quitButton.UnregisterCallback<ClickEvent>(QuitGame);
            serverAddressField.UnregisterValueChangedCallback(OnServerAddressChange);
            serverPortField.UnregisterValueChangedCallback(OnServerPortChange);
        }

        private void HostGame(ClickEvent evt)
        {
            networkManager.StartHost();
        }

        private void JoinGame(ClickEvent evt)
        {
            networkManager.StartClient();
        }

        private void QuitGame(ClickEvent evt)
        {
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnServerAddressChange(ChangeEvent<string> evt)
        {
            ConnectionAddressData current = networkTransport.ConnectionData;
            current.Address = evt.newValue;
            networkTransport.ConnectionData = current;
        }

        private void OnServerPortChange(ChangeEvent<int> evt)
        {
            ConnectionAddressData current = networkTransport.ConnectionData;
            current.Port = (ushort) evt.newValue;
            networkTransport.ConnectionData = current;
        }
    }
}
