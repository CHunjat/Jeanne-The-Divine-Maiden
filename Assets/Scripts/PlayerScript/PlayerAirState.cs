using UnityEngine;

public class PlayerAirState : PlayerState
{

   

    public PlayerAirState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        // base.Enter(); 절대 사용 금지!
        stateTimer = 0;

        if (player.isSprinting)
        {
            // 전력질주 중이었다면 스프린트 전용 점프/낙하 모션 유지
            player.animator.Play(player.anim_SprintJump);
        }
        else
        {
            // 일반 상태라면 기본 낙하(Falling) 모션 재생
            player.animator.CrossFade(animHash, 0.1f);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // : BoxCollider 기반 착지 판정
        // 위로 솟구치는 중이 아니고(y <= 0.1f), 컨트롤러의 IsGrounded가 true일 때
        if (player.rb.linearVelocity.y <= 0.1f && player.IsGrounded())
        {
            stateMachine.ChangeState(player.LandState);
            return;
        }

        if (player.inputReader.JumpPressed && player.CanJump)
        {
            player.inputReader.JumpPressed = false;
            stateMachine.ChangeState(player.JumpState); // 다시 JumpState로!
            return;
        }

        // 공중에서 대쉬하고 싶다면 여기에 추가
        if (player.inputReader.DashPressed && player.CanDash) // 쿨타임 확인 추가
        {
            player.inputReader.DashPressed = false;
            stateMachine.ChangeState(player.DashState);
            return;
        }
        else
        {
            player.inputReader.DashPressed = false ;
        }
        #region 벽타기 진입스위치 및 코드 
        float xInput = player.inputReader.MoveValue.x;
        float facingDir = player.isFacingRight ? 1f : -1f;

        // 누르는 방향과 바라보는 방향이 같고 + 벽에 닿았고 + 아래로 떨어지는 중일 때
        if (xInput == facingDir && player.IsTouchingWall(facingDir) && player.rb.linearVelocity.y < 0f)
        {
            stateMachine.ChangeState(player.WallSlideState);
            return;
        }
        #endregion
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 공중 제어 (Air Control)
        float xInput = player.inputReader.MoveValue.x;
        float currentX = player.rb.linearVelocity.x;

        if (xInput != 0 && Mathf.Sign(xInput) != Mathf.Sign(currentX))
        {
            // 대쉬 관성을 즉시 무시하고 일반 이동 속도로 꺾어버림
            player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        }
        // 2. 대쉬 관성 유지 및 감속 (기존 로직)
        else if (Mathf.Abs(currentX) > player.moveSpeed)
        {
            float targetX = xInput * player.moveSpeed;
            // 3f~5f 사이에서 취향껏 감속 속도 조절
            float lerpedX = Mathf.Lerp(currentX, targetX, Time.deltaTime * player.airDeceleration); //감속속도 조절 인스펙터로 빼고 설정해보잨ㅋ airdece....
            player.SetVelocity(lerpedX, player.rb.linearVelocity.y);
        }
        // 3. 일반 공중 제어
        else
        {
            player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        }
        if (xInput != 0) player.FlipController(xInput);

        

        #region 이전버전코드
        //if (Mathf.Abs(currentX) > player.moveSpeed)
        //{

        //    // 대쉬 속도에서 일반 속도로 부드럽게 줄어들게 함
        //    float targetX = xInput * player.moveSpeed;
        //    // JumpState와 동일한 수치(예: 3f)를 사용해야 전환이 이질감이 없습니다.
        //    float lerpedX = Mathf.Lerp(currentX, targetX, Time.deltaTime * 1.5f);

        //    //if (xInput == 0)
        //    //    player.SetVelocity(currentX, player.rb.linearVelocity.y);
        //    //else
        //    //    player.SetVelocity(xInput * player.dashSpeed, player.rb.linearVelocity.y);
        //}
        //else
        //{
        //    player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        //}

        //if (xInput != 0) player.FlipController(xInput);

        //// 중력 가속 로직 (기존 유지)
        //if (player.rb.linearVelocity.y < 0)
        //{
        //    player.rb.linearVelocity += Vector3.up * Physics.gravity.y * 1.5f * Time.deltaTime;
        //}
        #endregion
    }
}