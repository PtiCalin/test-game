using UnityEngine;

/// <summary>
/// Minimal player placeholder used by MazeGenerator. Add movement logic as needed.
/// </summary>
public class Player : MonoBehaviour
{
    [field: SerializeField] public float MaxSpeed { get; set; } = 7f;
    [field: SerializeField] public float JumpForce { get; set; } = 5f;
}
