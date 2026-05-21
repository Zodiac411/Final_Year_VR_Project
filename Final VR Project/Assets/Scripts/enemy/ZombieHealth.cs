using UnityEngine;

public class ZombieHealth : MonoBehaviour, IDamageable
{
    public SpawnManager spawnManager;
    public float health = 70f;

    private EnemyDism enemyDism;
    private bool isDead;

    void Start()
    {
        enemyDism = GetComponent<EnemyDism>();
    }

    public void ApplyDamage(in DamageContext context)
    {
        if (isDead)
        {
            return;
        }

        health -= context.Amount;

        if (health <= 0f)
        {
            Die();
        }
    }

    public void TakeDamage(float amount, BodyPart hitPart = null)
    {
        float multiplier = hitPart != null ? hitPart.DamageMultiplier : 1f;
        ApplyDamage(new DamageContext(amount * multiplier, null, transform.position, Vector3.up, 1f));
    }

    void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        if (spawnManager != null)
        {
            spawnManager.ZombieKilled();
        }

        GetComponent<ZombieAI>().isAlive = false;
        if (CreditsManager.Instance != null)
        {
            CreditsManager.Instance.AddCredits(30);
        }
        enemyDism.StartRagdoll();
        Destroy(gameObject, 5f);
    }
}
