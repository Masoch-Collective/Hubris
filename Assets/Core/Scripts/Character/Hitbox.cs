using System;
using System.Collections;
using Systems;
using UnityEngine;
using UnityEngine.Events;
using Utils.Editor;

namespace Character {

    [RequireComponent(typeof(PolygonCollider2D))]
    [RequireComponent(typeof(PolygonCollider2DVisualizer))]
    public class Hitbox : CharacterComponent {

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

        [NonSerialized]
        private PolygonCollider2D _collider;
        public PolygonCollider2D Collider {
            get {
                if (_collider == null) {
                    _collider = GetComponent<PolygonCollider2D>();
                    _collider.excludeLayers = 1 << gameObject.layer;
                    _collider.includeLayers = 1 << opponentLayerIndex;
                }
                return _collider;
            }
        }
        
        public Animator animator;
        
        public int opponentLayerIndex;
        public int animationTriggerHash;
        public bool useAnimationEvents;
        public float windup;
        public float hurtDuration;
        public float cooldown;

        public AttackStatus Status {
            get => _status;
            set {
                _status = value;
                StatusChanged.Invoke(Status, Type);
                if (value == AttackStatus.Idle)
                    _attackLanded = false;
            }
        }
        [NonSerialized] private AttackStatus _status;
        public AttackType Type {
            get => _type;
            set {
                _type = value;
                StatusChanged.Invoke(Status, Type);
            }
        }
        [NonSerialized] private AttackType _type;
        [field:NonSerialized]
        public bool OpponentInHitbox { get; private set; }
        [field:NonSerialized] 
        public IDamageable Opponent { get; private set; }
        [NonSerialized]
        private bool _attackLanded;

        public UnityEvent<AttackStatus, AttackType> StatusChanged => _statusChanged ??= new();
        [NonSerialized] private UnityEvent<AttackStatus, AttackType> _statusChanged;

        private void Update() {
            if (_status == AttackStatus.Hurting && Opponent != null && OpponentInHitbox && !_attackLanded) {
                Opponent.Damage(this);
                _attackLanded = true;
            }
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
                    throw new MissingComponentException(
                        "Tried to initiate attack using animation events, but no Animator is available.");
            else
                StartCoroutine(nameof(AttackCoroutine));
            Status = AttackStatus.Windup;
        }

        private IEnumerator AttackCoroutine() {
            Status = AttackStatus.Windup;
            yield return new WaitForSeconds(windup);
            Status = AttackStatus.Hurting;
            yield return new WaitForSeconds(hurtDuration);
            Status = AttackStatus.Cooldown;
            yield return new WaitForSeconds(cooldown);
            Status = AttackStatus.Idle;
        }

        public void HurtStart() => Status = AttackStatus.Hurting;

        public void HurtEnd() => Status = AttackStatus.Cooldown;
        
        public void AttackEnd() => Status = AttackStatus.Idle;

        public void HurtForSeconds() => HurtForSeconds(hurtDuration, cooldown);
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

        private void OnGUI() {
            Debug.DrawLine(transform.position, transform.position + Vector3.up,
                OpponentInHitbox ? Color.deepPink : Color.darkMagenta);
            if (Opponent != null)
                Debug.DrawLine(transform.position + Vector3.up, Opponent.Hurtbox.transform.position, OpponentInHitbox ? Color.deepPink : Color.darkMagenta);
        }

    }

}