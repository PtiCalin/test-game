using UnityEngine;

/// <summary>
/// Minimal collectible placeholder used by MazeGenerator. Extensible for scoring/FX.
/// </summary>
public class Collectible : MonoBehaviour
{
    public bool IsTreasure { get; private set; }
    public int Points { get; private set; }
    public float RotateSpeed { get; private set; }
    public float BobAmplitude { get; private set; }
    public float BobFrequency { get; private set; }

    public void Configure(bool isTreasure, int points, float rotateSpeed, float bobAmplitude, float bobFrequency)
    {
        IsTreasure = isTreasure;
        Points = points;
        RotateSpeed = rotateSpeed;
        BobAmplitude = bobAmplitude;
        BobFrequency = bobFrequency;
    }
}
