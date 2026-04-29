using UnityEngine;

// 상속을 PlayerState가 아니라 방금 만든 PlayerAttackState로 받습니다!
public class PlayerAttack1State : PlayerAttackState
{
    private bool comboInputRegistered; // 다음 공격 예약 여부
    public PlayerAttack1State(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();

        // 1타를 때릴 때는 제자리에 딱 멈춰서 묵직하게 휘두르게 속도를 0으로 뺍니다.
        comboInputRegistered = false; // 진입 시 예약 초기화
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. 1타를 휘두르는 도중에 공격 버튼을 누르면 '예약'을 걸어둠
        if (player.inputReader.AttackPressed)
        {
            player.inputReader.AttackPressed = false;
            comboInputRegistered = true;
        }



        // 2. 애니메이션이 60% 이상 진행됐고, 예약이 걸려 있다면? -> 2타로 캔슬!
        // (이 0.6f 수치를 조절해서 콤보가 이어지는 쫀득한 타이밍을 찾으세요)
        if (comboInputRegistered && stateTimer > 0.1f && GetNormalizedTime() >= 0.7f)
        {
            stateMachine.ChangeState(player.Attack2State); // 2타 스크립트면 Attack3State로
        }
    }

    // (나중에 여기에 2타로 넘어가는 콤보 로직을 추가할 겁니다)
}