using System;
using System.Collections;
using Systems;
using UnityEngine;
using Utils;

namespace Character {

    [RequireComponent(typeof(PolygonCollider2D))]
    [RequireComponent(typeof(PolygonCollider2DVisualizer))]
    [ExecuteInEditMode]
    public class Hitbox : CharacterComponent {

        #region Enums
        public enum AttackStatus {
            Idle,
            Windup,
            Hurting,
            Cooldown
        }
        public enum AttackType {
            Upwards,
            Downwards
        }
        #endregion
        
        public PolygonCollider2D Collider {
            get {
                if (_collider == null) {
                    _collider = GetComponent<PolygonCollider2D>();
                    _collider.excludeLayers = 1 << gameObject.layer;
                    _collider.includeLayers = opponentLayerMask;
                }
                return _collider;
            }
        }
        [NonSerialized] private PolygonCollider2D _collider;
        public PolygonCollider2DVisualizer Visualizer {
            get {
                if (_visualizer == null)
                    _visualizer = GetComponent<PolygonCollider2DVisualizer>();
                return _visualizer;
            }
        }
        [NonSerialized] private PolygonCollider2DVisualizer _visualizer;

        public Color VizColor => Status switch {
            AttackStatus.Idle       => colIdle,
            AttackStatus.Windup     => colWindup,
            AttackStatus.Hurting    => colHurting,
            AttackStatus.Cooldown   => colCooldown,
            _ => Color.black
        };
        public Color VizColorFill {
            get {
                Color col = VizColor;
                col.a = OpponentInHitbox ? vizOpacityHasOpp : vizOpacityEmpty;
                return col;
            }
        }
        public Color colIdle        = Color.slateGray;
        public Color colWindup      = Color.gold;
        public Color colHurting     = Color.deepPink;
        public Color colCooldown    = Color.deepSkyBlue;
        public Animator animator;
        public LayerMask opponentLayerMask;
        public int animationTriggerHash;
        public bool useAnimationEvents;
        public bool useVisualizer;
        [Min(0)] public float windupDuration;
        [Min(0)] public float hurtDuration;
        [Min(0)] public float cooldownDuration;
        public float vizOpacityEmpty;
        public float vizOpacityHasOpp;

        #region Runtime Variables
        public AttackStatus Status {
            get => _status;
            set {
                _status = value;
                if (value == AttackStatus.Idle)
                    _attackLanded = false;
                UpdateVizColor();
            }
        }
        [NonSerialized] private AttackStatus _status;
        public AttackType Type {
            get => _type;
            set {
                _type = value;
                UpdateVizColor();
            }
        }
        [NonSerialized] private AttackType _type;
        [field:NonSerialized]
        public bool OpponentInHitbox { get; private set; }
        [field:NonSerialized] 
        public IDamageable Opponent { get; private set; }
        [NonSerialized]
        private bool _attackLanded;
        #endregion

        private void Update() {
            UpdateVizColor();
            if (!Application.isPlaying)
                return;
            if (_status == AttackStatus.Hurting && Opponent != null && OpponentInHitbox && !_attackLanded) {
                Opponent.Damage(this);
                _attackLanded = true;
            }
        }
        
        private void UpdateVizColor(){
            Visualizer.outlineColor = VizColor;
            Visualizer.fillColor = VizColorFill;
        }

        public void Attack(AttackType type) {
            if (Status != AttackStatus.Idle)
                return;
            _attackLanded = false;
            Type = type;
            if (useAnimationEvents)
                if (animator)
                    animator.SetTrigger(animationTriggerHash);
                else
                    throw new MissingComponentException("Tried to initiate attack using animation events, but no Animator is available.");
            else
                StartCoroutine(nameof(AttackCoroutine));
            Status = AttackStatus.Windup;
        }

        private IEnumerator AttackCoroutine() {
            Status = AttackStatus.Windup;
            yield return new WaitForSeconds(windupDuration);
            Status = AttackStatus.Hurting;
            yield return new WaitForSeconds(hurtDuration);
            Status = AttackStatus.Cooldown;
            yield return new WaitForSeconds(cooldownDuration);
            Status = AttackStatus.Idle;
        }

        public void HurtStart() => Status = AttackStatus.Hurting;

        public void HurtEnd() => Status = AttackStatus.Cooldown;
        
        public void AttackEnd() => Status = AttackStatus.Idle;

        public void HurtForSeconds() => HurtForSeconds(hurtDuration, cooldownDuration);
        public void HurtForSeconds(float hurt, float cool) {
            StartCoroutine(nameof(HurtCoroutine), hurt);
        }

        private IEnumerator HurtCoroutine(float hurt, float cool) {
            Status = AttackStatus.Hurting;
            yield return new WaitForSeconds(hurt);
            Status = AttackStatus.Cooldown;
            yield return new WaitForSeconds(cool);
            Status = AttackStatus.Idle;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (Opponent == null || other != Opponent.Hurtbox)
                Opponent = other.GetComponent<IDamageable>();
            OpponentInHitbox = true;
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other == Opponent.Hurtbox)
                OpponentInHitbox = false;
        }

    }

}