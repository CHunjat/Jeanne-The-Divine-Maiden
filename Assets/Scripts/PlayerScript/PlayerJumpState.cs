using UnityEngine;

public class PlayerJumpState : PlayerState
{

    
    public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        //base.Enter();
        if (player.isSprinting && !player.CanSprintJump)
        {
            player.inputReader.JumpPressed = false;
            player.SetVelocity(player.rb.linearVelocity.x, 0f);

            player.isJumpCut = true; 

            stateMachine.ChangeState(player.MoveState);
            return;
        }

        stateTimer = 0f;
        player.wallGrabTimer = player.wallGrabCooldown;

        if (player.isSprinting)
        {
            //쿨타임적용
            player.ResetSprintJumpCooldown();

            //반복문돌려서 유즈점프를 다돌리고 (캔점프가 될때까지)
            while(player.CanJump)
            {
                player.UseJump();
            }
            player.animator.Play(player.anim_SprintJump);//실행한다.
        }
        else
        {
            player.UseJump();
            player.animator.CrossFade(animHash, 0.1f); // 일반 점프 모션
        }
        //player.isSprinting = false;
        player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, player.jumpForce);

        // 현재 수평 속도가 거의 없다면 (수직 점프 상황 방지)
        // 대쉬 중이었던 방향을 강제로 읽어서 속도를할당
        float moveX = player.inputReader.MoveValue.x;

        // 입력이 없으면 제자리점프
        if (Mathf.Abs(player.rb.linearVelocity.x) < 0.1f)
        {
            player.rb.linearVelocity = new Vector3(player.rb.linearVelocity.x, player.jumpForce, 0f);
        }
        else
        {
            // 이미 속도가 있다면 (이동 중 점프) 그 속도를 유지하며 점프
            player.rb.linearVelocity = new Vector3(player.rb.linearVelocity.x, player.jumpForce, 0f);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        //상승중 언제든지 점프대시 가능하도록 코드추가
        if (player.inputReader.DashPressed && player.CanDash)
        {
            player.inputReader.DashPressed = false;
            stateMachine.ChangeState(player.DashState);
            return; // 대쉬로 전환했으므로 아래 로직은 실행 안 함
        }
        #region 예전코드
        //// 상승 중에도 좌우 입력을 받아 속도를 조절합니다.
        //float xInput = player.inputReader.MoveValue.x;

        //// 수평 속도는 입력에 따라, 수직 속도는 현재 리지드바디 속도(상승분)를 유지
        //player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);

        //if (xInput != 0) //방향전환 즉각적으로 처리 
        //{
        //    player.FlipController(xInput);
        //}
        #endregion


        // 위로 올라가다가 속도가 줄어들어 떨어지기 시작하면 Air 상태로 (필요 시)
        if (player.rb.linearVelocity.y < -0.1f)
        {
            // IdleState 대신 AirState로 보냅니다.
            stateMachine.ChangeState(player.AirState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        float xInput = player.inputReader.MoveValue.x;
        float currentXVelocity = player.rb.linearVelocity.x;

        // 1. 대쉬 점프 속도 유지 로직
        // 현재 속도가 일반 이동 속도보다 빠르다면 (대쉬 중 점프했다는 뜻)

        if (Mathf.Abs(xInput) > 0.1f)
        {
            // 2. 누르고 있는 방향을 1 또는 -1로 정규화 (손가락의 의지)
            float inputDir = Mathf.Sign(xInput);

            // 3. 타이머(쿨타임)가 끝났는지 확인 + 누른 방향(inputDir)으로 레이더 발사!
            if (player.wallGrabTimer <= 0f && player.IsTouchingWall(inputDir))
            {
                // 4. 상승 중일 때 너무 일찍 붙는 게 싫다면 추가 조건 (선택 사항)
                // if (player.rb.linearVelocity.y < 2f) 

                stateMachine.ChangeState(player.WallSlideState);
                return;
            }
        }
        if (xInput != 0 && Mathf.Sign(xInput) != Mathf.Sign(currentXVelocity))
        {
            // 대쉬 관성을 즉시 무시하고 일반 이동 속도로 꺾어버림
            player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        }
        // 2. 대쉬 관성 유지 및 감속 (기존 로직)
        else if (Mathf.Abs(currentXVelocity) > player.moveSpeed)
        {
            float targetX = xInput * player.moveSpeed;
            // 3f~5f 사이에서 취향껏 감속 속도 조절
            float lerpedX = Mathf.Lerp(currentXVelocity, targetX, Time.deltaTime * player.airDeceleration); //감속속도 조절 인스펙터로 빼고 설정해보잨ㅋ airdece....
            player.SetVelocity(lerpedX, player.rb.linearVelocity.y);
        }
        // 3. 일반 공중 제어
        else
        {
            player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        }

        if (xInput != 0) player.FlipController(xInput);
        #region 이전버전코드
        //if (Mathf.Abs(currentXVelocity) > player.moveSpeed)
        //{
        //    // 대쉬의 관성을 유지하며 상승 (X값은 그대로, Y값만 물리 연산에 맡김)
        //    // 이때 xInput이 0(중립)이라면 속도를 죽이지 않고 현재 속도(대쉬 속도) 유지
        //    //if (xInput == 0)
        //    //{
        //    //    player.SetVelocity(currentXVelocity, player.rb.linearVelocity.y);
        //    //}
        //    //else
        //    //{
        //    //    // 반대 방향을 누르면 브레이크가 걸리도록 처리 (선택 사항)
        //    //    player.SetVelocity(xInput * player.dashSpeed, player.rb.linearVelocity.y);
        //    //}
        //    if (xInput != 0 && Mathf.Sign(xInput) != Mathf.Sign(currentXVelocity))
        //    {
        //        // 대쉬 관성을 즉시 무시하고 일반 이동 속도로 꺾어버림
        //        player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        //    }
        //    else 
        //    {
        //        float targetX = xInput * player.moveSpeed;
        //        float lerpedX = Mathf.Lerp(currentXVelocity, targetX, Time.deltaTime * 1.5f);
        //        player.SetVelocity(lerpedX, player.rb.linearVelocity.y);
        //    }

        //}
        //else
        //{
        //    // 일반 점프 시에는 공중 제어 속도 적용
        //    player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        //}

        //// 2. 즉각적인 방향 전환 (Flip)
        //if (xInput != 0)
        //{
        //    player.FlipController(xInput);
        //}
        #endregion
    }
}