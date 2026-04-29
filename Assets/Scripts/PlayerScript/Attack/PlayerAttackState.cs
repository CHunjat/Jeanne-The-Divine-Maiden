using UnityEditor;
using UnityEngine;

public abstract class PlayerAttackState : PlayerState
{
    protected bool comboInputRegistered;

    public PlayerAttackState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }


    public override void Enter()
    {
        base.Enter();
        player.inputReader.AttackPressed = false;
        comboInputRegistered = false; // 진입 시 예약 초기화
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 애니메이션 재생이 100%(1.0f) 끝났다면?
        if (GetNormalizedTime() >= 1.0f)
        {
            // 기본 대기 상태로 돌려보냄
            stateMachine.ChangeState(player.IdleState);
        }

        if (stateTimer > 0.1f && GetNormalizedTime() >= 1.0f)
        {
            stateMachine.ChangeState(player.IdleState);
        }

        if (!comboInputRegistered && stateTimer > 0.1f && GetNormalizedTime() >= 1.0f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    // 현재 애니메이션이 얼마나 재생되었는지(0.0 ~ 1.0) 퍼센트로 가져오는 함수
    protected float GetNormalizedTime()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.shortNameHash == animHash)
        {
            return stateInfo.normalizedTime; 
        }
        return 0;
    }
}