using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public float Health = 100;
    private float maxHealth;

    [SerializeField] private float SelfDestructDelay = 0.1f;
    private bool SelfDestruct = false;

    [SerializeField] private GameObject SpawnOnDeath;

    [SerializeField] private List<GameObject> ActivateGameObjectsOnDeath;
    [SerializeField] private List<GameObject> DeactivateGameObjectsOnDeath;
    [SerializeField] private List<Collider> DeactivateCollidersOnDeath;

    [SerializeField] private bool DestroyOnDeath = true;
    [SerializeField] private bool DropOnDeath = true;

    [SerializeField] private float DestroyDelay = 0f;
    [SerializeField] private bool Respawn = false;

    [SerializeField] private float RespawnTime = 10f;

    public bool RemoveBulletHolesOnDeath = true;

    [Header("Events")]
    public FloatEvent onDamaged;
    public UnityEvent onDestroyed;
    public UnityEvent onRespawn;

    Rigidbody rb;

    bool destroyed = false;
    bool initialWasKinematic;

    private void Start()
    {
        maxHealth = Health;

        rb = GetComponent<Rigidbody>();
        if (rb)
        {
            initialWasKinematic = rb.isKinematic;
        }
    }

    void OnEnable()
    {
        if (SelfDestruct)
        {
            Invoke("DestroyThis", SelfDestructDelay);
        }
    }

    public virtual void DealDamage(float damageAmount)
    {
        DealDamage(damageAmount, transform.position);
    }

    public virtual void DealDamage(float damageAmount, Vector3? hitPosition = null, Vector3? hitNormal = null, bool reactToHit = true, GameObject sender = null, GameObject receiver = null)
    {

        if (destroyed)
        {
            return;
        }

        Health -= damageAmount;

        onDamaged?.Invoke(damageAmount);

        if (Health <= 0)
        {
            DestroyThis();
        }
    }

    public virtual void DestroyThis()
    {
        Health = 0;
        destroyed = true;

        foreach (var go in ActivateGameObjectsOnDeath)
        {
            go.SetActive(true);
        }

        foreach (var go in DeactivateGameObjectsOnDeath)
        {
            go.SetActive(false);
        }

        foreach (var col in DeactivateCollidersOnDeath)
        {
            col.enabled = false;
        }

        if (SpawnOnDeath != null)
        {
            var go = GameObject.Instantiate(SpawnOnDeath);
            go.transform.position = transform.position;
            go.transform.rotation = transform.rotation;
        }

        if (rb)
        {
            rb.isKinematic = true;
        }

        if (onDestroyed != null)
        {
            onDestroyed.Invoke();
        }

        if (DestroyOnDeath)
        {
            Destroy(this.gameObject, DestroyDelay);
        }
        else if (Respawn)
        {
            StartCoroutine(RespawnRoutine(RespawnTime));
        }

        Grabbable grab = GetComponent<Grabbable>();
        if (DropOnDeath && grab != null && grab.BeingHeld)
        {
            grab.DropItem(false, true);
        }

        if (RemoveBulletHolesOnDeath)
        {
            BulletHole[] holes = GetComponentsInChildren<BulletHole>();
            foreach (var hole in holes)
            {
                GameObject.Destroy(hole.gameObject);
            }

            Transform decal = transform.Find("Decal");
            if (decal)
            {
                GameObject.Destroy(decal.gameObject);
            }
        }
    }

    private IEnumerator RespawnRoutine(float seconds)
    {

        yield return new WaitForSeconds(seconds);

        Health = maxHealth;
        destroyed = false;

        foreach (var go in ActivateGameObjectsOnDeath)
        {
            go.SetActive(false);
        }

        foreach (var go in DeactivateGameObjectsOnDeath)
        {
            go.SetActive(true);
        }
        foreach (var col in DeactivateCollidersOnDeath)
        {
            col.enabled = true;
        }

        if (rb)
        {
            rb.isKinematic = initialWasKinematic;
        }

        if (onRespawn != null)
        {
            onRespawn.Invoke();
        }
    }
}