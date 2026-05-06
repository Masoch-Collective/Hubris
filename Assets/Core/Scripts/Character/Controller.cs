using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Character {

    [RequireComponent(typeof(Rigidbody))]
    public class Controller : MonoBehaviour {

        private Rigidbody _rigidbody;
        public Rigidbody Rigidbody {
            get {
                if (!_rigidbody)
                    _rigidbody = GetComponent<Rigidbody>();
                return _rigidbody;
            }
        }

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
        private float DragToApply => Rigidbody.grounded ? groundDrag : airDrag;

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
            get => FramesSinceLastGrounded <= coyoteTimeDuration;
            private set => LastGroundedFrame = value ? FrameCount : int.MinValue;
        }

        public int LastGroundedFrame {get; private set; }
        public int FramesSinceLastGrounded => LastGroundedFrame == int.MinValue ? int.MaxValue : FrameCount - LastGroundedFrame;
        public int FrameCount {get; private set; }

        private void Start() {
            if (gravityMultRising < gravityMultJumping)
                Debug.LogWarning("Non-jump upwards gravity (gravMultRising) is less than jump upwards gravity (gravMultJumping). This will make players jump higher if jump is not held. Was this intended?");
        }

        private void Update() {
            if (Keyboard.current.spaceKey.wasPressedThisFrame && CanJump) {
                
                Rigidbody.velocity = Rigidbody.velocity * Vector2.right + Vector2.up * jumpForce;
                CanJump = false;
            }
        }

        private void FixedUpdate() {
            
            FrameCount++;

            if (Rigidbody.grounded)
                CanJump = true;

            // I ended up not needing to do it like this, but it's funnier this way, so I'm gonna keep it lol
            bool l = Keyboard.current.aKey.isPressed;
            bool r = Keyboard.current.dKey.isPressed;
            int walkDir = l == r ? 0 : l ? -1 : 1;

            Vector2 velocity = Rigidbody.velocity;

            if (walkDir > 0)
                velocity.x = HorizontalForward(velocity.x);
            else if (walkDir < 0)
                velocity.x = -HorizontalForward(-velocity.x);
            else 
                velocity.x *= DragToApply;

            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);

            Rigidbody.velocity = velocity;
            Rigidbody.gravityMult = velocity.y > 0 ? 
                // Use either jumping gravity if jump is held to go higher, else use (stronger) rising gravity 
                Keyboard.current.spaceKey.isPressed ? gravityMultJumping : gravityMultRising : 
                // If falling, use falling gravity
                gravityMultFalling;

        }
        
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