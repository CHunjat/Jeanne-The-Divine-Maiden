using UnityEngine;

public class PlayerHeavyChargeState : PlayerState
{
    private bool isFullyCharged;
    private float fullChargeTime = 2.0f; // 1초를 모으면 풀차지

    public PlayerHeavyChargeState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        player.SetVelocity(0f, player.rb.linearVelocity.y);
        isFullyCharged = false; // 진입 시 풀차지 초기화
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. 유저가 손을 떼는 순간! (짧게 누른 건 이미 ReadyState가 걸러줬으므로 무조건 차지 공격 발동)
        if (!player.inputReader.HeavyAttackHeld)
        {
            stateMachine.ChangeState(player.HeavyAttackState);
            return;
        }

        // 2. 풀차지 도달 체크 (원한다면 2단계, 3단계 차지로 늘릴 수도 있습니다)
        if (stateTimer >= fullChargeTime && !isFullyCharged)
        {
            isFullyCharged = true;
            Debug.Log("풀차지 완료! 번쩍!");
            // TODO: 파티클 생성이나 이펙트 재생
        }
    }
}