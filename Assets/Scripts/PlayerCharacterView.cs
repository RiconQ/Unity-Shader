using UnityEngine;
using R3;
using KinematicCharacterController;

public class PlayerCharacterView : MonoBehaviour, ICharacterController
{
    [Header("Reference")]
    [SerializeField] private KinematicCharacterMotor m_moter;
    [SerializeField] private GameInputReader m_inputReader;

    [Header("Setting")]
    [SerializeField] private PlayerStats m_playerStat;

    private PlayerViewModel m_playerViewModel;
    private Transform m_mainCameraTransform;

    private void Awake()
    {
        m_moter.CharacterController = this;
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
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        //var forward = Vector3.ProjectOnPlane
        //(
        //
        //);
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
        throw new System.NotImplementedException();
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
