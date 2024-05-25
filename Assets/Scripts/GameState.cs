using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ProjectCoda
{
    public class GameState : MonoBehaviour
    {
        public static GameState Instance;

        [SerializeField]
        private NetworkObject spectatorPlayerPrefab;

        [SerializeField]
        private NetworkObject musicianPlayerPrefab;

        private void OnEnable()
        {
            foreach( ulong id in NetworkManager.Singleton.ConnectedClientsIds )
            {
                SpawnMusicianPlayer(id);
            }

            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += SpawnSpectatorPlayer;
                NetworkManager.Singleton.OnClientDisconnectCallback += CleanupPlayer;
            }
        }

        public void SpawnMusicianPlayer(ulong clientId)
        {
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(musicianPlayerPrefab, clientId, true, true, true);
        }

        public void SpawnSpectatorPlayer(ulong clientId)
        {
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(spectatorPlayerPrefab, clientId, true, true, true);
        }

        public void CleanupPlayer(ulong clientId)
        {
            NetworkManager.Singleton?.SpawnManager?.GetPlayerNetworkObject(clientId)?.Despawn();
        }
    }
}
