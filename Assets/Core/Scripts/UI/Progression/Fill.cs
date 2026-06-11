using System;
using Systems;
using UnityEngine;
using Utils;

namespace UI.Progression {

    [ExecuteInEditMode]
    public class Fill : MonoBehaviour {

        public PlayerRoles represent;
        public SpriteRenderer fillSprite;
        private Core Parent {
            get {
                if (_parent == null)
                    _parent = GetComponentInParent<Core>();
                return _parent;
            }
        }
        [NonSerialized] private Core _parent;

        public void Update() {
            // I'm getting lost in the sauce I can no longer tell if this kinda logic is a masterpiece or cursed;
            int effectiveIndex = !Application.isPlaying || CombatLoopManager.Instance.Leader && CombatLoopManager.EvaluateRole(CombatLoopManager.Instance.Leader, represent)
                ? represent == PlayerRoles.TopGoal
                    ? (Parent.current + Parent.middleIndex) // Use increasing index for ascending player
                : Parent.levels - (Parent.current + Parent.middleIndex) + 1 // Use decreasing index for descending player
            : 0; // Default to 0 if the character this UI represents is not the leader
            float setToHeight = effectiveIndex == 0 ? 0 : (Parent.max - Parent.min) * (effectiveIndex / (float)Parent.levels) + Parent.min;
            Vector2 size = fillSprite.size;
            size.y = Mathf.Lerp(size.y, setToHeight, Application.isPlaying ? Time.deltaTime * Parent.lerpSpeed : 1);
            fillSprite.size = size;
        }

        private void OnValidate() {
            if (represent != 0) {
                // Disable setting multiple roles
                if (((int)represent & (int)represent - 1) != 0)
                    represent = 0;
                // Disable setting leader/seeker role
                if (represent < PlayerRoles.TopGoal)
                    represent = 0;
            }
        }

    }

}
