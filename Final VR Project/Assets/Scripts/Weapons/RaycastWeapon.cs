using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public enum FiringType
{
    Semi,
    Automatic
}

public enum ReloadType
{
    InfiniteAmmo,
    ManualClip,
    InternalAmmo
}

public class RaycastWeapon : GrabbableEvents
{
    [Header("Damage")]
    [SerializeField] private float MaxRange = 25f;
    [SerializeField] private float Damage = 25f;

    [Header("Select Fire")]
    [SerializeField] private FiringType FiringMethod = FiringType.Semi;
    [SerializeField] private ReloadType ReloadMethod = ReloadType.InfiniteAmmo;

    public float MaxInternalAmmo = 10;
    [SerializeField] private float FiringRate = 0.2f;
    [SerializeField] private float BulletImpactForce = 1000f;
    [SerializeField] private float InternalAmmo = 0;
    [SerializeField] private bool AutoChamberRounds = true;
    [SerializeField] private bool MustChamberRounds = false;
    [SerializeField] private bool AlwaysFireProjectile = false;
    [SerializeField] private bool FireProjectileInSlowMo = true;

    [SerializeField] private float SlowMoRateOfFire = 0.3f;
    [SerializeField] private float ShotForce = 10f;
    [SerializeField] private float BulletCasingForce = 3f;

    [SerializeField] private bool LaserGuided = false;

    [SerializeField] private Transform LaserPoint;
    [SerializeField] private Vector3 RecoilForce = Vector3.zero;

    [SerializeField] private float RecoilDuration = 0.3f;

    private Rigidbody weaponRigid;

    [SerializeField] private LayerMask ValidLayers;

    [SerializeField] private Transform MuzzlePointTransform;
    [SerializeField] private Transform EjectPointTransform;
    [SerializeField] private Transform TriggerTransform;
    [SerializeField] private Transform SlideTransform;

    [SerializeField] private Transform ChamberedBullet;

    [Header("Muzzle Flash")]
    [SerializeField] private GameObject MuzzleFlashObject;

    [Header("Prefabs")]
    [SerializeField] private GameObject BulletCasingPrefab;
    [SerializeField] private GameObject ProjectilePrefab;
    [SerializeField] private GameObject HitFXPrefab;

    [SerializeField] private AudioClip GunShotSound;

    [Range(0.0f, 1f)]
    [SerializeField] private float GunShotVolume = 0.75f;
    [SerializeField] private float slideSpeed = 1;

    [SerializeField] private AudioClip EmptySound;

    [Range(0.0f, 1f)]
    [SerializeField] private float EmptySoundVolume = 1f;
    [SerializeField] private float SlideDistance = -0.028f;

    [SerializeField] private bool ForceSlideBackOnLastShot = true;
    [SerializeField] private bool EmptyBulletInChamber = false;

    private float minSlideDistance = 0.001f;

    [SerializeField] private List<GrabbedControllerBinding> EjectInput =
        new List<GrabbedControllerBinding>() { GrabbedControllerBinding.Button2Down };

    [SerializeField] private List<GrabbedControllerBinding> ReleaseSlideInput = 
        new List<GrabbedControllerBinding>() { GrabbedControllerBinding.Button1Down };

    [SerializeField] private List<GrabbedControllerBinding> ReloadInput = 
        new List<GrabbedControllerBinding>() { GrabbedControllerBinding.Button2Down };

    public bool BulletInChamber = false;

    [Header("Events")]
    [SerializeField] private UnityEvent onShootEvent;
    [SerializeField] private UnityEvent onAttachedAmmoEvent;
    [SerializeField] private UnityEvent onDetachedAmmoEvent;
    [SerializeField] private UnityEvent onWeaponChargedEvent;
    [SerializeField] private FloatEvent onDealtDamageEvent;
    [SerializeField] private RaycastHitEvent onRaycastHitEvent;

    private float lastShotTime;

    protected WeaponSlide ws;

    protected bool slideForcedBack = false;
    protected bool readyToShoot = true;

    void Start()
    {
        weaponRigid = GetComponent<Rigidbody>();

        if (MuzzleFlashObject)
        {
            MuzzleFlashObject.SetActive(false);
        }

        ws = GetComponentInChildren<WeaponSlide>();

        updateChamberedBullet();
    }

    public override void OnTrigger(float triggerValue)
    {
        triggerValue = Mathf.Clamp01(triggerValue);

        if (TriggerTransform)
        {
            TriggerTransform.localEulerAngles = new Vector3(triggerValue * 15, 0, 0);
        }

        if (triggerValue <= 0.5)
        {
            readyToShoot = true;
            playedEmptySound = false;
        }

        if (readyToShoot && triggerValue >= 0.75f)
        {
            Shoot();

            readyToShoot = FiringMethod == FiringType.Automatic;
        }

        checkSlideInput();
        checkEjectInput();
        CheckReloadInput();
        updateChamberedBullet();

        base.OnTrigger(triggerValue);
    }

    void checkSlideInput()
    {
        if (IsAnyInputBindingPressed(ReleaseSlideInput))
        {
            UnlockSlide();
        }
    }

    void checkEjectInput()
    {
        if (IsAnyInputBindingPressed(EjectInput))
        {
            EjectMagazine();
        }
    }

    public virtual void CheckReloadInput()
    {
        if (ReloadMethod == ReloadType.InternalAmmo)
        {
            if (IsAnyInputBindingPressed(ReloadInput))
            {
                Reload();
            }
        }
    }

    private bool IsAnyInputBindingPressed(List<GrabbedControllerBinding> bindings)
    {
        for (int x = 0; x < bindings.Count; x++)
        {
            if (XRInput.Instance.GetGrabbedControllerBinding(bindings[x], thisGrabber.HandSide))
            {
                return true;
            }
        }

        return false;
    }

    public virtual void UnlockSlide()
    {
        if (ws != null)
        {
            ws.UnlockBack();
        }
    }

    public virtual void EjectMagazine()
    {
        MagazineSlide ms = GetComponentInChildren<MagazineSlide>();
        if (ms != null)
        {
            ms.EjectMagazine();
        }
    }

    protected bool playedEmptySound = false;

    public virtual void Shoot()
    {

        float shotInterval = Time.timeScale < 1 ? SlowMoRateOfFire : FiringRate;
        if (Time.time - lastShotTime < shotInterval)
        {
            return;
        }

        if (!BulletInChamber && MustChamberRounds)
        {
            if (!playedEmptySound)
            {
                XRManager.Instance.PlaySpatialClipAt(EmptySound, transform.position, EmptySoundVolume, 0.5f);
                playedEmptySound = true;
            }

            return;
        }

        if (ws != null && ws.LockedBack)
        {
            XRManager.Instance.PlaySpatialClipAt(EmptySound, transform.position, EmptySoundVolume, 0.5f);
            return;
        }

        XRManager.Instance.PlaySpatialClipAt(GunShotSound, transform.position, GunShotVolume);

        if (thisGrabber != null)
        {
            input.VibrateController(0.1f, 0.2f, 0.1f, thisGrabber.HandSide);
        }

        bool useProjectile = AlwaysFireProjectile || (FireProjectileInSlowMo && Time.timeScale < 1);
        if (useProjectile)
        {
            GameObject projectile = Instantiate(ProjectilePrefab, MuzzlePointTransform.position, MuzzlePointTransform.rotation) as GameObject;
            Rigidbody projectileRigid = projectile.GetComponentInChildren<Rigidbody>();
            projectileRigid.AddForce(MuzzlePointTransform.forward * ShotForce, ForceMode.VelocityChange);

            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj && !AlwaysFireProjectile)
            {
                proj.MarkAsRaycastBullet();
            }

            if (proj && LaserGuided)
            {
                if (LaserPoint == null)
                {
                    LaserPoint = MuzzlePointTransform;
                }

                proj.MarkAsLaserGuided(MuzzlePointTransform);
            }

            Destroy(projectile, 20);
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(MuzzlePointTransform.position, MuzzlePointTransform.forward, out hit, MaxRange, ValidLayers, QueryTriggerInteraction.Ignore))
            {
                OnRaycastHit(hit);
            }
        }

        ApplyRecoil();
        BulletInChamber = false;

        if (AutoChamberRounds)
        {
            chamberRound();
        }
        else
        {
            EmptyBulletInChamber = true;
        }

        if (!BulletInChamber)
        {
            slideForcedBack = ForceSlideBackOnLastShot;

            if (slideForcedBack && ws != null)
            {
                ws.LockBack();
            }
        }

        if (onShootEvent != null)
        {
            onShootEvent.Invoke();
        }

        lastShotTime = Time.time;

        if (shotRoutine != null)
        {
            MuzzleFlashObject.SetActive(false);
            StopCoroutine(shotRoutine);
        }

        if (AutoChamberRounds)
        {
            shotRoutine = animateSlideAndEject();
            StartCoroutine(shotRoutine);
        }
        else
        {
            shotRoutine = doMuzzleFlash();
            StartCoroutine(shotRoutine);
        }
    }

    public virtual void ApplyRecoil()
    {
        if (weaponRigid != null && RecoilForce != Vector3.zero)
        {
            grab.RequestSpringTime(RecoilDuration);
            weaponRigid.AddForceAtPosition(MuzzlePointTransform.TransformDirection(RecoilForce), MuzzlePointTransform.position, ForceMode.VelocityChange);
        }
    }

    public virtual void OnRaycastHit(RaycastHit hit)
    {
        ApplyParticleFX(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), hit.collider);

        Rigidbody hitRigid = hit.collider.attachedRigidbody;
        if (hitRigid != null)
        {
            hitRigid.AddForceAtPosition(BulletImpactForce * MuzzlePointTransform.forward, hit.point);
        }

        Damageable d = hit.collider.GetComponent<Damageable>();
        if (d)
        {
            d.DealDamage(Damage, hit.point, hit.normal, true, gameObject, hit.collider.gameObject);

            if (onDealtDamageEvent != null)
            {
                onDealtDamageEvent.Invoke(Damage);
            }
        }

        if (onRaycastHitEvent != null)
        {
            onRaycastHitEvent.Invoke(hit);
        }
        BodyPart hitPart = hit.collider.GetComponent<BodyPart>();
        if (hitPart != null)
        {
            // If it's a body part, deal damage specifically to that part.
            hitPart.TakeDamage(Damage);
        }
    }

    public virtual void ApplyParticleFX(Vector3 position, Quaternion rotation, Collider attachTo)
    {
        if (HitFXPrefab)
        {
            GameObject impact = Instantiate(HitFXPrefab, position, rotation) as GameObject;

            BulletHole hole = impact.GetComponent<BulletHole>();
            if (hole)
            {
                hole.TryAttachTo(attachTo);
            }
        }
    }

    public virtual void OnAttachedAmmo()
    {
        updateChamberedBullet();

        if (onAttachedAmmoEvent != null)
        {
            onAttachedAmmoEvent.Invoke();
        }
    }

    public virtual void OnDetachedAmmo()
    {
        updateChamberedBullet();

        if (onDetachedAmmoEvent != null)
        {
            onDetachedAmmoEvent.Invoke();
        }
    }

    public virtual int GetBulletCount()
    {
        if (ReloadMethod == ReloadType.InfiniteAmmo)
        {
            return 9999;
        }
        else if (ReloadMethod == ReloadType.InternalAmmo)
        {
            return (int)InternalAmmo;
        }
        else if (ReloadMethod == ReloadType.ManualClip)
        {
            return GetComponentsInChildren<Bullet>(false).Length;
        }

        return GetComponentsInChildren<Bullet>(false).Length;
    }

    public virtual void RemoveBullet()
    {
        if (ReloadMethod == ReloadType.InfiniteAmmo)
        {
            return;
        }

        else if (ReloadMethod == ReloadType.InternalAmmo)
        {
            InternalAmmo--;
        }
        else if (ReloadMethod == ReloadType.ManualClip)
        {
            Bullet firstB = GetComponentInChildren<Bullet>(false);
            if (firstB != null)
            {
                Destroy(firstB.gameObject);
            }
        }

        updateChamberedBullet();
    }


    public virtual void Reload()
    {
        InternalAmmo = MaxInternalAmmo;
    }

    void updateChamberedBullet()
    {
        if (ChamberedBullet != null)
        {
            ChamberedBullet.gameObject.SetActive(BulletInChamber || EmptyBulletInChamber);
        }
    }

    void chamberRound()
    {

        int currentBulletCount = GetBulletCount();

        if (currentBulletCount > 0)
        {
            RemoveBullet();

            BulletInChamber = true;
        }
        else
        {
            BulletInChamber = false;
        }
    }

    protected IEnumerator shotRoutine;

    private void randomizeMuzzleFlashScaleRotation()
    {
        MuzzleFlashObject.transform.localScale = Vector3.one * Random.Range(0.75f, 1.5f);
        MuzzleFlashObject.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0, 90f));
    }

    public virtual void OnWeaponCharged(bool allowCasingEject)
    {
        if (BulletInChamber && allowCasingEject)
        {
            ejectCasing();
        }
        else if (EmptyBulletInChamber && allowCasingEject)
        {
            ejectCasing();
            EmptyBulletInChamber = false;
        }

        chamberRound();

        slideForcedBack = false;

        if (onWeaponChargedEvent != null)
        {
            onWeaponChargedEvent.Invoke();
        }
    }

    protected virtual void ejectCasing()
    {
        GameObject shell = Instantiate(BulletCasingPrefab, EjectPointTransform.position, EjectPointTransform.rotation) as GameObject;
        Rigidbody rb = shell.GetComponentInChildren<Rigidbody>();

        if (rb)
        {
            rb.AddRelativeForce(Vector3.right * BulletCasingForce, ForceMode.VelocityChange);
        }

        Destroy(shell, 5);
    }

    protected virtual IEnumerator doMuzzleFlash()
    {
        MuzzleFlashObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);

        randomizeMuzzleFlashScaleRotation();
        yield return new WaitForSeconds(0.05f);

        MuzzleFlashObject.SetActive(false);
    }

    protected virtual IEnumerator animateSlideAndEject()
    {

        MuzzleFlashObject.SetActive(true);

        int frames = 0;
        bool slideEndReached = false;
        Vector3 slideDestination = new Vector3(0, 0, SlideDistance);

        if (SlideTransform)
        {
            while (!slideEndReached)
            {


                SlideTransform.localPosition = Vector3.MoveTowards(SlideTransform.localPosition, slideDestination, Time.deltaTime * slideSpeed);
                float distance = Vector3.Distance(SlideTransform.localPosition, slideDestination);

                if (distance <= minSlideDistance)
                {
                    slideEndReached = true;
                }

                frames++;

                if (frames < 2)
                {
                    randomizeMuzzleFlashScaleRotation();
                }
                else
                {
                    slideEndReached = true;
                    MuzzleFlashObject.SetActive(false);
                }

                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            yield return new WaitForEndOfFrame();
            randomizeMuzzleFlashScaleRotation();
            yield return new WaitForEndOfFrame();

            MuzzleFlashObject.SetActive(false);
            slideEndReached = true;
        }

        if (SlideTransform)
        {
            SlideTransform.localPosition = slideDestination;
        }

        yield return new WaitForEndOfFrame();
        MuzzleFlashObject.SetActive(false);

        ejectCasing();

        yield return new WaitForEndOfFrame();


        if (!slideForcedBack && SlideTransform != null)
        {
            frames = 0;
            bool slideBeginningReached = false;
            while (!slideBeginningReached)
            {

                SlideTransform.localPosition = Vector3.MoveTowards(SlideTransform.localPosition, Vector3.zero, Time.deltaTime * slideSpeed);
                float distance = Vector3.Distance(SlideTransform.localPosition, Vector3.zero);

                if (distance <= minSlideDistance)
                {
                    slideBeginningReached = true;
                }

                if (frames > 2)
                {
                    slideBeginningReached = true;
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
