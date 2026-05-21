using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class RaycastWeaponDamage : MonoBehaviour
{
    [FormerlySerializedAs("damage")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float bulletImpactForce = 1000f;
    [SerializeField] private FloatEvent onDealtDamageEvent;
    [SerializeField] private RaycastHitEvent onRaycastHitEvent;
    private RaycastWeaponVfx vfx;
    private Transform muzzlePoint;

    private void Awake()
    {
        vfx = GetComponent<RaycastWeaponVfx>();
    }

    public float Damage => damage;

    public void Configure(WeaponDefinition definition, Transform muzzle)
    {
        if (definition != null)
        {
            damage = definition.damage;
            bulletImpactForce = definition.bulletImpactForce;
        }

        muzzlePoint = muzzle;
    }

    public void OnRaycastHit(RaycastHit hit, GameObject source)
    {
        vfx?.ApplyParticleFX(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), hit.collider);

        Rigidbody hitRigid = hit.collider.attachedRigidbody;
        if (hitRigid != null && muzzlePoint != null)
        {
            hitRigid.AddForceAtPosition(bulletImpactForce * muzzlePoint.forward, hit.point);
        }

        float multiplier = 1f;
        BodyPart bodyPart = hit.collider.GetComponent<BodyPart>();
        if (bodyPart != null)
        {
            multiplier = bodyPart.DamageMultiplier;
        }

        IDamageable damageable = hit.collider.GetComponent<IDamageable>()
            ?? hit.collider.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            float finalDamage = damage * multiplier;
            DamageContext context = new DamageContext(
                finalDamage,
                source,
                hit.point,
                hit.normal,
                1f);

            damageable.ApplyDamage(in context);
            onDealtDamageEvent?.Invoke(finalDamage);
        }

        onRaycastHitEvent?.Invoke(hit);
    }
}
