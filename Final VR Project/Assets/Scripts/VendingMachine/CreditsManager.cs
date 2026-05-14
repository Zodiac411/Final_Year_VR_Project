using UnityEngine;

public class CreditsManager : MonoBehaviour
{
    public static CreditsManager Instance { get; private set; }

    public int Credits { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCredits(int amount)
    {
        Credits += amount;
        
    }

    public bool CanAfford(int amount)
    {
        return Credits >= amount;
    }

    public void SpendCredits(int amount)
    {
        
            Credits -= amount;
           
        
        
           // Debug.LogWarning("Not enough credits to spend.");
        
    }
}
