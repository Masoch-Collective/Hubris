using UnityEngine;
using Utils;

namespace Character {

    public class Controller : MonoBehaviour {

        
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
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            UpdateRaycasterParameters();
        }

        private void Move(Vector3 travel) {
            UpdateRaycasters();
            transform.Translate(travel);
        }

        private void VerticalCollisions(ref Vector3 travel) {
        }

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
        
        private void UpdateRaycasters() {
            _footLeft.Cast();
            _footRight.Cast();
            _headLeft.Cast();
            _headRight.Cast();
        }

        // Update is called once per frame
        void FixedUpdate() {
            
            
            
        }

        private void OnValidate() => UpdateRaycasterParameters();

        private void OnDrawGizmos() {
            _footLeft   .Draw(Color.darkGreen);
            _footRight  .Draw(Color.darkGreen);
            _headLeft   .Draw(Color.greenYellow);
            _headRight  .Draw(Color.greenYellow);
        }

    }

}