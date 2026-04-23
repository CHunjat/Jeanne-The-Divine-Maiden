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

        if (dashTime <= 0)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }
}