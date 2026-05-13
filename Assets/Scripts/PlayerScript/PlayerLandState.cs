using UnityEngine;

public class PlayerLandState : PlayerState
{
    public PlayerLandState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        stateTimer = 0f;
        player.ResetLandTimer();

        // 🔥 1. 착지 판정이 뜨자마자 모든 관성을 0으로 멱살 잡고 멈춤 (물리 충돌 원천 차단)
        player.rb.linearVelocity = Vector3.zero;

        if (player.isSprinting)
        {
            player.animator.Play(player.anim_SprintLand, 0, 0);
        }
        else
        {
            player.animator.CrossFade(animHash, 0.1f);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.isSprinting)
        {
            if (stateTimer < 0.4f) return;
        }
        else
        {
            if (stateTimer < 0.1f) return;
        }

        if (player.inputReader.MoveValue.x != 0)
        {
            stateMachine.ChangeState(player.MoveState);
            return;
        }

        if (stateTimer > 0.5f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (player.isSprinting)
        {
            float dir = player.isFacingRight ? 1f : -1f;
            float currentSpeed = player.sprintSpeed;

            if (player.OnSlope())
            {
                player.rb.useGravity = false;
                Vector3 moveDir = new Vector3(dir, 0f, 0f);
                Vector3 slopeMoveDir = player.GetSlopeMoveDirection(moveDir);

                // 🔥 2. "이륙(가짜 튕김)" 방지용 접착제!
                // 경사면 벡터(slopeMoveDir.y)대로만 움직이면 허공으로 날아가 버리므로, 
                // Y축에 강제로 -4f (접착제)를 빼주어 바닥에 찰싹 달라붙어 미끄러지게 만듭니다.
                player.SetVelocity(slopeMoveDir.x * currentSpeed, (slopeMoveDir.y * currentSpeed) - 4f);
            }
            else
            {
                player.rb.useGravity = true;
                player.SetVelocity(dir * currentSpeed, player.rb.linearVelocity.y);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.useGravity = true;
    }
}