using UnityEngine;

public class DamageCollider : MonoBehaviour 
{
    [SerializeField] private Rigidbody ColliderRigidbody;

    private float damage = 25f;

    public void Configure(WeaponDefinition definition)
    {
        if (definition != null)
        {
            damage = definition.damage;
        }
    }
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
            Vector3 contactPoint = collision.GetContact(0).point;
            Vector3 contactNormal = collision.GetContact(0).normal;

            float multiplier = 1f;
            BodyPart hitPart = collision.collider.GetComponent<BodyPart>();
            if (hitPart != null)
            {
                multiplier = hitPart.DamageMultiplier;
            }

            IDamageable target = collision.collider.GetComponent<IDamageable>()
                ?? collision.collider.GetComponentInParent<IDamageable>();

            if (target != null)
            {
                DamageContext context = new DamageContext(
                    damage * multiplier,
                    gameObject,
                    contactPoint,
                    contactNormal,
                    1f);
                target.ApplyDamage(in context);
            }
            else if (TakeCollisionDamage && thisDamageable != null)
            {
                thisDamageable.DealDamage(CollisionDamage, contactPoint, contactNormal, true, gameObject, collision.gameObject);
            }
        }
    }
}