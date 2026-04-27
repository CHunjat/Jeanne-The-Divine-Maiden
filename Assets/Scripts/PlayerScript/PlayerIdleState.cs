using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        //base.Enter();
        stateTimer = 0f;
        player.isSprinting = false;
        player.SetVelocity(0f, player.rb.linearVelocity.y);
        if (player.wasSprinting)
        {
            player.animator.Play(player.anim_SprintBreak);
            player.wasSprinting = false; // 신호 초기화
        }
        else
        {
            player.animator.CrossFade(animHash, 0.1f); // 기본 대기 모션
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        float xInput = player.inputReader.MoveValue.x;

        if (player.inputReader.DashPressed && player.CanDash) // 쿨타임 확인 추가
        {
            player.inputReader.DashPressed = false;
            stateMachine.ChangeState(player.DashState);
            return;
        }
        

        //점프
        if (player.inputReader.JumpPressed && player.IsGrounded())
        {
            player.inputReader.JumpPressed = false; // 입력 초기화
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        if (player.inputReader.MoveValue.x != 0)
        {
            stateMachine.ChangeState(player.MoveState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }
}