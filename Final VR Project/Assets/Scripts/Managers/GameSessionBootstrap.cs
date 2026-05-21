using UnityEngine;

public class GameSessionBootstrap : MonoBehaviour
{
    public static GameSessionBootstrap Instance { get; private set; }

    [SerializeField] private Transform playerTransform;
    [SerializeField] private AmmoSpawn ammoSpawner;

    public Transform PlayerTransform => playerTransform;
    public AmmoSpawn AmmoSpawner => ammoSpawner;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
