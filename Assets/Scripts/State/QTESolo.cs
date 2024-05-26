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
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR.Haptics;

namespace ProjectCoda.State
{
    public class QTESolo : NetworkBehaviour
    {
        public static QTESolo Instance;

        public InputActionReference up, down, right, left;

        // Empty game object used to determine how close
        // the player was to striking a perfect note
        public GameObject noteTarget;
        
        // List of "DDR" / "Guitar Hero" notes.
        public List<GameObject> notes;

        public void Start()
        {
            Instance = this;   
        }

        public void OnEnable()
        {
            
        }

        public void OnDisable()
        {
            
        }

        public static void StartSolo(ulong clientIdForSolo)
        {
            Instance.enabled = true;

            // TODO: Generate Custom Notes

            if( NetworkManager.Singleton.LocalClientId == clientIdForSolo )
            {
                Instance.GetComponent<NetworkObject>().ChangeOwnership(clientIdForSolo);
                
                // Register callbacks for user input
            }
        }

    }
}
