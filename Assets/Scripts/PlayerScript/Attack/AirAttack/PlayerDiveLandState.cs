using UnityEngine;

public class PlayerDiveLandState : PlayerState
{
    private bool isExitingState;

    public PlayerDiveLandState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        isExitingState = false;

        // 1. 땅에 닿았으니 속도를 완벽하게 0으로 꽉 잡아줍니다. (미끄러짐 방지)
        player.SetVelocity(0f, 0f);

        // 💡 여기에 나중에 [카메라 쉐이크 함수]나 [바닥 먼지 이펙트 생성] 코드를 넣으면 기가 막힙니다!
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 2. 착지 애니메이션의 실제 길이를 가져옵니다.
        float animLength = player.animator.GetCurrentAnimatorStateInfo(0).length;

        // 3. 애니메이션이 다 끝나면 다시 일어납니다 (Idle 상태로 복귀)
        if (stateTimer >= animLength && !isExitingState)
        {
            isExitingState = true;
            stateMachine.ChangeState(player.IdleState);
        }
    }
}