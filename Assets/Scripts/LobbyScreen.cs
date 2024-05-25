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

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace nickmaltbie.ProjectCoda
{
    public class LobbyScreen : MonoBehaviour
    {
        public static LobbyScreen Instance;

        [SerializeField]
        private NetworkObject lobbyPlayerPrefab;
        private Button leaveButton;
        private ScrollView playerList;
        private Dictionary<ulong, VisualElement> playerItems = new Dictionary<ulong, VisualElement>();

        public void OnEnable()
        {
            Instance ??= this;

            // The UXML is already instantiated by the UIDocument component
            UIDocument uiDocument = GetComponent<UIDocument>();

            leaveButton = uiDocument.rootVisualElement.Q("leave-game") as Button;
            playerList = uiDocument.rootVisualElement.Q("player-list") as ScrollView;
            leaveButton.RegisterCallback<ClickEvent>(LeaveGame);

            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += SpawnLobbyPlayer;
                NetworkManager.Singleton.OnClientConnectedCallback += CleanupLobbyPlayer;
            }

            if (NetworkManager.Singleton.IsHost)
            {
                SpawnLobbyPlayer(NetworkManager.Singleton.LocalClientId);
            }
        }

        public void SpawnLobbyPlayer(ulong clientId)
        {
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(lobbyPlayerPrefab, clientId, true, true, true);
        }

        public void CleanupLobbyPlayer(ulong clientId)
        {
            NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId).Despawn();
        }

        public void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            leaveButton.UnregisterCallback<ClickEvent>(LeaveGame);
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnLobbyPlayer;
            NetworkManager.Singleton.OnClientConnectedCallback -= CleanupLobbyPlayer;
        }

        public void AddPlayer(ulong clientId)
        {
            if (!playerItems.ContainsKey(clientId))
            {
                playerItems[clientId] = new Label(GetPlayerName(clientId));
                playerList.Add(playerItems[clientId]);
            }
        }

        public void RemovePlayer(ulong clientId)
        {
            if (playerItems.Remove(clientId, out VisualElement removed))
            {
                playerList.Remove(removed);
            }
        }

        private string GetPlayerName(ulong id)
        {
            return $"Player({id})";
        }

        private void LeaveGame(ClickEvent evt)
        {
            Debug.Log("Leaving Game");
            NetworkManager.Singleton?.Shutdown();
        }
    }
}
