using System.Collections;
using UnityEngine;

public class WaveScheduler : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private WaveDefinition[] waveSequence;
    [SerializeField] private WaveDefinition fallbackWave;

    private int waveIndex;

    private void Awake()
    {
        if (spawnManager == null)
        {
            spawnManager = GetComponent<SpawnManager>();
        }
    }

    public WaveDefinition GetWave(int waveNumber)
    {
        if (waveSequence != null && waveSequence.Length > 0)
        {
            int index = Mathf.Clamp(waveNumber - 1, 0, waveSequence.Length - 1);
            return waveSequence[index];
        }

        if (fallbackWave != null)
        {
            return fallbackWave;
        }

        return CreateRuntimeFallback(waveNumber);
    }

    public void AdvanceWave()
    {
        if (waveSequence != null && waveSequence.Length > 0)
        {
            waveIndex = Mathf.Min(waveIndex + 1, waveSequence.Length - 1);
        }
    }

    public IEnumerator RunSpawnWave(WaveDefinition wave, int waveNumber)
    {
        if (spawnManager == null || wave == null)
        {
            yield break;
        }

        int count = wave.zombieCount > 0 ? wave.zombieCount : waveNumber + 5;
        yield return spawnManager.SpawnWaveZombies(count, wave.spawnIntervalSeconds);
    }

    public float GetInterWaveDelay(WaveDefinition wave)
    {
        return wave != null ? wave.interWaveDelaySeconds : 45f;
    }

    public float GetVendingWindow(WaveDefinition wave)
    {
        return wave != null ? wave.vendingWindowSeconds : 45f;
    }

    private static WaveDefinition CreateRuntimeFallback(int waveNumber)
    {
        var wave = ScriptableObject.CreateInstance<WaveDefinition>();
        wave.zombieCount = waveNumber + 5;
        wave.spawnIntervalSeconds = 1f;
        wave.interWaveDelaySeconds = 45f;
        wave.vendingWindowSeconds = 45f;
        wave.healthBonusBetweenWaves = 25;
        wave.healthBonusThreshold = 75f;
        return wave;
    }
}
