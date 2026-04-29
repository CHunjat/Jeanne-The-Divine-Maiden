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

        // [핵심] 초반 0.15초 동안만 바라보는 방향으로 스텝을 밟습니다.
        if (stateTimer < 0.15f)
        {
            float facingDir = player.isFacingRight ? 1f : -1f;
            player.SetVelocity(facingDir * stepSpeed, player.rb.linearVelocity.y);
        }
        else // 전진 스텝이 끝나면 그 자리에 딱 멈춰서 묵직하게 후딜레이 모션 재생
        {
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }
    }
}