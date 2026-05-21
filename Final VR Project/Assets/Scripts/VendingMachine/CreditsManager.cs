using System;
using UnityEngine;
using UnityEngine.Events;

public class CreditsManager : MonoBehaviour
{
    public static CreditsManager Instance { get; private set; }

    public int Credits { get; private set; }
    public int Balance => Credits;

    public event Action<int> OnCreditsChanged;
    [SerializeField] private UnityEvent<int> onCreditsChangedUnity;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            NotifyCreditsChanged();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void AddCredits(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Credits += amount;
        NotifyCreditsChanged();
    }

    public bool CanAfford(int amount)
    {
        return Credits >= amount;
    }

    public bool TrySpendCredits(int amount)
    {
        if (!CanAfford(amount))
        {
            return false;
        }

        Credits -= amount;
        NotifyCreditsChanged();
        return true;
    }

    public void SpendCredits(int amount)
    {
        TrySpendCredits(amount);
    }

    private void NotifyCreditsChanged()
    {
        OnCreditsChanged?.Invoke(Credits);
        onCreditsChangedUnity?.Invoke(Credits);
    }
}
