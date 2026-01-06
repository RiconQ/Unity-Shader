using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("Movement")]
    public float RotationSpeed = 5f; // 캐릭터 회전 속도
    public float MoveSpeed = 10f;   // 캐릭터 이동 속도
    public float SprintSpeed = 20f; // 캐릭터 달리기 속도
    public float CrouchSpeed = 4f;  // 캐릭터 웅크리기 속도
    public float JumpForce = 5f;    // 점프력 설정

    [Header("Physics")]
    public float Gravity = -30f;    // 중력
    public float AirDrag = 0.1f;    // 공기 저항

    [Header("Character")]
    public float NormalHeight = 2f; // 서 있을 때 키
    public float CrouchHeight = 1f; // 웅크렸을 때 키
    public float Radius = 0.5f;     // 캐릭터 두께
}
