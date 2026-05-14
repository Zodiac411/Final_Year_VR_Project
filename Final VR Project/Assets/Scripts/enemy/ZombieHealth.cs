    using UnityEngine;

    public class ZombieHealth : MonoBehaviour
    {
        public SpawnManager spawnManager;
        public float health = 70f;
        //public float totalHealth = 100f;
        private EnemyDism enemyDism;
        private bool isDead = false;

        void Start()
        {
            enemyDism = GetComponent<EnemyDism>();
        }



    

    void Die()
    {
        if (!isDead)
        {
            isDead = true; 

            if (spawnManager != null)
            {
                spawnManager.ZombieKilled();
            }

            GetComponent<ZombieAI>().isAlive = false;
            CreditsManager.Instance.AddCredits(30);
            enemyDism.StartRagdoll();
            Destroy(gameObject, 5f);
        }
    }



    public void TakeDamage(float amount, BodyPart hitPart = null)
    {
        
        if (hitPart != null)
        {
           
            float damageMultiplier = 2.0f; 
               

            health -= (amount * damageMultiplier);
        }
        else
        {
            
            health -= amount;
        }

        if (health <= 0f)
        {
            Die();
        }
    }
}
        

    
