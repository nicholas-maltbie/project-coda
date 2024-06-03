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

using System.Collections;
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
        private InputActionReference playerAttack;

        [SerializeField]
        private AudioClip knockSfx;

        public bool IsAttacking { get; private set; } = false;

        private NetworkVariable<bool> isKnocked = new NetworkVariable<bool>(
            value: false,
            writePerm: NetworkVariableWritePermission.Owner
        );

        private bool canAttack = true;

        private const float ATTACK_COOLDOWN = 2f;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        public bool Dead { get; private set; }

        private Rigidbody2D rb;

        public void Start()
        {
            PlayerNameTracker.Singleton.AddPlayer(this);

            cc = GetComponent<CharacterController2D>();
            rb = GetComponent<Rigidbody2D>();
            rb.isKinematic = true;
            facingRight.OnValueChanged += FacingChange;
            FacingChange(true, true);
        }

        public override void OnDestroy()
        {
            PlayerNameTracker.Singleton.RemovePlayer(this);
            base.OnDestroy();
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
            bool attackStart = playerAttack.action.IsPressed();

            Vector2 move = Vector2.zero;
            if (!IsAttacking && !isKnocked.Value)
            {
                if (attackStart && canAttack)
                {
                    IsAttacking = true;
                    StartCoroutine(Attack());
                }
                else
                {
                    move = playerMove.action.ReadValue<Vector2>();
                    cc.Move(move.x, crouching, jumping);
                    facingRight.Value = cc.FacingRight;
                }
            }

            Animator anim = GetComponent<Animator>();
            anim.SetBool("Knocked", isKnocked.Value);
            anim.SetBool("Attacking", IsAttacking);
            anim.SetBool("Moving", Mathf.Abs(move.x) > .1f);
        }

        public void KillPlayer()
        {
            // Spawn in a death object
            // Vector3 pos = transform.position;
            // var rot = Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.up, -transform.position, Vector3.forward));
            // NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(deathPrefab, position: pos, rotation: rot);

            Dead = true;

            // Replace with lobby player
            GetComponent<NetworkObject>().Despawn();
            GameState.Instance.SpawnSpectatorPlayer(OwnerClientId);
        }

        private void FacingChange(bool previousValue, bool newValue)
        {
            spriteRenderer.flipX = !newValue;
        }

        [ServerRpc(RequireOwnership = false)]
        public void KnockPlayerServerRpc(Vector3 origin)
        {
            const float HORIZONTAL_KNOCK = 10f;
            Vector3 direction = origin.x - transform.position.x > 0 ? Vector3.left : Vector3.right;
            direction *= HORIZONTAL_KNOCK;

            const float VERTICAL_KNOCK = 10f;
            direction += VERTICAL_KNOCK * Vector3.up;

            DoKnockClientRpc(direction);
            if (!isKnocked.Value)
            {
                GetComponent<AudioSource>().PlayOneShot(knockSfx);
            }
        }

        [ClientRpc(RequireOwnership = false)]
        public void DoKnockClientRpc(Vector3 direction)
        {
            if (!isKnocked.Value)
            {
                GetComponent<AudioSource>().PlayOneShot(knockSfx);
                if (IsOwner)
                {
                    Debug.Log("Knock " + direction);
                    StartCoroutine(DoKnock(direction));
                }
            }
        }

        public IEnumerator DoKnock(Vector3 direction)
        {
            isKnocked.Value = true;
            rb.velocity = direction;
            const float KNOCK_DURATION = .75f;
            yield return new WaitForSeconds(KNOCK_DURATION);
            isKnocked.Value = false;
        }

        public IEnumerator Attack()
        {
            const float ATTACK_DURATION = .5f;
            const float ATTACK_RANGE = 1f;
            StartCoroutine(AttackCooldown());
            foreach (Animator anim in GetComponentsInChildren<Animator>())
            {
                anim.SetTrigger("IsAttacking");
            }

            GetComponent<Animator>().SetBool("Attacking", true);

            Vector3 direction = Vector3.left;
            if (facingRight.Value)
            {
                direction = Vector3.right;
            }

            var offset = new Vector3(0, .5f);

            rb.velocity = new Vector3(direction.x * 12, rb.velocity.y);

            float elapsed = 0;
            while (elapsed < ATTACK_DURATION)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position + offset, direction, ATTACK_RANGE);
                Debug.DrawRay(transform.position + offset, direction * ATTACK_RANGE, Color.blue);
                if (hit)
                {
                    // knock hit player
                    // Debug.Log("Hit " + hit.collider.gameObject.name);
                    hit.collider.GetComponent<MusicianPlayer>().KnockPlayerServerRpc(transform.position);
                }

                yield return null;
                elapsed += Time.deltaTime;
            }

            IsAttacking = false;
            GetComponent<Animator>().SetBool("Attacking", false);
        }

        public IEnumerator AttackCooldown()
        {
            canAttack = false;
            yield return new WaitForSeconds(ATTACK_COOLDOWN);
            canAttack = true;
        }
    }
}
