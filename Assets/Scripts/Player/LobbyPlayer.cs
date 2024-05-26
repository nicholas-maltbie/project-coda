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

using Unity.Collections;
using Unity.Netcode;

namespace ProjectCoda
{
    public class LobbyPlayer : NetworkBehaviour
    {
        private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(
            writePerm: NetworkVariableWritePermission.Owner,
            readPerm: NetworkVariableReadPermission.Everyone
        );

        public void Start()
        {
            playerName.OnValueChanged += OnPlayerNameChanged;
        }

        private string GetPlayerName(FixedString32Bytes value)
        {
            return value.ToString();
        }

        public void OnPlayerNameChanged(FixedString32Bytes previous, FixedString32Bytes current)
        {
            LobbyScreen.Instance?.AddOrUpdatePlayerName(OwnerClientId, GetPlayerName(current));
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner)
            {
                playerName.Value = PlayerInfo.PlayerName;
            }

            LobbyScreen.Instance?.AddOrUpdatePlayerName(OwnerClientId, GetPlayerName(playerName.Value));
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            LobbyScreen.Instance?.RemovePlayerById(OwnerClientId);
        }
    }
}
