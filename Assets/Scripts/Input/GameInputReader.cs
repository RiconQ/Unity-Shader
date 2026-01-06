using UnityEngine;
using R3;

public class GameInputReader : MonoBehaviour
{
    private PlayerInputActions m_inputActions;

    private readonly ReactiveProperty<Vector2> m_moveInput = new(Vector2.zero);
    private readonly Subject<Unit> m_jumpSubject = new();
    private readonly ReactiveProperty<bool> m_sprintInput = new(false);

    public ReadOnlyReactiveProperty<Vector2> MoveInput => m_moveInput;
    public Observable<Unit> JumpInput => m_jumpSubject;
    public ReadOnlyReactiveProperty<bool> SprintInput => m_sprintInput;

    private void Awake()
    {
        m_inputActions = new();

        m_inputActions.Gameplay.Move.performed += context => m_moveInput.Value = context.ReadValue<Vector2>();
        m_inputActions.Gameplay.Move.canceled += context => m_moveInput.Value = Vector2.zero;

        m_inputActions.Gameplay.Jump.performed += context => m_jumpSubject.OnNext(Unit.Default);

        m_inputActions.Gameplay.Shift.performed += contex => m_sprintInput.Value = true;
        m_inputActions.Gameplay.Shift.canceled += contex => m_sprintInput.Value = false;
    }

    private void OnEnable()
    {
        m_inputActions.Enable();
    }

    private void OnDisable()
    {
        m_inputActions.Disable();
    }

    private void OnDestroy()
    {
        m_inputActions.Dispose();
        m_moveInput.Dispose();
        m_jumpSubject.Dispose();
        m_sprintInput.Dispose();
    }
}
