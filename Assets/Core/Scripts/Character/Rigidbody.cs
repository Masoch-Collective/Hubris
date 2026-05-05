using UnityEditor;
using UnityEngine;
using Utils;

namespace Character {

    public class Rigidbody : MonoBehaviour {

        
        [SerializeField]
        private LayerMask collisionLayers; // What layers to use for all other collision raycasts (will be combined with groundLayers)
        [SerializeField]
        private PixelPerfectFloat verticalGap; // How far apart the upwards & downwards raycasts should be from one another
        [SerializeField]
        private PixelPerfectFloat centerHeight; // Vertical raycast start
        [SerializeField]
        private PixelPerfectFloat sidesHeight; // Vertical raycast start
        [SerializeField]
        private PixelPerfectFloat headHeight; // How far head raycasts travel
        [SerializeField]
        private PixelPerfectFloat sideWidth; // How far apart the side raycasts should be from one another
        [SerializeField]
        private PixelPerfectFloat footHeight; // How far head raycasts travel
        [SerializeField]
        private bool applyVerticalOverlapCorrectionIfBothSidesCollide; // Isekai-ass variable name. Used to specify if vertical overlap correction should occur if both horizontal Raycasters hit

        private Raycaster _headLeft;
        private Raycaster _headRight;
        private Raycaster _sideLeft;
        private Raycaster _sideRight;
        private Raycaster _footLeft;
        private Raycaster _footRight;

        public Vector2 velocity;
        public bool grounded;
        [field: SerializeField]
        public bool bothSidesHit;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            UpdateRaycasterParameters();
        }

        // Update is called once per frame
        void FixedUpdate() {

            velocity += Physics2D.gravity * Time.fixedDeltaTime;

            Move(ref velocity);

            grounded = _footLeft.LastHit || _footRight.LastHit;
            bothSidesHit = _sideLeft.LastHit && _sideRight.LastHit;

        }
        
        /// <summary>
        /// Moves the body by the given <paramref name="amount"/>, corrects for collision overlaps, and resets <paramref name="amount"/> on collision;
        /// </summary>
        /// <param name="amount">Movement to apply to the body</param>
        private void Move(ref Vector2 amount) {
            
            transform.Translate(amount);
            
            // Apply horizontal collision + correction first so vertical overlap correction only occurs iff there is still vertical overlap after correcting for horizontal overlap
            UpdateHorizontalRaycasterHits();
            transform.Translate(HorizontalCollisions(ref amount));

            UpdateVerticalRaycasterHits();
            if (!applyVerticalOverlapCorrectionIfBothSidesCollide && _sideLeft.LastHit && _sideRight.LastHit) {
                // If both sides collide and body is configured not to apply vertical correction in such case, only perform collision check
                VerticalCollisions(ref amount);
                transform.Translate(Vector3.up * -amount.y);
            } else {
                //Otherwise, perform collision check and apply vertical overlap correction
                transform.Translate(VerticalCollisions(ref amount));
            }
        }

        /// <summary>
        /// Resets velocity and calculates overlap correction if a collision is detected horizontally; aborts if collision is detected on both sides
        /// </summary>
        /// <param name="amount">Reference amount to move; resets x value on collision</param>
        /// <returns>Overlap</returns>
        private Vector2 HorizontalCollisions(ref Vector2 amount) {

            Vector3 correction = Vector3.zero;

            // If both or neither side raycasts hit something, abort horizontal collision
            if (_sideLeft.LastHit == _sideRight.LastHit)
                return correction;
            
            if (_sideLeft.LastHit) {
                correction.x = _sideLeft.distance - _sideLeft.LastHit.distance;
                amount.x = 0;
            }
            if (_sideRight.LastHit) {
                correction.x = -(_sideRight.distance - _sideRight.LastHit.distance);
                amount.x = 0;
            }

            return correction;

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
                    if (_footLeft.LastHit && _footRight.LastHit)
                        // If both feet collided, apply overlap correction according to whichever one is stepped higher (allows for good ramp behaviour)
                        if (_footLeft.LastHit.distance < _footRight.LastHit.distance)
                            validCast = _footLeft;
                        else 
                            validCast = _footRight;
                    else
                        validCast = _footLeft.LastHit ? _footLeft : _footRight;
                    correction.y = validCast.distance - validCast.LastHit.distance;
                    amount.y = 0;
                }
            } else {
                if (_headLeft.LastHit || _headRight.LastHit) {
                    if (_headLeft.LastHit && _headRight.LastHit)
                        // If both feet collided, apply overlap correction according to whichever one is stepped higher (allows for good ramp behaviour)
                        if (_headLeft.LastHit.distance < _headRight.LastHit.distance)
                            validCast = _headLeft;
                        else 
                            validCast = _headRight;
                    else
                        validCast = _headLeft.LastHit ? _headLeft : _footRight;
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
            _headLeft   ??= new Raycaster(transform);
            _headRight  ??= new Raycaster(transform);
            _sideLeft   ??= new Raycaster(transform);
            _sideRight  ??= new Raycaster(transform);
            _footLeft   ??= new Raycaster(transform);
            _footRight  ??= new Raycaster(transform);

            // Ensure all Raycasters have the same masks and origins
            _headLeft.mask  = 
            _headRight.mask = 
            _sideLeft.mask  = 
            _sideRight.mask = 
            _footLeft.mask  = 
            _footRight.mask = collisionLayers;
            _headLeft.Origin  = 
            _headRight.Origin = 
            _sideLeft.Origin  = 
            _sideRight.Origin = 
            _footLeft.Origin  = 
            _footRight.Origin = transform;
            
            // Set distances
            _headLeft.distance  = 
            _headRight.distance = 
            headHeight - centerHeight;

            _sideLeft.distance =
            _sideRight.distance =
            sideWidth;
            
            _footLeft.distance  = 
            _footRight.distance = 
            footHeight + centerHeight;
            
            // Set directions
            _headLeft   .direction = Vector3.up;
            _headRight  .direction = Vector3.up;
            _sideLeft   .direction = Vector3.left;
            _sideRight  .direction = Vector3.right;
            _footLeft   .direction = Vector3.down;
            _footRight  .direction = Vector3.down;

            // Set offsets
            _headLeft.originOffset =
                Vector3.up * centerHeight +
                Vector3.left * verticalGap;
            _headRight.originOffset =
                Vector3.up * centerHeight +
                Vector3.right * verticalGap;
            _sideLeft.originOffset  =
            _sideRight.originOffset =
                Vector3.up * sidesHeight;
            _footLeft.originOffset =
                Vector3.up * centerHeight +
                Vector3.left * verticalGap;
            _footRight.originOffset =
                Vector3.up * centerHeight +
                Vector3.right * verticalGap;
            
        }
        
        /// <summary>
        /// Casts all vertical raycasters (feet and heads) and updates their LastHit value
        /// </summary>
        private void UpdateVerticalRaycasterHits() {
            _headLeft.Cast();
            _headRight.Cast();
            _footLeft.Cast();
            _footRight.Cast();
        }
        /// <summary>
        /// Casts all horizontal raycasters (sides) and updates their LastHit value
        /// </summary>
        private void UpdateHorizontalRaycasterHits(){
            _sideLeft.Cast();
            _sideRight.Cast();
        }
        
        #if UNITY_EDITOR

        private void OnValidate() => UpdateRaycasterParameters();

        private void OnDrawGizmos() {
            _headLeft   .DrawGizmo(Color.greenYellow,   1f / PixelPerfectFloat.PixelsPerUnit);
            _headRight  .DrawGizmo(Color.greenYellow,   1f / PixelPerfectFloat.PixelsPerUnit);
            _sideLeft   .DrawGizmo(Color.red,           1f / PixelPerfectFloat.PixelsPerUnit);
            _sideRight  .DrawGizmo(Color.red,           1f / PixelPerfectFloat.PixelsPerUnit);
            _footLeft   .DrawGizmo(Color.darkGreen,     1f / PixelPerfectFloat.PixelsPerUnit);
            _footRight  .DrawGizmo(Color.darkGreen,     1f / PixelPerfectFloat.PixelsPerUnit);
        }

        [MenuItem("Utilities/Reset All Raycasters")]
        private static void ResetRaycasters() {
            foreach (Rigidbody body in FindObjectsByType<Rigidbody>(FindObjectsSortMode.None)) {
                body._headLeft  = 
                body._headRight = 
                body._sideLeft  = 
                body._sideRight = 
                body._footLeft  = 
                body._footRight = 
                null;
                body.UpdateRaycasterParameters();
            }
        }
        
        #endif

    }

}