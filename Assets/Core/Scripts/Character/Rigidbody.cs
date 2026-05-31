using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Character {

    public class Rigidbody : MonoBehaviour {

        public float gravityMult = 1;
        [SerializeField]
        private LayerMask collisionLayers; // What layers to use for collision raycasts
        [SerializeField]
        private PixelPerfectFloat horizontalGap; // How far apart the upwards & downwards raycasts should be from one another
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
        private float oneWayThreshold; // Threshold for dot product when comparing one-way platform direction and velocity
        [SerializeField]
        private bool applyVerticalOverlapCorrectionIfBothSidesCollide; // Isekai-ass variable name. Used to specify if vertical overlap correction should occur if both horizontal Raycasters hit
        [SerializeField, Tooltip("How many physics update frames to preserve upwards momentum for. Allows bodies to maintain their momentum if they bonk their head on the very edge of a collider that would not be in their way a few frames later.\nUse 0 for no preservation, -1 for infinite preservation.")]
        private int upwardsMomentumPreservationWindow;
        [NonSerialized]
        private int _upwardsMomentumPreservationTimer;

        [NonSerialized] private Raycaster _headLeft;
        [NonSerialized] private Raycaster _headRight;
        [NonSerialized] private Raycaster _sideLeft;
        [NonSerialized] private Raycaster _sideRight;
        [NonSerialized] private Raycaster _footLeft;
        [NonSerialized] private Raycaster _footRight;

        // ReSharper disable once MemberCanBePrivate.Global
        public Vector2 ColliderCentroid => transform.position + Vector3.up * centerHeight;
        public Vector2 velocity;
        public bool grounded;

        public float ShortestRaycastDistance => Mathf.Max(float.Epsilon, Mathf.Min(
            headHeight, 
            sideWidth, 
            footHeight
        ));
        [field: SerializeField]
        public bool bothSidesHit;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            UpdateRaycasterParameters();
        }

        // Update is called once per frame
        void FixedUpdate() {

            velocity += gravityMult * Time.fixedDeltaTime * Physics2D.gravity;

            Move(ref velocity);

            grounded = _footLeft.LastHit || _footRight.LastHit;
            bothSidesHit = _sideLeft.LastHit && _sideRight.LastHit;

        }

        /// <summary>
        /// Moves the body by the given <paramref name="amount"/>, corrects for collision overlaps, and resets <paramref name="amount"/> on collision.
        /// </summary>
        /// <param name="amount">Movement to apply to the body.</param>
        /// <param name="preventClipping">Whether to reduce <paramref name="amount"/> if it would move the body inside a collider (defaults to true.)</param>
        private void Move(ref Vector2 amount, bool preventClipping = true) {
            // NOTE: The following calculation is rather primitive and not at all physically accurate, but is adequate for our purposes
            if (preventClipping) {
                // Determine if the desired move amount will move the centroid inside a collider
                RaycastHit2D hit = Physics2D.Raycast(ColliderCentroid, amount,
                    amount.magnitude, collisionLayers);
                // Apply collision overlap correction if the overlap check is true
                if (hit && (!hit.collider.CompareTag("OneWay") || Vector2.Dot(hit.collider.transform.up, amount.normalized) < oneWayThreshold)) {
                    // Limit the amount to the max distance we can move without colliding
                    amount = hit.point - ColliderCentroid;
                    // Reduce the amount slightly more according to the body's smallest collider dimension
                    amount += -amount.normalized * ShortestRaycastDistance;
                    Debug.LogWarning("Overlap correction from last frame position applied!");
                }
            }
            
            transform.Translate(amount);

            // Apply horizontal collision + correction first so vertical overlap correction only occurs iff there is still vertical overlap after correcting for horizontal overlap
            UpdateHorizontalRaycasterHits();
            transform.Translate(HorizontalCollisions(ref amount));

            UpdateVerticalRaycasterHits();
            if (!applyVerticalOverlapCorrectionIfBothSidesCollide && CheckOneWay(_sideLeft) && CheckOneWay(_sideRight)) {
                // If both sides collide and body is configured not to apply vertical correction in such case, only perform collision check
                VerticalCollisions(ref amount);
                transform.Translate(Vector3.up * -amount.y);
            } else {
                //Otherwise, perform collision check and apply vertical overlap correction
                transform.Translate(VerticalCollisions(ref amount));
            }
        }

        /// <summary>
        /// Resets velocity and calculates overlap correction if a collision is detected horizontally; aborts if collision is detected on both sides.
        /// </summary>
        /// <param name="amount">Reference amount to move; resets x value on collision.</param>
        /// <returns>Overlapping distance.</returns>
        private Vector2 HorizontalCollisions(ref Vector2 amount) {

            Vector3 correction = Vector3.zero;

            // If both or neither side raycasts hit something do not perform overlap correction (since we don't know in which direction to correct)
            if (CheckOneWay(_sideLeft) && CheckOneWay(_sideRight)) {
                Debug.LogWarning("Both left and right raycasts hit something!");
                amount.x = 0;
                return correction;
            }

            if (CheckOneWay(_sideLeft)) {
                correction.x = (_sideLeft.LastHit.distance - _sideLeft.distance) * Mathf.Sign(_sideLeft.GlobalDirection.x);
                amount.x = 0;
            }
            if (CheckOneWay(_sideRight)) {
                correction.x = (_sideRight.LastHit.distance - _sideRight.distance) * Mathf.Sign(_sideRight.GlobalDirection.x);
                amount.x = 0;
            }

            return correction;

        }

        /// <summary>
        /// Resets velocity and calculates overlap correction if a collision is detected vertically.
        /// </summary>
        /// <param name="amount">Reference amount to move; resets y value on collision.</param>
        /// <returns>Overlapping distance.</returns>
        private Vector2 VerticalCollisions(ref Vector2 amount) {

            float direction = amount.y == 0 ? 0 : Mathf.Sign(amount.y);
            Vector3 correction = Vector3.zero;

            Raycaster validCast;
            if (direction <= 0) {
                if (CheckOneWay(_footLeft) || CheckOneWay(_footRight)) {
                    if (CheckOneWay(_footLeft) && CheckOneWay(_footRight))
                        // If both feet collided, apply overlap correction according to whichever one is stepped higher (allows for good ramp behaviour)
                        if (_footLeft.LastHit.distance < _footRight.LastHit.distance)
                            validCast = _footLeft;
                        else 
                            validCast = _footRight;
                    else
                        validCast = CheckOneWay(_footLeft) ? _footLeft : _footRight;
                    correction.y = validCast.distance - validCast.LastHit.distance;
                    amount.y = 0;
                }
            } else {
                if (CheckOneWay(_headLeft) || CheckOneWay(_headRight)) {
                    if (CheckOneWay(_headLeft) && CheckOneWay(_headRight))

                        // If both side of head collided, apply overlap correction according to whichever one is stepped higher (allows for good ramp behaviour)
                        if (_headLeft.LastHit.distance < _headRight.LastHit.distance)
                            validCast = _headLeft;
                        else
                            validCast = _headRight;
                    else
                        validCast = CheckOneWay(_headLeft) ? _headLeft : _footRight;
                    correction.y = -(validCast.distance - validCast.LastHit.distance);
                    // Only reset upwards momentum if hitting something above for more than n consecutive frames
                    // Where n is upwardsMomentumPreservationWindow
                    _upwardsMomentumPreservationTimer++;
                    if (upwardsMomentumPreservationWindow != -1 &&
                        _upwardsMomentumPreservationTimer > upwardsMomentumPreservationWindow)
                        amount.y = 0;
                } else
                    _upwardsMomentumPreservationTimer = 0;
            }

            return correction;

        }

        /// <summary>
        /// Evaluate Raycasters to check the approach vector for one-way platforms.
        /// </summary>
        /// <param name="rc">Raycaster.</param>
        /// <returns>Whether the raycaster hit solid ground, or if a one-way platform is approached from below.</returns>
        private bool CheckOneWay(Raycaster rc) => CheckOneWay(rc, velocity);

        /// <summary>
        /// Evaluate Raycasters to check the approach vector for one-way platforms.
        /// </summary>
        /// <param name="rc">Raycaster.</param>
        /// <param name="direction">Approach vector.</param>
        /// <returns>Whether the raycaster hit solid ground, or if a one-way platform is approached from below.</returns>
        private bool CheckOneWay(Raycaster rc, Vector2 direction) {
            
            if (!rc.LastHit)
                return false;

            if (!rc.LastHit.collider.CompareTag("OneWay"))
                return true;

            float dot = Vector2.Dot(rc.LastHit.transform.up, direction.normalized);

            return dot < oneWayThreshold;

        }
        
        /// <summary>
        /// Updates all Raycasters' fields to reflect body configuration.
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
            
            // Disable scale inheritance for sides to avoid issues when flipping rigidbody horizontally
            _headLeft   .useOriginScale =
            _headRight  .useOriginScale =
            _sideLeft   .useOriginScale =
            _sideRight  .useOriginScale =
            _footLeft   .useOriginScale =
            _footRight  .useOriginScale = false;

            // Set offsets
            _headLeft.originOffset =
                Vector3.up * centerHeight +
                Vector3.left * horizontalGap;
            _headRight.originOffset =
                Vector3.up * centerHeight +
                Vector3.right * horizontalGap;
            _sideLeft.originOffset  =
            _sideRight.originOffset =
                Vector3.up * sidesHeight;
            _footLeft.originOffset =
                Vector3.up * centerHeight +
                Vector3.left * horizontalGap;
            _footRight.originOffset =
                Vector3.up * centerHeight +
                Vector3.right * horizontalGap;
            
        }
        
        /// <summary>
        /// Casts all vertical Raycasters (feet and heads) and updates their LastHit value.
        /// </summary>
        private void UpdateVerticalRaycasterHits() {
            _headLeft.Cast();
            _headRight.Cast();
            _footLeft.Cast();
            _footRight.Cast();
        }
        
        /// <summary>
        /// Casts all horizontal Raycasters (sides) and updates their LastHit value.
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
            _sideLeft   .DrawGizmo(Color.cyan,          1f / PixelPerfectFloat.PixelsPerUnit);
            _sideRight  .DrawGizmo(Color.magenta,       1f / PixelPerfectFloat.PixelsPerUnit);
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