using UnityEngine;

public class PlayerJumpState : PlayerState
{

    
    public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        //base.Enter();
        stateTimer = 0f;

        player.UseJump();

        if (player.isSprinting)
        {
            player.animator.Play(player.anim_SprintJump);
        }
        else
        {
            player.animator.CrossFade(animHash, 0.1f); // 일반 점프 모션
        }
        //player.isSprinting = false;
        player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, player.jumpForce);

        // 현재 수평 속도가 거의 없다면 (수직 점프 상황 방지)
        // 대쉬 중이었던 방향을 강제로 읽어서 속도를 꽂아넣습니다.
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
        float facingDir = player.isFacingRight ? 1f : -1f;

        // 1. 대쉬 점프 속도 유지 로직
        // 현재 속도가 일반 이동 속도보다 빠르다면 (대쉬 중 점프했다는 뜻)

        if ((xInput * facingDir) > 0.1f && player.IsTouchingWall(facingDir))
        {
            stateMachine.ChangeState(player.WallSlideState);
            return;
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