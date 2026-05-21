using System;
using System.Collections;
using System.Collections.Generic;
using Systems;
using UnityEngine;
using Utils;

namespace Character {

    [RequireComponent(typeof(PolygonCollider2D))]
    [RequireComponent(typeof(PolygonCollider2DVisualizer))]
    [ExecuteInEditMode]
    public class Hitbox : CharacterComponent {

        #region Enums & Structs
        public enum AttackStatus {
            Idle,
            Windup,
            Active,
            Cooldown
        }
        public enum AttackType {
            Upwards,
            Downwards
        }
        #endregion
        
        #region Variables
        public PolygonCollider2D Collider {
            get {
                if (_collider == null) {
                    _collider = GetComponent<PolygonCollider2D>();
                    // Exclude the opposite of whatever is selected as the opponent layer mask
                    _collider.excludeLayers = ~opponentLayerMask;
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

        public Color VizColor (AttackStatus status) => status switch {
            AttackStatus.Idle       => colIdle,
            AttackStatus.Windup     => colWindup,
            AttackStatus.Active     => colActive,
            AttackStatus.Cooldown   => colCooldown,
            _ => Color.black
        };
        public Color VizColorFill {
            get {
                Color col = VizColor(Status);
                col.a = InHitbox is { Count: > 0 } ? vizOpacityHasOpp : vizOpacityEmpty;
                return col;
            }
        }
        public Color colIdle        = Color.slateGray;
        public Color colWindup      = Color.gold;
        public Color colActive      = Color.deepPink;
        public Color colCooldown    = Color.deepSkyBlue;
        public Animator animator;
        public LayerMask opponentLayerMask;
        public HitboxShape shapeUpwards;
        public HitboxShape shapeDownwards;
        public int animationTriggerHash;
        public bool useAnimationEvents;
        public bool useVisualizer;
        [Min(0)] public float windupDuration;
        [Min(0)] public float hurtDuration;
        [Min(0)] public float cooldownDuration;
        public float vizOpacityEmpty;
        public float vizOpacityHasOpp;
        #endregion

        #region Runtime Variables
        public event Action<CharacterCore.CharacterStatus> OnAttackEnd;
        public AttackStatus Status {
            get => _status;
            set {
                _status = value;
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
        public List<IDamageable> InHitbox => _inHitbox ??= new();
        private List<IDamageable> _inHitbox;
        public List<IDamageable> AlreadyDamaged => _alreadyDamaged ??= new();
        private List<IDamageable> _alreadyDamaged;
        #endregion

        private void Update() {
            UpdateVizColor();
            if (!Application.isPlaying)
                return;

            if (_status == AttackStatus.Active) {
                foreach (var damageable in InHitbox)
                    if (!AlreadyDamaged.Contains(damageable)) {
                        damageable.ReceiveDamage(this, (int)Type);
                        AlreadyDamaged.Add(damageable);
                    }
            } else if (InHitbox.Count > 0) {
                InHitbox.Clear();
                AlreadyDamaged.Clear();
            }
        }

        private void UpdateVizColor(){
            Visualizer.outlineColor = VizColor(Status);
            Visualizer.fillColor = VizColorFill;
        }

        public void Attack(AttackType type) {
            if (Core.Status != CharacterCore.CharacterStatus.Idle)
                return;
            if (Status != AttackStatus.Idle)
                return;
            Type = type;
            InHitbox.Clear();
            AlreadyDamaged.Clear();
            switch (Type) {
                case AttackType.Upwards:
                    if (shapeUpwards)
                        Collider.points = shapeUpwards.Points;
                    else
                        Debug.LogError("Missing upwards HitboxShape");
                    break;
                case AttackType.Downwards:
                    if (shapeDownwards)
                        Collider.points = shapeDownwards.Points;
                    else
                        Debug.LogError("Missing downwards HitboxShape");
                    break;
            }
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
            Status = AttackStatus.Active;
            yield return new WaitForSeconds(hurtDuration);
            Status = AttackStatus.Cooldown;
            yield return new WaitForSeconds(cooldownDuration);
            Status = AttackStatus.Idle;
            if (OnAttackEnd != null) OnAttackEnd.Invoke(CharacterCore.CharacterStatus.Attacking);
        }

        public void ActiveStart() => Status = AttackStatus.Active;

        public void ActiveCooldown() => Status = AttackStatus.Cooldown;
        
        public void AttackEnd() {
            Status = AttackStatus.Idle;
            if (OnAttackEnd != null) OnAttackEnd.Invoke(CharacterCore.CharacterStatus.Attacking);
        }

        public void ActiveForSeconds() => ActiveForSeconds(hurtDuration, cooldownDuration);
        public void ActiveForSeconds(float hurt, float cool) {
            StartCoroutine(nameof(ActiveForSecondsCoroutine), hurt);
        }

        private IEnumerator ActiveForSecondsCoroutine(float hurt, float cool) {
            Status = AttackStatus.Active;
            yield return new WaitForSeconds(hurt);
            Status = AttackStatus.Cooldown;
            yield return new WaitForSeconds(cool);
            Status = AttackStatus.Idle;
            if (OnAttackEnd != null) OnAttackEnd.Invoke(CharacterCore.CharacterStatus.Attacking);
        }

        private void OnTriggerStay2D(Collider2D other) {
            if (other.isTrigger)
                return; // Hitboxes are being registered despite "Queries Hit Triggers" being disabled, so we'll have to do this...
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (Status == AttackStatus.Active && damageable != null && !InHitbox.Contains(damageable))
                InHitbox.Add(damageable);
        }
        
        public void ForceReset() {
            Debug.Log("Resetting Hitbox");
            try {
                StopCoroutine(nameof(AttackCoroutine));
            } catch { /* ignored */ }
            try {
                StopCoroutine(nameof(ActiveForSecondsCoroutine));
            } catch { /* ignored */ }
            InHitbox.Clear();
            AlreadyDamaged.Clear();
            Status = AttackStatus.Idle;
        }

    }

}