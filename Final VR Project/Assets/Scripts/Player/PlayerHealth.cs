using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    [SerializeField] public int currentHealth;

    [SerializeField] private HealthUISlider healthUI;

    public UnityEvent OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void UpdateHealthUI()
    {
        if (healthUI != null)
        {
            healthUI.UpdateHealth(currentHealth);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponentInChildren<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10);
            }
        }
    }
}

