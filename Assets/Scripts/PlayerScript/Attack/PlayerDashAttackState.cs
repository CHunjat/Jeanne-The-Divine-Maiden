using UnityEngine;

public class PlayerDashAttackState : PlayerAttackState
{
    // 대시 어택 특유의 미끄러지는 속도
    private float slideSpeed;

    public PlayerDashAttackState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();

        // 콤보 연계 차단 (단타기)
        comboInputRegistered = false;

        // 진입 시 속도 설정 (기존 대시 속도 활용하거나 별도 수치 지정)
        // ex: 일반 이동 속도의 1.5배 ~ 2배
        if (player.isSprinting)
        {
            slideSpeed = player.moveSpeed * 2.5f;
            player.isSprinting = false;

        }
        else
        {
            slideSpeed = player.moveSpeed * 1.5f; // 만약 dashSpeed가 너무 빠르면 player.moveSpeed * 1.5f 로 조절

        }
    }


    public override void Exit()
    {
        base.Exit();

        // [핵심] 대시 어택 상태를 빠져나가는 그 즉시, X축 관성 없앰
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 방향 체크
        float facingDir = player.isFacingRight ? 1f : -1f;

        // [핵심] 애니메이션 진행도에 따른 관성 제어
        float normalizedTime = GetNormalizedTime();

        if (normalizedTime < 0.5f)
        {
            // 0% ~ 50%: 미끄러지며 공격 (풀 스피드)
            player.SetVelocity(facingDir * slideSpeed, player.rb.linearVelocity.y);
        }
        else if (normalizedTime < 0.8f)
        {
            // 50% ~ 80%: 서서히 브레이크 (Lerp를 쓰면 더 부드러움)
            float slowedSpeed = Mathf.Lerp(slideSpeed, 0f, (normalizedTime - 0.5f) / 0.3f);
            player.SetVelocity(facingDir * slowedSpeed, player.rb.linearVelocity.y);
        }
        else
        {
            // 80% ~ 100%: 멈춰서 후딜레이
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 대시 어택 도중 낭떠러지로 떨어지면 공중 상태로 즉시 전환
        if (!player.IsGrounded())
        {
            stateMachine.ChangeState(player.AirState);
            return;
        }

        // 100% 끝나면 Idle로 돌아가는 건 부모(PlayerAttackState)의 LogicUpdate가 처리함
    }
}