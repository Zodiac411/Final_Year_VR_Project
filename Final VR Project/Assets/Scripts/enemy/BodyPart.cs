using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] BodyPart[] parts;
    [SerializeField] GameObject Limb;
    [SerializeField] float partHealth;
    [SerializeField] private GameObject headVFX; 
    [SerializeField] private bool isHeadPart;
    [SerializeField] private Transform neckTransform;

    public float DamageMultiplier => 2f;

    private ZombieHealth zombieHealth;

    void Start()
    {
        zombieHealth = GetComponentInParent<ZombieHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [System.Obsolete("Use IDamageable raycast damage instead.")]
    public void TakeDamage(float amount)
    {
        partHealth -= amount / DamageMultiplier;
        if (partHealth <= 0f && Limb != null)
        {
            hitBodyPart();
        }

        if (zombieHealth != null)
        {
            zombieHealth.ApplyDamage(new DamageContext(amount, null, transform.position, Vector3.up, 1f));
        }
    }




    public void hitBodyPart()
    {
        if (isHeadPart)
        {
            if (headVFX != null)
            {
                GameObject vfxInstance = Instantiate(headVFX, neckTransform.position, neckTransform.rotation);
                vfxInstance.transform.SetParent(neckTransform);
            }
            else
            {
                Debug.LogError("Head VFX prefab is not assigned.");
            }
        }

        if (parts.Length > 0)
        {
            foreach (BodyPart part in parts)
            {
                if (part != null)
                {
                    part.hitBodyPart();
                }
            }
        }

        if (Limb != null)
        {
            GameObject newLimb = Instantiate(Limb, transform.position, transform.rotation);
            newLimb.AddComponent<Rigidbody>();
            newLimb.AddComponent<BoxCollider>();
        }

        transform.localScale = Vector3.zero;
        Destroy(this);
    }

}
