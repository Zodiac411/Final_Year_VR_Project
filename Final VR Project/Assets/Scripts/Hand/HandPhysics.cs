using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine;

public class HandPhysics : MonoBehaviour
{
    public bool HoldingObject
    {
        get
        {
            return ThisGrabber != null && ThisGrabber.HeldGrabbable != null;
        }
    }

    [SerializeField] private Transform AttachTo;

    [SerializeField] private float HandVelocity = 1500f;
    [SerializeField] private float SnapBackDistance = 1f;

    [SerializeField] private bool DisableHandCollidersOnGrab = true;

    [SerializeField] private Grabber ThisGrabber;
    [SerializeField] private Grabber DisableGrabber;

    [SerializeField] private RemoteGrabber ThisRemoteGrabber;
    [SerializeField] private RemoteGrabber DisableRemoteGrabber;

    [SerializeField] private PhysicMaterial ColliderMaterial;

    [SerializeField] private Transform HandModel;
    [SerializeField] private Transform HandModelOffset;

    private List<Collider> collisions = new List<Collider>();
    private List<Collider> handColliders;

    private Rigidbody rb;
    private ConfigurableJoint configJoint;

    private Grabbable heldGrabbable;
    private Grabbable remoteIgnoredGrabbable;

    private LineRenderer line;

    private Transform priorParent;
    private Vector3 priorLocalOffsetPos;
    private Vector3 localHandOffset;
    private Vector3 localHandOffsetRotation;

    private bool wasHoldingObject = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        configJoint = GetComponent<ConfigurableJoint>();
        line = GetComponent<LineRenderer>();

        if (AttachTo == null)
        {
            AttachTo = new GameObject("AttachToTransform").transform;
        }

        AttachTo.parent = transform.parent;
        AttachTo.SetPositionAndRotation(transform.position, transform.rotation);

        Rigidbody attachRB = AttachTo.gameObject.AddComponent<Rigidbody>();
        attachRB.useGravity = false;
        attachRB.isKinematic = true;
        attachRB.constraints = RigidbodyConstraints.FreezeAll;
        Destroy(configJoint);

        localHandOffset = HandModel.localPosition;
        localHandOffsetRotation = HandModel.localEulerAngles;

        initHandColliders();

        priorParent = transform.parent;
        transform.parent = null;
    }

    private void Update()
    {
        updateHandGraphics();
        drawDistanceLine();
        checkRemoteCollision();
        checkBreakDistance();

        if (!AttachTo.gameObject.activeSelf)
        {
            transform.parent = AttachTo;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            return;
        }
        if (HoldingObject)
        {
            if (!wasHoldingObject)
            {
                OnGrabbedObject(ThisGrabber.HeldGrabbable);
            }
        }
        else
        {
            if (wasHoldingObject)
            {
                OnReleasedObject(heldGrabbable);
            }
        }

        wasHoldingObject = HoldingObject;
    }

    private void FixedUpdate()
    {
        if (HoldingObject && ThisGrabber.HeldGrabbable.DidParentHands)
        {
            rb.MovePosition(AttachTo.position);
            rb.MoveRotation(AttachTo.rotation);
        }
        else
        {
            Vector3 positionDelta = AttachTo.position - transform.position;
            rb.velocity = Vector3.MoveTowards(rb.velocity, (positionDelta * HandVelocity) * Time.fixedDeltaTime, 5f);

            float angle;
            Vector3 axis;
            Quaternion rotationDelta = AttachTo.rotation * Quaternion.Inverse(transform.rotation);
            rotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180)
            {
                angle -= 360;
            }

            if (angle != 0)
            {
                Vector3 angularTarget = angle * axis;
                angularTarget = (angularTarget * 60f) * Time.fixedDeltaTime;
                rb.angularVelocity = Vector3.MoveTowards(rb.angularVelocity, angularTarget, 20f);
            }
        }

        collisions = new List<Collider>();
    }

    private void initHandColliders()
    {
        handColliders = new List<Collider>();

        var tempColliders = GetComponentsInChildren<Collider>(false);
        for (int i = 0; i < tempColliders.Length; i++)
        {
            Collider col = tempColliders[i];
            if (!col.isTrigger && col.enabled)
            {
                if (ColliderMaterial)
                {
                    col.material = ColliderMaterial;
                }

                handColliders.Add(col);
            }
        }

        for (int i = 0; i < handColliders.Count; i++)
        {
            Collider thisCollider = handColliders[i];

            for (int y = 0; y < handColliders.Count; y++)
            {
                Physics.IgnoreCollision(thisCollider, handColliders[y], true);
            }
        }
    }

    private void checkRemoteCollision()
    {
        if (remoteIgnoredGrabbable != null && ThisGrabber.RemoteGrabbingGrabbable != remoteIgnoredGrabbable)
        {
            if (ThisGrabber.HeldGrabbable == remoteIgnoredGrabbable)
            {
                remoteIgnoredGrabbable = null;
            }
            else
            {
                IgnoreGrabbableCollisions(remoteIgnoredGrabbable, false);
                remoteIgnoredGrabbable = null;
            }
        }

        if (ThisGrabber.RemoteGrabbingGrabbable != null && ThisGrabber.RemoteGrabbingGrabbable != remoteIgnoredGrabbable)
        {
            remoteIgnoredGrabbable = ThisGrabber.RemoteGrabbingGrabbable;
            IgnoreGrabbableCollisions(remoteIgnoredGrabbable, true);
        }
    }

    private void drawDistanceLine()
    {
        if (line)
        {
            if (Vector3.Distance(transform.position, AttachTo.position) > 0.05f)
            {
                line.enabled = true;
                line.SetPosition(0, transform.position);
                line.SetPosition(1, AttachTo.position);
            }
            else
            {
                line.enabled = false;
            }
        }
    }

    private void checkBreakDistance()
    {
        if (SnapBackDistance > 0 && Vector3.Distance(transform.position, AttachTo.position) > SnapBackDistance)
        {
            transform.position = AttachTo.position;
        }
    }

    private void updateHandGraphics()
    {
        bool holdingObject = ThisGrabber.HeldGrabbable != null;
        if (!holdingObject)
        {
            if (HandModelOffset)
            {
                HandModelOffset.parent = HandModel;
                HandModelOffset.localPosition = Vector3.zero;
                HandModelOffset.localEulerAngles = Vector3.zero;
            }

            return;
        }

        if (HandModelOffset && ThisGrabber.HandsGraphics)
        {
            HandModelOffset.parent = ThisGrabber.HandsGraphics;
            HandModelOffset.localPosition = localHandOffset;
            HandModelOffset.localEulerAngles = localHandOffsetRotation;
        }
    }

    private IEnumerator UnignoreAllCollisions()
    {
        var thisGrabbable = heldGrabbable;
        heldGrabbable = null;

        yield return new WaitForSeconds(0.1f);

        IgnoreGrabbableCollisions(thisGrabbable, false);
    }

    public void IgnoreGrabbableCollisions(Grabbable grab, bool ignorePhysics)
    {
        var grabColliders = grab.GetComponentsInChildren<Collider>();

        for (int x = 0; x < grabColliders.Length; x++)
        {
            Collider thisGrabCollider = grabColliders[x];

            for (int y = 0; y < handColliders.Count; y++)
            {
                Physics.IgnoreCollision(thisGrabCollider, handColliders[y], ignorePhysics);
            }
        }
    }

    public void DisableHandColliders()
    {
        for (int x = 0; x < handColliders.Count; x++)
        {
            if (handColliders[x] != null && handColliders[x].enabled)
            {
                handColliders[x].enabled = false;
            }
        }
    }

    public void EnableHandColliders()
    {
        for (int x = 0; x < handColliders.Count; x++)
        {
            if (handColliders[x] != null && handColliders[x].enabled == false)
            {
                handColliders[x].enabled = true;
            }
        }
    }

    public virtual void OnGrabbedObject(Grabbable grabbedObject)
    {
        heldGrabbable = grabbedObject;

        if (DisableHandCollidersOnGrab)
        {
            DisableHandColliders();
        }
        else
        {

            IgnoreGrabbableCollisions(heldGrabbable, true);
        }
    }

    public virtual void LockLocalPosition()
    {
        priorParent = transform.parent;
        transform.parent = AttachTo;
    }

    public virtual void UnlockLocalPosition()
    {
        transform.parent = priorParent;
    }

    public virtual void OnReleasedObject(Grabbable grabbedObject)
    {
        if (heldGrabbable != null)
        {
            if (DisableHandCollidersOnGrab)
            {
                EnableHandColliders();
            }
            else
            {
                StartCoroutine(UnignoreAllCollisions());
            }
        }

        heldGrabbable = null;
    }

    private void OnEnable()
    {
        if (DisableGrabber)
        {
            DisableGrabber.enabled = false;
        }

        if (ThisGrabber)
        {
            ThisGrabber.enabled = true;
        }

        if (ThisRemoteGrabber)
        {
            ThisRemoteGrabber.enabled = true;
            DisableRemoteGrabber.enabled = false;
        }

        PlayerRotation.OnBeforeRotate += LockLocalPosition;
        PlayerRotation.OnAfterRotate += UnlockLocalPosition;

        SmoothLocomotion.OnBeforeMove += LockOffset;
        SmoothLocomotion.OnAfterMove += UnlockOffset;
    }

    public virtual void LockOffset()
    {
        priorLocalOffsetPos = AttachTo.InverseTransformPoint(transform.position);
    }

    public virtual void UnlockOffset()
    {
        Vector3 dest = AttachTo.TransformPoint(priorLocalOffsetPos);
        float dist = Vector3.Distance(transform.position, dest);

        if (dist > 0.0005f)
        {
            transform.position = dest;
        }
    }

    private void OnDisable()
    {
        if (ThisGrabber)
        {
            ThisGrabber.enabled = false;
        }

        if (DisableGrabber)
        {
            DisableGrabber.enabled = true;
        }

        if (ThisRemoteGrabber)
        {
            ThisRemoteGrabber.enabled = false;
        }

        if (DisableRemoteGrabber)
        {
            DisableRemoteGrabber.enabled = true;
        }

        PlayerRotation.OnBeforeRotate -= LockLocalPosition;
        PlayerRotation.OnAfterRotate -= UnlockLocalPosition;

        SmoothLocomotion.OnBeforeMove -= LockOffset;
        SmoothLocomotion.OnAfterMove -= UnlockOffset;
    }

    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contacts.Length; i++)
        {
            ContactPoint contact = collision.contacts[i];
            if (IsValidCollision(contact.otherCollider) && !collisions.Contains(contact.otherCollider))
            {
                collisions.Add(contact.otherCollider);
            }
        }
    }

    public bool IsValidCollision(Collider col)
    {
        return true;
    }
}