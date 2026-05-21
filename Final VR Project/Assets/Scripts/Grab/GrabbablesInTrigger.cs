using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine;

public class GrabbablesInTrigger : MonoBehaviour
{
    public Grabbable ClosestRemoteGrabbable;
    public Grabbable ClosestGrabbable;

    public Dictionary<Collider, Grabbable> NearbyGrabbables;

    [SerializeField] private Dictionary<Collider, Grabbable> ValidRemoteGrabbables = new Dictionary<Collider, Grabbable>();
    [SerializeField] private Dictionary<Collider, Grabbable> ValidGrabbables;

    [Header("Events")]
    public bool FireGrabbableEvents = true;

    [Header("Collision Checks")]
    public LayerMask RemoteCollisionLayers = 1;

    public bool RaycastRemoteGrabbables = false;
    public bool RemoteGrabbablesMustBeVisible = false;

    private float lastDis;
    private float thisDis;

    private Grabbable _closest;
    private Grabbable grabbable;
    private GrabbableChild grabbableChild;

    private Dictionary<Collider, Grabbable> valids;
    private Dictionary<Collider, Grabbable> filtered;

    private Transform eyeTransform;

    private void Start()
    {
        NearbyGrabbables = new Dictionary<Collider, Grabbable>();
        ValidGrabbables = new Dictionary<Collider, Grabbable>();

        if (Camera.main != null)
        {
            eyeTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        updateClosestGrabbable();
        updateClosestRemoteGrabbables();
    }

    private void updateClosestGrabbable()
    {
        NearbyGrabbables = SanitizeGrabbables(NearbyGrabbables);
        ValidGrabbables = GetValidGrabbables(NearbyGrabbables);
        ClosestGrabbable = GetClosestGrabbable(ValidGrabbables);
    }

    private void updateClosestRemoteGrabbables()
    {
        ClosestRemoteGrabbable = GetClosestGrabbable(ValidRemoteGrabbables, true, RaycastRemoteGrabbables);

        if (ClosestGrabbable != null)
        {
            ClosestRemoteGrabbable = null;
        }
    }

    public virtual Grabbable GetClosestGrabbable(Dictionary<Collider, Grabbable> grabbables, bool remoteOnly = false, bool raycastCheck = false)
    {
        _closest = null;
        lastDis = 9999f;

        if (grabbables == null)
        {
            return null;
        }

        foreach (var kvp in grabbables)
        {

            if (kvp.Value == null || !kvp.Value.IsGrabbable())
            {
                continue;
            }

            thisDis = Vector3.Distance(kvp.Value.transform.position, transform.position);
            if (thisDis < lastDis && kvp.Value.isActiveAndEnabled)
            {
                if (remoteOnly && !kvp.Value.RemoteGrabbable)
                {
                    continue;
                }

                if (remoteOnly && thisDis > kvp.Value.RemoteGrabDistance)
                {
                    continue;
                }

                if (raycastCheck && !kvp.Value.RemoteGrabbing)
                {
                    if (CheckObjectBetweenGrabbable(transform.position, kvp.Value))
                    {
                        continue;
                    }

                    if (RemoteGrabbablesMustBeVisible && eyeTransform != null)
                    {
                        if (CheckObjectBetweenGrabbable(eyeTransform.position, kvp.Value))
                        {
                            continue;
                        }
                    }
                }

                lastDis = thisDis;
                _closest = kvp.Value;
            }
        }

        return _closest;
    }

    public virtual bool CheckObjectBetweenGrabbable(Vector3 startingPosition, Grabbable theGrabbable)
    {
        RaycastHit hit;
        if (Physics.Linecast(startingPosition, theGrabbable.transform.position, out hit, RemoteCollisionLayers, QueryTriggerInteraction.Ignore))
        {

            float hitDistance = Vector3.Distance(startingPosition, hit.point);
            if (hit.collider.gameObject != theGrabbable.gameObject)
            {
                if (hitDistance > 0.09f)
                {
                    return true;
                }
                else { }
            }
        }

        return false;
    }

    public Dictionary<Collider, Grabbable> GetValidGrabbables(Dictionary<Collider, Grabbable> grabs)
    {
        valids = new Dictionary<Collider, Grabbable>();

        if (grabs == null)
        {
            return valids;
        }

        foreach (var kvp in grabs)
        {
            if (isValidGrabbable(kvp.Key, kvp.Value) && !valids.ContainsKey(kvp.Key))
            {
                valids.Add(kvp.Key, kvp.Value);
            }
        }

        return valids;
    }

    protected virtual bool isValidGrabbable(Collider col, Grabbable grab)
    {

        if (col == null || grab == null || !grab.isActiveAndEnabled || !col.enabled)
        {
            return false;
        }
        else if (!grab.IsGrabbable())
        {
            return false;
        }
        else if (grab.GetComponent<SnapZone>() != null && grab.GetComponent<SnapZone>().HeldItem == null)
        {
            return false;
        }
        else if (grab == ClosestGrabbable)
        {
            if (grab.BreakDistance > 0 && Vector3.Distance(grab.transform.position, transform.position) > grab.BreakDistance)
            {
                return false;
            }
        }

        return true;
    }

    public virtual Dictionary<Collider, Grabbable> SanitizeGrabbables(Dictionary<Collider, Grabbable> grabs)
    {
        filtered = new Dictionary<Collider, Grabbable>();

        if (grabs == null)
        {
            return filtered;
        }

        foreach (var g in grabs)
        {
            if (g.Key != null && g.Key.enabled && g.Value.isActiveAndEnabled)
            {
                if (g.Value.BreakDistance > 0 && Vector3.Distance(g.Key.transform.position, transform.position) > g.Value.BreakDistance)
                {
                    continue;
                }

                filtered.Add(g.Key, g.Value);
            }
        }

        return filtered;
    }

    public virtual void AddNearbyGrabbable(Collider col, Grabbable grabObject)
    {

        if (NearbyGrabbables == null)
        {
            NearbyGrabbables = new Dictionary<Collider, Grabbable>();
        }

        if (grabObject != null && !NearbyGrabbables.ContainsKey(col))
        {
            NearbyGrabbables.Add(col, grabObject);
        }
    }

    public virtual void RemoveNearbyGrabbable(Collider col, Grabbable grabObject)
    {
        if (grabObject != null && NearbyGrabbables != null && NearbyGrabbables.ContainsKey(col))
        {
            NearbyGrabbables.Remove(col);
        }
    }

    public virtual void RemoveNearbyGrabbable(Grabbable grabObject)
    {
        if (grabObject != null)
        {

            foreach (var x in NearbyGrabbables)
            {
                if (x.Value == grabObject)
                {
                    NearbyGrabbables.Remove(x.Key);
                    break;
                }
            }
        }
    }

    public virtual void AddValidRemoteGrabbable(Collider col, Grabbable grabObject)
    {

        if (col == null || grabObject == null)
        {
            return;
        }

        if (grabObject != null && grabObject.RemoteGrabbable && col != null && !ValidRemoteGrabbables.ContainsKey(col))
        {
            ValidRemoteGrabbables.Add(col, grabObject);
        }
    }

    public virtual void RemoveValidRemoteGrabbable(Collider col, Grabbable grabObject)
    {
        if (grabObject != null && ValidRemoteGrabbables != null && ValidRemoteGrabbables.ContainsKey(col))
        {
            ValidRemoteGrabbables.Remove(col);
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        grabbable = other.GetComponent<Grabbable>();
        if (grabbable != null)
        {
            AddNearbyGrabbable(other, grabbable);
            return;
        }

        grabbableChild = other.GetComponent<GrabbableChild>();
        if (grabbableChild != null && grabbableChild.ParentGrabbable != null)
        {
            AddNearbyGrabbable(other, grabbableChild.ParentGrabbable);
            return;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        grabbable = other.GetComponent<Grabbable>();
        if (grabbable != null)
        {
            RemoveNearbyGrabbable(other, grabbable);
            return;
        }

        grabbableChild = other.GetComponent<GrabbableChild>();
        if (grabbableChild != null)
        {
            RemoveNearbyGrabbable(other, grabbableChild.ParentGrabbable);
            return;
        }
    }
}