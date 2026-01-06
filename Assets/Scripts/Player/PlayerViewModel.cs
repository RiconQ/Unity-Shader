using R3;
using UnityEngine;

public class PlayerViewModel
{
    // 입력값
    public ReactiveProperty<Vector2> InputDirection { get; } = new(Vector2.zero);
    public Subject<Unit> JumpRequest { get; } = new();
    public ReactiveProperty<bool> IsSprinting { get; } = new(false);

    // 상태
    public ReadOnlyReactiveProperty<bool> IsMoving { get; }

    // Model 설정값
    private readonly PlayerStats playerStats;

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

        if(input.sqrMagnitude < 0.001f)
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

        if(targetDirection.sqrMagnitude > 0.001f)
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
        // 입력값 가져오기
        var input = InputDirection.CurrentValue;

        // 이동 방향 계산
        var moveDir = Vector3.zero;
        if (input.sqrMagnitude > 0.001f)
        {
            var camForward = cameraRotation * Vector3.forward;
            var camRight = cameraRotation * Vector3.right;

            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            moveDir = (camForward * input.y + camRight * input.x).normalized;
        }

        // 달리기 상태에 따라 속도 선택
        var speedMultiplier = IsSprinting.Value ? playerStats.SprintMultiplier : 1; 

        // 목표 수평 속도
        var targetVelocity = moveDir * playerStats.MoveSpeed * speedMultiplier;

        // 수직 속도, 중력 처리
        var yVelocity = currentVelocity.y;

        if(isGrounded)
        {
            yVelocity = -0.1f;
        }
        else
        {
            yVelocity += playerStats.Gravity * deltaTime;
        }

        // 최종 합성
        return new Vector3(targetVelocity.x, yVelocity, targetVelocity.z);
    }

    public float CalculateJumpVelocity()
    {
        return Mathf.Sqrt(2f * -playerStats.Gravity * playerStats.JumpForce);
    }
}
