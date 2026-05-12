using UnityEngine;

public class PlayerThrustReadyState : PlayerState
{
    public PlayerThrustReadyState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();

        // 1. 진입 즉시 미끄러지지 않게 제자리에 딱 멈춤! (브레이크 효과)
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // [중요] 차지 중에는 중력을 유지하되 좌우 이동은 확실히 잠금
        player.SetVelocity(0f, player.rb.linearVelocity.y);

        // [발동 조건] 만약 유저가 차지 키(예: 중간 공격 키)를 뗐다면?!
        // (inputReader에 ThrustAttackReleased 같은 뗀 판정 bool이 있다고 가정)
        if (!player.inputReader.ThrustAttackHeld)
        {
            Debug.Log(" [발사] Held 스위치가 꺼져서 찌르기로 넘어갑니다!");
            stateMachine.ChangeState(player.ThrustAttackState);
            return;
        }

        // (선택) 만약 공중에서 찌르기를 못하게 하려면 땅에서 떨어졌을 때 취소
        if (!player.IsGrounded())
        {
            stateMachine.ChangeState(player.AirState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 🔥 공격 중에는 비탈길에서 본드처럼 딱 붙어있어야 함
        if (player.OnSlope())
        {
            player.rb.useGravity = false; // 중력 끄기
            player.SetVelocity(0f, 0f);   // 속도 완전 고정 (이동 공격이 아닐 경우)
        }
        else
        {
            // 평지라면 기존 중력/마찰력 로직 유지
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.useGravity = true; // 상태 나갈 때 중력 원복 필수!
    }
}