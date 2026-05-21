using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnapZone : MonoBehaviour
{
    [Header("Starting / Held Item")]
    public Grabbable HeldItem;
    [SerializeField] private Grabbable StartingItem;

    [Header("Options")]
    public bool CanSwapItem = true;
    [SerializeField] private bool CanDropItem = true;
    [SerializeField] private bool CanRemoveItem = true;
    [SerializeField] private bool DisableColliders = true;

    [SerializeField] private float MaxDropTime = 0.1f;
    [SerializeField] private float ScaleItem = 1f;

    List<Collider> disabledColliders = new List<Collider>();

    [SerializeField] private bool DuplicateItemOnGrab = false;

    [HideInInspector]
    [SerializeField] private float LastSnapTime;

    [Header("Filtering")]

    [SerializeField] private List<string> OnlyAllowNames;
    [SerializeField] private List<string> ExcludeTransformNames;

    [Header("Audio")]
    [SerializeField] private AudioClip SoundOnSnap;
    [SerializeField] private AudioClip SoundOnUnsnap;

    [Header("Events")]

    public GrabbableEvent OnSnapEvent;
    public GrabbableEvent OnDetachEvent;

    GrabbablesInTrigger gZone;

    Rigidbody heldItemRigid;
    Grabbable trackedItem;
    SnapZoneOffset offset;

    private float scaleTo;
    private bool heldItemWasKinematic;

    [HideInInspector] public Grabbable ClosestGrabbable;

    private void Start()
    {
        gZone = GetComponent<GrabbablesInTrigger>();
        scaleTo = ScaleItem;

        if (StartingItem != null)
        {
            StartingItem.transform.position = transform.position;
            GrabGrabbable(StartingItem);
        }
        else if (HeldItem != null)
        {
            HeldItem.transform.position = transform.position;
            GrabGrabbable(HeldItem);
        }
    }

    private void Update()
    {

        ClosestGrabbable = getClosestGrabbable();

        if (HeldItem == null && ClosestGrabbable != null)
        {
            float secondsSinceDrop = Time.time - ClosestGrabbable.LastDropTime;
            if (secondsSinceDrop < MaxDropTime)
            {
                GrabGrabbable(ClosestGrabbable);
            }
        }

        if (HeldItem != null)
        {

            if (HeldItem.BeingHeld || HeldItem.transform.parent != transform)
            {
                ReleaseAll();
            }
            else
            {
                HeldItem.transform.localScale = Vector3.Lerp(HeldItem.transform.localScale, HeldItem.OriginalScale * scaleTo, Time.deltaTime * 30f);

                if (HeldItem.enabled || (disabledColliders != null && disabledColliders.Count > 0 && disabledColliders[0] != null && disabledColliders[0].enabled))
                {
                    disableGrabbable(HeldItem);
                }
            }
        }

        if (!CanDropItem && trackedItem != null && HeldItem == null)
        {
            if (!trackedItem.BeingHeld)
            {
                GrabGrabbable(trackedItem);
            }
        }
    }

    private Grabbable getClosestGrabbable()
    {

        Grabbable closest = null;
        float lastDistance = 9999f;

        if (gZone == null || gZone.NearbyGrabbables == null)
        {
            return null;
        }

        foreach (var g in gZone.NearbyGrabbables)
        {
            if (g.Key == null)
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, g.Value.transform.position);
            if (dist < lastDistance)
            {

                if (g.Value.OtherGrabbableMustBeGrabbed != null)
                {
                    continue;
                }

                if (g.Value.GetComponent<SnapZone>() != null)
                {
                    continue;
                }

                if (g.Value.CanBeSnappedToSnapZone == false)
                {
                    continue;
                }

                if (OnlyAllowNames != null && OnlyAllowNames.Count > 0)
                {
                    string transformName = g.Value.transform.name;
                    bool matchFound = false;
                    for (int x = 0; x < OnlyAllowNames.Count; x++)
                    {
                        string name = OnlyAllowNames[x];
                        if (transformName.Contains(name))
                        {
                            matchFound = true;
                        }
                    }

                    if (!matchFound)
                    {
                        continue;
                    }
                }

                if (ExcludeTransformNames != null)
                {
                    string transformName = g.Value.transform.name;
                    bool matchFound = false;
                    for (int x = 0; x < ExcludeTransformNames.Count; x++)
                    {
                        if (transformName.Contains(ExcludeTransformNames[x]))
                        {
                            matchFound = true;
                        }
                    }
                    if (matchFound)
                    {
                        continue;
                    }
                }

                if (g.Value.BeingHeld || (Time.time - g.Value.LastDropTime < MaxDropTime))
                {
                    closest = g.Value;
                    lastDistance = dist;
                }
            }
        }

        return closest;
    }

    public virtual void GrabGrabbable(Grabbable grab)
    {
        if (grab.transform.parent != null && grab.transform.parent.GetComponent<SnapZone>() != null)
        {
            return;
        }

        if (HeldItem != null)
        {
            ReleaseAll();
        }

        HeldItem = grab;
        heldItemRigid = HeldItem.GetComponent<Rigidbody>();

        if (heldItemRigid)
        {
            heldItemWasKinematic = heldItemRigid.isKinematic;
            heldItemRigid.isKinematic = true;
        }
        else
        {
            heldItemWasKinematic = false;
        }

        grab.transform.parent = transform;

        if (grab.GetComponent<SnapZoneScale>())
        {
            scaleTo = grab.GetComponent<SnapZoneScale>().Scale;
        }
        else
        {
            scaleTo = ScaleItem;
        }

        SnapZoneOffset off = grab.GetComponent<SnapZoneOffset>();
        if (off)
        {
            offset = off;
        }
        else
        {
            offset = grab.gameObject.AddComponent<SnapZoneOffset>();
            offset.LocalPositionOffset = Vector3.zero;
            offset.LocalRotationOffset = Vector3.zero;
        }

        if (offset)
        {
            HeldItem.transform.localPosition = offset.LocalPositionOffset;
            HeldItem.transform.localEulerAngles = offset.LocalRotationOffset;
        }
        else
        {
            HeldItem.transform.localPosition = Vector3.zero;
            HeldItem.transform.localEulerAngles = Vector3.zero;
        }

        disableGrabbable(grab);

        if (OnSnapEvent != null)
        {
            OnSnapEvent.Invoke(grab);
        }

        GrabbableEvents[] ge = grab.GetComponents<GrabbableEvents>();
        if (ge != null)
        {
            for (int x = 0; x < ge.Length; x++)
            {
                ge[x].OnSnapZoneEnter();
            }
        }

        if (SoundOnSnap)
        {
            if (Time.timeSinceLevelLoad > 0.1f)
            {
                XRManager.Instance.PlaySpatialClipAt(SoundOnSnap, transform.position, 0.75f);
            }
        }

        LastSnapTime = Time.time;
    }

    void disableGrabbable(Grabbable grab)
    {

        if (DisableColliders)
        {
            disabledColliders = grab.GetComponentsInChildren<Collider>(false).ToList();
            for (int x = 0; x < disabledColliders.Count; x++)
            {
                disabledColliders[x].enabled = false;
            }
        }

        grab.enabled = false;
    }

    public virtual void GrabEquipped(Grabber grabber)
    {
        if (grabber != null)
        {
            if (HeldItem)
            {
                if (!CanBeRemoved())
                {
                    return;
                }

                var g = HeldItem;
                if (DuplicateItemOnGrab)
                {

                    ReleaseAll();

                    if (Vector3.Distance(g.transform.position, grabber.transform.position) > 0.2f)
                    {
                        g.transform.position = grabber.transform.position;
                    }

                    GameObject go = Instantiate(g.gameObject, transform.position, Quaternion.identity) as GameObject;
                    Grabbable grab = go.GetComponent<Grabbable>();

                    this.GrabGrabbable(grab);
                    grabber.GrabGrabbable(g);
                }
                else
                {
                    ReleaseAll();

                    if (Vector3.Distance(g.transform.position, grabber.transform.position) > 0.2f)
                    {
                        g.transform.position = grabber.transform.position;
                    }

                    grabber.GrabGrabbable(g);
                }
            }
        }
    }

    public virtual bool CanBeRemoved()
    {
        if (!CanRemoveItem)
        {
            return false;
        }

        if (HeldItem.Grabtype == HoldType.Toggle && (Time.time - LastSnapTime < 0.1f))
        {
            return false;
        }

        return true;
    }

    public virtual void ReleaseAll()
    {

        if (HeldItem == null)
        {
            return;
        }

        if (!CanDropItem && HeldItem != null)
        {
            trackedItem = HeldItem;
        }

        HeldItem.ResetScale();

        if (DisableColliders && disabledColliders != null)
        {
            foreach (var c in disabledColliders)
            {
                if (c)
                {
                    c.enabled = true;
                }
            }
        }
        disabledColliders = null;

        if (heldItemRigid)
        {
            heldItemRigid.isKinematic = heldItemWasKinematic;
        }

        HeldItem.enabled = true;
        HeldItem.transform.parent = null;

        if (HeldItem != null)
        {
            if (SoundOnUnsnap)
            {
                if (Time.timeSinceLevelLoad > 0.1f)
                {
                    XRManager.Instance.PlaySpatialClipAt(SoundOnUnsnap, transform.position, 0.75f);
                }
            }

            if (OnDetachEvent != null)
            {
                OnDetachEvent.Invoke(HeldItem);
            }

            GrabbableEvents[] ge = HeldItem.GetComponents<GrabbableEvents>();
            if (ge != null)
            {
                for (int x = 0; x < ge.Length; x++)
                {
                    ge[x].OnSnapZoneExit();
                }
            }
        }

        HeldItem = null;
    }
}