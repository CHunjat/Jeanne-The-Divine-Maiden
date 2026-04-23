public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 대쉬 전환
        if (player.inputReader.DashPressed)
        {
            player.inputReader.DashPressed = false;
            stateMachine.ChangeState(player.DashState);
            return;
        }

        // 멈추면 Idle로
        if (player.inputReader.MoveValue.x == 0)
        {
            stateMachine.ChangeState(player.IdleState);
        }
        //점프
        if (player.inputReader.JumpPressed && player.IsGrounded())
        {
            player.inputReader.JumpPressed = false; // 입력 초기화
            stateMachine.ChangeState(player.JumpState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        float xInput = player.inputReader.MoveValue.x;

        // ★ 누락됐던 반전 호출 코드!
        if (xInput != 0)
        {
            player.FlipController(xInput);
        }

        player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
    }
}