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
        public float jumpHeight;

        private void Start() {
        }

        private void Update() {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                Rigidbody.velocity = Vector3.up * jumpHeight;
        }

    }

}