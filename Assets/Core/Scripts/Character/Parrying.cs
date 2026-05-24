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
        public bool useAnimationEvents;
        [Min(0)] public float windupDuration;
        [Min(0)] public float parryDuration;
        [Min(0)] public float cooldownDuration;
        
        #region Runtime Variables
        public event Action<CharacterCore.CharacterStatus> OnParryEnd;
        [field: NonSerialized] public Hitbox.AttackStatus Status { get; private set; }
        [field: NonSerialized] public CharacterCore.ActionType Type { get; private set; }
        #endregion

        public void Parry(CharacterCore.ActionType type) {
            if (Core.Status != CharacterCore.CharacterStatus.Idle)
                return;
            if (Status != Hitbox.AttackStatus.Idle)
                return;
            Type = type;
            if (useAnimationEvents)
                if (Core.Animator)
                    Core.Animator.SetTrigger(Core.AnimHashTriggerParry);
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
            if (OnParryEnd != null) OnParryEnd.Invoke(CharacterCore.CharacterStatus.Parrying);
        }

        public void ParryActive() => Status = Hitbox.AttackStatus.Active;

        public void ParryCooldown() => Status = Hitbox.AttackStatus.Cooldown;
        
        public void ParryEnd() {
            Status = Hitbox.AttackStatus.Idle;
            if (OnParryEnd != null) OnParryEnd.Invoke(CharacterCore.CharacterStatus.Parrying);
            
        }

        private void OnDrawGizmos() {
            Gizmos.color = VizColor;
            Gizmos.DrawWireSphere(transform.position, 1);
        }
        
        public void ForceReset() {
            Debug.Log("Resetting Parry");
            try {
                StopCoroutine(nameof(ParryCoroutine));
            } catch { /* ignored */ }
            Status = Hitbox.AttackStatus.Idle;
        }

    }

}