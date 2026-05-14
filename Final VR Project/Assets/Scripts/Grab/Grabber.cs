using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine;

[RequireComponent(typeof(GrabbablesInTrigger))]
public class Grabber : MonoBehaviour
{
    [Header("Hand Side")]
    public ControllerHand HandSide = ControllerHand.Left;

    [Header("Grab Settings")]
    public HoldType DefaultHoldType = HoldType.HoldDown;
    public GrabButton DefaultGrabButton = GrabButton.Grip;
    public InputActionReference GrabAction;

    [Header("Hold / Release")]
    [Range(0f, 1f)] public float GripAmount = .9f;
    [Range(0f, 1f)] public float ReleaseGripAmount = .5f;
    public float GrabCheckSeconds = 0.5f;

    [Header("Equip on Start")]
    public Grabbable EquipGrabbableOnStart;

    [Header("Hand Graphics")]
    public Transform HandsGraphics;

    Transform handsGraphicsParent;
    Vector3 handsGraphicsPosition;
    Quaternion handsGraphicsRotation;

    [Header("Shown for Debug :")]
    public Grabbable HeldGrabbable;

    public bool ForceGrab = false;
    public bool ForceRelease = false;

    public float LastDropTime;

    Grabbable previousClosest;
    Grabbable previousClosestRemote;

    public bool HoldingItem { get { return HeldGrabbable != null; } }
    public bool RemoteGrabbingItem { get { return flyingGrabbable != null; } }

    public GrabbablesInTrigger GrabsInTrigger { get { return grabsInTrigger; } }
    public Grabbable RemoteGrabbingGrabbable { get { return flyingGrabbable; } }

    public Vector3 handsGraphicsGrabberOffset { get; private set; }
    public Vector3 handsGraphicsGrabberOffsetRotation { get; private set; }

    GrabbablesInTrigger grabsInTrigger;
    Rigidbody rb;
    XRInput input;
    ConfigurableJoint joint;

    [Header("Grabber Events")]
    public GrabbableEvent onGrabEvent;
    public GrabbableEvent onAfterGrabEvent;
    public GrabbableEvent onReleaseEvent;

    [HideInInspector] public Transform DummyTransform;
    [HideInInspector] public VelocityTracker velocityTracker;
    [HideInInspector] public Vector3 PreviousPosition;

    [HideInInspector] public bool FreshGrip = true;
    
    private Grabbable flyingGrabbable;

    float flyingTime = 0;
    float currentGrabTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabsInTrigger = GetComponent<GrabbablesInTrigger>();
        joint = GetComponent<ConfigurableJoint>();
        input = XRInput.Instance;

        if (joint == null)
        {
            joint = gameObject.AddComponent<ConfigurableJoint>();
            joint.rotationDriveMode = RotationDriveMode.Slerp;

            JointDrive slerpDrive = joint.slerpDrive;
            slerpDrive.positionSpring = 600;

            JointDrive xDrive = joint.xDrive;
            xDrive.positionSpring = 2500;
            JointDrive yDrive = joint.yDrive;
            yDrive.positionSpring = 2500;
            JointDrive zDrive = joint.zDrive;
            zDrive.positionSpring = 2500;
        }

        if (HandsGraphics)
        {
            handsGraphicsParent = HandsGraphics.transform.parent;
            handsGraphicsPosition = HandsGraphics.transform.localPosition;
            handsGraphicsRotation = HandsGraphics.transform.localRotation;

            handsGraphicsGrabberOffset = transform.InverseTransformPoint(HandsGraphics.position);
            handsGraphicsGrabberOffsetRotation = transform.localEulerAngles;
        }

        if (rb && rb.isKinematic)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        if (EquipGrabbableOnStart != null)
        {
            GrabGrabbable(EquipGrabbableOnStart);
        }

        if (velocityTracker == null)
        {
            velocityTracker = GetComponent<VelocityTracker>();
        }

        if (velocityTracker == null)
        {
            velocityTracker = gameObject.AddComponent<VelocityTracker>();
            velocityTracker.controllerHand = HandSide;
        }
    }

    void Update()
    {
        if (flyingGrabbable != null)
        {
            flyingTime += Time.deltaTime;

            float maxFlyingGrabbableTime = 5;
            if (flyingTime > maxFlyingGrabbableTime)
            {
                resetFlyingGrabbable();
            }
        }

        updateFreshGrabStatus();
        checkGrabbableEvents();

        if ((HoldingItem == false && InputCheckGrab()) || ForceGrab)
        {
            TryGrab();
        }
        else if (((HoldingItem || RemoteGrabbingItem) && inputCheckRelease()) || ForceRelease)
        {
            TryRelease();
        }
    }

    protected virtual void updateFreshGrabStatus()
    {
        if (getGrabInput(GrabButton.Grip) <= ReleaseGripAmount)
        {
            FreshGrip = true;
            currentGrabTime = 0;
        }

        if (getGrabInput(GrabButton.Grip) > GripAmount)
        {
            currentGrabTime += Time.deltaTime;
        }

        if (currentGrabTime > GrabCheckSeconds)
        {
            FreshGrip = false;
        }
    }

    void checkGrabbableEvents()
    {
        if (grabsInTrigger == null)
        {
            return;
        }

        if (previousClosest != grabsInTrigger.ClosestGrabbable)
        {
            if (previousClosest != null)
            {

                GrabbableEvents[] ge = previousClosest.GetComponents<GrabbableEvents>();
                if (ge != null)
                {
                    for (int x = 0; x < ge.Length; x++)
                    {
                        ge[x].OnNoLongerClosestGrabbable(HandSide);
                        ge[x].OnNoLongerClosestGrabbable(this);
                    }
                }
                previousClosest.RemoveValidGrabber(this);
            }

            if (grabsInTrigger.ClosestGrabbable != null && !HoldingItem)
            {

                GrabbableEvents[] ge = grabsInTrigger.ClosestGrabbable.GetComponents<GrabbableEvents>();
                if (ge != null)
                {
                    for (int x = 0; x < ge.Length; x++)
                    {
                        ge[x].OnBecomesClosestGrabbable(HandSide);
                        ge[x].OnBecomesClosestGrabbable(this);
                    }
                }
                grabsInTrigger.ClosestGrabbable.AddValidGrabber(this);
            }
        }

        if (grabsInTrigger.ClosestGrabbable != null && !HoldingItem)
        {
            grabsInTrigger.ClosestGrabbable.AddValidGrabber(this);
        }

        if (previousClosestRemote != grabsInTrigger.ClosestRemoteGrabbable)
        {
            if (previousClosestRemote != null)
            {
                GrabbableEvents[] ge = previousClosestRemote.GetComponents<GrabbableEvents>();
                if (ge != null)
                {
                    for (int x = 0; x < ge.Length; x++)
                    {
                        ge[x].OnNoLongerClosestRemoteGrabbable(HandSide);
                        ge[x].OnNoLongerClosestRemoteGrabbable(this);
                    }

                }
                previousClosestRemote.RemoveValidGrabber(this);
            }

            if (grabsInTrigger.ClosestRemoteGrabbable != null && !HoldingItem)
            {

                GrabbableEvents[] ge = grabsInTrigger.ClosestRemoteGrabbable.GetComponents<GrabbableEvents>();
                if (ge != null)
                {
                    for (int x = 0; x < ge.Length; x++)
                    {
                        ge[x].OnBecomesClosestRemoteGrabbable(HandSide);
                        ge[x].OnBecomesClosestRemoteGrabbable(this);
                    }
                }

                grabsInTrigger.ClosestRemoteGrabbable.AddValidGrabber(this);
            }
        }

        previousClosest = grabsInTrigger.ClosestGrabbable;
        previousClosestRemote = grabsInTrigger.ClosestRemoteGrabbable;
    }

    public virtual bool InputCheckGrab()
    {

        Grabbable closest = getClosestOrRemote();

        return GetInputDownForGrabbable(closest);
    }

    public virtual bool GetInputDownForGrabbable(Grabbable grabObject)
    {

        if (grabObject == null)
        {
            return false;
        }

        HoldType closestHoldType = getHoldType(grabObject);
        GrabButton closestGrabButton = GetGrabButton(grabObject);

        if (ForceGrab)
        {
            return true;
        }
        else if (closestHoldType == HoldType.HoldDown)
        {
            bool grabInput = getGrabInput(closestGrabButton) >= GripAmount;

            if (!grabInput && GrabAction != null)
            {
                // Check Input Action
                grabInput = GrabAction.action.ReadValue<float>() >= GripAmount;
            }

            if (closestGrabButton == GrabButton.Grip && !FreshGrip)
            {
                return false;
            }

            return grabInput;
        }
        else if (closestHoldType == HoldType.Toggle)
        {
            return getToggleInput(closestGrabButton);
        }

        return false;
    }

    HoldType getHoldType(Grabbable grab)
    {
        HoldType closestHoldType = grab.Grabtype;

        if (closestHoldType == HoldType.Inherit)
        {
            closestHoldType = DefaultHoldType;
        }

        if (closestHoldType == HoldType.Inherit)
        {
            closestHoldType = HoldType.HoldDown;
        }

        return closestHoldType;
    }

    public virtual GrabButton GetGrabButton(Grabbable grab)
    {
        GrabButton grabButton = grab.GrabButton;

        if (grabButton == GrabButton.Inherit)
        {
            grabButton = DefaultGrabButton;
        }

        if (grabButton == GrabButton.Inherit)
        {
            grabButton = GrabButton.Grip;
        }

        return grabButton;
    }


    Grabbable getClosestOrRemote()
    {
        if (grabsInTrigger.ClosestGrabbable != null)
        {
            return grabsInTrigger.ClosestGrabbable;
        }
        else if (grabsInTrigger.ClosestRemoteGrabbable != null)
        {
            return grabsInTrigger.ClosestRemoteGrabbable;
        }

        return null;
    }

    protected virtual bool inputCheckRelease()
    {
        var grabbingGrabbable = RemoteGrabbingItem ? flyingGrabbable : HeldGrabbable;

        if (grabbingGrabbable == null)
        {
            return false;
        }

        HoldType closestHoldType = getHoldType(grabbingGrabbable);
        GrabButton closestGrabButton = GetGrabButton(grabbingGrabbable);

        if (closestHoldType == HoldType.HoldDown)
        {
            return getGrabInput(closestGrabButton) <= ReleaseGripAmount;
        }
        else if (closestHoldType == HoldType.Toggle)
        {
            return getToggleInput(closestGrabButton);
        }

        return false;
    }

    protected virtual float getGrabInput(GrabButton btn)
    {
        float gripValue = 0;

        if (input == null)
        {
            return 0;
        }
        if (HandSide == ControllerHand.Left)
        {
            if (btn == GrabButton.GripOrTrigger)
            {
                gripValue = Mathf.Max(input.LeftGrip, input.LeftTrigger);
            }
            else if (btn == GrabButton.Grip)
            {
                gripValue = input.LeftGrip;
            }
            else if (btn == GrabButton.Trigger)
            {
                gripValue = input.LeftTrigger;
            }
        }
        else if (HandSide == ControllerHand.Right)
        {
            if (btn == GrabButton.GripOrTrigger)
            {
                gripValue = Mathf.Max(input.RightGrip, input.RightTrigger);
            }
            else if (btn == GrabButton.Grip)
            {
                gripValue = input.RightGrip;
            }
            else if (btn == GrabButton.Trigger)
            {
                gripValue = input.RightTrigger;
            }
        }

        return gripValue;
    }

    protected virtual bool getToggleInput(GrabButton btn)
    {
        if (input == null)
        {
            return false;
        }
        if (HandSide == ControllerHand.Left)
        {
            if (btn == GrabButton.GripOrTrigger)
            {
                return input.LeftGripDown || input.LeftTriggerDown;
            }
            else if (btn == GrabButton.Grip)
            {
                return input.LeftGripDown;
            }
            else if (btn == GrabButton.Trigger)
            {
                return input.LeftTriggerDown;
            }
        }
        else if (HandSide == ControllerHand.Right)
        {
            if (btn == GrabButton.GripOrTrigger)
            {
                return input.RightGripDown || input.RightTriggerDown;
            }
            else if (btn == GrabButton.Grip)
            {
                return input.RightGripDown;
            }
            else if (btn == GrabButton.Trigger)
            {
                return input.RightTriggerDown;
            }
        }

        return false;
    }

    public virtual bool TryGrab()
    {
        if (HeldGrabbable != null)
        {
            return false;
        }
        if (grabsInTrigger.ClosestGrabbable != null)
        {
            GrabGrabbable(grabsInTrigger.ClosestGrabbable);

            return true;
        }
        else if (grabsInTrigger.ClosestRemoteGrabbable != null && flyingGrabbable == null)
        {
            flyingGrabbable = grabsInTrigger.ClosestRemoteGrabbable;
            flyingGrabbable.GrabRemoteItem(this);
        }

        return false;
    }

    public virtual void GrabGrabbable(Grabbable item)
    {
        if (flyingGrabbable != null && item != flyingGrabbable)
        {
            return;
        }

        resetFlyingGrabbable();
        if (HeldGrabbable != null && HeldGrabbable)
        {
            TryRelease();
        }

        HeldGrabbable = item;
        FreshGrip = false;
        onGrabEvent?.Invoke(item);
        item.GrabItem(this);
        onAfterGrabEvent?.Invoke(item);
    }

    public virtual void DidDrop()
    {
        if (onReleaseEvent != null && HeldGrabbable != null)
        {
            onReleaseEvent.Invoke(HeldGrabbable);
        }

        HeldGrabbable = null;
        transform.localEulerAngles = Vector3.zero;
        LastDropTime = Time.time;
        resetFlyingGrabbable();
        ResetHandGraphics();
    }

    public virtual void HideHandGraphics()
    {
        if (HandsGraphics != null)
        {
            HandsGraphics.gameObject.SetActive(false);
        }
    }

    public virtual void ResetHandGraphics()
    {
        if (HandsGraphics != null)
        {
            HandsGraphics.gameObject.SetActive(true);

            HandsGraphics.transform.parent = handsGraphicsParent;
            HandsGraphics.transform.localPosition = handsGraphicsPosition;
            HandsGraphics.transform.localRotation = handsGraphicsRotation;
        }
    }

    public virtual void TryRelease()
    {
        if (HeldGrabbable != null && HeldGrabbable.CanBeDropped)
        {
            HeldGrabbable.DropItem(this);
        }

        resetFlyingGrabbable();
    }

    void resetFlyingGrabbable()
    {
        if (flyingGrabbable != null)
        {
            flyingGrabbable.ResetGrabbing();
            flyingGrabbable = null;
            flyingTime = 0;
        }
    }

    public virtual Vector3 GetGrabberAveragedVelocity()
    {
        return velocityTracker.GetAveragedVelocity();
    }

    public virtual Vector3 GetGrabberAveragedAngularVelocity()
    {
        return velocityTracker.GetAveragedAngularVelocity();
    }
}