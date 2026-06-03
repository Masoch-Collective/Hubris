using System;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Character {

    [RequireComponent(typeof(Rigidbody))]
    public class Controller : CharacterComponent {
        
        [SerializeField]
        private BufferedInput bufferedJump = new();

        [Header("Walking")]
        [SerializeField]
        private float walkForce = 0.01f;
        [SerializeField]
        private float maxWalkSpeed = 0.05f;
        [SerializeField]
        private float groundDrag;
        [SerializeField]
        private float airDrag;
        // Precalculated factor to multiply current velocity by
        //TODO make this framerate independent
        private float DragToApply => Core.Rigidbody.grounded ? groundDrag : airDrag;

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

        private void Start() {
            
            if (gravityMultRising < gravityMultJumping)
                Debug.LogWarning("Non-jump upwards gravity (gravMultRising) is less than jump upwards gravity (gravMultJumping). This will make players jump higher if jump is not held. Was this intended?");
            
            bufferedJump.SetAction(Core.ActionJump);

        }

        private void FixedUpdate() {
            
            FrameCount++;
            bufferedJump.customTime++;
            
            if (Core.Rigidbody.grounded)
                CanJump = true;
            if (Core.Animator)
                Core.Animator.SetBool(Core.AnimHashBoolGrounded, CanJump);

            Vector2 velocity = Core.Rigidbody.velocity;
            
            #region Jump +++++++++++++++
            if (bufferedJump && CanJump) {
                bufferedJump.ClearBuffer(); // Consume the last jump input once a jump is performed
                CanJump = false; // Clear coyote time
                velocity.y = JumpForce;
            }
            #endregion -----------------

            #region Walk +++++++++++++++

            bool running = true;
            if (Core.AllowRunning.Evaluate(Core)) {
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
            if (!running)
                velocity.x *= DragToApply;
            
            #endregion -----------------

            #region Gravity ++++++++++++
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
            Core.Rigidbody.gravityMult = velocity.y > 0 ? 
                // Use either jumping gravity if jump is held to go higher, else use (stronger) rising gravity 
                JumpHeld ? gravityMultJumping : gravityMultRising : 
                // If falling, use falling gravity
                gravityMultFalling;
            #endregion -----------------
            
            Core.Rigidbody.velocity = velocity;

        }

        public void RequestJump() => bufferedJump.Buffer();
        
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
