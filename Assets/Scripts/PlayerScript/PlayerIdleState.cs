using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        float xInput = player.inputReader.MoveValue.x;

        if (player.inputReader.DashPressed)
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