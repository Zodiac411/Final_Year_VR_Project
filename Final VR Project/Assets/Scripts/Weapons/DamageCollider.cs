using UnityEngine;

public class DamageCollider : MonoBehaviour 
{
    [SerializeField] private Rigidbody ColliderRigidbody;

    private float damage = 25f;
    private float MinForce = 0.1f;
    private float LastRelativeVelocity = 0;
    private float LastDamageForce = 0;
    private float CollisionDamage = 5;

    private bool TakeCollisionDamage = false;

    private Damageable thisDamageable;

    private void Start() 
    {
        if (ColliderRigidbody == null) 
        {
            ColliderRigidbody = GetComponent<Rigidbody>();
        }

        thisDamageable = GetComponent<Damageable>();
    }

    private void OnCollisionEnter(Collision collision) 
    {

        if(!this.isActiveAndEnabled) 
        {
            return;
        }

        OnCollisionEvent(collision);
    }

    public virtual void OnCollisionEvent(Collision collision) {
        LastDamageForce = collision.impulse.magnitude;
        LastRelativeVelocity = collision.relativeVelocity.magnitude;

        if (LastDamageForce >= MinForce) 
        {
            Damageable damageable = collision.gameObject.GetComponent<Damageable>();
            if (damageable) 
            {
                damageable.DealDamage(damage, collision.GetContact(0).point, collision.GetContact(0).normal, true, gameObject, collision.gameObject);
            }
            else if (TakeCollisionDamage && thisDamageable != null) 
            {
                thisDamageable.DealDamage(CollisionDamage, collision.GetContact(0).point, collision.GetContact(0).normal, true, gameObject, collision.gameObject);
            }
        }

        BodyPart hitPart = collision.collider.GetComponent<BodyPart>();
        if (hitPart != null)
        {
            // If it's a body part, deal damage specifically to that part.
            hitPart.TakeDamage(CollisionDamage);
        }
    }
}