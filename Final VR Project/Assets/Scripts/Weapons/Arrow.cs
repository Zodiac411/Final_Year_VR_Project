using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public Collider ShaftCollider;
    
    [SerializeField] private Projectile ProjectileObject;
    [SerializeField] private Coroutine queueDestroy;
    
    [SerializeField] private float arrowDamage;

    private Rigidbody rb;
    private Grabbable grab;
    private AudioSource impactSound;

    private float ZVel = 0;
    private float flightTime = 0f;
    private float destroyTime = 10f;

    private bool Flying = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        impactSound = GetComponent<AudioSource>();
        ShaftCollider = GetComponent<Collider>();
        grab = GetComponent<Grabbable>();

        if (ProjectileObject == null)
        {
            ProjectileObject = gameObject.AddComponent<Projectile>();
            ProjectileObject.Damage = 50;
            ProjectileObject.StickToObject = true;
            ProjectileObject.enabled = false;
        }

        arrowDamage = ProjectileObject.Damage;
    }

    private void FixedUpdate()
    {
        bool beingHeld = grab != null && grab.BeingHeld;

        if (!beingHeld && rb != null && rb.velocity != Vector3.zero && Flying && ZVel > 0.02)
        {
            rb.rotation = Quaternion.LookRotation(rb.velocity);
        }

        ZVel = transform.InverseTransformDirection(rb.velocity).z;

        if (Flying)
        {
            flightTime += Time.fixedDeltaTime;
        }

        if (queueDestroy != null && grab != null && grab.BeingHeld)
        {
            StopCoroutine(queueDestroy);
        }
    }

    public void ShootArrow(Vector3 shotForce)
    {
        flightTime = 0f;
        Flying = true;

        transform.parent = null;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.None;
        rb.AddForce(shotForce, ForceMode.VelocityChange);
        StartCoroutine(ReEnableCollider());
        queueDestroy = StartCoroutine(QueueDestroy());
    }

    private IEnumerator QueueDestroy()
    {
        yield return new WaitForSeconds(destroyTime);

        if (grab != null && !grab.BeingHeld && transform.parent == null)
        {
            Destroy(this.gameObject);
        }
    }

    private IEnumerator ReEnableCollider()
    {
        int waitFrames = 3;
        for (int x = 0; x < waitFrames; x++)
        {
            yield return new WaitForFixedUpdate();
        }

        ShaftCollider.enabled = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (transform.parent != null && collision.transform == transform.parent)
        {
            return;
        }

        if (grab != null && grab.BeingHeld)
        {
            return;
        }

        if (collision.collider.isTrigger)
        {
            return;
        }

        string colNameLower = collision.transform.name.ToLower();

        if (flightTime < 1 && (colNameLower.Contains("arrow") || colNameLower.Contains("bow")))
        {
            Physics.IgnoreCollision(collision.collider, ShaftCollider, true);
            return;
        }

        if (flightTime < 1 && collision.transform.name.ToLower().Contains("player"))
        {
            Physics.IgnoreCollision(collision.collider, ShaftCollider, true);
            return;
        }

        float zVel = System.Math.Abs(transform.InverseTransformDirection(rb.velocity).z);
        bool doStick = true;

        if (zVel > 0.02f && !rb.isKinematic)
        {
            Damageable d = collision.gameObject.GetComponent<Damageable>();
            if (d)
            {
                d.DealDamage(arrowDamage, collision.GetContact(0).point, collision.GetContact(0).normal, true, gameObject, collision.collider.gameObject);
            }

            if (d != null && d.Health <= 0)
            {
                doStick = false;
            }
        }

        if (!rb.isKinematic && Flying)
        {
            if (zVel > 0.02f)
            {
                if (grab != null && grab.BeingHeld)
                {
                    grab.DropItem(false, false);
                }
                if (doStick)
                {
                    tryStickArrow(collision);
                }

                Flying = false;

                playSoundInterval(2.462f, 2.68f);
            }
        }
    }

    private void tryStickArrow(Collision collision)
    {
        Rigidbody colRigid = collision.collider.GetComponent<Rigidbody>();
        transform.parent = null;

        if (collision.gameObject.isStatic)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.isKinematic = true;
        }
        else if (colRigid != null && !colRigid.isKinematic)
        {
            FixedJoint joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = colRigid;
            joint.enableCollision = false;
            joint.breakForce = float.MaxValue;
            joint.breakTorque = float.MaxValue;
        }
        else if (colRigid != null && colRigid.isKinematic && collision.transform.localScale == Vector3.one)
        {
            transform.SetParent(collision.transform);
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.WakeUp();
        }
        else
        {
            if (collision.transform.localScale == Vector3.one)
            {
                transform.SetParent(collision.transform);
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.useGravity = false;
                rb.isKinematic = true;
            }
        }
    }

    private void playSoundInterval(float fromSeconds, float toSeconds)
    {
        if (impactSound)
        {

            if (impactSound.isPlaying)
            {
                impactSound.Stop();
            }

            impactSound.time = fromSeconds;
            impactSound.pitch = Time.timeScale;
            impactSound.Play();
            impactSound.SetScheduledEndTime(AudioSettings.dspTime + (toSeconds - fromSeconds));
        }
    }
}