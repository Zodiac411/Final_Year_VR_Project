using UnityEngine;

[CreateAssetMenu(fileName = "WeaponOffer", menuName = "Vending/Weapon Offer")]
public class WeaponOffer : ScriptableObject
{
    public string offerId;
    public string displayName;
    public int price;
    public GameObject weaponMesh;
    public GameObject weaponPrefab;
}
