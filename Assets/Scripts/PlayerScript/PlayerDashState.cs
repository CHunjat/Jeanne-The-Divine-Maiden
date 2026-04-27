using UnityEngine;

public class PlayerDashState : PlayerState
{
    private float dashTime;
    private float dashDirection;
  

    public PlayerDashState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();

        player.rb.useGravity = false;
        dashTime = player.dashDuration;

        float xInput = player.inputReader.MoveValue.x;

        // 대쉬 시작 시점의 방향 결정
        if (xInput != 0)
        {
            dashDirection = Mathf.Sign(xInput);
            player.FlipController(xInput);
        }
        else
        {
            dashDirection = player.isFacingRight ? 1f : -1f;
            player.FlipController(dashDirection);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.useGravity = true;

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        // 중력 무시하고 수평 대쉬 (y값을 0으로 주면 공중 대쉬 가능)
        player.SetVelocity(dashDirection * player.dashSpeed, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        dashTime -= Time.deltaTime;

        if (player.inputReader.JumpPressed)
        {
            if (player.IsGrounded())
            {
                player.inputReader.JumpPressed = false;
                player.ResetDashCooldown();
                float dir = player.isFacingRight ? 1f : -1f;
                player.SetDashJumpVelocity(dir); // 대쉬 관성 주입 점프

                stateMachine.ChangeState(player.JumpState);
                return;
            }
            else
            {
                // 공중 대쉬 중이면 점프 입력 그냥 씹기 (선입력 방지)
                player.inputReader.JumpPressed = false;
            } 
        }

        if (dashTime > 0) return;




        player.ResetDashCooldown();

        if (player.IsGrounded())
        {
            if (Mathf.Abs(player.inputReader.MoveValue.x) > 0.1f)
            {
                player.isSprinting = true;
                //player.animator.Play(player.anim_SprintStart); // 시작 모션 즉시 재생
                stateMachine.ChangeState(player.MoveState);
            }
            else
            {
                player.isSprinting = false;
                stateMachine.ChangeState(player.IdleState);
            }
        }
        else
        {
            player.isSprinting = false;
            player.SetVelocity(0f, player.rb.linearVelocity.y);
            stateMachine.ChangeState(player.AirState);
        }

        #region 필요없는코드
        //if (dashTime <= 0)
        //{
        //    player.ResetDashCooldown();
        //    if (!player.IsGrounded())
        //    {
        //        player.isSprinting = false;
        //        player.SetVelocity(0f, player.rb.linearVelocity.y); //대쉬점프 관성없애기 있게하고싶으면 지우삼

        //        stateMachine.ChangeState(player.AirState);
        //    }
        //    // 2. 바닥에 붙어 있다면 Idle로
        //    else
        //    {
        //        if (Mathf.Abs(player.inputReader.MoveValue.x) > 0.1f)
        //        {
        //            player.isSprinting = true;
        //            stateMachine.ChangeState(player.MoveState); // 이동 상태로 가되 속도는 전력질주 적용
        //        }
        //        else
        //        {
        //            player.isSprinting = false;
        //            stateMachine.ChangeState(player.IdleState);
        //        }
        //    }
        //}
        #endregion
    }
}