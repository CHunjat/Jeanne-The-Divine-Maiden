public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }


    public override void Enter()
    {
        //base.Enter(); // 기본 Move가 틀어지지만
        stateTimer = 0f;
       
        if (player.isSprinting)
        {
            if (!player.isJumpCut) //점프 쿨타임 때문에 돌아온 게 아닐 때만 
            {
                player.animator.Play(player.anim_SprintStart);
            } //시작 모션 재생 SprintStart로 덮어씌운다.

            player.isJumpCut = false; //꺼준다
        }
        else
        {
            player.animator.CrossFade(animHash, 0.1f); // 기본 달리기 모션
        }
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        player.HandleAttackInput();
        if (player.isSprinting)
        {
            // Sprint_Start 애니메이션이 끝났는지 체크 (normalizedTime이 1.0을 넘으면 종료)
            var stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(player.anim_SprintStart) && stateInfo.normalizedTime >= 1.0f)
            {
                player.animator.Play(player.anim_SprintIng); // 시작이 끝나면 루프 재생
            }
        }

        // 멈췄을 때
        if (player.inputReader.MoveValue.x == 0)
        {
            if (player.isSprinting)
            {
                player.wasSprinting = true;
                player.animator.Play(player.anim_SprintBreak); // 급정지 모션 재생
            }
            //player.isSprinting = false;
            stateMachine.ChangeState(player.IdleState);
        }
        // 대쉬 전환
        if (player.inputReader.DashPressed && player.CanDash) // 쿨타임 확인 추가
        {
            player.inputReader.DashPressed = false;
            stateMachine.ChangeState(player.DashState);
            return;
        }

        // 일반 run멈추면 Idle로
        if (player.inputReader.MoveValue.x == 0)
        {
            //player.isSprinting = false;
            stateMachine.ChangeState(player.IdleState);
            return;
        }
        //점프
        if (player.inputReader.JumpPressed && player.IsGrounded())
        {
            //player.isSprinting = false;
            player.inputReader.JumpPressed = false; // 입력 초기화
            stateMachine.ChangeState(player.JumpState);
        }
        
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        float xInput = player.inputReader.MoveValue.x;

        // ★ 누락됐던 반전 호출 코드!
        if (xInput != 0)
        {
            player.FlipController(xInput);
        }
        float currentSpeed = player.isSprinting ? player.sprintSpeed : player.moveSpeed;
        player.SetVelocity(xInput * currentSpeed, player.rb.linearVelocity.y);
    }
}