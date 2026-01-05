using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public float RotationSpeed = 5f;
    public float MoveSpeed = 10f;
    public float JumpForce = 5f;

    public float Gravity = -30f;
    public float AirDrag = 0.1f;
}
