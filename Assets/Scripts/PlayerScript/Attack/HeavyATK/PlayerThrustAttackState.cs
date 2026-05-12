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

        player.rb.useGravity = false;
        player.SetVelocity(player.rb.linearVelocity.x, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        stateTimer += Time.deltaTime; // 시간 누적

        float dir = player.isFacingRight ? 1f : -1f;
        // 그래프에서 현재 시간에 해당하는 'Y값(배율)'
        // Evaluate 함수가 stateTimer를 인식해서 곡선 위의 값
        float currentMultiplier = player.thrustVelocityCurve.Evaluate(stateTimer);
        float thrustSpeed = player.moveSpeed * currentMultiplier;
        // 물리 적용 (X축 방향 * 기본속도 * 그래프 배율)
        if (player.OnSlope())
        {
            player.rb.useGravity = false; // 돌진 중 중력 차단
            Vector3 moveDir = new Vector3(dir, 0f, 0f);
            // 🔥 [핵심] 돌진 방향을 경사면에 평행하게 투영(Project)
            Vector3 slopeDir = player.GetSlopeMoveDirection(moveDir);

            // 경사면을 타고 돌진! (X, Y 속도가 모두 계산됨)
            player.SetVelocity(slopeDir.x * thrustSpeed, slopeDir.y * thrustSpeed);

            // 내리막 돌진 시 바닥에 더 밀착시키기 위해 하향 압력 살짝 추가
            if (slopeDir.y < 0)
            {
                player.rb.AddForce(Vector3.down * 50f, ForceMode.Force);
            }
        }
        else
        {
            player.rb.useGravity = true;
            player.SetVelocity(dir * thrustSpeed, player.rb.linearVelocity.y);
        }

        //탈출 조건: 애니메이션 진행도가 아니라 설정한 '지속 시간'이 다 되면 탈출!
        if (stateTimer >= player.thrustDuration)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        
    }

    public override void Exit()
    {
        base.Exit();
        player.SetVelocity(0f, player.rb.linearVelocity.y); // 상태 나갈 때 속도 0
        player.rb.useGravity = true; // 상태 나갈 때 중력 원복 필수!
    }
}