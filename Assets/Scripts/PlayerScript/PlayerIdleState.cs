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

        if (player.OnSlope()) player.SetVelocity(0f, 0f);

        else player.SetVelocity(0f, player.rb.linearVelocity.y);


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
        player.HandleAttackInput();
        float xInput = player.inputReader.MoveValue.x;

        //idle 시 갑자기 공중으로 떨어졌을때 air 스테이트로 넘겨주는 코드
        if (!player.IsGrounded())
        {
            stateMachine.ChangeState(player.AirState);
            return;
        }


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

        // 🔥 비탈길 미끄러짐 방지 로직
        if (player.OnSlope())
        {
            player.rb.useGravity = false; // 중력 차단
            player.SetVelocity(0f, 0f);   // 완전 정지
        }
        else
        {
            player.rb.useGravity = true;
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }
    }

    // 🔥 나갈 때 중력 원복!
    public override void Exit()
    {
        base.Exit();
        player.rb.useGravity = true;
    }
}