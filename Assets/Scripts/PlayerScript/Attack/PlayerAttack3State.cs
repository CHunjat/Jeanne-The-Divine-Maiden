using UnityEngine;

public class PlayerAttack3State : PlayerAttackState
{
    private float stepSpeed = 4f;
    public PlayerAttack3State(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();

        // 막타의 묵직함을 위해 제자리 고정! (이펙트나 화면 흔들림 넣기 딱 좋은 위치)

        player.SetVelocity(0f, player.rb.linearVelocity.y);

        float dir = player.isFacingRight ? 1f : -1f;
        comboInputRegistered = false;
    }
    public override void LogicUpdate()
    {
        // [중요] 부모의 LogicUpdate만 실행합니다. 
        // 부모의 로직: !comboInputRegistered(true) && GetNormalizedTime >= 1.0f 일 때 Idle로 복구
        base.LogicUpdate();

        // 여기서 추가적인 공격 입력 체크(comboInputRegistered = true)를 하지 않습니다.
        // 그러면 3타 애니메이션이 100% 끝날 때까지 무조건 기다리게 됩니다.
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        float facingDir = player.isFacingRight ? 1f : -1f;


        // [핵심] 초반 0.15초 동안만 바라보는 방향으로 스텝을 밟습니다.
        if (player.OnSlope())
        {
            player.rb.useGravity = false; // 중력 끄기
            player.SetVelocity(0f, 0f);   // 속도 완전 고정 (이동 공격이 아닐 경우)
        }
        if (stateTimer < 0.15f)
        {

            Vector3 moveVec = new Vector3(facingDir, 0f, 0f);
            Vector3 slopeVec = player.GetSlopeMoveDirection(moveVec);


            float downwardStickiness = slopeVec.y < 0 ? 2.0f : 1.0f;

            player.SetVelocity(slopeVec.x * stepSpeed,(slopeVec.y * stepSpeed * downwardStickiness) - (slopeVec.y < 0 ? 2f : 0f));
        }
        else // 전진 스텝이 끝나면 그 자리에 딱 멈춰서 묵직하게 후딜레이 모션 재생
        {
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.useGravity = true; // 상태 나갈 때 중력 원복 필수!
    }

}