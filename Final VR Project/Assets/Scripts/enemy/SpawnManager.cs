using System.Collections;
using TMPro;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] spawnPoints;
    public GameObject[] zombiePrefabs;
    public GameObject vendingMachine;
    public TextMeshProUGUI zombieCountText, vendingMachineText;

    [SerializeField] private WaveScheduler waveScheduler;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private AmmoSpawn ammoSpawner;

    private int totalZombiesToSpawn;
    private int totalZombiesAlive;
    private int waveNumber;
    private bool canSpawn;
    private float vendingCountdown;

    void Start()
    {
        if (waveScheduler == null)
        {
            waveScheduler = GetComponent<WaveScheduler>();
        }

        if (playerTransform == null && GameSessionBootstrap.Instance != null)
        {
            playerTransform = GameSessionBootstrap.Instance.PlayerTransform;
        }

        if (ammoSpawner == null && GameSessionBootstrap.Instance != null)
        {
            ammoSpawner = GameSessionBootstrap.Instance.AmmoSpawner;
        }

        if (vendingMachineText != null)
        {
            vendingMachineText.enabled = false;
        }
    }

    void UpdateZombieCountText()
    {
        if (zombieCountText != null)
        {
            zombieCountText.text = "Zombies Left: " + totalZombiesAlive;
        }
    }

    public void ZombieKilled()
    {
        if (totalZombiesAlive > 0)
        {
            totalZombiesAlive--;
            UpdateZombieCountText();
        }

        if (totalZombiesAlive <= 0)
        {
            StartCoroutine(WaitAndStartNextWave());
        }
    }

    public IEnumerator SpawnWaveZombies(int count, float intervalSeconds)
    {
        canSpawn = true;
        totalZombiesToSpawn = count;

        for (int i = 0; i < totalZombiesToSpawn && canSpawn; i++)
        {
            GameObject spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject zombiePrefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];
            GameObject zombie = Instantiate(zombiePrefab, spawnPoint.transform.position, Quaternion.identity);
            zombie.GetComponent<ZombieHealth>().spawnManager = this;

            totalZombiesAlive++;
            yield return new WaitForSeconds(Mathf.Max(0.1f, intervalSeconds));
        }

        canSpawn = false;
        UpdateZombieCountText();
    }

    IEnumerator WaitAndStartNextWave()
    {
        WaveDefinition completedWave = waveScheduler != null
            ? waveScheduler.GetWave(waveNumber)
            : null;

        if (zombieCountText != null)
        {
            zombieCountText.enabled = false;
        }

        if (vendingMachineText != null)
        {
            vendingMachineText.enabled = true;
        }

        if (vendingMachine != null)
        {
            vendingMachine.SetActive(true);
        }

        vendingCountdown = waveScheduler != null
            ? waveScheduler.GetVendingWindow(completedWave)
            : 45f;

        while (vendingCountdown > 0f)
        {
            if (vendingMachineText != null)
            {
                vendingMachineText.text = "Time for weapons shopping!" + "\n" + "Time left: " + Mathf.Round(vendingCountdown);
            }

            vendingCountdown -= Time.deltaTime;
            yield return null;
        }

        if (vendingMachine != null)
        {
            vendingMachine.SetActive(false);
        }

        if (vendingMachineText != null)
        {
            vendingMachineText.enabled = false;
        }

        StartNextWave();

        if (zombieCountText != null)
        {
            zombieCountText.enabled = true;
        }
    }

    public void StartNextWave()
    {
        waveNumber++;
        WaveDefinition wave = waveScheduler != null
            ? waveScheduler.GetWave(waveNumber)
            : null;

        ApplyBetweenWaveBonus(wave);

        if (ammoSpawner != null)
        {
            ammoSpawner.SpawnAmmo();
        }
        else if (GameSessionBootstrap.Instance != null && GameSessionBootstrap.Instance.AmmoSpawner != null)
        {
            GameSessionBootstrap.Instance.AmmoSpawner.SpawnAmmo();
        }

        if (vendingMachine != null)
        {
            vendingMachine.SetActive(false);
        }

        if (waveScheduler != null)
        {
            waveScheduler.AdvanceWave();
            StartCoroutine(waveScheduler.RunSpawnWave(wave, waveNumber));
        }
        else
        {
            totalZombiesToSpawn = waveNumber + 5;
            StartCoroutine(SpawnWaveZombies(totalZombiesToSpawn, 1f));
        }
    }

    private void ApplyBetweenWaveBonus(WaveDefinition wave)
    {
        if (playerTransform == null)
        {
            return;
        }

        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            return;
        }

        float threshold = wave != null ? wave.healthBonusThreshold : 75f;
        int bonus = wave != null ? wave.healthBonusBetweenWaves : 25;

        if (playerHealth.currentHealth <= threshold)
        {
            playerHealth.currentHealth = Mathf.Clamp(playerHealth.currentHealth + bonus, 0, playerHealth.maxHealth);
            playerHealth.UpdateHealthUI();
        }
    }
}
