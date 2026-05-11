using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Systems {

    public class MapVerticalFlipper : MonoBehaviour {

        [Header("Input")]
        [SerializeField] private Key flipKey = Key.F;

        [Header("Animation")]
        [SerializeField] private float duration = 0.45f;
        [SerializeField] private float liftAmount = 0.35f;
        [SerializeField] private float tiltDegrees = 4f;
        [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Transform _visualRoot;
        private bool _isFlipped;
        private Coroutine _flipRoutine;
        private Vector3 _restPosition;
        private Vector3 _restScale;
        private Quaternion _restRotation;

        private void Awake() {
            _visualRoot = CreateVisualRoot();
            _restPosition = _visualRoot.localPosition;
            _restScale = _visualRoot.localScale;
            _restRotation = _visualRoot.localRotation;
            _isFlipped = _restScale.y < 0f;
        }

        private void Update() {
            if (Keyboard.current == null || _flipRoutine != null)
                return;

            if (Keyboard.current[flipKey].wasPressedThisFrame)
                _flipRoutine = StartCoroutine(FlipRoutine());
        }

        private IEnumerator FlipRoutine() {
            float elapsed = 0f;
            Vector3 startScale = _visualRoot.localScale;
            float targetY = Mathf.Abs(_restScale.y) * (_isFlipped ? 1f : -1f);
            float tiltDirection = _isFlipped ? -1f : 1f;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / duration);
                float easedTime = easing.Evaluate(normalizedTime);
                float arc = Mathf.Sin(normalizedTime * Mathf.PI);

                _visualRoot.localScale = new Vector3(
                    startScale.x,
                    Mathf.Lerp(startScale.y, targetY, easedTime),
                    startScale.z
                );
                _visualRoot.localPosition = _restPosition + Vector3.up * (arc * liftAmount);
                _visualRoot.localRotation = _restRotation * Quaternion.Euler(0f, 0f, arc * tiltDegrees * tiltDirection);

                yield return null;
            }

            _visualRoot.localScale = new Vector3(startScale.x, targetY, startScale.z);
            _visualRoot.localPosition = _restPosition;
            _visualRoot.localRotation = _restRotation;
            _isFlipped = !_isFlipped;
            _flipRoutine = null;
        }

        private Transform CreateVisualRoot() {
            GameObject visualRootObject = new($"{name} Visual Flip Root");
            Transform visualRoot = visualRootObject.transform;
            visualRoot.SetParent(transform, false);

            Tilemap[] tilemaps = GetComponentsInChildren<Tilemap>();
            foreach (Tilemap sourceTilemap in tilemaps) {
                TilemapRenderer sourceRenderer = sourceTilemap.GetComponent<TilemapRenderer>();
                if (!sourceRenderer)
                    continue;

                Tilemap visualTilemap = CopyTilemapVisual(sourceTilemap, visualRoot);
                CopyTilemapRenderer(sourceRenderer, visualTilemap.gameObject.AddComponent<TilemapRenderer>());
                sourceRenderer.enabled = false;
            }

            return visualRoot;
        }

        private static Tilemap CopyTilemapVisual(Tilemap sourceTilemap, Transform visualRoot) {
            GameObject visualTilemapObject = new($"{sourceTilemap.name} Visual");
            Transform visualTransform = visualTilemapObject.transform;
            visualTransform.SetParent(visualRoot, false);
            visualTransform.localPosition = sourceTilemap.transform.localPosition;
            visualTransform.localRotation = sourceTilemap.transform.localRotation;
            visualTransform.localScale = sourceTilemap.transform.localScale;

            Tilemap visualTilemap = visualTilemapObject.AddComponent<Tilemap>();
            BoundsInt bounds = sourceTilemap.cellBounds;
            visualTilemap.SetTilesBlock(bounds, sourceTilemap.GetTilesBlock(bounds));
            visualTilemap.tileAnchor = sourceTilemap.tileAnchor;
            visualTilemap.orientation = sourceTilemap.orientation;
            visualTilemap.orientationMatrix = sourceTilemap.orientationMatrix;
            visualTilemap.color = sourceTilemap.color;

            foreach (Vector3Int position in bounds.allPositionsWithin) {
                if (!sourceTilemap.HasTile(position))
                    continue;

                TileFlags sourceFlags = sourceTilemap.GetTileFlags(position);
                visualTilemap.SetTileFlags(position, TileFlags.None);
                visualTilemap.SetTransformMatrix(position, sourceTilemap.GetTransformMatrix(position));
                visualTilemap.SetColor(position, sourceTilemap.GetColor(position));
                visualTilemap.SetTileFlags(position, sourceFlags);
            }

            return visualTilemap;
        }

        private static void CopyTilemapRenderer(TilemapRenderer source, TilemapRenderer target) {
            target.enabled = source.enabled;
            target.sortingLayerID = source.sortingLayerID;
            target.sortingOrder = source.sortingOrder;
            target.sortOrder = source.sortOrder;
            target.mode = source.mode;
            target.maskInteraction = source.maskInteraction;
            target.sharedMaterials = source.sharedMaterials;
        }

        private void OnValidate() {
            duration = Mathf.Max(0.01f, duration);
            liftAmount = Mathf.Max(0f, liftAmount);
            tiltDegrees = Mathf.Max(0f, tiltDegrees);
        }
    }
}
