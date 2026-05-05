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

        [SerializeField]
        private float jumpForce = 0.25f;
        [SerializeField]
        private float walkForce = 0.01f;
        
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
        }

        private void Update() {
            if (Keyboard.current.spaceKey.wasPressedThisFrame && CanJump) {
                Rigidbody.velocity = Vector3.up * jumpForce;
                CanJump = false;
            }
        }

        private void FixedUpdate() {
            
            FrameCount++;

            if (Rigidbody.grounded)
                CanJump = true;

            int walkDir = 0;
            if (Keyboard.current.aKey.isPressed)
                walkDir--;
            if (Keyboard.current.dKey.isPressed)
                walkDir++;
            Rigidbody.velocity += Vector2.right * (walkForce * walkDir);

        }

    }

}