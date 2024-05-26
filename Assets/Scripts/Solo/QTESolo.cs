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

using System.Collections.Generic;
using Newtonsoft.Json.Bson;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR.Haptics;

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

        public GameObject noteContainer;

        // List of "DDR" / "Guitar Hero" notes.
        public List<GameObject> notes;

        public ulong currentSoloist;

        public int score;

        public void Start()
        {
            Instance = this;
            ToggleEnabled(false);
        }

        public void Update()
        {
            if( NetworkManager.Singleton.LocalClientId == currentSoloist && notes.Count > 0 )
            {
                bool hasInput = false;
                bool inputCorrect = false;

                if( up.action.WasPressedThisFrame() )
                {
                    hasInput = true;
                    inputCorrect = notes[0].GetComponent<QTENote>().direction.Value == QTEDirection.UP;
                }
                else if( down.action.WasPressedThisFrame() )
                {
                    hasInput = true;
                    inputCorrect = notes[0].GetComponent<QTENote>().direction.Value == QTEDirection.DOWN;
                }
                else if( right.action.WasPressedThisFrame() )
                {
                    hasInput = true;
                    inputCorrect = notes[0].GetComponent<QTENote>().direction.Value == QTEDirection.RIGHT;
                }
                else if( left.action.WasPressedThisFrame() )
                {
                    hasInput = true;
                    inputCorrect = notes[0].GetComponent<QTENote>().direction.Value == QTEDirection.LEFT;
                }

                if( hasInput )
                {
                    if( inputCorrect ) 
                    {
                        score += 1;
                    }
                    else
                    {
                        score += 0;
                    }
                }
            }
        }

        [ClientRpc(RequireOwnership = false)]
        public void ToggleEnabledClientRpc( bool enabled )
        {
            ToggleEnabled(enabled);
        }

        public void ToggleEnabled( bool enabled )
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

            for( int i = 0; i < MAX_NOTE_COUNT; i++ )
            {
                NetworkObject obj = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(notePrefab, clientIdForSolo, false, true);
                obj.transform.parent = noteContainer.transform;
                obj.transform.position = noteContainer.transform.position + Vector3.up * 1.50f * i + Vector3.right * (1.5f * Random.value - .75f );
                obj.GetComponent<QTENote>().Initialize();
            }
        }

        public static void StartSolo(ulong clientIdForSolo)
        {
            if( NetworkManager.Singleton.IsServer)
            {
                Instance.ToggleEnabledClientRpc(true);
                Instance.ToggleEnabled(true);

                Instance.GenerateNotes(clientIdForSolo);

                Instance.GetComponent<NetworkAnimator>().SetTrigger("Start");

                // TODO: Generate Custom Notes

                Instance.currentSoloist = clientIdForSolo;
            }
        }

        

    }
}
