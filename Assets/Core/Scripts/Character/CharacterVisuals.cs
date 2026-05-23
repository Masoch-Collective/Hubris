using System;
using Character;
using UnityEngine;

namespace Character
{

    public class CharacterVisuals : CharacterComponent
    {
        public Animator animator;
        public SpriteRenderer bodyRenderer;
        public CharacterCore characterCore;


        //TODO: These should be inspector-exposed fields, not readonly variables
        private readonly int _idleHash = Animator.StringToHash("Idle");
        private readonly int _attackingHash = Animator.StringToHash("Attacking");
        private readonly int _parryingHash = Animator.StringToHash("Parrying");
        private readonly int _stunnedHash = Animator.StringToHash("Stunned");
        private readonly int _deadHash = Animator.StringToHash("Dead");

        private void Start() {
            Core.OnStatusChanged += UpdateVisuals;
        }
        
        private void UpdateVisuals(CharacterCore.CharacterStatus status) {
            switch (status)
            {
                case CharacterCore.CharacterStatus.Idle:
                    animator.CrossFade(_idleHash, 0);
                    break;
                case CharacterCore.CharacterStatus.Attacking:
                    animator.CrossFade(_attackingHash, 0);
                    break;
                case CharacterCore.CharacterStatus.Parrying:
                    animator.CrossFade(_parryingHash, 0);
                    break;
                case CharacterCore.CharacterStatus.Stunned:
                    animator.CrossFade(_stunnedHash, 0);
                    break;
                case CharacterCore.CharacterStatus.Dead:
                    animator.CrossFade(_deadHash, 0);
                    break;
            }
        }
    }
}
