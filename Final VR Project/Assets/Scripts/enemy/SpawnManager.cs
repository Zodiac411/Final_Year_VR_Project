using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] spawnPoints;
    public GameObject[] zombiePrefabs; 
    public GameObject vendingMachine;
    public TextMeshProUGUI zombieCountText, vendingMachineText;
    private int totalZombiesToSpawn;
    private int totalZombiesAlive;
    private int waveNumber = 0;
    private bool canSpawn = false;
    public float timeBetweenWaves = 30;

    void Start()
    {
        vendingMachineText.enabled = false;
        // StartNextWave();
    }

    void UpdateZombieCountText()
    {
        zombieCountText.text = "Zombies Left: " + totalZombiesAlive.ToString();
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

    IEnumerator SpawnZombies()
    {


        canSpawn = true; 
        for (int i = 0; i < totalZombiesToSpawn && canSpawn; i++)
        {
            GameObject spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject zombiePrefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];
            GameObject zombie = Instantiate(zombiePrefab, spawnPoint.transform.position, Quaternion.identity);
            zombie.GetComponent<ZombieHealth>().spawnManager = this; 

            totalZombiesAlive++;
            yield return new WaitForSeconds(1f); 
        }
        canSpawn = false;
        UpdateZombieCountText();
    }

    IEnumerator WaitAndStartNextWave()
    {
        zombieCountText.enabled = false;
        vendingMachineText.enabled = true;
        vendingMachine.SetActive(true);

        yield return new WaitForSeconds(45); 
        vendingMachine.SetActive(false);
        vendingMachineText.enabled = false;
        StartNextWave();
        zombieCountText.enabled = true;
    }

    public void StartNextWave()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player.GetComponent<PlayerHealth>().currentHealth <= 75)
        {
            player.GetComponent<PlayerHealth>().currentHealth += 25;
            player.GetComponent<PlayerHealth>().UpdateHealthUI();
        }
        GameObject.Find("AmmoSpawner").GetComponent<AmmoSpawn>().SpawnAmmo();
        vendingMachine.SetActive(false);
        waveNumber++;
        totalZombiesToSpawn = waveNumber + 5;
        //totalZombiesToSpawn = 1;
        StartCoroutine(SpawnZombies());
    }

    private void Update()
    {
        if (vendingMachineText.enabled)
        {
            timeBetweenWaves -= Time.deltaTime;
            vendingMachineText.text = "Time for weapons shopping!" + "\n" + "Time left: " + Mathf.Round(timeBetweenWaves);
        }
        else
        {
            timeBetweenWaves = 45;
        }
    }
}
