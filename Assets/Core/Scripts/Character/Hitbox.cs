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

        public Color VizColor (CharacterCore.ActionStage stage) => stage switch {
            CharacterCore.ActionStage.Idle       => colIdle,
            CharacterCore.ActionStage.Windup     => colWindup,
            CharacterCore.ActionStage.Active     => colActive,
            CharacterCore.ActionStage.Cooldown   => colCooldown,
            _ => Color.black
        };
        public Color VizColorFill {
            get {
                Color col = VizColor(Stage);
                col.a = InHitbox is { Count: > 0 } ? vizOpacityHasOpp : vizOpacityEmpty;
                return col;
            }
        }
        public Color colIdle        = Color.slateGray;
        public Color colWindup      = Color.gold;
        public Color colActive      = Color.deepPink;
        public Color colCooldown    = Color.deepSkyBlue;
        public LayerMask opponentLayerMask;
        public HitboxShape shapeUpwards;
        public HitboxShape shapeDownwards;
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
        public CharacterCore.ActionStage Stage {
            get => _stage;
            set {
                _stage = value;
                UpdateVizColor();
            }
        }
        [NonSerialized] private CharacterCore.ActionStage _stage = CharacterCore.ActionStage.Idle;
        public CharacterCore.ActionType Type {
            get => _type;
            set {
                _type = value;
                UpdateVizColor();
            }
        }
        [NonSerialized] private CharacterCore.ActionType _type;
        public List<IDamageable> InHitbox => _inHitbox ??= new();
        private List<IDamageable> _inHitbox;
        public List<IDamageable> AlreadyDamaged => _alreadyDamaged ??= new();
        private List<IDamageable> _alreadyDamaged;
        #endregion

        private void Update() {
            UpdateVizColor();
            if (!Application.isPlaying)
                return;

            bool stun = false;
            if (_stage == CharacterCore.ActionStage.Active) {
                foreach (var damageable in InHitbox)
                    if (!AlreadyDamaged.Contains(damageable)) {
                        // [Attempt to] damage the opponent. If their action type matches ours (i.e., they perfect-parried), we should get stunned.
                        // If we want to differentiate between parry-induced stun and mutual attack stun, simply check if damageable's Status is Attacking
                        if (damageable.ReceiveDamage(Core, Type) == Type)
                            stun = true;
                        AlreadyDamaged.Add(damageable);
                    }
            } else if (InHitbox.Count > 0) {
                InHitbox.Clear();
                AlreadyDamaged.Clear();
            }
            if (stun)
                Core.Stun();
        }

        private void UpdateVizColor(){
            Visualizer.outlineColor = VizColor(Stage);
            Visualizer.fillColor = VizColorFill;
        }

        public void Attack(CharacterCore.ActionType type) {
            if (Core.Status != CharacterCore.CharacterStatus.Idle)
                return;
            if (Stage != CharacterCore.ActionStage.Idle)
                return;
            Type = type;
            InHitbox.Clear();
            AlreadyDamaged.Clear();
            switch (Type) {
                case CharacterCore.ActionType.Upwards:
                    if (shapeUpwards)
                        Collider.points = shapeUpwards.Points;
                    else
                        Debug.LogError("Missing upwards HitboxShape");
                    break;
                case CharacterCore.ActionType.Downwards:
                    if (shapeDownwards)
                        Collider.points = shapeDownwards.Points;
                    else
                        Debug.LogError("Missing downwards HitboxShape");
                    break;
            }
            if (useAnimationEvents)
                if (Core.Animator)
                    Core.Animator.SetTrigger(Core.AnimHashTriggerAttack);
                else
                    throw new MissingComponentException("Tried to initiate attack using animation events, but no Animator is available.");
            else
                StartCoroutine(nameof(AttackCoroutine));
            Stage = CharacterCore.ActionStage.Windup;
        }

        private IEnumerator AttackCoroutine() {
            Stage = CharacterCore.ActionStage.Windup;
            yield return new WaitForSeconds(windupDuration);
            Stage = CharacterCore.ActionStage.Active;
            yield return new WaitForSeconds(hurtDuration);
            Stage = CharacterCore.ActionStage.Cooldown;
            yield return new WaitForSeconds(cooldownDuration);
            Stage = CharacterCore.ActionStage.Idle;
            if (OnAttackEnd != null) OnAttackEnd.Invoke(CharacterCore.CharacterStatus.Attacking);
        }

        public void AttackActive() => Stage = CharacterCore.ActionStage.Active;

        public void AttackCooldown() => Stage = CharacterCore.ActionStage.Cooldown;
        
        public void AttackEnd() {
            Stage = CharacterCore.ActionStage.Idle;
            if (OnAttackEnd != null) OnAttackEnd.Invoke(CharacterCore.CharacterStatus.Attacking);
        }

        private void OnTriggerStay2D(Collider2D other) {
            if (other.isTrigger)
                return; // Hitboxes are being registered despite "Queries Hit Triggers" being disabled, so we'll have to do this...
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (Stage == CharacterCore.ActionStage.Active && damageable != null && !InHitbox.Contains(damageable))
                InHitbox.Add(damageable);
        }
        
        public void ForceReset() {
            try {
                StopCoroutine(nameof(AttackCoroutine));
            } catch { /* ignored */ }
            InHitbox.Clear();
            AlreadyDamaged.Clear();
            _stage = CharacterCore.ActionStage.Idle;
            Core.ReturnToIdle(CharacterCore.CharacterStatus.Attacking);
        }

    }

}