using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public float MoveSpeed = 10f;
    public float JumpForce = 5f;
}
