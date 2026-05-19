using UnityEngine;

public class CharacterVisuals : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer bodyRenderer;
    public PlayerController playerController;

    private readonly int IdleHash = Animator.StringToHash("Idle");
    private readonly int RunningHash = Animator.StringToHash("Running");
    private readonly int JumpingHash = Animator.StringToHash("Jumping");
    private readonly int DeadHash = Animator.StringToHash("Dead");
    private readonly int StunnedHash = Animator.StringToHash("Stunned");
    private readonly int UpHash = Animator.StringToHash("Up");
    private readonly int DownHash = Animator.StringToHash("Down");
    private readonly int UpWindupHash = Animator.StringToHash("UpWindup");
    private readonly int UpAttackHash = Animator.StringToHash("UpAttack");
    private readonly int UpParryHash = Animator.StringToHash("UpParry");
    private readonly int DownWindupHash = Animator.StringToHash("DownWindup");
    private readonly int DownAttackHash = Animator.StringToHash("DownAttack");
    private readonly int DownParryHash = Animator.StringToHash("DownParry");

    void Update()
    {
        UpdateVisuals();

        switch (playerController.GetFacingDirection())
        {
            case FacingDirection.left:
                bodyRenderer.flipX = true;
                break;
            case FacingDirection.right:
                bodyRenderer.flipX = false;
                break;
        }
    }

    private void UpdateVisuals()
    {
        if (playerController.previousState != playerController.currentState)
        {
            switch (playerController.currentState)
            {
                case PlayerState.idle:
                    animator.CrossFade(IdleHash, 0);
                    break;
                case PlayerState.walking:
                    animator.CrossFade(WalkingHash, 0);
                    break;
                case PlayerState.jumping:
                    animator.CrossFade(JumpingHash, 0);
                    break;
                case PlayerState.dead:
                    animator.CrossFade(DeadHash, 0);
                    break;
                case PlayerState.stunned:
                    animator.CrossFade(StunnedHash, 0);
                    break;
                case PlayerState.up:
                    animator.CrossFade(UpHash, 0);
                    break;
                case PlayerState.down:
                    animator.CrossFade(DownHash, 0);
                    break;
                case PlayerState.upwindup:
                    animator.CrossFade(UpWindupHash, 0);
                    break;
                case PlayerState.upattack:
                    animator.CrossFade(UpAttackHash, 0);
                    break;
                case PlayerState.upparry:
                    animator.CrossFade(UpparryHash, 0);
                    break;
                case PlayerState.downwindup:
                    animator.CrossFade(DownWindupHash, 0);
                    break;
                case PlayerState.downattack:
                    animator.CrossFade(DownAttackHash, 0);
                    break;
                case PlayerState.downparry:
                    animator.CrossFade(DownparryHash, 0);
                    break;
            }
        }
    }
}
