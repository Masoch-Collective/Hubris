using System.Collections;
using UnityEngine;
using Utils;

namespace Utils {

    public class Juice : Singleton<Juice> {

        private float _time;
        private float _shakeFactor;
        private bool _hitFrozen;

        [Header("Screen Shake")]
        [SerializeField] private AnimationCurve shakeIntensityMult;
        [SerializeField] private float shakeSpeed;
        [SerializeField] private float shakeDecay;
        [Header("Hit Freeze")]
        [SerializeField] private float hitFreezeDuration = 0.1f;
        [Tooltip("Minimum amount of time between hitfreezes (prevents prolonged hitfreeze when hit by multiple bullets in quick succession).")]
        [SerializeField] private float hitFreezeGrace = 0.2f;

        private float _duration;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
        }

        // Update is called once per frame
        void Update() {

            _time += Time.unscaledDeltaTime;

            transform.localPosition = new Vector2(
                Mathf.PerlinNoise(0,        _time * shakeSpeed),
                Mathf.PerlinNoise(Mathf.PI, _time * shakeSpeed)
            ) * shakeIntensityMult.Evaluate(_shakeFactor);

            _shakeFactor -= shakeDecay * Time.deltaTime;
            _shakeFactor = Mathf.Clamp01(_shakeFactor);

        }

        public void AddShake(float amount) {
            _shakeFactor += amount;
        }

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

    }

}