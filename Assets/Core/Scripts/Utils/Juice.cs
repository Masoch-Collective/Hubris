using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Utils {

    public class Juice : Singleton<Juice> {

        private float _time;
        private float _shakeFactor;
        private bool _hitFrozen;

        [Header("Screen Shake")]
        [SerializeField] private AnimationCurve shakeIntensityMult;
        [SerializeField] private float shakeSpeed = 10;
        [SerializeField] private float shakeDecay = 2;
        [Header("Rumble")]
        [SerializeField] private float rumbleDecay = 2;
        [SerializeField] private InputAction testRumbleDeep;
        [SerializeField] private InputAction testRumbleHigh;
        [Header("Hit Freeze")]
        [SerializeField] private float hitFreezeDuration = 0.1f;
        [Tooltip("Minimum amount of time between hitfreezes (prevents prolonged hitfreeze when hit by multiple bullets in quick succession).")]
        [SerializeField] private float hitFreezeGrace = 0.2f;

        private readonly Dictionary<Gamepad, float[]> _rumble = new();
        private float _duration;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            #if UNITY_EDITOR
            testRumbleDeep.performed += context => RumbleDeep((Gamepad)context.control.device, 1);
            testRumbleHigh.performed += context => RumbleHigh((Gamepad)context.control.device, 1);
            #endif
        }

        // Update is called once per frame
        void Update() {

            _time += Time.unscaledDeltaTime;

            // Apply Screen Shake Motion
            transform.localPosition = new Vector2(
                Mathf.PerlinNoise(0,        _time * shakeSpeed),
                Mathf.PerlinNoise(Mathf.PI, _time * shakeSpeed)
            ) * shakeIntensityMult.Evaluate(_shakeFactor);
            
            // Screen Shake Decay
            _shakeFactor = Mathf.Max(0, _shakeFactor - shakeDecay * Time.deltaTime);
            // Rumble Decay
            foreach (var gamepad in _rumble) {
                gamepad.Value[0] = Mathf.Clamp01(gamepad.Value[0] - rumbleDecay * Time.deltaTime);
                gamepad.Value[1] = Mathf.Clamp01(gamepad.Value[1] - rumbleDecay * Time.deltaTime);
                gamepad.Key.SetMotorSpeeds(gamepad.Value[0], gamepad.Value[1]);
            }

        }

        #region Screen Shake
        public void AddShake(float amount) {
            _shakeFactor = Mathf.Min(_shakeFactor + amount, 1);
        }
        public void OverrideShake(float absoluteAmount) {
            _shakeFactor = absoluteAmount;
        }
        #endregion

        #region Rumble
        private void RumbleDeep(Gamepad gamepad, float intensity) {
            if (_rumble.TryGetValue(gamepad, out var value))
                value[0] += intensity;
            else 
                _rumble.Add(gamepad, new []{intensity, 0});
        }
        private void RumbleHigh(Gamepad gamepad, float intensity) {
            if (_rumble.TryGetValue(gamepad, out var value))
                value[1] += intensity;
            else 
                _rumble.Add(gamepad, new []{0, intensity});
        }
        #endregion

        #region Hitfreeze
        public void InvokeHitFreeze(float duration) {
            _duration = duration;
            InvokeHitFreeze();
        } 
        
        public void InvokeHitFreeze() {
            _duration = hitFreezeDuration;
            if (_hitFrozen)
                return;
            StartCoroutine(nameof(HitFreezeCoroutine));
        }

        private IEnumerator HitFreezeCoroutine() {
            _hitFrozen = true;
            float defaultTimeScale = Time.timeScale;
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(_duration);
            Time.timeScale = defaultTimeScale;
            yield return new WaitForSecondsRealtime(hitFreezeGrace);
            _hitFrozen = false;
        }
        #endregion

    }

}