using System;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input Data")]
    public InputReader inputReader;

    [Header("Components")]
    public Rigidbody rb;
    public BoxCollider cd;
    public Animator animator;

    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashcooltime = 1.5f;
    private float dashCooltimer;


    [Header("착지딜레이 버니합금지")]
    public float landDashDelay = 0.5f;
    private float landTimer;


    public bool CanDash => dashCooltimer <= 0 && landTimer <= 0;


    [Header("Orientation")]
    public bool isFacingRight = true;

    [Header("점프랑 공중세팅")]
    public float jumpForce = 12f; // 인스펙터에서 조절 가능
    public float airDeceleration = 5f;
    public int MaxJumpCount = 2;
    private int currentjumpCount;
    public void RestJumpCount() => currentjumpCount = MaxJumpCount;
    public bool CanJump => currentjumpCount > 0;
    public void UseJump() => currentjumpCount--;


    [Header("Ground Check Settings (BoxCast)")]
    public Vector3 groundCheckSize = new Vector3(0.5f, 0.1f, 0.5f);
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;

    [Header("벽, 벽점프 관리 세팅")]
    public float wallSlideSpeed = 2f;
    public Vector2 wallJumpForce = new Vector2(10f, 12f);
    public float wallCheckDistance = 0.1f;
    public LayerMask wallLayer;
    public Vector3 WallCheckSize = new Vector3(0.05f, 1.5f, 0.5f);

    
    [Header("대쉬후 전력질주기능")]
    public float sprintSpeed = 10f;
    public bool isSprinting;
    public bool wasSprinting; // ★ 방금 전까지 전력질주 중이었는지 저장 (Break 모션용)

    [Header("스프린트 관리변수")]
    public string anim_SprintStart = "To sprint";
    public string anim_SprintIng = "Sprinting";
    public string anim_SprintBreak = "SprintBreak";
    public string anim_SprintJump = "Sprint-jump-falling";
    public string anim_SprintLand = "sprint-falling-land";
    //스프린트 점프 쿨타임 변수
    public float sprintJumpCooldown = 0.5f; // 스프린트 점프 후 대기 시간
    private float sprintJumpCooldownTimer;
    public bool isJumpCut; //쿨타임때문에 점프가 캔슬되었는지 기록, 쿨타임이 안돌았는데 스프린트 점프시 다시 movestate로 돌아가는 코드로 스프린트애니메이션이 다시 재생되는 현상으로 컷내려고만듦

    [Header("강공 찌르기 애니메이션 관리변수")] //스킬X 스킬아님!!
    public string anim_ThrustReady = "MiddleToCharge"; // 기 모으기 모션 이름
    public string anim_ThrustAtk = "MiddleChargeATK";  // 찌르기 모션 이름


    [Header("벽 애니메이션 관리변수")]
    public string anim_WallSlide = "walling";
    public string anim_WallJump = "WallJump";

    [Header("점프 직후 벽감지 쿨타임 추가")]
    public float wallGrabCooldown = 0.2f;
    public float wallGrabTimer;

    [Header("공격 관리")]
    public PlayerAttack1State Attack1State { get; private set; }
    public PlayerAttack2State Attack2State { get; private set; }
    public PlayerAttack3State Attack3State { get; private set; }

    public PlayerDashAttackState DashAndSprintATK { get; private set; }


    [Header("Thrust Attack Settings")]
    public AnimationCurve thrustVelocityCurve; // 찌르기 속도 그래프
    public float thrustDuration = 0.5f;        // 찌르기 전체 지속 시간 (초)


    [Header("공중공격 평타 설정")]
    public int currentAirActionCount = 0;   // 현재 공중 공격 횟수
    public int maxAirActions = 2;           // 최대 허용 횟수
    public float airAttackBounceForce = 2f; // 허공답보 (위로 살짝 뜨는 힘)

    [Header("공중 찍기공격")]
    public float diveDropSpeed = 25f; // 밑으로 내리꽂는 속도 (엄청 빨라야 찰집니다!)
    public string anim_DiveDrop = "AirHeavyDrop";
    public string anim_DiveLand = "AirHeavyAtk";

    [Header("방향키 공중 공격 애니메이션")]
    public string anim_AirUpAtk = "AirUpAtk";
    public string anim_AirDownAtk = "AirDownAtk";

    public bool hasUsedAirUp;   // 윗공격 1회 제한 스위치


    // 상태 선언

    [Header("비탈길(Slope) 세팅")]
    public float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;

    // 1. 비탈길인지 확인하고 경사면 정보(slopeHit)를 업데이트함
    public bool OnSlope()
    {
        if (cd == null) return false;

        //얇은 선(Raycast) 대신 상자(BoxCast)를 아래로 쏴서 발끝부터 발뒤꿈치까지 넓게 체크합니다.
        Vector3 center = cd.bounds.center;
        Vector3 halfExtents = new Vector3(cd.bounds.extents.x * 0.9f, 0.1f, cd.bounds.extents.z * 0.9f); // 캐릭터 너비에 맞춤
        float maxDistance = cd.bounds.extents.y + 0.03f; // 발 아래로 쏘는 길이 // 0.3 -> 0.03 바꾸니까 시발존나잘되잖아 ㅡㅡ 왤케 길게잡았찌

        // BoxCast로 바닥 충돌 정보 가져오기
        if (Physics.BoxCast(center, halfExtents, Vector3.down, out slopeHit, transform.rotation, maxDistance, groundLayer))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);

            // 각도가 0.1도 이상(평지가 아님)이고, 등반 가능 각도 이하일 때만 비탈길로 인정
            return angle > 0.1f && angle <= maxSlopeAngle;
        }
        return false;
    }
    //public bool OnSlope()
    //{
    //    // 캐릭터 발밑으로 레이저를 쏴서 바닥의 각도를 체크
    //    if (Physics.Raycast(cd.bounds.center, Vector3.down, out slopeHit, cd.bounds.extents.y + 0.5f, groundLayer))
    //    {
    //        float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
    //        return angle < maxSlopeAngle && angle != 0;
    //    }
    //    return false;
    //}

    // 2. 가고자 하는 방향(Vector3)을 경사면에 맞춰 꺾어주는 함수
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        // ProjectOnPlane: direction(나의 진행방향)을 slopeHit.normal(바닥의 기울기)에 맞춰 평면투영시킴
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }


    // 공중 횟수 초기화 함수
    public void ResetAirActions() => currentAirActionCount = 0;

    //방어코드 (공중 윗공격 찰나의순간, 땅에서 써버리는거 막기위함)
    public bool IsTooCloseToGround()
    {
        
        Vector3 rayStartPos = new Vector3(cd.bounds.center.x, cd.bounds.min.y + 0.1f, cd.bounds.center.z);
        return Physics.BoxCast(rayStartPos, groundCheckSize / 2, Vector3.down, transform.rotation, 0.5f, groundLayer);
    }


    //스프린트 점프 쿨타임 리셋함수
    public void ResetSprintJumpCooldown()
    {
        sprintJumpCooldownTimer = sprintJumpCooldown; // 
    }
    //프로퍼티
    public bool CanSprintJump => sprintJumpCooldownTimer <= 0;



    //public bool IsGrounded()
    //{
    //    float extraDistance = OnSlope() ? 0.5f : 0.1f; // 🔥 비탈길에선 더 길게 체크
    //    Vector3 boxSize = new Vector3(groundCheckSize.x, 0.05f, groundCheckSize.z);
    //    return Physics.BoxCast(cd.bounds.center, boxSize / 2, Vector3.down, transform.rotation, cd.bounds.extents.y + extraDistance, groundLayer);
    //}
    public bool IsGrounded()
    {
        if (cd == null) return false;
        Vector3 rayStartPos = new Vector3(cd.bounds.center.x, cd.bounds.min.y + 0.1f, cd.bounds.center.z);


        // groundCheckSize를 인스펙터에서 받아와서 사용
        return Physics.BoxCast(rayStartPos, groundCheckSize / 2, Vector3.down, transform.rotation, groundCheckDistance + 0.1f, groundLayer);
    }

    private void OnDrawGizmos()
    {
        if (cd == null) return;

        Vector3 rayStartPos = new Vector3(cd.bounds.center.x, cd.bounds.min.y + 0.1f, cd.bounds.center.z);

        // 여기서도 IsGrounded()를 호출하므로 내부적으로 groundLayer를 사용하게 됨
        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawWireCube(rayStartPos + Vector3.down * (groundCheckDistance + 0.1f), groundCheckSize);

        Gizmos.color = Color.blue;
        DrawWallGizmo(1f);
        DrawWallGizmo(-1f);

    }

    private void DrawWallGizmo(float dir)
    {
        Vector3 origin = cd.bounds.center;
        float checkDist = cd.bounds.extents.x + wallCheckDistance;
        Vector3 hitCenter = origin + (Vector3.right * dir * checkDist);
        Gizmos.DrawWireCube(hitCenter, WallCheckSize);
    }

    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerAirState AirState { get; private set; }
    public PlayerLandState LandState { get; private set; }
    public PlayerWallSlideState WallSlideState { get; private set; }
    public PlayerWallJumpState WallJumpState { get; private set; }

    public PlayerHeavyReadyState HeavyReadyState { get; private set; }
    public PlayerHeavyChargeState HeavyChargeState { get; private set; }
    public PlayerHeavyAttackState HeavyAttackState { get; private set; }

    public PlayerThrustReadyState ThrustReadyState { get; private set; }
    public PlayerThrustAttackState ThrustAttackState { get; private set; }
    public PlayerAirAttack1State AirAttack1State { get; private set; }
    public PlayerAirAttack2State AirAttack2State { get; private set; }

    public PlayerDiveDropState DiveDropState { get; private set; }
    public PlayerDiveLandState DiveLandState { get; private set; }

    public PlayerAirUpAttackState AirUpAttackState { get; private set; }
   // public PlayerAirDownAttackState AirDownAttackState { get; private set; }


    //public PlayerAttack1State AttackState { get; private set; }





    private void Awake()
    {
        StateMachine = new PlayerStateMachine();
        IdleState = new PlayerIdleState(this, StateMachine, "Idle");
        MoveState = new PlayerMoveState(this, StateMachine, "Move");
        DashState = new PlayerDashState(this, StateMachine, "Dash");
        JumpState = new PlayerJumpState(this, StateMachine, "Jump");
        AirState = new PlayerAirState(this, StateMachine, "Falling");
        LandState = new PlayerLandState(this, StateMachine, "Landing");

        WallSlideState = new PlayerWallSlideState(this, StateMachine, "walling");
        WallJumpState = new PlayerWallJumpState(this, StateMachine, "WallJump");

        Attack1State = new PlayerAttack1State(this, StateMachine, "ATK1");
        Attack2State = new PlayerAttack2State(this, StateMachine, "ATK2");
        Attack3State = new PlayerAttack3State(this, StateMachine, "ATK3");
        DashAndSprintATK = new PlayerDashAttackState(this, StateMachine, "Dash(Sprint)ATK");

        HeavyReadyState = new PlayerHeavyReadyState(this, StateMachine, "Idle");
        HeavyChargeState = new PlayerHeavyChargeState(this, StateMachine, "ToCharge");
        HeavyAttackState = new PlayerHeavyAttackState(this, StateMachine, "ChargingAtk");


        ThrustReadyState = new PlayerThrustReadyState(this, StateMachine, "MiddleToCharge");
        ThrustAttackState = new PlayerThrustAttackState(this, StateMachine, "MiddleChargeATK");

        AirAttack1State = new PlayerAirAttack1State(this, StateMachine, "AirAtk1");
        AirAttack2State = new PlayerAirAttack2State(this, StateMachine, "AirAtk2");

        DiveDropState = new PlayerDiveDropState(this, StateMachine, anim_DiveDrop);
        DiveLandState = new PlayerDiveLandState(this, StateMachine, anim_DiveLand);
        AirUpAttackState = new PlayerAirUpAttackState(this, StateMachine, anim_AirUpAtk);
       // AirDownAttackState = new PlayerAirDownAttackState(this, StateMachine, anim_AirDownAtk);

        if (rb == null) rb = GetComponent<Rigidbody>(); //리지드바디 할당
        if (cd == null) cd = GetComponent<BoxCollider>(); // 콜라이더 할당
    }

    private void Start() => StateMachine.Initialize(IdleState);

    private void Update()
    {

        if (sprintJumpCooldownTimer > 0)
            sprintJumpCooldownTimer -= Time.deltaTime;

        //지상에서점프시 벽판정쿨타임
        if (wallGrabTimer > 0)
        { wallGrabTimer -= Time.deltaTime; }

        if (dashCooltimer > 0)
            dashCooltimer -= Time.deltaTime;
        if (landTimer > 0) landTimer -= Time.deltaTime;

        if (inputReader.DashPressed && !CanDash)
        {
            inputReader.DashPressed = false;
        }
        //테스트 공격중일떄는 대시 입력을 강제로 차단
        if (StateMachine.CurrentState is PlayerAttackState && inputReader.DashPressed)
        {
            inputReader.DashPressed = false;
        }

        if (IsGrounded() && rb.linearVelocity.y <= 0.1f)
        {
            RestJumpCount();
        }

        if (IsGrounded() && rb.linearVelocity.y <= 0.1f)
        {
            RestJumpCount();
            ResetAirActions(); // 바닥에 닿으면 공중 공격 횟수 초기화
        }

        //딱 idle, move에서만 가능
        HandleThrustAttackInput(); //강공찌르기 판독기 추가
        HandleHeavyAttackInput(); //스킬찌르기 판독기 추가
       

        StateMachine.CurrentState.HandleInput();
        StateMachine.CurrentState.LogicUpdate();
    }
    public void ResetDashCooldown() => dashCooltimer = dashcooltime;
    public void ResetLandTimer() => landTimer = landDashDelay;
    private void FixedUpdate() => StateMachine.CurrentState.PhysicsUpdate();

    // 3D 리지드바디이므로 Vector3를 사용하며 Z축은 항상 0으로 고정
    public void SetVelocity(float x, float y)
    {
        rb.linearVelocity = new Vector3(x, y, 0f);
    }

    public void FlipController(float xInput)
    {

       

        if (xInput > 0) isFacingRight = true;
        else if (xInput < 0) isFacingRight = false;

        // 사이드뷰 3D 반전 (모델에 따라 90/270 혹은 0/180 사용)
        float targetY = isFacingRight ? 0f : 180f;
        transform.rotation = Quaternion.Euler(0, targetY, 0);

        if (xInput < 0) Debug.Log("왼쪽 바라보기 성공!");
    }

    public void SetDashJumpVelocity(float dashDir)
    {
        rb.linearVelocity = new Vector3(dashDir * dashSpeed, jumpForce, 0f);
    }
    //벽체크 함수
    public bool IsTouchingWall(float dir)
    {
        if (cd == null) return false;

        Vector3 origin = cd.bounds.center;

        // BoxCast (시작점, 박스크기/2, 방향, 회전, 거리, 레이어)
        // 거리는 콜라이더 절반 + 설정한 체크 거리
        float checkDist = cd.bounds.extents.x + wallCheckDistance;

        return Physics.BoxCast(origin, WallCheckSize / 2f, Vector3.right * dir, transform.rotation, checkDist, wallLayer);
    }

    //4.29 시작, 공격 분배기 함수
    public void HandleAttackInput()
    {
        // 1. 방금 1단계에서 만든 InputReader를 통해 공격을 눌렀는지 확인 (확인 즉시 값은 false로 깎임)
        if (!inputReader.AttackPressed) return;

        if (StateMachine.CurrentState == DashState || isSprinting)
        {
            // 대시 어택 진입 시 스프린트 상태를 강제로 끄기
            StateMachine.ChangeState(DashAndSprintATK); 
            return;
        }
        // 2. 만약 땅에 있고, 콤보 중이 아니라면? -> 1타 발동!
        if (IsGrounded() && StateMachine.CurrentState != Attack1State)
        {
            StateMachine.ChangeState(Attack1State);
        }
        //공중 1타 분배
        else if (!IsGrounded() && currentAirActionCount < maxAirActions
            && !(StateMachine.CurrentState is PlayerAirAttack1State)
            && !(StateMachine.CurrentState is PlayerAirAttack2State)
            && !(StateMachine.CurrentState is PlayerAirUpAttackState))
            //&& !(StateMachine.CurrentState is PlayerAirDownAttackState))
        {
            if (IsTooCloseToGround()) return; //공중 윗공격 땅 x
            float yInput = inputReader.MoveValue.y;

            if (yInput > 0.5f)
            {
                // 윗방향키 + 공격
                StateMachine.ChangeState(AirUpAttackState);
            }
            //else if (yInput < -0.5f)
            //{
            //    // 아랫방향키 + 공격
            //    StateMachine.ChangeState(AirDownAttackState);
            //}
            else
            {
                // 방향키 입력이 없거나 좌우만 누르고 있을 때 -> 기본 공중 1타
                StateMachine.ChangeState(AirAttack1State);
            }
        }

    }

    //스킬공격 분배기 함수 // 헤비어택(스킬) 할당키 "E"
    public void HandleHeavyAttackInput()
    {
        // 1.
        if (!inputReader.HAttackPressed) return;

        if (StateMachine.CurrentState is PlayerAttackState )
        {
            Debug.Log("현재 공격 중이라 강공격 입력을 무시합니다.");
            inputReader.HAttackPressed = false;
            return;
        }
        //스프린트중 강공격막음 없애려면 이거 지워라 기획;;
        if (isSprinting)
        {
            inputReader.HAttackPressed = false;
            return;
        }

        // 2. 땅에 있고, 이미 기모으기 준비 중이 아닐 때만 진입
        if (IsGrounded() && StateMachine.CurrentState != HeavyReadyState &&
            StateMachine.CurrentState != HeavyChargeState && StateMachine.CurrentState != HeavyAttackState)
        {
            // 판독기(ReadyState)로 보냅니다.
            StateMachine.ChangeState(HeavyReadyState);
        }
    }



    //강공 찌르기 //키 F
    public void HandleThrustAttackInput()
    {
        if (!inputReader.ThrustAttackPressed) return;

        // 1. 이미 내려찍기 중이면 중복 방지
        if (StateMachine.CurrentState == DiveDropState || StateMachine.CurrentState == DiveLandState)
        {
            inputReader.ThrustAttackPressed = false;
            return;
        }

        bool isActuallyOnGround = IsGrounded() || OnSlope();

        // --- [A] 지상/비탈길 (찌르기) ---
        if (isActuallyOnGround)
        {
            //if (isSprinting) 
            //{
            //    inputReader.ThrustAttackPressed = false; return;
            //}

            if (!(StateMachine.CurrentState is PlayerAttackState) &&
                StateMachine.CurrentState != ThrustReadyState &&
                StateMachine.CurrentState != ThrustAttackState)
            {
                if (OnSlope()) SetVelocity(0f, 0f);
                inputReader.ThrustAttackPressed = false;
                StateMachine.ChangeState(ThrustReadyState);
            }
        }
        // --- [B] 공중 (하강 공격) ---
        else
        {
            // 🚨 어제 맞췄던 '공격 진행도' 로직 부활
            // 현재 공중 공격 중이라면 애니메이션이 어느 정도 진행되었는지 확인
            if (StateMachine.CurrentState is PlayerAirAttack1State ||
                StateMachine.CurrentState is PlayerAirAttack2State ||
                StateMachine.CurrentState is PlayerAirUpAttackState)
            {
                // 🔥 normalizedTime이 0.4f~0.5f 정도는 지나야 하강 공격으로 캔슬 가능
                // 이 수치보다 작을 때 누르면 "두 번 눌러야 한다"고 느끼게 됩니다.
                float nTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

                if (nTime < 0.4f)
                {
                    // 아직 애니메이션 초반이면 입력을 소모하지 않고 리턴 (예약 대기 느낌)
                    // 하지만 유저가 답답할 수 있으니 여기서는 소모하지 않고 '보류'합니다.
                    return;
                }
            }

            // 공중 일반 상태거나, 위 조건을 통과한 공격 후반부라면 즉시 발동
            inputReader.ThrustAttackPressed = false;
            StateMachine.ChangeState(DiveDropState);
        }
    }
}