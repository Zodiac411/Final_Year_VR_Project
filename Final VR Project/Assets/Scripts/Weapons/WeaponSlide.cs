using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine;

public class WeaponSlide : MonoBehaviour
{
    [SerializeField] private float MinLocalZ = -0.03f;
    [SerializeField] private float MaxLocalZ = 0;

    [SerializeField] private bool slidingBack = true;
    [SerializeField] private bool ZeroMassWhenNotHeld = true;

    [SerializeField] private AudioClip SlideReleaseSound;
    [SerializeField] private AudioClip LockedBackSound;

    [SerializeField] private Renderer slideRenderer;
    [SerializeField] private Material flashMaterial;
    private Material originalMaterial;

    public bool LockedBack = false;

    private RaycastWeapon parentWeapon;
    private Grabbable parentGrabbable;
    private Grabbable thisGrabbable;
    private Vector3 initialLocalPos;
    private Vector3 _lockPosition;
    private AudioSource audioSource;
    private Rigidbody rigid;

    private float initialMass;

    private bool isFlashing = false;
    private bool lockSlidePosition;

    private void Start()
    {
        initialLocalPos = transform.localPosition;
        audioSource = GetComponent<AudioSource>();
        parentWeapon = transform.parent.GetComponent<RaycastWeapon>();
        parentGrabbable = transform.parent.GetComponent<Grabbable>();
        thisGrabbable = GetComponent<Grabbable>();
        rigid = GetComponent<Rigidbody>();
        initialMass = rigid.mass;

        if (slideRenderer == null)
        {
            string parentName = transform.parent != null ? transform.parent.gameObject.name : "No parent";
            Debug.LogError($"slideRenderer not assigned in {gameObject.name}. Parent: {parentName}");
            return;
        }

        originalMaterial = slideRenderer.material;

        if (parentWeapon != null)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), parentWeapon.GetComponent<Collider>());
        }
    }

    private void Update()
    {
        if (lockSlidePosition)
        {
            transform.localPosition = _lockPosition;
            return;
        }

        float localZ = transform.localPosition.z;

        if (LockedBack)
        {
            transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, MinLocalZ);

            if (thisGrabbable != null && thisGrabbable.BeingHeld)
            {
                UnlockBack();
            }
        }

        if (!LockedBack)
        {
            if (localZ <= MinLocalZ)
            {
                transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, MinLocalZ);
                if (slidingBack)
                {
                    onSlideBack();
                }
            }
            else if (localZ >= MaxLocalZ)
            {
                transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, MaxLocalZ);

                if (!slidingBack)
                {
                    onSlideForward();
                }
            }
        }

        if ((LockedBack || !parentWeapon.BulletInChamber) && !isFlashing && parentWeapon.GetComponent<Grabbable>().BeingHeld)
        {
            StartCoroutine(FlashSlide());
        }
        else if (!LockedBack && (parentWeapon.BulletInChamber) && isFlashing && !parentWeapon.GetComponent<Grabbable>().BeingHeld)
        {
            StopCoroutine(FlashSlide());
            slideRenderer.material.SetColor("_EmissionColor", Color.black);
            isFlashing = false;
        }
    }

    private void FixedUpdate()
    {
        if (ZeroMassWhenNotHeld && parentGrabbable.BeingHeld && rigid)
        {
            rigid.mass = initialMass;
        }
        else if (ZeroMassWhenNotHeld && rigid)
        {
            rigid.mass = 0.0001f;
        }
    }

    public virtual void LockBack()
    {

        if (!LockedBack)
        {
            if (thisGrabbable.BeingHeld || parentGrabbable.BeingHeld)
            {
                XRManager.Instance.PlaySpatialClipAt(LockedBackSound, transform.position, 1f, 0.8f);
            }

            LockedBack = true;

            if (!isFlashing && slideRenderer != null)
            {
                StartCoroutine(FlashSlide());
            }
        }
    }

    public virtual void UnlockBack()
    {
        if (LockedBack)
        {
            if (thisGrabbable.BeingHeld || parentGrabbable.BeingHeld)
            {
                XRManager.Instance.PlaySpatialClipAt(SlideReleaseSound, transform.position, 1f, 0.9f);
            }

            LockedBack = false;

            if (parentWeapon != null)
            {
                parentWeapon.OnWeaponCharged(false);
            }

            if (isFlashing)
            {
                StopCoroutine(FlashSlide());
                slideRenderer.material.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    private void onSlideBack()
    {

        if (thisGrabbable.BeingHeld || parentGrabbable.BeingHeld)
        {
            playSoundInterval(0, 0.2f, 0.9f);
        }

        if (parentWeapon != null)
        {
            parentWeapon.OnWeaponCharged(true);
        }

        slidingBack = false;
    }

    private void onSlideForward()
    {

        if (thisGrabbable.BeingHeld || parentGrabbable.BeingHeld)
        {
            playSoundInterval(0.2f, 0.35f, 1f);
        }

        slidingBack = true;
    }

    public virtual void LockSlidePosition()
    {
        if (parentGrabbable.BeingHeld && !thisGrabbable.BeingHeld && !lockSlidePosition)
        {
            _lockPosition = transform.localPosition;
            lockSlidePosition = true;
        }
    }

    public virtual void UnlockSlidePosition()
    {
        if (lockSlidePosition)
        {
            StartCoroutine(UnlockSlideRoutine());
        }
    }

    public IEnumerator UnlockSlideRoutine()
    {
        yield return new WaitForSeconds(0.2f);
        lockSlidePosition = false;
    }

    private IEnumerator FlashSlide()
    {
        isFlashing = true;
        while ((LockedBack || !parentWeapon.BulletInChamber) && parentWeapon.GetComponent<Grabbable>().BeingHeld)
        {
            slideRenderer.material = flashMaterial;
            yield return new WaitForSeconds(0.5f);
            slideRenderer.material = originalMaterial;
            yield return new WaitForSeconds(0.5f);
        }
        isFlashing = false;
        slideRenderer.material = originalMaterial;
    }

    private void playSoundInterval(float fromSeconds, float toSeconds, float volume)
    {
        if (audioSource)
        {

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.pitch = Time.timeScale;
            audioSource.time = fromSeconds;
            audioSource.volume = volume;
            audioSource.Play();
            audioSource.SetScheduledEndTime(AudioSettings.dspTime + (toSeconds - fromSeconds));
        }
    }
}