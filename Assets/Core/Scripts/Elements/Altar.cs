using System;
using Freya;
using Character;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using Utils;

namespace Elements {

    public class Altar : MonoBehaviour {

        public float duration;
        public float auraFadeSpeed = 0.25f;
        [field:SerializeField, Range(0, 1)]
        public float Progress { get; private set; }
        public ParticleSystem particlesBuildup;
        public AnimationCurve particlesBuildupEmission;
        public AnimationCurve characterSwayIntensity;
        public AnimationCurve characterSwaySpeed;
        public AnimationCurve shakeAmount;
        public AnimationCurve auraIntensity;
        public float climaxShake;
        public Transform hoverPoint;
        public Light2D aura;
        [Header("Continue")]
        public float gracePeriod = 5;
        public float spamReduceGrace = 0.5f;
        public float blinkFrequency = 2;
        public Color blinkA;
        public Color blinkB;
        public TextMeshPro continueText;
        [Header("Events")]
        public UnityEvent<float> onAltarSequenceRelative;
        public UnityEvent<float> onAltarSequenceAbsolute;
        public UnityEvent<CharacterCore> onAltarActivate;
        public UnityEvent<CharacterCore> onAltarClimax;
        
        [field:SerializeField] public CharacterCore Winner { get; private set; }
        private float _startTime;
        private float _perlinTime;
        private Vector3 _startPosition;
        private ParticleSystem.EmissionModule _emissionModule;
        private Light2D _winnerAura;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            _emissionModule = particlesBuildup.emission;
        }

        // Update is called once per frame
        void Update() {

            Progress = (Time.time - _startTime) / duration;

            if (_winnerAura) {
                _winnerAura.intensity = Mathf.Lerp(_winnerAura.intensity, 0, Time.deltaTime * auraFadeSpeed);
                aura.intensity = Mathf.Lerp(aura.intensity, 0, Time.deltaTime * auraFadeSpeed);
                if (Time.time - _startTime > duration + gracePeriod) {
                    continueText.enabled = true;
                    continueText.color = Mathf.FloorToInt((Time.time * blinkFrequency) % 2) == 0 ? blinkA : blinkB;
                }
            }
            
            if (!Winner) {
                if (!_winnerAura)
                    aura.intensity = auraIntensity.Evaluate(0);
                return;
            }
            
            onAltarSequenceAbsolute.Invoke(Time.time - _startTime);
            onAltarSequenceRelative.Invoke(Progress);

            _perlinTime += Time.deltaTime * characterSwaySpeed.Evaluate(Progress);
            
            particlesBuildup.transform.position = Winner.transform.position = Vector3.Lerp(
                _startPosition, 
                hoverPoint.position,
                Mathfs.Smooth01(Progress)
            ) + new Vector3(
                Mathf.PerlinNoise(0, _perlinTime) * 2 - 1,
                Mathf.PerlinNoise(1, _perlinTime) * 2 - 1,
                0
            ) * characterSwayIntensity.Evaluate(Progress);

            _emissionModule.rateOverTimeMultiplier = particlesBuildupEmission.Evaluate(Progress);
            
            Juice.Instance.OverrideShake(shakeAmount.Evaluate(Progress));

            aura.intensity = auraIntensity.Evaluate(Progress);

            if (Progress >= 1) {
                onAltarClimax.Invoke(Winner);
                Winner.Die(Winner);
                _winnerAura = Winner.GetComponentInChildren<Light2D>();
                _winnerAura.transform.parent = transform;
                _emissionModule.rateOverTimeMultiplier = 0;
                Juice.Instance.OverrideShake(climaxShake);
                Winner.ActionContinue.performed += Continue;
                Winner.enabled = false;
                Winner = null;
            }

        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.GetComponent<CharacterCore>() == null || Winner)
                return;
            Winner = other.GetComponent<CharacterCore>();
            Winner.Controller.allowControl = false;
            _startTime = Time.time;
            _startPosition = Winner.transform.position;
            onAltarActivate.Invoke(Winner);
        }

        private void OnValidate() {
            if (aura)
                aura.intensity = auraIntensity.Evaluate(0);
        }

        private void Continue(InputAction.CallbackContext context) {
            if (Time.time - _startTime < duration + gracePeriod) {
                gracePeriod -= spamReduceGrace;
                return;
            }
            context.action.performed -= Continue;
        }

    }

}