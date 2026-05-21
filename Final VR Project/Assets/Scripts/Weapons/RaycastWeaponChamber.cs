using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class RaycastWeaponChamber : MonoBehaviour
{
    [FormerlySerializedAs("ReloadMethod")]
    [SerializeField] private ReloadType reloadMethod = ReloadType.InfiniteAmmo;
    [FormerlySerializedAs("MaxInternalAmmo")]
    [SerializeField] private float maxInternalAmmo = 10f;
    [SerializeField] private float internalAmmo;
    [FormerlySerializedAs("AutoChamberRounds")]
    [SerializeField] private bool autoChamberRounds = true;
    [FormerlySerializedAs("MustChamberRounds")]
    [SerializeField] private bool mustChamberRounds = false;
    [SerializeField] private bool forceSlideBackOnLastShot = true;
    [SerializeField] private bool emptyBulletInChamber = false;
    [SerializeField] private Transform chamberedBullet;
    [SerializeField] private Transform slideTransform;
    [SerializeField] private List<GrabbedControllerBinding> ejectInput =
        new List<GrabbedControllerBinding>() { GrabbedControllerBinding.Button2Down };
    [SerializeField] private List<GrabbedControllerBinding> releaseSlideInput =
        new List<GrabbedControllerBinding>() { GrabbedControllerBinding.Button1Down };
    [SerializeField] private List<GrabbedControllerBinding> reloadInput =
        new List<GrabbedControllerBinding>() { GrabbedControllerBinding.Button2Down };
    [SerializeField] private UnityEvent onAttachedAmmoEvent;
    [SerializeField] private UnityEvent onDetachedAmmoEvent;
    [SerializeField] private UnityEvent onWeaponChargedEvent;

    public bool BulletInChamber { get; set; }
    public bool SlideForcedBack { get; set; }

    private WeaponSlide weaponSlide;
    private RaycastWeapon weapon;

    private void Awake()
    {
        weapon = GetComponent<RaycastWeapon>();
        weaponSlide = GetComponentInChildren<WeaponSlide>();
    }

    public void Configure(WeaponDefinition definition)
    {
        if (definition == null)
        {
            return;
        }

        reloadMethod = definition.reloadMethod;
        maxInternalAmmo = definition.maxInternalAmmo;
        autoChamberRounds = definition.autoChamberRounds;
        mustChamberRounds = definition.mustChamberRounds;
    }

    public void TickInputs()
    {
        if (weapon.IsAnyInputBindingPressed(releaseSlideInput))
        {
            weaponSlide?.UnlockBack();
        }

        if (weapon.IsAnyInputBindingPressed(ejectInput))
        {
            GetComponentInChildren<MagazineSlide>()?.EjectMagazine();
        }

        if (reloadMethod == ReloadType.InternalAmmo && weapon.IsAnyInputBindingPressed(reloadInput))
        {
            Reload();
        }

        UpdateChamberedBullet();
    }

    public bool CanShoot(out bool shouldPlayEmptySound)
    {
        shouldPlayEmptySound = false;

        if (!BulletInChamber && mustChamberRounds)
        {
            shouldPlayEmptySound = true;
            return false;
        }

        if (weaponSlide != null && weaponSlide.LockedBack)
        {
            return false;
        }

        return true;
    }

    public void AfterShot()
    {
        BulletInChamber = false;

        if (autoChamberRounds)
        {
            ChamberRound();
        }
        else
        {
            emptyBulletInChamber = true;
        }

        if (!BulletInChamber)
        {
            SlideForcedBack = forceSlideBackOnLastShot;
            if (SlideForcedBack && weaponSlide != null)
            {
                weaponSlide.LockBack();
            }
        }
    }

    public void Reload()
    {
        internalAmmo = maxInternalAmmo;
    }

    public int GetBulletCount()
    {
        switch (reloadMethod)
        {
            case ReloadType.InfiniteAmmo:
                return 9999;
            case ReloadType.InternalAmmo:
                return (int)internalAmmo;
            default:
                return GetComponentsInChildren<Bullet>(false).Length;
        }
    }

    public void RemoveBullet()
    {
        if (reloadMethod == ReloadType.InfiniteAmmo)
        {
            return;
        }

        if (reloadMethod == ReloadType.InternalAmmo)
        {
            internalAmmo--;
            return;
        }

        Bullet firstBullet = GetComponentInChildren<Bullet>(false);
        if (firstBullet != null)
        {
            Destroy(firstBullet.gameObject);
        }
    }

    public void ChamberRound()
    {
        if (GetBulletCount() > 0)
        {
            RemoveBullet();
            BulletInChamber = true;
        }
        else
        {
            BulletInChamber = false;
        }

        UpdateChamberedBullet();
    }

    public void OnWeaponCharged(bool allowCasingEject, RaycastWeaponVfx vfx)
    {
        if (BulletInChamber && allowCasingEject)
        {
            vfx.EjectCasing();
        }
        else if (emptyBulletInChamber && allowCasingEject)
        {
            vfx.EjectCasing();
            emptyBulletInChamber = false;
        }

        ChamberRound();
        SlideForcedBack = false;
        onWeaponChargedEvent?.Invoke();
    }

    public void OnAttachedAmmo()
    {
        UpdateChamberedBullet();
        onAttachedAmmoEvent?.Invoke();
    }

    public void OnDetachedAmmo()
    {
        UpdateChamberedBullet();
        onDetachedAmmoEvent?.Invoke();
    }

    private void UpdateChamberedBullet()
    {
        if (chamberedBullet != null)
        {
            chamberedBullet.gameObject.SetActive(BulletInChamber || emptyBulletInChamber);
        }
    }
}
