using System;
using Freya;
using Character;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;
using Utils;

namespace Elements {

    public class Altar : MonoBehaviour {

        public float duration;
        public float auraFadeSpeed = 0.25f;
        [field:SerializeField, Range(0, 1)]
        public float Progress { get; private set; }
        public ParticleSystem particlesBuildup;
        public AnimationCurve particlesBuildupEmission;
        public ParticleSystem particlesClimax;
        public AnimationCurve characterSwayIntensity;
        public AnimationCurve characterSwaySpeed;
        public AnimationCurve shakeAmount;
        public AnimationCurve auraIntensity;
        public float climaxShake;
        public Transform hoverPoint;
        public Light2D aura;
        
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
            }
            
            if (!Winner) {
                if (!_winnerAura)
                    aura.intensity = auraIntensity.Evaluate(0);
                return;
            }

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
                particlesClimax.Play();
                Winner.Die(Winner);
                _winnerAura = Winner.GetComponentInChildren<Light2D>();
                _winnerAura.transform.parent = transform;
                _emissionModule.rateOverTimeMultiplier = 0;
                Juice.Instance.OverrideShake(climaxShake);
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
        }

        private void OnValidate() {
            if (aura)
                aura.intensity = auraIntensity.Evaluate(0);
        }

    }

}