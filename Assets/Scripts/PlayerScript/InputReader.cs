using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
public class InputReader : ScriptableObject, PlayerControls.IPlayerActions
{
    private PlayerControls controls;

    // 외부에서 읽어갈 프로퍼티들
    public Vector2 MoveValue { get; private set; }
    public bool DashPressed { get; set; }
    private bool _jumpPressed;
    public bool JumpPressed //예약입력 제한  좋은데?
    {
        get
        {
            bool value = _jumpPressed;
            _jumpPressed = false; // [핵심] 읽어가자마자 바로 꺼버림
            return value;
        }
        set => _jumpPressed = value;
        //이렇게 하면 JumpState에 진입하기 위해 한 번 읽는 순간 값이 사라지므로,
        //중복 실행이 물리적으로 불가능
    }

    private bool _AttackPressed;
    public bool AttackPressed
    {
        get
        {
            bool value = _AttackPressed;
            _AttackPressed = false;
            return value;
        }
        set => _AttackPressed = value;
        
    }

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new PlayerControls();
            controls.Player.SetCallbacks(this); // 인터페이스 연결
        }
        controls.Player.Enable();
    }

    private void OnDisable() => controls.Player.Disable();

    // 인터페이스 구현 (자동 호출됨)
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveValue = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed) DashPressed = true;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("점프누름");
        if (context.started) JumpPressed = true;
    }

    public void OnATK(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            AttackPressed = true;
        }
        // 2. 버튼에서 손을 떼면 혹시 모를 찌꺼기 입력을 지우기 위해 false를 줍니다.
        else if (context.canceled)
        {
            AttackPressed = false;
        }
    }
}