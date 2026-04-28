using UnityEngine;

public class PlayerWallSlideState : PlayerState
{
    private float wallDir; // 벽 방향 기억하기
    

    public PlayerWallSlideState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        stateTimer = 0f;
        player.isSprinting = false;

        // 벽 방향 확인 (오른쪽이면 1, 왼쪽이면 -1)
        wallDir = player.isFacingRight ? 1f : -1f;


        player.FlipController(-wallDir); // 벽 등지기
        player.animator.Play(player.anim_WallSlide, 0, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. 점프 키 누르면 벽 점프!
        if (player.inputReader.JumpPressed)
        {
            player.inputReader.JumpPressed = false;
            stateMachine.ChangeState(player.WallJumpState);
            return;
        }

        // 2. 바닥에 닿으면 착지
        if (player.IsGrounded())
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        float xInput = player.inputReader.MoveValue.x;

        // 3. [해결 완료] 키 떼면 떨어지기
        // xInput이 wallDir과 다르면 무조건 추락합니다.
        // 즉, 방향키에서 손을 떼서 0이 되거나, 반대 방향을 누르면 바로 떨어집니다!
        if (!player.IsTouchingWall(wallDir) || xInput != wallDir)
        {
            stateMachine.ChangeState(player.AirState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // ★ [해결 완료] 벽 밀착의 마법 ★
        // X축 속도를 0으로 멈추지 말고, 벽 방향(wallDir)으로 속도를 계속 줍니다! (예: 2f)
        // ZeroFriction 머티리얼 덕분에 끈적이지 않고, 벽에 빈틈없이 100% 딱 붙어서 긁고 내려옵니다.
        player.SetVelocity(wallDir * 2f, -player.wallSlideSpeed);
    }
}