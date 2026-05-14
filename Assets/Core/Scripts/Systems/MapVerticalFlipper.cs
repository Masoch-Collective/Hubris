using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Systems {

    public class MapVerticalFlipper : MonoBehaviour {

        private enum RotateZ180Mode {
            RotateBack,
            Continue
        }

        [Header("Input")]
        [SerializeField] private Key flipKey = Key.F;

        [Header("Flip")]
        [SerializeField] private Transform flipRoot;
        [SerializeField] private RotateZ180Mode rotateZ180Mode = RotateZ180Mode.RotateBack;

        [Header("Animation")]
        [SerializeField] private float duration = 0.45f;
        [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Transform _flipRoot;
        private bool _isFlipped;
        private Coroutine _flipRoutine;
        private Vector3 _restPosition;
        private Vector3 _restScale;
        private Quaternion _restRotation;
        private float _zFlipAngle;

        private void Awake() {
            _flipRoot = flipRoot ? flipRoot : transform;
            _restPosition = _flipRoot.localPosition;
            _restScale = _flipRoot.localScale;
            _restRotation = _flipRoot.localRotation;
            _isFlipped = _restScale.y < 0f;
            _zFlipAngle = _isFlipped ? 180f : 0f;
        }

        private void Update() {
            if (Keyboard.current == null || _flipRoutine != null)
                return;

            if (Keyboard.current[flipKey].wasPressedThisFrame)
                _flipRoutine = StartCoroutine(FlipRoutine());
        }

        private IEnumerator FlipRoutine() {
            float elapsed = 0f;
            float startAngle = _zFlipAngle;
            float targetAngle = rotateZ180Mode switch {
                RotateZ180Mode.Continue => startAngle + 180f,
                _ => _isFlipped ? 0f : 180f
            };

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / duration);
                float easedTime = easing.Evaluate(normalizedTime);
                float angle = Mathf.LerpUnclamped(startAngle, targetAngle, easedTime);

                _flipRoot.localRotation = _restRotation * Quaternion.Euler(0f, 0f, angle);

                yield return null;
            }

            _zFlipAngle = targetAngle;
            _flipRoot.localRotation = _restRotation * Quaternion.Euler(0f, 0f, _zFlipAngle);
            _isFlipped = !_isFlipped;
            _flipRoutine = null;
        }

        private void OnValidate() {
            duration = Mathf.Max(0.01f, duration);
        }
    }
}
