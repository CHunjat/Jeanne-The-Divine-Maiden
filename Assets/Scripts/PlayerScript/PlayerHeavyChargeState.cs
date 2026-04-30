using UnityEngine;

public class PlayerHeavyChargeState : PlayerState
{
    private float chargeThreshold = 0.2f;
    private bool isFullyCharged;
    private float fullChargeTime = 1.2f; // 1초를 모으면 풀차지
    private bool isLoopAnimStarted; 
    public PlayerHeavyChargeState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        player.SetVelocity(0f, player.rb.linearVelocity.y);
        isFullyCharged = false; // 진입 시 풀차지 초기화
        isLoopAnimStarted = false;
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();


        if (!isLoopAnimStarted && GetNormalizedTime() >= 1.0f)
        {
            isLoopAnimStarted = true;
            player.animator.Play("Charging"); // 코드로 직접 루프 애니메이션(Charging)으로 덮어씌움!
        }
        


        // 1. 유저가 손을 떼는 순간! (짧게 누른 건 이미 ReadyState가 걸러줬으므로 무조건 차지 공격 발동)
        if (!player.inputReader.HeavyAttackHeld)
        {
            if (isFullyCharged)
            {
                // [성공] 풀차지 상태에서 뗐을 때만 강력한 공격!
                stateMachine.ChangeState(player.HeavyAttackState);
            }
            else
            {
                // [실패/취소] 풀차지 전에 뗐을 때의 처리
                // 기획에 따라 Attack1State로 보내거나, 그냥 Idle로 보낼 수 있습니다.
                stateMachine.ChangeState(player.Attack2State);
                Debug.Log("차지 실패: 너무 빨리 뗐습니다.");
            }
            return;
        }
       

        // 2. 풀차지 도달 체크 ()
        if (stateTimer >= fullChargeTime && !isFullyCharged)
        {
            isFullyCharged = true;
            Debug.Log("풀차지 완료! 번쩍!");
            // TODO: 파티클 생성이나 이펙트 재생
        }
    }
}