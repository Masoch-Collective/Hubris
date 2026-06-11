using System;
using Systems;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace Elements {

    public class RespawnPoint : MonoBehaviour {
        
        public SpriteRenderer SpriteRenderer {
            get {
                if (_spriteRenderer == null)
                    _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                return _spriteRenderer;
            }
        }
        [NonSerialized] private SpriteRenderer _spriteRenderer;
        public Light2D Light2D {
            get {
                if (_light2D == null)
                    _light2D = GetComponentInChildren<Light2D>();
                return _light2D;
            }
        }
        [NonSerialized] private Light2D _light2D;

        public bool Selected => RespawnSystem.Instance.SelectedPoint == this;

        public RespawnPoint selectLeft;
        public RespawnPoint selectRight;

        [field:SerializeField] public PlayerRoles AllowRespawning { get; private set; }
        public Sprite spriteDefault;
        public Sprite spriteWaiting;
        public Sprite spriteActive;
        public Sprite spriteSelected;
        public float auraIntensityActive = 2;
        public float auraIntensitySelected = 5;
        public float auraRadiusActive = 2;
        public float auraRadiusSelected = 5;
        public float auraLerpSpeed = 10;
        public SpriteRenderer selectedSpriteRenderer;
        [Header("Events")]
        public UnityEvent onSelected;
        public UnityEvent onDeselected;
        public UnityEvent onSpawn;
        public UnityEvent onSpawnPremature;

        private void Start() {
            RespawnSystem.Instance.RespawnPoints.Add(this);
        }

        private void Update() {
            // Only enable this point if there's a character waiting to respawn,
            bool viableCandidate = RespawnSystem.Instance.RespawnTarget && 
            // and this point allows respawning that character,
                                   CombatLoopManager.EvaluateRole(RespawnSystem.Instance.RespawnTarget, AllowRespawning) && 
            // and this point is in the current room
                                   RoomManager.LocalPositionToIndex(transform.localPosition) == RoomManager.Instance.currentRoom;

            float intensity = 0;
            float radius = 0;

            // Set sprite
            if (viableCandidate) {
                if (!Selected)
                    SpriteRenderer.sprite = spriteWaiting;
                else
                    SpriteRenderer.sprite = !RespawnSystem.Instance.WaitingToActivate ? spriteSelected : spriteActive;
                if (!RespawnSystem.Instance.WaitingToActivate) {
                    intensity = Selected ? auraIntensitySelected : auraIntensityActive;
                    radius = Selected ? auraRadiusSelected : auraRadiusActive;
                }
            } else
                SpriteRenderer.sprite = spriteDefault;
            
            // Lerp light intensity and radius (if present)
            if (Light2D) {
                if (RespawnSystem.Instance.RespawnTarget)
                    Light2D.color = RespawnSystem.Instance.RespawnTarget.Color;
                Light2D.intensity = Mathf.Lerp(Light2D.intensity, intensity, Time.deltaTime * auraLerpSpeed);
                Light2D.pointLightOuterRadius = Mathf.Lerp(Light2D.intensity, radius, Time.deltaTime * auraLerpSpeed);
            }

            // Glow if selected and ready to respawn
            if (selectedSpriteRenderer)
                selectedSpriteRenderer.enabled = Selected && RespawnSystem.Instance.WaitingToActivate;

            if (Selected && !RespawnSystem.Instance.WaitingToActivate)
                onSelected.Invoke();
            else
                onDeselected.Invoke();
        }

        private void OnDestroy() {
            // TODO: This is causing a RespawnSystem to be instantiated after the existing RespawnSystem instance is destroyed during scene close cleanup
            RespawnSystem.Instance.RespawnPoints.Remove(this);
        }

        private void OnValidate() {
            SpriteRenderer.sprite = spriteDefault;
            if (selectLeft)
                selectLeft.selectRight = this;
            if (selectRight)
                selectRight.selectLeft = this;
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.deepPink;
            if (selectLeft)
                Gizmos.DrawLine(transform.position, selectLeft.transform.position);
            else {
                Gizmos.DrawRay(transform.position, -transform.right);
                Gizmos.DrawLine(transform.position - transform.right + transform.up, 
                    transform.position - transform.right - transform.up);
            }
            Gizmos.color = Color.deepSkyBlue;
            if (selectRight)
                Gizmos.DrawLine(transform.position, selectRight.transform.position);
            else {
                Gizmos.DrawRay(transform.position, transform.right);

                Gizmos.DrawLine(transform.position + transform.right + transform.up,
                    transform.position + transform.right - transform.up);
            }
        }

    }

}