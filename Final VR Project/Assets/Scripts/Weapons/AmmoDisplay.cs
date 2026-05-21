using UnityEngine;
using UnityEngine.UI;

public class AmmoDisplay : MonoBehaviour
{
    [SerializeField] private RaycastWeapon Weapon;
    [SerializeField] private Text AmmoLabel;

    private void OnGUI()
    {
        string loadedShot = Weapon.BulletInChamber ? "1" : "0";
        int currentAmmoCount = Weapon.GetBulletCount();
        AmmoLabel.text = loadedShot + " / " + currentAmmoCount;

        if (currentAmmoCount == 0)
        {
            AmmoLabel.color = Color.red;
        }
        else if (currentAmmoCount > 0 && currentAmmoCount < Weapon.MaxInternalAmmo)
        {
            AmmoLabel.color = Color.yellow;
        }
        else
        {
            AmmoLabel.color = Color.white;
        }
    }
}

