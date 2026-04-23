using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();

        player.rb.linearVelocity = new Vector3(player.rb.linearVelocity.x, player.jumpForce, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 위로 올라가다가 속도가 줄어들어 떨어지기 시작하면 Air 상태로 (필요 시)
        if (player.rb.linearVelocity.y < 0)
        {
            // 아직 AirState를 안 만들었다면 일단 Move나 Idle로 보낼 수 있습니다.
            stateMachine.ChangeState(player.IdleState);
        }
    }
}