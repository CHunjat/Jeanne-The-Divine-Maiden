using UnityEngine;

public class PlayerThrustAttackState : PlayerState
{
    private new float stateTimer; // 상태 전용 타이머

    public PlayerThrustAttackState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        stateTimer = 0f; // 진입 시 타이머 초기화

        // 씹힘 방지용 강제 재생
        player.animator.Play(player.anim_ThrustAtk, 0, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        stateTimer += Time.deltaTime; // 시간 누적

        float dir = player.isFacingRight ? 1f : -1f;

        // 🔥 핵심: 그래프에서 현재 시간에 해당하는 'Y값(배율)'을 가져옵니다.
        // Evaluate 함수가 stateTimer를 인식해서 곡선 위의 값을 뽑아줍니다.
        float currentMultiplier = player.thrustVelocityCurve.Evaluate(stateTimer);

        // 물리 적용 (X축 방향 * 기본속도 * 그래프 배율)
        player.SetVelocity(dir * player.moveSpeed * currentMultiplier, player.rb.linearVelocity.y);

        // 🌟 탈출 조건: 애니메이션 진행도가 아니라 설정한 '지속 시간'이 다 되면 탈출!
        if (stateTimer >= player.thrustDuration)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.SetVelocity(0f, player.rb.linearVelocity.y); // 상태 나갈 때 속도 0
    }
}