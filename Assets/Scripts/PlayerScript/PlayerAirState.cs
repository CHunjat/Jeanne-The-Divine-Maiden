using UnityEngine;

public class PlayerAirState : PlayerState
{
    public PlayerAirState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // ★ 핵심: BoxCollider 기반 착지 판정
        // 위로 솟구치는 중이 아니고(y <= 0.1f), 컨트롤러의 IsGrounded가 true일 때
        if (player.rb.linearVelocity.y <= 0.1f && player.IsGrounded())
        {
            stateMachine.ChangeState(player.IdleState);
        }

        // 공중에서 대쉬하고 싶다면 여기에 추가
        if (player.inputReader.DashPressed)
        {
            player.inputReader.DashPressed = false;
            stateMachine.ChangeState(player.DashState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 공중 제어 (Air Control)
        float xInput = player.inputReader.MoveValue.x;

        // 이동 속도는 지상의 80% 정도로 세팅 (취향껏 조절)
        player.SetVelocity(xInput * player.moveSpeed * 0.8f, player.rb.linearVelocity.y);

        if (xInput != 0)
            player.FlipController(xInput);

        // 하강 시 무게감을 위해 추가 중력 부여 (선택 사항)
        if (player.rb.linearVelocity.y < 0)
        {
            player.rb.linearVelocity += Vector3.up * Physics.gravity.y * 1.5f * Time.deltaTime;
        }
    }
}