using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public float RotationSpeed = 5f; // 캐릭터 회전 속도
    public float MoveSpeed = 10f;   // 캐릭터 이동 속도
    public float SprintMultiplier = 2f; // 캐릭터 달리기 배율 설정 (MoveSpeed * SprintMultiplier)
    public float JumpForce = 5f;    // 점프력 설정

    public float Gravity = -30f;    // 중력
    public float AirDrag = 0.1f;    // 공기 저항
}
