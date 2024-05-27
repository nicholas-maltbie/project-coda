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

using ProjectCoda.State;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectCoda.Player
{
    [RequireComponent(typeof(CharacterController2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class MusicianPlayer : NetworkBehaviour
    {
        private NetworkVariable<bool> facingRight = new NetworkVariable<bool>(
            value: true,
            writePerm: NetworkVariableWritePermission.Owner
        );

        private CharacterController2D cc;

        [SerializeField]
        private NetworkObject deathPrefab;

        [SerializeField]
        private InputActionReference playerMove;

        [SerializeField]
        private InputActionReference playerJump;

        [SerializeField]
        private InputActionReference playerCrouch;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        public bool Dead { get; private set; }

        private Rigidbody2D rb;

        public void Start()
        {
            cc = GetComponent<CharacterController2D>();
            rb = GetComponent<Rigidbody2D>();
            rb.isKinematic = true;
            facingRight.OnValueChanged += FacingChange;
            FacingChange(true, true);
        }

        public void FixedUpdate()
        {
            if (!IsOwner)
            {
                return;
            }

            // If not fighting, pause player
            if (GameState.Instance.Phase != GameState.GamePhase.Fighting)
            {
                rb.isKinematic = true;
                return;
            }
            else if (rb.isKinematic)
            {
                rb.isKinematic = false;
            }

            bool jumping = playerJump.action.IsPressed();
            bool crouching = playerCrouch.action.IsPressed();

            Vector2 move = playerMove.action.ReadValue<Vector2>();
            cc.Move(move.x, crouching, jumping);
            facingRight.Value = cc.FacingRight;
        }

        public void KillPlayer()
        {
            // Spawn in a death object
            Vector3 pos = transform.position;
            var rot = Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.up, -transform.position, Vector3.forward));
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(deathPrefab, position: pos, rotation: rot);

            Dead = true;

            // Replace with lobby player
            GetComponent<NetworkObject>().Despawn();
            GameState.Instance.SpawnSpectatorPlayer(OwnerClientId);
        }

        private void FacingChange(bool previousValue, bool newValue)
        {
            spriteRenderer.flipX = !newValue;
        }
    }
}
