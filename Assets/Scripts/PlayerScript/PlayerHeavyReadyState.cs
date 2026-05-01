using UnityEngine;

public class PlayerHeavyReadyState : PlayerState
{
    // 유저의 의도를 파악하기 위해 기다리는 시간 (0.15초 ~ 0.2초 추천)
    private float waitTime = 0.15f;

    public PlayerHeavyReadyState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
       
        base.Enter();
        // 공격할 준비를 하니 그 자리에 멈춰 세웁니다.
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. 0.15초를 못 버티고 손을 뗐다! -> 짧게 누른 것으로 판정 (일반 강공격)
        if (!player.inputReader.HeavyAttackHeld)
        {
            // TODO: 일반 강공격 State가 따로 있다면 거기로, 없다면 일단 Attack1State로 보냅니다.
            stateMachine.ChangeState(player.Attack1State);
            return;
        }

        // 2. 0.15초 동안 끝까지 꾹 누르고 버텼다! -> 진짜 기모으기(차지)로 인정!
        if (stateTimer >= waitTime)
        {
            stateMachine.ChangeState(player.HeavyChargeState);
        }
    }
}