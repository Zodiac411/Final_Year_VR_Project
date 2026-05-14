using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine;

public class MagazineSlide : MonoBehaviour
{
    [SerializeField] private string AcceptableMagazineName = "Clip";

    [SerializeField] private Grabbable AttachedWeapon;
    [SerializeField] private Grabbable HeldMagazine = null;

    [SerializeField] private float ClipSnapDistance = .075f;
    [SerializeField] private float ClipUnsnapDistance = .15f;
    [SerializeField] private float EjectForce = 1f;
    [SerializeField] private float flashDuration = .5f;

    [SerializeField] private float MagazineDistance = 0f;

    [SerializeField] private AudioClip ClipAttachSound;
    [SerializeField] private AudioClip ClipDetachSound;

    [SerializeField] private Renderer magRenderer;

    private Collider HeldCollider = null;
    private RaycastWeapon parentWeapon;
    private GrabberArea grabClipArea;
    private Coroutine flashRoutine = null;

    private float lastClipAttachSoundTime = 0f;
    private float lastClipDetachSoundTime = 0f;
    private float lastEjectTime;

    private bool magazineInPlace = false;
    private bool lockedInPlace = false;

    private bool IsWeaponHeld => AttachedWeapon != null && AttachedWeapon.BeingHeld;

    private void Awake()
    {
        grabClipArea = GetComponentInChildren<GrabberArea>();

        if (transform.parent != null)
        {
            parentWeapon = transform.parent.GetComponent<RaycastWeapon>();
        }

        if (HeldMagazine != null)
        {
            AttachGrabbableMagazine(HeldMagazine, HeldMagazine.GetComponent<Collider>());
        }

        if (magRenderer != null)
        {
            magRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (HeldMagazine == null && IsWeaponHeld)
        {
            startFlashing();
        }
        else
        {
            stopFlashing();
        }
    }

    private void LateUpdate()
    {
        CheckGrabClipInput();

        if (HeldMagazine != null)
        {
            HeldMagazine.transform.parent = transform;

            if (lockedInPlace)
            {
                HeldMagazine.transform.localPosition = Vector3.zero;
                HeldMagazine.transform.localEulerAngles = Vector3.zero;
                return;
            }

            Vector3 localPos = HeldMagazine.transform.localPosition;
            HeldMagazine.transform.localEulerAngles = Vector3.zero;

            float localY = localPos.y;
            if (localY > 0)
            {
                localY = 0;
            }

            moveMagazine(new Vector3(0, localY, 0));
            MagazineDistance = Vector3.Distance(transform.position, HeldMagazine.transform.position);
            bool clipRecentlyGrabbed = Time.time - HeldMagazine.LastGrabTime < 1f;

            if (MagazineDistance < ClipSnapDistance)
            {
                if (!magazineInPlace && !recentlyEjected() && !clipRecentlyGrabbed)
                {
                    attachMagazine();
                }

                if (!HeldMagazine.BeingHeld)
                {
                    moveMagazine(Vector3.zero);
                }
            }
            else if (MagazineDistance >= ClipUnsnapDistance && !recentlyEjected())
            {
                detachMagazine();
            }
        }

        if (HeldMagazine == null && !magazineInPlace)
        {
            startFlashing();
        }
        else
        {
            stopFlashing();
        }
    }

    private bool recentlyEjected()
    {
        return Time.time - lastEjectTime < 0.1f;
    }

    private void moveMagazine(Vector3 localPosition)
    {
        HeldMagazine.transform.localPosition = localPosition;
    }

    public void CheckGrabClipInput()
    {
        if (HeldMagazine == null || grabClipArea == null)
        {
            return;
        }

        if (AttachedWeapon != null && !AttachedWeapon.BeingHeld)
        {
            return;
        }

        Grabber nearestGrabber = grabClipArea.GetOpenGrabber();
        if (grabClipArea != null && nearestGrabber != null)
        {
            if (nearestGrabber.HandSide == ControllerHand.Left && XRInput.Instance.LeftGripDown)
            {
                OnGrabClipArea(nearestGrabber);
            }
            else if (nearestGrabber.HandSide == ControllerHand.Right && XRInput.Instance.RightGripDown)
            {
                OnGrabClipArea(nearestGrabber);
            }
        }
    }

    private void startFlashing()
    {
        if (flashRoutine == null && magRenderer != null)
        {
            flashRoutine = StartCoroutine(FlashMag());
        }
    }

    private void stopFlashing()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        if (magRenderer != null)
        {
            magRenderer.enabled = false;
        }
    }

    private IEnumerator FlashMag()
    {
        while (IsWeaponHeld && HeldMagazine == null)
        {
            magRenderer.enabled = !magRenderer.enabled;
            yield return new WaitForSeconds(flashDuration);
        }

        stopFlashing();
    }

    private void attachMagazine()
    {
        var grabber = HeldMagazine.GetPrimaryGrabber();
        HeldMagazine.DropItem(grabber, false, false);

        if (ClipAttachSound && Time.timeSinceLevelLoad > .1f && Time.time - lastClipAttachSoundTime > 1f)
        {
            XRManager.Instance.PlaySpatialClipAt(ClipAttachSound, transform.position, 1f);
            lastClipAttachSoundTime = Time.time;
        }

        moveMagazine(Vector3.zero);

        if (transform.parent != null)
        {
            Rigidbody parentRB = transform.parent.GetComponent<Rigidbody>();
            if (parentRB)
            {
                FixedJoint fj = HeldMagazine.gameObject.AddComponent<FixedJoint>();
                fj.autoConfigureConnectedAnchor = true;
                fj.axis = new Vector3(0, 1, 0);
                fj.connectedBody = parentRB;
            }

            if (parentWeapon)
            {
                parentWeapon.OnAttachedAmmo();
            }
        }

        HeldMagazine.enabled = false;
        lockedInPlace = true;
        magazineInPlace = true;
    }

    private Grabbable detachMagazine()
    {
        if (HeldMagazine == null)
        {
            return null;
        }

        if (ClipDetachSound && Time.time - lastClipDetachSoundTime > 1f)
        {
            XRManager.Instance.PlaySpatialClipAt(ClipDetachSound, transform.position, 1f, 0.9f);
            lastClipDetachSoundTime = Time.time;
        }

        HeldMagazine.transform.parent = null;

        if (transform.parent != null)
        {
            Rigidbody parentRB = transform.parent.GetComponent<Rigidbody>();
            if (parentRB)
            {
                FixedJoint fj = HeldMagazine.gameObject.GetComponent<FixedJoint>();
                if (fj)
                {
                    fj.connectedBody = null;
                    Destroy(fj);
                }
            }
        }

        if (HeldCollider != null)
        {
            HeldCollider.enabled = true;
            HeldCollider = null;
        }

        if (parentWeapon)
        {
            parentWeapon.OnDetachedAmmo();
        }

        HeldMagazine.enabled = true;
        magazineInPlace = false;
        lockedInPlace = false;
        lastEjectTime = Time.time;

        var returnGrab = HeldMagazine;
        HeldMagazine = null;

        return returnGrab;
    }

    public void EjectMagazine()
    {
        Grabbable ejectedMag = detachMagazine();
        lastEjectTime = Time.time;

        StartCoroutine(EjectMagRoutine(ejectedMag));
    }

    private IEnumerator EjectMagRoutine(Grabbable ejectedMag)
    {

        if (ejectedMag != null && ejectedMag.GetComponent<Rigidbody>() != null)
        {

            Rigidbody ejectRigid = ejectedMag.GetComponent<Rigidbody>();

            ejectedMag.transform.parent = transform;

            if (ejectedMag.transform.localPosition.y > -ClipSnapDistance)
            {
                ejectedMag.transform.localPosition = new Vector3(0, -0.1f, 0);
            }

            ejectedMag.transform.parent = null;
            ejectRigid.AddForce(-ejectedMag.transform.up * EjectForce, ForceMode.VelocityChange);

            yield return new WaitForFixedUpdate();
            ejectedMag.transform.parent = null;

        }

        yield return null;
    }

    public void OnGrabClipArea(Grabber grabbedBy)
    {
        if (HeldMagazine != null)
        {
            Grabbable temp = HeldMagazine;
            HeldMagazine.enabled = true;
            detachMagazine();
            temp.enabled = true;

            grabbedBy.GrabGrabbable(temp);
        }
    }

    public virtual void AttachGrabbableMagazine(Grabbable mag, Collider magCollider)
    {
        HeldMagazine = mag;
        HeldMagazine.transform.parent = transform;
        HeldCollider = magCollider;
        stopFlashing();

        if (HeldCollider != null)
        {
            HeldCollider.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Grabbable grab = other.GetComponent<Grabbable>();
        if (HeldMagazine == null && grab != null && grab.transform.name.Contains(AcceptableMagazineName))
        {
            AttachGrabbableMagazine(grab, other);
        }
    }
}