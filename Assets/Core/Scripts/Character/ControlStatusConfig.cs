using System;
using Systems;
using UnityEngine;

namespace Character {

    [Serializable]
    public struct ControlStatusConfig {

        public CharacterCore.CharacterStatus whenStatusIs;
        public CharacterCore.ActionStage duringAttackStage;
        public CharacterCore.ActionStage duringParryStage;

        public bool Evaluate(CharacterCore character) =>
            Evaluate(character.Status, character.Hitbox.Stage, character.Parrying.Stage);
        public bool Evaluate(
            CharacterCore.CharacterStatus status, 
            CharacterCore.ActionStage attackStage,
            CharacterCore.ActionStage parryStage) {
            if (Systems.PauseMenu.Core.Instance.Paused || !ReadyUpUtility.Instance.Done) // Disable controls if game state prohibits gameplay
                return false;
            if (!whenStatusIs.HasFlag(status)) // If the current status is not in the list of allowed control statuses, don't bother checking for specific status stage (i.e., if control isn't allowed during attack status, we don't need to check the individual attack stages)
                return false;
            if (status == CharacterCore.CharacterStatus.Attacking) // If attacking, proceed to evaluate attack stage
                if (!duringAttackStage.HasFlag(attackStage))
                    return false;
            if (status == CharacterCore.CharacterStatus.Parrying) // If parrying, proceed to evaluate parry stage
                if (!duringParryStage.HasFlag(parryStage))
                    return false;
            return true;
        }

    }

}