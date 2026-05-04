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

        private void Start() {
        }

        private void Update() {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                Rigidbody.velocity = Vector3.up * jumpForce;
            int walkDir = 0;
            if (Keyboard.current.aKey.isPressed)
                walkDir --;
            if (Keyboard.current.dKey.isPressed)
                walkDir++;
            Rigidbody.velocity += Vector2.right * (walkForce * walkDir);
        }

    }

}