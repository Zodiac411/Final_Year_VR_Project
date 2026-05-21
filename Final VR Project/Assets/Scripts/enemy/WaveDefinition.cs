using UnityEngine;

[CreateAssetMenu(fileName = "WaveDefinition", menuName = "Waves/Wave Definition")]
public class WaveDefinition : ScriptableObject
{
    [Min(1)] public int zombieCount = 5;
    [Min(0.1f)] public float spawnIntervalSeconds = 1f;
    [Min(0f)] public float interWaveDelaySeconds = 45f;
    [Min(0f)] public float vendingWindowSeconds = 45f;
    [Min(0)] public int healthBonusBetweenWaves = 25;
    public float healthBonusThreshold = 75f;
}
