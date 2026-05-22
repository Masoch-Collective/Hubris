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
        [SerializeField]
        private float jumpForce = 0.25f;
        [SerializeField]
        private float maxFallSpeed;
        [SerializeField]
        private float gravityMultJumping;
        [SerializeField]
        private float gravityMultRising;
        [SerializeField]
        private float gravityMultFalling;
        
        [HideInInspector]
        public int coyoteTimeDuration;
        /// <summary>
        /// Returns true if the lastGroundedTime is less than coyoteTime ago. Set to true to update lastGroundedTime, false to make lastGroundedTime -inf 
        /// </summary>
        public bool CanJump {
            get => FramesSinceLastGrounded <= coyoteTimeDuration && Core.Status == CharacterCore.CharacterStatus.Idle;
            private set => LastGroundedFrame = value ? FrameCount : int.MinValue;
        }

        public int LastGroundedFrame {get; private set; }
        public int FramesSinceLastGrounded => LastGroundedFrame == int.MinValue ? int.MaxValue : FrameCount - LastGroundedFrame;
        public int FrameCount {get; private set; }

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

            Vector2 velocity = Core.Rigidbody.velocity;
            
            #region Jump +++++++++++++++
            if (bufferedJump && CanJump) {
                bufferedJump.ClearBuffer(); // Consume the last jump input once a jump is performed
                CanJump = false; // Clear coyote time
                velocity.y = jumpForce;
            }
            #endregion -----------------

            #region Walk +++++++++++++++

            if (Core.Status == CharacterCore.CharacterStatus.Idle) {
                if (Core.DigitalAxisHorizontal > 0)
                    velocity.x = HorizontalForward(velocity.x);
                else if (Core.DigitalAxisHorizontal < 0)
                    velocity.x = -HorizontalForward(-velocity.x); 
                else
                    velocity.x *= DragToApply;
            } else
                velocity.x *= DragToApply;
            #endregion -----------------

            #region Gravity ++++++++++++
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
            Core.Rigidbody.gravityMult = velocity.y > 0 ? 
                // Use either jumping gravity if jump is held to go higher, else use (stronger) rising gravity 
                Core.ActionJump.inProgress ? gravityMultJumping : gravityMultRising : 
                // If falling, use falling gravity
                gravityMultFalling;
            #endregion -----------------
            
            Core.Rigidbody.velocity = velocity;

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