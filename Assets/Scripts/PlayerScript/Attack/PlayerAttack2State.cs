using UnityEngine;

public class PlayerAttack2State : PlayerAttackState
{
    private bool comboInputRegistered;

    public PlayerAttack2State(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        comboInputRegistered = false;
       
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.inputReader.AttackPressed)
        {
            player.inputReader.AttackPressed = false;
            comboInputRegistered = true;
        }



        // 2타에서 3타로 넘어가는 타이밍 (마찬가지로 0.6f 조절)
        if (comboInputRegistered && stateTimer > 0.1f && GetNormalizedTime() >= 0.7f)
        {
            stateMachine.ChangeState(player.Attack3State); // 2타 스크립트면 Attack3State로
        }
    }
}