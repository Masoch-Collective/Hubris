using System;
using Systems;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Utils;

namespace Character {

    [RequireComponent(typeof(Rigidbody))]
    public class Controller : CharacterComponent {
        
        [SerializeField]
        private BufferedInput bufferedJump = new();
        private InputAction _bufferedJumpAction;

        public bool allowControl = true;

        [Header("Walking")]
        [SerializeField]
        private float walkForce = 0.01f;
        [SerializeField]
        private float maxWalkSpeed = 0.05f;
        [SerializeField]
        private float groundDrag;
        [SerializeField]
        private float airDrag;
        [SerializeField]
        private float stunDrag = 0.8f;
        [SerializeField]
        private float attackDrag = 0.25f;
        [SerializeField]
        private Vector2 attackDragAerial = new Vector2(0.2f, 0.5f);
        [SerializeField]
        private CharacterCore.ActionStage attackStagesWithDrag;
        // Precalculated factor to multiply current velocity by
        //TODO make this framerate independent
        private float DragToApply {
            get {
                switch (Core.Status) {
                    case CharacterCore.CharacterStatus.Stunned:
                        return stunDrag;
                    case CharacterCore.CharacterStatus.Attacking:
                        if (attackStagesWithDrag.HasFlag(Core.Hitbox.Stage))
                            return CanJump ? attackDrag : 1;
                        goto default;
                    default:
                        return Core.Rigidbody.grounded ? groundDrag : airDrag;
                }
            }
        }

        [Header("Jumping")]
        [field:SerializeField]
        public float JumpForce {get; private set;} = 0.25f;
        [SerializeField]
        private float maxFallSpeed;
        [SerializeField]
        private float gravityMultJumping;
        [SerializeField]
        private float gravityMultRising;
        [SerializeField]
        private float gravityMultFalling;

        [Header("Events")]
        public UnityEvent<bool> isRunning;
        public UnityEvent onJump;
        public UnityEvent onLand;
        
        [HideInInspector]
        public int coyoteTimeDuration = 6;
        /// <summary>
        /// Returns true if the lastGroundedTime is less than coyoteTime ago. Set to true to update lastGroundedTime, false to make lastGroundedTime -inf 
        /// </summary>
        public bool CanJump {
            get => FramesSinceLastGrounded <= coyoteTimeDuration && Core.AllowJumping.Evaluate(Core);
            private set => LastGroundedFrame = value ? FrameCount : int.MinValue;
        }

        public int LastGroundedFrame {get; private set; }
        public int FramesSinceLastGrounded => LastGroundedFrame == int.MinValue ? int.MaxValue : FrameCount - LastGroundedFrame;
        public int FrameCount {get; private set; }
        private bool JumpHeld => Core.UseAIInput ? Core.AIJumpHeld : Core.ActionJump.inProgress;
        private float _pendingJumpForceMultiplier = 1f;

        private void Start() {
            
            if (gravityMultRising < gravityMultJumping)
                Debug.LogWarning("Non-jump upwards gravity (gravMultRising) is less than jump upwards gravity (gravMultJumping). This will make players jump higher if jump is not held. Was this intended?");
            
            _bufferedJumpAction = Core.ActionJump;
            bufferedJump.SetAction(_bufferedJumpAction);

        }

        private void OnDestroy() {
            if (_bufferedJumpAction != null)
                _bufferedJumpAction.started -= bufferedJump.Buffer;
        }

        private void FixedUpdate() {
            
            FrameCount++;
            bufferedJump.customTime++;
            
            if (Core.Rigidbody.grounded) {
                if (!CanJump && Core.Status == CharacterCore.CharacterStatus.Idle)
                    onLand.Invoke(); // Only consider it landing if in the air long enough for coyote time to expire (helps avoid rapid landing events when going down slopes)
                CanJump = true;
            }
            if (Core.Animator)
                Core.Animator.SetBool(Core.AnimHashBoolGrounded, CanJump);

            Vector2 velocity = Core.Rigidbody.velocity;
            
            #region Jump +++++++++++++++
            if (bufferedJump && CanJump && allowControl) {
                bufferedJump.ClearBuffer(); // Consume the last jump input once a jump is performed
                CanJump = false; // Clear coyote time
                velocity.y = JumpForce * _pendingJumpForceMultiplier;
                _pendingJumpForceMultiplier = 1f;
                onJump.Invoke();
            }
            #endregion -----------------

            #region Walk +++++++++++++++

            bool running = true;
            if (Core.AllowRunning.Evaluate(Core) && allowControl) {
                if (Core.DigitalAxisHorizontal > 0)
                    velocity.x = HorizontalForward(velocity.x);
                else if (Core.DigitalAxisHorizontal < 0)
                    velocity.x = -HorizontalForward(-velocity.x);
                else
                    running = false;
            } else
                running = false;
            if (Core.Animator)
                Core.Animator.SetBool(Core.AnimHashBoolRunning, running);
            if (!running || (Core.Status == CharacterCore.CharacterStatus.Attacking && attackStagesWithDrag.HasFlag(Core.Hitbox.Stage)))
                velocity.x *= DragToApply;
            isRunning.Invoke(running);
            
            #endregion -----------------

            #region Gravity ++++++++++++
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
            Core.Rigidbody.gravityMult = velocity.y > 0 ? 
                // Use either jumping gravity if jump is held to go higher, else use (stronger) rising gravity 
                JumpHeld ? gravityMultJumping : gravityMultRising : 
                // If falling, use falling gravity
                gravityMultFalling;
            #endregion -----------------
            
            // Freeze player in the air if attacking
            if (!CanJump && Core.Status == CharacterCore.CharacterStatus.Attacking && attackStagesWithDrag.HasFlag(Core.Hitbox.Stage)) {
                velocity *= attackDragAerial;
                Core.Rigidbody.gravityMult = 0;
            }
            Core.Rigidbody.velocity = velocity;

        }

        public void RequestJump(float jumpForceMultiplier = 1f) {
            _pendingJumpForceMultiplier = Mathf.Max(_pendingJumpForceMultiplier, jumpForceMultiplier);
            bufferedJump.Buffer();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="velocity"></param>
        /// <returns></returns>
        private float HorizontalForward(float velocity) {
            // If moving in the opposite direction that it should, apply drag
            if (velocity < 0)
                velocity *= DragToApply;
            // Apply velocity forward up to the limit of maxSpeedWalk
            if (velocity < maxWalkSpeed)
                velocity = Mathf.Max(Mathf.Min(
                        // Add walkForce to velocity
                        velocity + (walkForce * Time.fixedDeltaTime),
                        // Use maxSpeedWalk if velocity + walkForce is greater than maxSpeedWalk
                        maxWalkSpeed
                    ),
                    // Use current velocity 
                    velocity
                );
            return velocity;
        }

    }

}
