using UnityEngine;
using R3;
using KinematicCharacterController;

public class PlayerCharacterView : MonoBehaviour, ICharacterController
{
    [Header("Reference")]
    [SerializeField] private KinematicCharacterMotor m_motor;
    [SerializeField] private GameInputReader m_inputReader;

    [Header("Setting")]
    [SerializeField] private PlayerStats m_playerStat;

    private PlayerViewModel m_playerViewModel;
    private Transform m_mainCameraTransform;
    private bool m_jumpRequested = false;

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
    }

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
}
