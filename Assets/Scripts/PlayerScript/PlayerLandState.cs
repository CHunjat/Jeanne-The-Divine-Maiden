using System.Diagnostics;

public class PlayerLandState : PlayerState
{
    public PlayerLandState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        //base.Enter();
        stateTimer = 0f;
        
        player.ResetLandTimer();
        if (player.isSprinting)
        {
            float dir = player.isFacingRight ? 1f : -1f;
            player.SetVelocity(dir * player.sprintSpeed, 0f); //착지 구르기 속도조절
            player.animator.Play(player.anim_SprintLand, 0,0); // 스프린트 착지+ 리셋
        }
        else
        {
            player.animator.CrossFade(animHash, 0.1f); // 일반 점프 모션
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (player.isSprinting)
        {
            // 0.3초 동안은 무슨 키를 누르든 여기서 return
            // 착지 애니메이션 길이에 맞춰서 테스트
            if (stateTimer < 0.4f)
            {
                return;
            }
        }
        else
        {
            // 일반 착지는 0.1초만 딜레이
            if (stateTimer < 0.1f) return;
        }
        // ---------------------------------------------------------

        // 딜레이가 끝난 후, 방향키를 누르고 있다면 다시 달리기로 전환
        if (player.inputReader.MoveValue.x != 0)
        {
            stateMachine.ChangeState(player.MoveState);
            return;
        }

        // 아무것도 안 누르고 0.5초가 지나면 Idle 상태로
        if (stateTimer > 0.5f)
        {
            stateMachine.ChangeState(player.IdleState);
        }








        //// 방법 A: 일정 시간이 지나면 Idle로 (가장 깔끔함)
        //// 만약 착지 애니메이션이 0.2초라면 0.2f를 넣으세요.
        //if (player.isSprinting && stateTimer < 0.1f) return;
        //if (stateTimer > 0.5f)
        //{
        //    stateMachine.ChangeState(player.IdleState);
        //    return;
        //}

        //// 방법 B: 움직임 입력이 있으면 바로 Move로 캔슬
        //if (player.inputReader.MoveValue.x != 0)
        //{
        //    stateMachine.ChangeState(player.MoveState);
        //}
    }
}