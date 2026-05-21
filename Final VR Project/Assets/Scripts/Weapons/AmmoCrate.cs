using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine;

public class AmmoCrate : MonoBehaviour
{
    private Grabbable g;

    [System.Serializable]
    public class AmmoReward
    {
        public string ammoType;
        [Range(0, 20)] public int minAmount;
        [Range(0, 20)] public int maxAmount;

        public AmmoReward(string type, int min, int max)
        {
            ammoType = type;
            minAmount = min;
            maxAmount = max;
        }

    }

    [SerializeField] private List<AmmoReward> pr = new List<AmmoReward>();
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private Transform center;

    private void Awake()
    {
        g = GetComponent<Grabbable>();
    }

    private void Update()
    {
        if (g.BeingHeld)
        {
            GrantAmmo();
            PlayDisappearVFX();
            AudioManager.instance.PlayAudios("Ammo Crate Pickup", center.position);
            Destroy(gameObject);
        }
    }

    private void GrantAmmo()
    {
        AmmoDispenser ammoInven = FindObjectOfType<AmmoDispenser>();
        if (ammoInven == null)
        {
            Debug.LogError("AmmoDispenser not found in the scene.");
            return;
        }

        foreach (var r in pr)
        {
            int amount = Random.Range(r.minAmount, r.maxAmount + 1);
            for (int i = 0; i < amount; i++)
            {
                ammoInven.AddAmmo(r.ammoType);
            }
        }
    }

    private void PlayDisappearVFX()
    {
        if (vfxPrefab != null)
        {
            Instantiate(vfxPrefab, center.position, center.rotation);
        }
    }
}
