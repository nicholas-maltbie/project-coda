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

using System;
using System.Collections.Generic;
using System.Linq;
using Eflatun.SceneReference;
using ProjectCoda.Player;
using Unity.Netcode;
using UnityEngine;

namespace ProjectCoda.State
{
    [RequireComponent(typeof(AudioSource))]
    public class GameState : NetworkBehaviour
    {
        public enum SFX
        {
            Start,
            Winner,
        }

        public enum GamePhase
        {
            Unknown,
            Starting,
            Fighting,
            Ending,
        }

        public float startWaitSeconds = 3.0f;

        public float endTimeoutSeconds = 1.5f;

        public float gameTime = 30.0f;

        public static GameState Instance;

        [SerializeField]
        private NetworkObject spectatorPlayerPrefab;

        [SerializeField]
        private NetworkObject musicianPlayerPrefab;

        [SerializeField]
        private SceneReference lobbyScene;

        [SerializeField]
        private AudioClip winnerSfx;

        [SerializeField]
        private AudioClip startSfx;

        public NetworkVariable<GamePhase> phase = new NetworkVariable<GamePhase>();
        
        public GamePhase Phase => phase.Value;

        private NetworkVariable<float> startElapsed = new NetworkVariable<float>();

        private NetworkVariable<float> gameElapsed = new NetworkVariable<float>();

        private NetworkVariable<float> endElapsed = new NetworkVariable<float>();

        private AudioSource audioSource;

        public float TimeToStart => Math.Max(0, startWaitSeconds - startElapsed.Value);
        public float GameTimeRemaining => Math.Max(0, gameTime - gameElapsed.Value);
        public float TimeToClose => Math.Max(0, endTimeoutSeconds - endElapsed.Value);

        private List<MusicianPlayer> players;

        public void OnEnable()
        {
            Instance ??= this;
            players = new List<MusicianPlayer>();
        }

        public void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (NetworkManager.Singleton.IsServer)
            {
                phase.Value = GamePhase.Starting;
                foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    SpawnMusicianPlayer(id);
                }

                NetworkManager.Singleton.OnClientConnectedCallback += SpawnSpectatorPlayer;
                NetworkManager.Singleton.OnClientDisconnectCallback += CleanupPlayer;
            }
        }

        public void Update()
        {
            if (!IsServer)
            {
                return;
            }

            switch (Phase)
            {
                case GamePhase.Starting:
                    CheckForStartingElapsed();
                    break;
                case GamePhase.Fighting:
                    if (CheckForWinner())
                    {
                        phase.Value = GamePhase.Ending;
                        PlaySound(SFX.Winner);
                        PlaySoundClientRPC(SFX.Winner);
                    }

                    break;
                case GamePhase.Ending:
                    CheckForElapsedEnding();
                    break;
            }
        }

        public void OnDestory()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnSpectatorPlayer;
            NetworkManager.Singleton.OnClientDisconnectCallback -= CleanupPlayer;
        }

        public void SpawnMusicianPlayer(ulong clientId)
        {
            NetworkObject player = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(musicianPlayerPrefab, clientId, true, true);
            players.Add(player.GetComponent<MusicianPlayer>());
        }

        public void SpawnSpectatorPlayer(ulong clientId)
        {
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(spectatorPlayerPrefab, clientId, true, true);
        }

        public void CleanupPlayer(ulong clientId)
        {
            NetworkObject player = NetworkManager.Singleton?.SpawnManager?.GetPlayerNetworkObject(clientId);

            if (player != null)
            {
                player.Despawn();
                MusicianPlayer musician = player.GetComponent<MusicianPlayer>();
                if (musician != null)
                {
                    players.Remove(musician);
                }
            }
        }

        public void CheckForStartingElapsed()
        {
            startElapsed.Value += Time.deltaTime;
            if (startElapsed.Value >= startWaitSeconds)
            {
                // Advance to fighting phase
                phase.Value = GamePhase.Fighting;
                PlaySound(SFX.Start);
                PlaySoundClientRPC(SFX.Start);
            }
        }

        public void CheckForElapsedEnding()
        {
            endElapsed.Value += Time.deltaTime;
            if (endElapsed.Value >= endTimeoutSeconds)
            {
                SwapToLobbyScene();
            }
        }

        public bool CheckForWinner()
        {
            gameElapsed.Value += Time.deltaTime;
            if (gameElapsed.Value >= gameTime)
            {
                return true;
            }

            var dead = players.Where(player => player == null || player.Dead).ToList();
            foreach (MusicianPlayer deadPlayer in dead)
            {
                players.Remove(deadPlayer);
            }

            return players.Count == 0;
        }

        public void SwapToLobbyScene()
        {
            NetworkManager.Singleton.SceneManager.LoadScene(lobbyScene.Name, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        [ClientRpc]
        private void PlaySoundClientRPC(SFX sfx)
        {
            PlaySound(sfx);
        }

        private void PlaySound(SFX sfx)
        {
            switch (sfx)
            {
                case SFX.Start:
                    audioSource.clip = startSfx;
                    break;
                case SFX.Winner:
                    audioSource.clip = winnerSfx;
                    break;
                default:
                    return;
            }

            audioSource.Play();
        }
    }
}
