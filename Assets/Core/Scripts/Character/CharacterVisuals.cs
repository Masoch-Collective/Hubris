using Character;
using UnityEngine;

namespace Character
{

    public class CharacterVisuals : MonoBehaviour
    {
        public Animator animator;
        public SpriteRenderer bodyRenderer;
        public CharacterCore characterCore;


        private readonly int IdleHash = Animator.StringToHash("Idle");
        private readonly int AttackingHash = Animator.StringToHash("Attacking");
        private readonly int ParryingHash = Animator.StringToHash("Parrying");
        private readonly int StunnedHash = Animator.StringToHash("Stunned");
        private readonly int DeadHash = Animator.StringToHash("Dead");


        void Update()
        {
            //characterCore.OnStatusChanged += UpdateVisuals;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (characterCore.previousStatus != characterCore.Status)
            {
                switch (characterCore.Status)
                {
                    case CharacterCore.CharacterStatus.Idle:
                        animator.CrossFade(IdleHash, 0);
                        break;
                    case CharacterCore.CharacterStatus.Attacking:
                        animator.CrossFade(AttackingHash, 0);
                        break;
                    case CharacterCore.CharacterStatus.Parrying:
                        animator.CrossFade(ParryingHash, 0);
                        break;
                    case CharacterCore.CharacterStatus.Stunned:
                        animator.CrossFade(StunnedHash, 0);
                        break;
                    case CharacterCore.CharacterStatus.Dead:
                        animator.CrossFade(DeadHash, 0);
                        break;
                }
            }
        }

       /* private void UpdateVisuals(CharacterCore.CharacterStatus status)
        {
            print("swapping");
            switch (status)
            {
                case CharacterCore.CharacterStatus.Idle:
                    animator.CrossFade(IdleHash, 0);
                    break;
                case CharacterCore.CharacterStatus.Attacking:
                    animator.CrossFade(AttackingHash, 0);
                    break;
                case CharacterCore.CharacterStatus.Parrying:
                    animator.CrossFade(ParryingHash, 0);
                    break;
                case CharacterCore.CharacterStatus.Stunned:
                    animator.CrossFade(StunnedHash, 0);
                    break;
                case CharacterCore.CharacterStatus.Dead:
                    animator.CrossFade(DeadHash, 0);
                    break;
            }
        }
       */
    }
}
