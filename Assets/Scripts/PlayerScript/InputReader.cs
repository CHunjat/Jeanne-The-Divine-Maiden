using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
public class InputReader : ScriptableObject, PlayerControls.IPlayerActions
{
    private PlayerControls controls;

    // 외부에서 읽어갈 프로퍼티들
    public Vector2 MoveValue { get; private set; }
    public bool DashPressed { get; set; }
    public bool JumpPressed { get; set; } // 점프 상태 추가

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
}