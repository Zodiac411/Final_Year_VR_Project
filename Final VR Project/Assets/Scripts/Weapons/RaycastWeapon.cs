using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(RaycastWeaponTrigger))]
[RequireComponent(typeof(RaycastWeaponChamber))]
[RequireComponent(typeof(RaycastWeaponRecoil))]
[RequireComponent(typeof(RaycastWeaponVfx))]
[RequireComponent(typeof(RaycastWeaponDamage))]
public class RaycastWeapon : GrabbableEvents
{
    [SerializeField] private WeaponDefinition weaponDefinition;

    [Header("Damage")]
    [FormerlySerializedAs("MaxRange")]
    [SerializeField] private float maxRange = 25f;

    [Header("Select Fire")]
    [FormerlySerializedAs("FiringMethod")]
    [SerializeField] private FiringType firingMethod = FiringType.Semi;
    [FormerlySerializedAs("ReloadMethod")]
    [SerializeField] private ReloadType reloadMethod = ReloadType.InfiniteAmmo;

    [FormerlySerializedAs("MaxInternalAmmo")]
    public float maxInternalAmmo = 10;
    [FormerlySerializedAs("FiringRate")]
    [SerializeField] private float firingRate = 0.2f;
    [FormerlySerializedAs("ShotForce")]
    [SerializeField] private float shotForce = 10f;
    [FormerlySerializedAs("AutoChamberRounds")]
    [SerializeField] private bool autoChamberRounds = true;
    [FormerlySerializedAs("MustChamberRounds")]
    [SerializeField] private bool mustChamberRounds = false;
    [FormerlySerializedAs("AlwaysFireProjectile")]
    [SerializeField] private bool alwaysFireProjectile = false;
    [FormerlySerializedAs("FireProjectileInSlowMo")]
    [SerializeField] private bool fireProjectileInSlowMo = true;
    [FormerlySerializedAs("SlowMoRateOfFire")]
    [SerializeField] private float slowMoRateOfFire = 0.3f;
    [FormerlySerializedAs("LaserGuided")]
    [SerializeField] private bool laserGuided = false;
    [FormerlySerializedAs("LaserPoint")]
    [SerializeField] private Transform laserPoint;
    [FormerlySerializedAs("ValidLayers")]
    [SerializeField] private LayerMask validLayers;
    [FormerlySerializedAs("MuzzlePointTransform")]
    [SerializeField] private Transform muzzlePointTransform;
    [FormerlySerializedAs("TriggerTransform")]
    [SerializeField] private Transform triggerTransform;
    [FormerlySerializedAs("GunShotSound")]
    [SerializeField] private AudioClip gunShotSound;
    [FormerlySerializedAs("GunShotVolume")]
    [SerializeField] private float gunShotVolume = 0.75f;
    [FormerlySerializedAs("EmptySound")]
    [SerializeField] private AudioClip emptySound;
    [FormerlySerializedAs("EmptySoundVolume")]
    [SerializeField] private float emptySoundVolume = 1f;
    [FormerlySerializedAs("ProjectilePrefab")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private UnityEvent onShootEvent;

    private RaycastWeaponTrigger trigger;
    private RaycastWeaponChamber chamber;
    private RaycastWeaponRecoil recoil;
    private RaycastWeaponVfx vfx;
    private RaycastWeaponDamage damage;

    private float lastShotTime;
    private IEnumerator shotRoutine;

    public bool BulletInChamber
    {
        get => chamber.BulletInChamber;
        set => chamber.BulletInChamber = value;
    }

    protected override void Awake()
    {
        base.Awake();
        trigger = GetComponent<RaycastWeaponTrigger>();
        chamber = GetComponent<RaycastWeaponChamber>();
        recoil = GetComponent<RaycastWeaponRecoil>();
        vfx = GetComponent<RaycastWeaponVfx>();
        damage = GetComponent<RaycastWeaponDamage>();
    }

    void Start()
    {
        ApplyDefinition();
        vfx.PrepareMuzzleFlash();
        chamber.ChamberRound();
    }

    public override void OnTrigger(float triggerValue)
    {
        trigger.OnTrigger(triggerValue);
        chamber.TickInputs();
        base.OnTrigger(triggerValue);
    }

    public void Shoot(ref bool playedEmptySound)
    {
        float shotInterval = Time.timeScale < 1 ? slowMoRateOfFire : firingRate;
        if (Time.time - lastShotTime < shotInterval)
        {
            return;
        }

        if (!chamber.CanShoot(out bool shouldPlayEmptySound))
        {
            if (shouldPlayEmptySound && !playedEmptySound)
            {
                XRManager.Instance.PlaySpatialClipAt(emptySound, transform.position, emptySoundVolume, 0.5f);
                playedEmptySound = true;
            }

            return;
        }

        XRManager.Instance.PlaySpatialClipAt(gunShotSound, transform.position, gunShotVolume);
        if (thisGrabber != null)
        {
            input.VibrateController(0.1f, 0.2f, 0.1f, thisGrabber.HandSide);
        }

        bool useProjectile = alwaysFireProjectile || (fireProjectileInSlowMo && Time.timeScale < 1);
        if (useProjectile)
        {
            FireProjectile();
        }
        else if (Physics.Raycast(muzzlePointTransform.position, muzzlePointTransform.forward, out RaycastHit hit, maxRange, validLayers, QueryTriggerInteraction.Ignore))
        {
            damage.OnRaycastHit(hit, gameObject);
        }

        recoil.ApplyRecoil();
        chamber.AfterShot();
        onShootEvent?.Invoke();
        lastShotTime = Time.time;

        if (shotRoutine != null)
        {
            StopCoroutine(shotRoutine);
        }

        shotRoutine = autoChamberRounds ? vfx.AnimateSlideAndEject(chamber.SlideForcedBack) : vfx.DoMuzzleFlash();
        StartCoroutine(shotRoutine);
    }

    public bool IsAnyInputBindingPressed(System.Collections.Generic.List<GrabbedControllerBinding> bindings)
    {
        for (int x = 0; x < bindings.Count; x++)
        {
            if (input.GetGrabbedControllerBinding(bindings[x], thisGrabber.HandSide))
            {
                return true;
            }
        }

        return false;
    }

    public virtual void OnAttachedAmmo() => chamber.OnAttachedAmmo();
    public virtual void OnDetachedAmmo() => chamber.OnDetachedAmmo();
    public virtual int GetBulletCount() => chamber.GetBulletCount();
    public virtual void RemoveBullet() => chamber.RemoveBullet();
    public virtual void Reload() => chamber.Reload();
    public virtual void OnWeaponCharged(bool allowCasingEject) => chamber.OnWeaponCharged(allowCasingEject, vfx);
    public virtual void UnlockSlide() => GetComponentInChildren<WeaponSlide>()?.UnlockBack();
    public virtual void EjectMagazine() => GetComponentInChildren<MagazineSlide>()?.EjectMagazine();
    public virtual void CheckReloadInput() { }

    private void ApplyDefinition()
    {
        if (weaponDefinition != null)
        {
            maxRange = weaponDefinition.maxRange;
            firingRate = weaponDefinition.firingRate;
            shotForce = weaponDefinition.shotForce;
            maxInternalAmmo = weaponDefinition.maxInternalAmmo;
            slowMoRateOfFire = weaponDefinition.slowMoRateOfFire;
            firingMethod = weaponDefinition.firingMethod;
            reloadMethod = weaponDefinition.reloadMethod;
            autoChamberRounds = weaponDefinition.autoChamberRounds;
            mustChamberRounds = weaponDefinition.mustChamberRounds;
            alwaysFireProjectile = weaponDefinition.alwaysFireProjectile;
            fireProjectileInSlowMo = weaponDefinition.fireProjectileInSlowMo;
        }

        trigger.Configure(weaponDefinition, triggerTransform);
        chamber.Configure(weaponDefinition);
        recoil.Configure(weaponDefinition, muzzlePointTransform);
        vfx.Configure(weaponDefinition);
        damage.Configure(weaponDefinition, muzzlePointTransform);

        DamageCollider damageCollider = GetComponentInChildren<DamageCollider>();
        if (damageCollider != null)
        {
            damageCollider.Configure(weaponDefinition);
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || muzzlePointTransform == null)
        {
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, muzzlePointTransform.position, muzzlePointTransform.rotation);
        if (projectile == null)
        {
            return;
        }

        Rigidbody projectileRigid = projectile.GetComponentInChildren<Rigidbody>();
        if (projectileRigid != null)
        {
            projectileRigid.AddForce(muzzlePointTransform.forward * shotForce, ForceMode.VelocityChange);
        }

        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null && !alwaysFireProjectile)
        {
            proj.MarkAsRaycastBullet();
        }

        if (proj != null && laserGuided)
        {
            if (laserPoint == null)
            {
                laserPoint = muzzlePointTransform;
            }

            proj.MarkAsLaserGuided(muzzlePointTransform);
        }

        Destroy(projectile, 20);
    }
}
