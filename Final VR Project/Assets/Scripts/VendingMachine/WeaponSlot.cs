using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WeaponSlot : MonoBehaviour
{
    [SerializeField] private List<WeaponOffer> weaponOffers = new List<WeaponOffer>();
    [FormerlySerializedAs("weaponEntries")]
    [SerializeField] private List<WeaponEntry> legacyWeaponEntries;

    private readonly IWeaponSelectionStrategy weaponSelectionStrategy = new RandomWeaponSelectionStrategy();
    private readonly IWeaponSpawnFactory weaponSpawnFactory = new DefaultWeaponSpawnFactory();

    private readonly Dictionary<string, WeaponOffer> offerById = new Dictionary<string, WeaponOffer>();
    public string selectedWeaponName;
    [HideInInspector] public WeaponOffer selectedOffer;

    private void Awake()
    {
        BuildOfferLookup();
    }

    private void BuildOfferLookup()
    {
        offerById.Clear();

        foreach (WeaponOffer offer in weaponOffers)
        {
            if (offer == null || string.IsNullOrEmpty(offer.offerId))
            {
                continue;
            }

            if (!offerById.ContainsKey(offer.offerId))
            {
                offerById.Add(offer.offerId, offer);
            }
        }

        if (offerById.Count == 0 && legacyWeaponEntries != null)
        {
            foreach (WeaponEntry entry in legacyWeaponEntries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.weaponName))
                {
                    continue;
                }

                WeaponOffer runtimeOffer = ScriptableObject.CreateInstance<WeaponOffer>();
                runtimeOffer.offerId = entry.weaponName;
                runtimeOffer.displayName = entry.weaponName;
                runtimeOffer.price = entry.weaponDetails.price;
                runtimeOffer.weaponMesh = entry.weaponDetails.weaponMesh;
                runtimeOffer.weaponPrefab = entry.weaponDetails.weaponPrefab;
                offerById.Add(runtimeOffer.offerId, runtimeOffer);
            }
        }
    }

    public void HideWeapons()
    {
        foreach (WeaponOffer offer in offerById.Values)
        {
            if (offer.weaponMesh != null)
            {
                offer.weaponMesh.SetActive(false);
            }
        }
    }

    public int GetPrice()
    {
        return selectedOffer != null ? selectedOffer.price : -1;
    }

    public void RandomizeWeapon()
    {
        List<WeaponOffer> offers = new List<WeaponOffer>(offerById.Values);
        if (offers.Count == 0)
        {
            Debug.LogError("No weapon offers are available to select from.");
            return;
        }

        selectedOffer = weaponSelectionStrategy.SelectOffer(offers);
        if (selectedOffer == null)
        {
            Debug.LogError("Weapon selection strategy returned null.");
            return;
        }

        selectedWeaponName = selectedOffer.displayName;
        HideWeapons();

        if (selectedOffer.weaponMesh != null)
        {
            selectedOffer.weaponMesh.SetActive(true);
        }

        VendingMachine.Instance.UpdateUIForSlot(this);
    }

    public void VendWeapon(GameObject spawner)
    {
        if (selectedOffer == null || selectedOffer.weaponPrefab == null)
        {
            Debug.LogError("Selected weapon offer is invalid.");
            return;
        }

        if (selectedOffer.weaponMesh != null)
        {
            selectedOffer.weaponMesh.SetActive(false);
            Rigidbody rb = selectedOffer.weaponMesh.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }

        StartCoroutine(SpawnWeaponPrefab(selectedOffer.weaponPrefab, spawner));
    }

    private IEnumerator SpawnWeaponPrefab(GameObject prefab, GameObject spawner)
    {
        yield return new WaitForSeconds(1);
        weaponSpawnFactory.Spawn(prefab, spawner.transform.position, spawner.transform.rotation, spawner.transform);
    }
}

public interface IWeaponSelectionStrategy
{
    WeaponEntry Select(IReadOnlyList<WeaponEntry> weaponEntries);
    WeaponOffer SelectOffer(IReadOnlyList<WeaponOffer> offers);
}

public sealed class RandomWeaponSelectionStrategy : IWeaponSelectionStrategy
{
    public WeaponEntry Select(IReadOnlyList<WeaponEntry> weaponEntries)
    {
        if (weaponEntries == null || weaponEntries.Count == 0)
        {
            return null;
        }

        return weaponEntries[Random.Range(0, weaponEntries.Count)];
    }

    public WeaponOffer SelectOffer(IReadOnlyList<WeaponOffer> offers)
    {
        if (offers == null || offers.Count == 0)
        {
            return null;
        }

        return offers[Random.Range(0, offers.Count)];
    }
}

[System.Serializable]
public class WeaponDetails
{
    public GameObject weaponMesh;
    public int price;
    public GameObject weaponPrefab;

    public WeaponDetails(GameObject mesh, int weaponPrice, GameObject prefab)
    {
        weaponMesh = mesh;
        price = weaponPrice;
        weaponPrefab = prefab;
    }
}

[System.Serializable]
public class WeaponEntry
{
    public string weaponName;
    public WeaponDetails weaponDetails;

    public WeaponEntry(string name, WeaponDetails details)
    {
        weaponName = name;
        weaponDetails = details;
    }
}
