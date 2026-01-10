using R3;
using UnityEngine;

public class PlayerViewModel
{
    // 입력값
    public ReactiveProperty<Vector2> InputDirection { get; } = new(Vector2.zero);
    public Subject<Unit> JumpRequest { get; } = new();

    // 상태
    public ReadOnlyReactiveProperty<bool> IsMoving { get; }
    public ReactiveProperty<bool> IsSprinting { get; } = new(false);
    public ReactiveProperty<bool> IsCrouching { get; } = new(false);
    public ReactiveProperty<bool> IsSliding { get; } = new(false);

    // Model 설정값
    private readonly PlayerStats playerStats;

    // 내부 계산용 변수
    private Vector3 m_slideDirection; // 슬라이딩 방향
    private float m_currentSlideSpeed; // 슬라이딩 중 속도 관리

    public PlayerViewModel(PlayerStats stats)
    {
        playerStats = stats;

        IsMoving = InputDirection
            .Select(vector => vector.sqrMagnitude > 0.001f)
            .ToReadOnlyReactiveProperty();
    }

    /// <summary>
    /// KCC UpdateRotation에서 호출할 캐릭터 회전 함수
    /// </summary>
    public Quaternion CalculateTargetRotation(Quaternion currentRotation, Quaternion cameraRotation, float deltaTime)
    {
        var input = InputDirection.Value;

        if (input.sqrMagnitude < 0.001f)
        {
            // 입력이 없으면 현재 회전값 유지
            return currentRotation;
        }

        // 현재 카메라의 회전값을 기준으로 앞, 오른쪽 방향 구함
        var cameraForward = cameraRotation * Vector3.forward;
        var cameraRight = cameraRotation * Vector3.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 입력 방향과 카메라 방향
        Vector3 targetDirection = (cameraForward * input.y + cameraRight * input.x).normalized;

        if (targetDirection.sqrMagnitude > 0.001f)
        {
            var targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

            return Quaternion.Slerp(currentRotation, targetRotation, playerStats.RotationSpeed * deltaTime);
        }

        return currentRotation;
    }

    /// <summary>
    /// KCC가 호출할 이동 속도 계산 함수
    /// </summary>
    public Vector3 CalculateVelocity(Vector3 currentVelocity, Quaternion cameraRotation, bool isGrounded, float deltaTime)
    {
        // 슬라이딩 상태 갱신
        UpdateSlidingState(currentVelocity, isGrounded);

        // 수평 속도 계산
        var planarVelocity = CalculatePlanarVelocity(cameraRotation, deltaTime);

        // 수직 속도 계산
        var yVelocity = CalculateVerticalVelocity(currentVelocity.y, isGrounded, deltaTime);

        // 최종 속도 반환
        return new Vector3(planarVelocity.x, yVelocity, planarVelocity.z);
    }

    /// <summary>
    /// 플레이어의 점프 시작 속도를 계산
    /// </summary>
    public float CalculateJumpVelocity()
    {
        return Mathf.Sqrt(2f * -playerStats.Gravity * playerStats.JumpForce);
    }

    /// <summary>
    /// 슬라이딩 진입 및 해제 조건을 체크하고 상태를 업데이트
    /// </summary>
    private void UpdateSlidingState(Vector3 currentVelocity, bool isGrounded)
    {
        // 현재 수평 이동 벡터
        var currentPlanarVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        //현재 수평 이동 속도
        var currentSpeed = currentPlanarVelocity.magnitude;

        var isCrouchInput = IsCrouching.Value;
        var isSliding = IsSliding.Value;

        // 슬라이딩 진입 조건 :
        // 땅에 있고 && 웅크리기 키가 입력되고 && 슬라이딩 상태가 아니고 && 속도가 충분하다면
        if (isGrounded && isCrouchInput && !isSliding
            && currentSpeed > playerStats.MoveSpeed + 0.1f)
        {
            IsSliding.Value = true;

            // 슬라이딩 방향 고정 및 초기 속도 설정
            m_slideDirection = currentPlanarVelocity.normalized;
            if (m_slideDirection.magnitude < 0.01f)
            {
                m_slideDirection = Vector3.forward;
            }
            m_currentSlideSpeed = playerStats.SlideSpeed;


        }
        // 슬라이딩 해제 조건
        // 웅크리기 키를 뗌 || 속도가 느림 || 공중
        else if (isSliding && (!isCrouchInput || m_currentSlideSpeed < playerStats.SlideThreshold || !isGrounded))
        {
            IsSliding.Value = false;
        }
    }

    /// <summary>
    /// 현재 상태(슬라이딩, 일반)에 따라 수평 속도 계산
    /// </summary>
    private Vector3 CalculatePlanarVelocity(Quaternion cameraRotation, float deltaTime)
    {
        if (IsSliding.Value)
        {
            // 마찰력 적용
            m_currentSlideSpeed -= playerStats.SlideFriction * deltaTime;
            m_currentSlideSpeed = Mathf.Max(m_currentSlideSpeed, 0);

            return m_slideDirection * m_currentSlideSpeed;
        }

        // 일반 이동 속도 계산
        return CalculateStandardMovement(cameraRotation);
    }

    /// <summary>
    /// WASD 기반의 일반 이동 속도 계산
    /// </summary>
    private Vector3 CalculateStandardMovement(Quaternion cameraRotation)
    {
        var input = InputDirection.CurrentValue;
        if (input.sqrMagnitude < 0.001f)
        {
            return Vector3.zero;
        }

        // 카메라 기준 방향 계산
        var camForward = cameraRotation * Vector3.forward;
        var camRight = cameraRotation * Vector3.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        var moveDir = (camForward * input.y + camRight * input.x).normalized;

        // 속도 결정
        // 우선순위 : 웅크리기 > 달리기 > 걷기
        var targetSpeed = playerStats.MoveSpeed;
        if (IsCrouching.Value)
        {
            targetSpeed = playerStats.CrouchSpeed;
        }
        else if (IsSprinting.Value)
        {
            targetSpeed = playerStats.SprintSpeed;
        }

        // 방향 * 속도 반환
        return moveDir * targetSpeed;
    }

    /// <summary>
    /// 중력 및 접지 처리
    /// </summary>
    private float CalculateVerticalVelocity(float currentYVelocity, bool isGrounded, float deltaTime)
    {
        if (isGrounded)
        {
            // 바닥 밀착용
            return -0.1f;
        }
        else
        {
            return currentYVelocity + (playerStats.Gravity * deltaTime);
        }
    }
}
