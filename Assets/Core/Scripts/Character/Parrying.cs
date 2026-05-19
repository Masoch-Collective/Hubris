using System;
using System.Collections;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global

namespace Character {
    
    public class Parrying : CharacterComponent {

        public Color VizColor => Status switch {
            Hitbox.AttackStatus.Idle        => Core.Hitbox.colIdle,
            Hitbox.AttackStatus.Windup      => Core.Hitbox.colWindup,
            Hitbox.AttackStatus.Active      => Core.Hitbox.colActive,
            Hitbox.AttackStatus.Cooldown    => Core.Hitbox.colCooldown,
            _ => Color.black
        };
        public Animator animator;
        public int animationTriggerHash;
        public bool useAnimationEvents;
        [Min(0)] public float windupDuration;
        [Min(0)] public float parryDuration;
        [Min(0)] public float cooldownDuration;
        
        #region Runtime Variables
        public event Action<CharacterCore.CharacterStates> OnParryEnd;
        [field: NonSerialized] public Hitbox.AttackStatus Status { get; private set; }
        [field: NonSerialized] public Hitbox.AttackType Type { get; private set; }
        #endregion

        public void Parry(Hitbox.AttackType type) {
            if (Status != Hitbox.AttackStatus.Idle)
                return;
            Type = type;
            if (useAnimationEvents)
                if (animator)
                    animator.SetTrigger(animationTriggerHash);
                else
                    throw new MissingComponentException("Tried to initiate parry using animation events, but no Animator is available.");
            else
                StartCoroutine(nameof(ParryCoroutine));
            Status = Hitbox.AttackStatus.Windup;
        }

        private IEnumerator ParryCoroutine() {
            Status = Hitbox.AttackStatus.Windup;
            yield return new WaitForSeconds(windupDuration);
            Status = Hitbox.AttackStatus.Active;
            yield return new WaitForSeconds(parryDuration);
            Status = Hitbox.AttackStatus.Cooldown;
            yield return new WaitForSeconds(cooldownDuration);
            Status = Hitbox.AttackStatus.Idle;
            if (OnParryEnd != null) OnParryEnd.Invoke(CharacterCore.CharacterStates.Parrying);
        }

        public void ActiveStart() => Status = Hitbox.AttackStatus.Active;

        public void ActiveCooldown() => Status = Hitbox.AttackStatus.Cooldown;
        
        public void ParryEnd() {
            Status = Hitbox.AttackStatus.Idle;
            if (OnParryEnd != null) OnParryEnd.Invoke(CharacterCore.CharacterStates.Parrying);
            
        }

        public void ActiveForSeconds() => ActiveForSeconds(parryDuration, cooldownDuration);
        public void ActiveForSeconds(float parry, float cool) {
            StartCoroutine(nameof(ActiveForSecondsCoroutine), parry);
        }

        private IEnumerator ActiveForSecondsCoroutine(float parry, float cool) {
            Status = Hitbox.AttackStatus.Active;
            yield return new WaitForSeconds(parry);
            Status = Hitbox.AttackStatus.Cooldown;
            yield return new WaitForSeconds(cool);
            Status = Hitbox.AttackStatus.Idle;
            if (OnParryEnd != null) OnParryEnd.Invoke(CharacterCore.CharacterStates.Parrying);
        }

        private void OnDrawGizmos() {
            Gizmos.color = VizColor;
            Gizmos.DrawWireSphere(transform.position, 1);
        }

        protected override void OnDeath() => ForceReset();
        
        private void ForceReset() {
            try {
                StopCoroutine(nameof(ActiveForSecondsCoroutine));
            } catch { /* ignored */ }

            Status = Hitbox.AttackStatus.Idle;
        }

    }

}