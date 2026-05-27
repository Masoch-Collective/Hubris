using System;
using System.Collections;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global

namespace Character {
    
    public class Parrying : CharacterComponent {

        public Color VizColor => Stage switch {
            CharacterCore.ActionStage.Idle        => Core.Hitbox.colIdle,
            CharacterCore.ActionStage.Windup      => Core.Hitbox.colWindup,
            CharacterCore.ActionStage.Active      => Core.Hitbox.colActive,
            CharacterCore.ActionStage.Cooldown    => Core.Hitbox.colCooldown,
            _ => Color.black
        };
        public bool useAnimationEvents;
        [Min(0)] public float windupDuration;
        [Min(0)] public float parryDuration;
        [Min(0)] public float cooldownDuration;
        
        #region Runtime Variables
        public event Action<CharacterCore.CharacterStatus> OnParryEnd;

        [field: NonSerialized] public CharacterCore.ActionStage Stage { get; private set; } = CharacterCore.ActionStage.Idle;
        [field: NonSerialized] public CharacterCore.ActionType Type { get; private set; }
        #endregion

        public void Parry(CharacterCore.ActionType type) {
            if (Core.Status != CharacterCore.CharacterStatus.Idle)
                return;
            if (Stage != CharacterCore.ActionStage.Idle)
                return;
            Type = type;
            if (useAnimationEvents)
                if (Core.Animator)
                    Core.Animator.SetTrigger(Core.AnimHashTriggerParry);
                else
                    throw new MissingComponentException("Tried to initiate parry using animation events, but no Animator is available.");
            else
                StartCoroutine(nameof(ParryCoroutine));
            Stage = CharacterCore.ActionStage.Windup;
        }

        private IEnumerator ParryCoroutine() {
            Stage = CharacterCore.ActionStage.Windup;
            yield return new WaitForSeconds(windupDuration);
            Stage = CharacterCore.ActionStage.Active;
            yield return new WaitForSeconds(parryDuration);
            Stage = CharacterCore.ActionStage.Cooldown;
            yield return new WaitForSeconds(cooldownDuration);
            Stage = CharacterCore.ActionStage.Idle;
            if (OnParryEnd != null) OnParryEnd.Invoke(CharacterCore.CharacterStatus.Parrying);
        }

        public void ParryActive() => Stage = CharacterCore.ActionStage.Active;

        public void ParryCooldown() => Stage = CharacterCore.ActionStage.Cooldown;
        
        public void ParryEnd() {
            Stage = CharacterCore.ActionStage.Idle;
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
            Core.ReturnToIdle(CharacterCore.CharacterStatus.Parrying);
        }

    }

}