using System.Collections;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    [Header("Explosion Settings : ")]
    [SerializeField] private float ExplosionRadius = 5f;
    [SerializeField] private float ExplosionDamage = 0f;
    [SerializeField] private float ExplosionForce = 500f;
    [SerializeField] private float ExplosiveUpwardsModifier = 3f;

    private bool ShowExplosionRadius = false;

    public virtual void DoExplosion()
    {
        StartCoroutine(explosionRoutine());
    }

    private IEnumerator explosionRoutine()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, ExplosionRadius);

        for (int x = 0; x < colliders.Length; x++)
        {
            Collider hit = colliders[x];

            if (ExplosionDamage > 0)
            {
                Damageable damageable = hit.GetComponent<Damageable>();
                if (damageable)
                {
                    if (hit.GetComponent<Explosive>() != null)
                    {
                        StartCoroutine(dealDelayedDamaged(damageable, 0.1f));
                    }
                    else
                    {
                        damageable.DealDamage(ExplosionDamage, hit.ClosestPoint(transform.position), transform.eulerAngles, true, gameObject, hit.gameObject);
                    }
                }
            }
        }

        yield return new WaitForFixedUpdate();
        colliders = Physics.OverlapSphere(transform.position, ExplosionRadius);

        for (int x = 0; x < colliders.Length; x++)
        {
            Collider hit = colliders[x];

            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddExplosionForce(ExplosionForce, transform.position, ExplosionRadius, ExplosiveUpwardsModifier);
            }
        }

        yield return null;
    }

    IEnumerator dealDelayedDamaged(Damageable damageable, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        damageable.DealDamage(ExplosionDamage);
    }

    void OnDrawGizmosSelected()
    {
        if (ShowExplosionRadius)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, ExplosionRadius);
        }
    }
}
