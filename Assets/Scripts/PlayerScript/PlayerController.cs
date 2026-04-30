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


    //스프린트 점프 쿨타임 리셋함수
    public void ResetSprintJumpCooldown()
    {
        sprintJumpCooldownTimer = sprintJumpCooldown; // 
    }
    //프로퍼티
    public bool CanSprintJump => sprintJumpCooldownTimer <= 0;


    public bool IsGrounded()
    {
        if (cd == null) return false;

        // 시작점을 약간 위로 올려서 파묻힘 방지
        Vector3 rayStartPos = new Vector3(cd.bounds.center.x, cd.bounds.min.y + 0.1f, cd.bounds.center.z);
        // groundLayer 하나만 사용!
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

    public PlayerAttack1State AttackState { get; private set; }
    

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
        HandleHeavyAttackInput();

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
            // 대시 어택 진입 시 스프린트 상태를 강제로 꺼주는 게 좋습니다 (무한 질주 버그 방지)
            //isSprinting = false;
            StateMachine.ChangeState(DashAndSprintATK); 
            return;
        }
        // 2. 만약 땅에 있고, 콤보 중이 아니라면? -> 1타 발동!
        if (IsGrounded() && StateMachine.CurrentState != Attack1State)
        {
            StateMachine.ChangeState(Attack1State);
        }

    }

    //헤비공격 분배기 함수
    public void HandleHeavyAttackInput()
    {
        // 1. F키를 눌렀을 때 발급된 티켓이 있는지 확인
        if (!inputReader.HAttackPressed) return;

        if (StateMachine.CurrentState is PlayerAttackState)
        {
            Debug.Log("현재 공격 중이라 강공격 입력을 무시합니다.");
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
}