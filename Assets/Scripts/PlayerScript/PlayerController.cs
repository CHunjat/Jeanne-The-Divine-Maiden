using NUnit.Framework;
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

    [Header("Orientation")]
    public bool isFacingRight = true;

    [Header("Jump Settings")]
    public float jumpForce = 12f; // 인스펙터에서 조절 가능

    [Header("Ground Check Settings (BoxCast)")]
    public Vector3 groundCheckSize = new Vector3(0.5f, 0.1f, 0.5f);
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;

    [Header("기즈모 땅체크 조절")]
    public LayerMask groundLayerMask;

    
    private void OnDrawGizmos() //디버그용 땅체크
    {
        if (cd == null) return;

        Vector3 rayStartPos = new Vector3(cd.bounds.center.x, cd.bounds.min.y, cd.bounds.center.z);
        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawWireCube(rayStartPos + Vector3.down * groundCheckDistance, groundCheckSize);
    }
    // 지면 접촉 여부 확인 함수
    public bool IsGrounded()
    {
        // 캐릭터 발밑에 작은 구체를 그려 땅 레이어와 닿는지 확인
        if (cd == null) return false;

        // groundCheck.position 대신 콜라이더의 바닥(min.y)을 사용!
        Vector3 rayStartPos = new Vector3(cd.bounds.center.x, cd.bounds.min.y, cd.bounds.center.z);

        return Physics.BoxCast(rayStartPos, groundCheckSize / 2, Vector3.down, transform.rotation, groundCheckDistance, groundLayer);
    }    

    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerAirState AirState { get; private set; }

    private void Awake()
    {
        StateMachine = new PlayerStateMachine();
        IdleState = new PlayerIdleState(this, StateMachine, "Idle");
        MoveState = new PlayerMoveState(this, StateMachine, "Move");
        DashState = new PlayerDashState(this, StateMachine, "Dash");
        JumpState = new PlayerJumpState(this, StateMachine, "Jump");

        if (rb == null) rb = GetComponent<Rigidbody>(); //리지드바디 할당
        if (cd == null) cd = GetComponent<BoxCollider>(); // 콜라이더 할당
    }

    private void Start() => StateMachine.Initialize(IdleState);

    private void Update()
    {
        StateMachine.CurrentState.HandleInput();
        StateMachine.CurrentState.LogicUpdate();
    }

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
}