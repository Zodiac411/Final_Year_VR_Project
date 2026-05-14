using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Damage = 25;

    [SerializeField] private GameObject HitFXPrefab;
    [SerializeField] private Transform MuzzleOrigin;
    [SerializeField] private LayerMask ValidLayers;

    [SerializeField] private float MinForceHit = 0.02f;
    [SerializeField] private float AddRigidForce = 5;

    public bool IsLaserGuided = false;

    [SerializeField] private float MissileForce = 2f;
    [SerializeField] private float TurningSpeed = 1f;

    public bool StickToObject = false;

    [SerializeField] private UnityEvent onDealtDamageEvent;

    private bool _checkRaycast;

    Rigidbody rb;

    private Quaternion targetRotation;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEvent(collision);
    }

    private void Update()
    {
        if (IsLaserGuided)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, MuzzleOrigin.transform.rotation, Time.deltaTime * TurningSpeed);
        }
    }


    private void FixedUpdate()
    {
        if (IsLaserGuided)
        {
            rb.AddForce(transform.forward * MissileForce, ForceMode.Force);
        }
    }

    public virtual void OnCollisionEvent(Collision collision)
    {
        if (collision.collider.isTrigger)
        {
            return;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb && MinForceHit != 0)
        {
            float zVel = System.Math.Abs(transform.InverseTransformDirection(rb.velocity).z);

            if (zVel < MinForceHit)
            {
                return;
            }
        }

        Vector3 hitPosition = collision.contacts[0].point;
        Vector3 normal = collision.contacts[0].normal;
        Quaternion hitNormal = Quaternion.FromToRotation(Vector3.forward, normal);
        doHitEffects(hitPosition, hitNormal, collision.collider);

        Damageable dmgable = collision.collider.GetComponent<Damageable>();
        if (dmgable)
        {
            dmgable.DealDamage(Damage, hitPosition, normal, true, gameObject, collision.collider.gameObject);

            if (onDealtDamageEvent != null)
            {
                onDealtDamageEvent.Invoke();
            }
        }

        if (StickToObject)
        {
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public virtual void doHitEffects(Vector3 pos, Quaternion rot, Collider col)
    {
        if (HitFXPrefab)
        {
            GameObject impact = Instantiate(HitFXPrefab, pos, rot) as GameObject;
            BulletHole hole = impact.GetComponent<BulletHole>();
            if (hole)
            {
                hole.TryAttachTo(col);
            }
        }

        Rigidbody hitRigid = col.attachedRigidbody;
        if (hitRigid != null)
        {
            hitRigid.AddForceAtPosition(transform.forward * AddRigidForce, pos, ForceMode.VelocityChange);
        }
    }

    public virtual void MarkAsRaycastBullet()
    {
        _checkRaycast = true;
        StartCoroutine(CheckForRaycast());
    }

    public virtual void DoRayCastProjectile()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 25f, ValidLayers, QueryTriggerInteraction.Ignore))
        {
            Quaternion decalRotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);
            doHitEffects(hit.point, decalRotation, hit.collider);
        }

        _checkRaycast = false;
        Destroy(this.gameObject);
    }

    private IEnumerator CheckForRaycast()
    {
        while (this.gameObject.activeSelf && _checkRaycast)
        {
            if (Time.timeScale >= 1)
            {
                DoRayCastProjectile();
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void MarkAsLaserGuided(Transform startingOrigin)
    {

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }

        IsLaserGuided = true;

        MuzzleOrigin = startingOrigin;
    }
}