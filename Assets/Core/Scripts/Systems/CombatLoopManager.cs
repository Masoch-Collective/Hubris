using System;
using Character;
using UnityEngine;

namespace Systems {

    public class CombatLoopManager : MonoBehaviour {

        public CharacterCore Leader     { get; private set; }
        public CharacterCore Seeker     { get; private set; }
        public CharacterCore TopGoal    { get; private set; }
        public CharacterCore BottomGoal { get; private set; }
        public int Orientation { get; private set; }
        public event Action<int> OnRoleSwap;

        public void CharacterEliminated(CharacterCore killer, CharacterCore killed) {
            if (killer == Leader) return; // Ignore elimination if Leader eliminated Seeker
            Leader = killer;
            Seeker = killed;
            if (Orientation == 0)
                Orientation = 1;
            else
                Orientation *= -1;
            if (OnRoleSwap != null)
                OnRoleSwap.Invoke(Orientation);
            if (TopGoal == null || BottomGoal == null) { // If either player is null, this was the first elimination of the match; assign top/bottom
                if (TopGoal != BottomGoal) // Print warning if somehow only one player is null
                    Debug.LogWarning("One goal was null, but the other wasn't??");
                TopGoal = Leader;
                BottomGoal = Seeker;
            }
        }

    }

}