using R3;
using UnityEngine;

public class PlayerViewModel
{
    // 입력값
    public ReactiveProperty<Vector2> InputDirection { get; } = new(Vector2.zero);
    public Subject<Unit> JumpRequest { get; } = new();

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
    /// KCC가 호출할 속도 계산 함수
    /// </summary>
    public Vector3 CalculateVelocity(Quaternion cameraRotation)
    {
        var input = new Vector3(InputDirection.Value.x, 0, InputDirection.Value.y);

        return (cameraRotation * input) * playerStats.MoveSpeed;
    }
}
