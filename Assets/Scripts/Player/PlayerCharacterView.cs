using UnityEngine;
using R3;
using KinematicCharacterController;

public class PlayerCharacterView : MonoBehaviour, ICharacterController
{
    [Header("Reference")]
    [SerializeField] private KinematicCharacterMotor m_motor;
    [SerializeField] private GameInputReader m_inputReader;
    [SerializeField] private Transform m_visualTransform;

    [Header("Setting")]
    [SerializeField] private PlayerStats m_playerStat;

    private PlayerViewModel m_playerViewModel;
    private Transform m_mainCameraTransform;
    private bool m_jumpRequested = false;

    // DebugUI용 ViewModel
    public PlayerViewModel PlayerViewModel => m_playerViewModel;

    private void Awake()
    {
        m_motor.CharacterController = this;
        m_playerViewModel = new(m_playerStat);

        m_mainCameraTransform = Camera.main.transform;
    }

    private void Start()
    {
        // Input Binding
        // 이동 입력
        m_inputReader.MoveInput
            .Subscribe(value => m_playerViewModel.InputDirection.Value = value)
            .AddTo(this);

        // 점프 입력
        m_inputReader.JumpInput
            .Subscribe(value => m_playerViewModel.JumpRequest.OnNext(Unit.Default))
            .AddTo(this);

        m_playerViewModel.JumpRequest
            .Subscribe(value => m_jumpRequested = true)
            .AddTo(this);

        // 달리기 입력 
        // inputReader의 SprintInput값이 바뀌면 viewModel의 IsSprinting 값 변경
        m_inputReader.SprintInput
            .Subscribe(value => m_playerViewModel.IsSprinting.Value = value)
            .AddTo(this);

        // 웅크리기 입력
        m_inputReader.CrouchInput
            .Subscribe(value => m_playerViewModel.IsCrouching.Value = value)
            .AddTo(this);

        // 웅크리기 상태에 따라 Capsule크기 변경
        m_inputReader.CrouchInput
            .DistinctUntilChanged() // 값이 바뀔때만 실행
            .Subscribe(HandleCrouchChange)
            .AddTo(this);
    }

    #region KCC 함수
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        var isGrounded = m_motor.GroundingStatus.IsStableOnGround;

        // 이동 로직
        var finalVelocity = m_playerViewModel.CalculateVelocity(
                currentVelocity,
                m_mainCameraTransform.rotation,
                isGrounded,
                deltaTime
            );

        // 점프 로직
        if (m_jumpRequested)
        {
            // 땅에 있을때 점프
            if (isGrounded)
            {
                m_motor.ForceUnground();

                finalVelocity.y = m_playerViewModel.CalculateJumpVelocity();
            }

            m_jumpRequested = false;
        }

        // 최종 속도 적용
        currentVelocity = finalVelocity;
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        var newRotation = m_playerViewModel.CalculateTargetRotation
        (
            currentRotation,
            m_mainCameraTransform.rotation,
            deltaTime
        );

        currentRotation = newRotation;
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
    }
    #endregion

    #region 내부 함수
    private void HandleCrouchChange(bool isCrouching)
    {
        var targetHeight = isCrouching ? m_playerStat.CrouchHeight : m_playerStat.NormalHeight;

        m_motor.SetCapsuleDimensions(
            m_playerStat.Radius,
            targetHeight,
            targetHeight * 0.5f
            );

        var heightRatio = isCrouching ?
            (m_playerStat.CrouchHeight / m_playerStat.NormalHeight) : 1f;
        m_visualTransform.localScale = new Vector3(1f, heightRatio, 1f);
    }
    #endregion
}
