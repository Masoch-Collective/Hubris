using UnityEngine;
using Utils;

namespace Character {

    public class Rigidbody : MonoBehaviour {

        
        [SerializeField]
        private LayerMask collisionLayers; // What layers to use for all other collision raycasts (will be combined with groundLayers)
        [SerializeField]
        private PixelPerfectFloat verticalRaycastGap; // How far apart the downwards raycasts should be from one another
        [SerializeField]
        private PixelPerfectFloat feetBottom; // How far down raycasts start
        [SerializeField]
        private PixelPerfectFloat headHeight; // How far up raycasts start
        [SerializeField]
        private PixelPerfectFloat raycastLength; // How far raycasts travel

        private Raycaster _footLeft;
        private Raycaster _footRight;
        private Raycaster _headLeft;
        private Raycaster _headRight;

        public Vector2 velocity;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            UpdateRaycasterParameters();
        }

        // Update is called once per frame
        void FixedUpdate() {

            velocity += Physics2D.gravity * Time.fixedDeltaTime;

            Move(ref velocity);

        }
        
        /// <summary>
        /// Moves the body by the given <paramref name="amount"/>, corrects for collision overlaps, and resets <paramref name="amount"/> on collision;
        /// </summary>
        /// <param name="amount">Movement to apply to the body</param>
        private void Move(ref Vector2 amount) {
            transform.Translate(amount);
            UpdateRaycasterHits();
            transform.Translate(VerticalCollisions(ref amount));
        }

        /// <summary>
        /// Resets velocity and calculates overlap correction if a collision is detected vertically
        /// </summary>
        /// <param name="amount">Reference amount to move; resets y value on collision</param>
        /// <returns>Overlap</returns>
        private Vector2 VerticalCollisions(ref Vector2 amount) {

            float direction = amount.y == 0 ? 0 : Mathf.Sign(amount.y);
            Vector3 correction = Vector3.zero;

            Raycaster validCast;
            if (direction <= 0) {
                if (_footLeft.LastHit || _footRight.LastHit) {
                    validCast = _footLeft.LastHit ? _footLeft : _footRight;
                    correction.y = validCast.distance - validCast.LastHit.distance;
                    amount.y = 0;
                }
            } else {
                if (_headLeft.LastHit || _headRight.LastHit) {
                    validCast = _headLeft.LastHit ? _headLeft : _headRight;
                    correction.y = -(validCast.distance - validCast.LastHit.distance);
                    amount.y = 0;
                }
            }

            return correction;

        }
        
        /// <summary>
        /// Updates all Raycasters' fields to reflect character configuration
        /// </summary>
        private void UpdateRaycasterParameters() {
            
            // Ensure all Raycasters exist
            _footLeft   ??= new Raycaster(transform);
            _footRight  ??= new Raycaster(transform);
            _headLeft   ??= new Raycaster(transform);
            _headRight  ??= new Raycaster(transform);

            // Ensure all Raycasters have the same masks and distances
            _footLeft.mask      = _footRight.mask       = _headLeft.mask        = _footRight.mask       = collisionLayers;
            _footLeft.distance  = _footRight.distance   = _headLeft.distance    = _headRight.distance   = raycastLength;
            
            // Set directions
            _footLeft   .direction = Vector3.down;
            _footRight  .direction = Vector3.down;
            _headLeft   .direction = Vector3.up;
            _headRight  .direction = Vector3.up;

            // Set offsets
            _footLeft.originOffset =
                Vector3.down * feetBottom +
                Vector3.left * verticalRaycastGap;
            _footRight.originOffset =
                Vector3.down * feetBottom +
                Vector3.right * verticalRaycastGap;
            _headLeft.originOffset =
                Vector3.up * headHeight +
                Vector3.left * verticalRaycastGap;
            _headRight.originOffset =
                Vector3.up * headHeight +
                Vector3.right * verticalRaycastGap;
            
        }
        
        /// <summary>
        /// Casts all raycasters and updates their LastHit value
        /// </summary>
        private void UpdateRaycasterHits() {
            _footLeft.Cast();
            _footRight.Cast();
            _headLeft.Cast();
            _headRight.Cast();
        }
        
        #if UNITY_EDITOR

        private void OnValidate() => UpdateRaycasterParameters();

        private void OnDrawGizmos() {
            _footLeft   .DrawGizmo(Color.darkGreen, 1f / PixelPerfectFloat.PixelsPerUnit);
            _footRight  .DrawGizmo(Color.darkGreen, 1f / PixelPerfectFloat.PixelsPerUnit);
            _headLeft   .DrawGizmo(Color.greenYellow, 1f / PixelPerfectFloat.PixelsPerUnit);
            _headRight  .DrawGizmo(Color.greenYellow, 1f / PixelPerfectFloat.PixelsPerUnit);
        }
        
        #endif

    }

}