using System;
using Character;
using UnityEngine;
using Utils;

namespace Systems {
    
    [Flags]
    public enum PlayerRoles {
        Leader      = 1 << 0,
        Seeker      = 1 << 1,
        TopGoal     = 1 << 2,
        BottomGoal  = 1 << 3
    }

    public class CombatLoopManager : Singleton<CombatLoopManager> {

        public CharacterCore Leader     { get; private set; }
        public CharacterCore Seeker     { get; private set; }
        public CharacterCore TopGoal    { get; private set; }
        public CharacterCore BottomGoal { get; private set; }
        public int Orientation { get; private set; }
        public event Action<int> OnRoleSwap;

        private void Awake() {
            OnRoleSwap += _ => MapVerticalFlipper.Instance.Flip();
        }

        public void CharacterEliminated(CharacterCore killer, CharacterCore killed) {
            if (killer == null || killed == null) // TODO: Clear leader status if leader was killed but not by opponent
                return;
            if (killer == Leader) return; // Ignore elimination if Leader eliminated Seeker
            Leader = killer;
            Seeker = killed;
            if (Orientation == 0)
                Orientation = 1;
            else {
                Orientation *= -1;
                OnRoleSwap?.Invoke(Orientation);
            }
            if (TopGoal == null || BottomGoal == null) { // If either player is null, this was the first elimination of the match; assign top/bottom
                if (TopGoal != BottomGoal) // Print warning if somehow only one player is null
                    Debug.LogWarning("One goal was null, but the other wasn't??");
                TopGoal = Leader;
                BottomGoal = Seeker;
            }
        }

    }

}