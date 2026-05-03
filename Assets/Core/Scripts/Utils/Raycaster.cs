using System;
using UnityEngine;

namespace Utils {

    [Serializable]
    public class Raycaster {

        [SerializeField]
        private Transform origin;
        public Transform Origin {
            get => origin;
            set {
                if (!value)
                    useOriginPosition = useOriginRotation = useOriginScale = false;
                origin = value;
            }
        }
        [SerializeField]
        public bool useOriginPosition   = true;
        [SerializeField]
        public bool useOriginRotation   = true;
        [SerializeField]
        public bool useOriginScale      = true;
        [SerializeField]
        public Vector3 originOffset;
        [SerializeField]
        public Vector3 direction;
        [SerializeField]
        public float distance;
        [SerializeField]
        public LayerMask mask;

        public Vector3 Start {
            get {
                // By default, return the offset value
                Vector3 value = originOffset;
                // Apply scale to the offset if using the origin scale
                if (useOriginScale) value = Vector3.Scale(value, Origin.lossyScale);
                // Apply rotation to the offset if using the origin rotation
                if (useOriginRotation) value = Origin.rotation * value;
                // Apply position to the offset if using the origin position
                if (useOriginPosition) value += origin.position;
                // Return the resulting vector
                return value;
            }
        }

        public Vector3 GlobalDirection {
            get {
                Vector3 value = direction;
                // Apply scale to the direction if using the origin scale
                if (useOriginScale) value = Vector3.Scale(value, Origin.lossyScale);
                // Apply rotation to the direction if using the origin rotation
                if (useOriginRotation) value = Origin.rotation * value;
                // Return the resulting vector
                return value.normalized;
            }
        }

        public RaycastHit2D LastHit { get; private set; }

        public Raycaster(Transform origin = null, Vector3? originOffset = null, Vector3? direction = null, float distance = 1, LayerMask? mask = null) {
            Origin = origin;
            this.originOffset = originOffset ?? Vector3.zero;
            this.direction = (direction ?? Vector3.up).normalized;
            this.distance = distance;
            this.mask = mask ?? LayerMask.NameToLayer("Default");
        }

        public RaycastHit2D Cast() {
            LastHit = Physics2D.Raycast(Start, GlobalDirection, distance, mask);
            return LastHit;
        }

        public void Draw(Color col) => Debug.DrawRay(Start, GlobalDirection * distance, col);

    }

}