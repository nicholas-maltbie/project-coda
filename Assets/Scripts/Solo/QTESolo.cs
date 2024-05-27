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

using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectCoda.Solo
{
    public class QTESolo : NetworkBehaviour
    {
        public static QTESolo Instance;

        public InputActionReference up, down, right, left;

        // Empty game object used to determine how close
        // the player was to striking a perfect note
        public GameObject noteTarget;

        public NetworkObject notePrefab;

        public NetworkObject beepPrefab;

        public GameObject noteContainer;

        public NetworkVariable<ulong> currentSoloist;

        public float score;

        public void Start()
        {
            Instance = this;
            ToggleEnabled(false);
        }

        public void Update()
        {
            if (NetworkManager.Singleton.LocalClientId == currentSoloist.Value && GetComponent<SpriteRenderer>().enabled)
            {
                bool hasInput = false;
                bool inputCorrect = false;

                if (up.action.WasPressedThisFrame())
                {
                    hasInput = true;
                    inputCorrect = noteContainer.transform.childCount > 0 && noteContainer.transform.GetChild(0).GetComponent<QTENote>().direction.Value == QTEDirection.UP;
                }
                else if (down.action.WasPressedThisFrame())
                {
                    hasInput = true;
                    inputCorrect = noteContainer.transform.childCount > 0 && noteContainer.transform.GetChild(0).GetComponent<QTENote>().direction.Value == QTEDirection.DOWN;
                }
                else if (right.action.WasPressedThisFrame())
                {
                    hasInput = true;
                    inputCorrect = noteContainer.transform.childCount > 0 && noteContainer.transform.GetChild(0).GetComponent<QTENote>().direction.Value == QTEDirection.RIGHT;
                }
                else if (left.action.WasPressedThisFrame())
                {
                    hasInput = true;
                    inputCorrect = noteContainer.transform.childCount > 0 && noteContainer.transform.GetChild(0).GetComponent<QTENote>().direction.Value == QTEDirection.LEFT;
                }

                if (hasInput)
                {
                    if (inputCorrect)
                    {
                        float scoreIncrease = Mathf.Max(1f - Mathf.Abs(noteContainer.transform.GetChild(0).position.y - noteTarget.transform.position.y), 0);
                        score += scoreIncrease;
                    }

                    BeepServerRpc();
                    PopNoteServerRpc();

                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void PopNoteServerRpc()
        {
            if (noteContainer.transform.childCount != 0)
            {
                Destroy(noteContainer.transform.GetChild(0).gameObject);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void BeepServerRpc()
        {
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(beepPrefab, 0, true);
        }

        [ClientRpc(RequireOwnership = false)]
        public void ToggleEnabledClientRpc(bool enabled)
        {
            ToggleEnabled(enabled);
        }

        public void ToggleEnabled(bool enabled)
        {
            GetComponent<SpriteRenderer>().enabled = enabled;
        }

        public void GenerateNotes(ulong clientIdForSolo)
        {
            // Destroy old children
            foreach (Transform child in noteContainer.transform)
            {
                Destroy(child.gameObject);
            }

            const int MAX_NOTE_COUNT = 4;

            for (int i = 0; i < MAX_NOTE_COUNT; i++)
            {
                NetworkObject obj = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(notePrefab, clientIdForSolo, true, false);
                obj.transform.parent = noteContainer.transform;
                obj.transform.position = noteContainer.transform.position + Vector3.up * 1.50f * i + Vector3.right * (1.5f * Random.value - .75f);
                obj.GetComponent<QTENote>().Initialize();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartSoloServerRpc(ulong clientIdForSolo)
        {
            ToggleEnabledClientRpc(true);
            ToggleEnabled(true);

            GenerateNotes(clientIdForSolo);

            GetComponent<NetworkAnimator>().SetTrigger("Start");

            // TODO: Generate Custom Notes

            currentSoloist.Value = clientIdForSolo;
        }
    }
}
