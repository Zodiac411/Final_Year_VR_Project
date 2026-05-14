using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using UnityEngine;

public class GrabbableUnityEvents : GrabbableEvents
{
    [SerializeField] private GrabberEvent onGrab;
    [SerializeField] private UnityEvent onRelease;
    [SerializeField] private UnityEvent onBecomesClosestGrabbable;
    [SerializeField] private UnityEvent onNoLongerClosestGrabbable;
    [SerializeField] private UnityEvent onBecomesClosestRemoteGrabbable;
    [SerializeField] private UnityEvent onNoLongerClosestRemoteGrabbable;
    [SerializeField] private FloatEvent onGrip;
    [SerializeField] private FloatEvent onTrigger;
    [SerializeField] private UnityEvent onTriggerDown;
    [SerializeField] private UnityEvent onTriggerUp;
    [SerializeField] private UnityEvent onButton1;
    [SerializeField] private UnityEvent onButton1Down;
    [SerializeField] private UnityEvent onButton1Up;
    [SerializeField] private UnityEvent onButton2;
    [SerializeField] private UnityEvent onButton2Down;
    [SerializeField] private UnityEvent onButton2Up;
    [SerializeField] private UnityEvent onSnapZoneEnter;
    [SerializeField] private UnityEvent onSnapZoneExit;

    public override void OnGrab(Grabber grabber)
    {
        base.OnGrab(grabber);

        if (onGrab != null)
        {
            onGrab.Invoke(grabber);
        }
    }

    public override void OnRelease()
    {
        base.OnRelease();

        if (onRelease != null)
        {
            onRelease.Invoke();
        }
    }

    public override void OnBecomesClosestGrabbable(ControllerHand touchingHand)
    {
        base.OnBecomesClosestGrabbable(touchingHand);

        if (onBecomesClosestGrabbable != null)
        {
            onBecomesClosestGrabbable.Invoke();
        }
    }

    public override void OnNoLongerClosestGrabbable(ControllerHand touchingHand)
    {
        base.OnNoLongerClosestGrabbable(touchingHand);

        if (onNoLongerClosestGrabbable != null)
        {
            onNoLongerClosestGrabbable.Invoke();
        }
    }

    public override void OnBecomesClosestRemoteGrabbable(ControllerHand touchingHand)
    {
        base.OnBecomesClosestRemoteGrabbable(touchingHand);

        if (onBecomesClosestRemoteGrabbable != null)
        {
            onBecomesClosestRemoteGrabbable.Invoke();
        }
    }

    public override void OnNoLongerClosestRemoteGrabbable(ControllerHand touchingHand)
    {
        base.OnNoLongerClosestRemoteGrabbable(touchingHand);

        if (onNoLongerClosestRemoteGrabbable != null)
        {
            onNoLongerClosestRemoteGrabbable.Invoke();
        }
    }

    public override void OnGrip(float gripValue)
    {
        base.OnGrip(gripValue);

        if (onGrip != null)
        {
            onGrip.Invoke(gripValue);
        }
    }

    public override void OnTrigger(float triggerValue)
    {
        base.OnTrigger(triggerValue);

        if (onTrigger != null)
        {
            onTrigger.Invoke(triggerValue);
        }
    }

    public override void OnTriggerDown()
    {
        base.OnTriggerDown();

        if (onTriggerDown != null)
        {
            onTriggerDown.Invoke();
        }
    }

    public override void OnTriggerUp()
    {
        base.OnTriggerUp();

        if (onTriggerUp != null)
        {
            onTriggerUp.Invoke();
        }
    }

    public override void OnButton1()
    {
        base.OnButton1();

        if (onButton1 != null)
        {
            onButton1.Invoke();
        }
    }

    public override void OnButton1Down()
    {
        base.OnButton1Down();

        if (onButton1Down != null)
        {
            onButton1Down.Invoke();
        }
    }

    public override void OnButton1Up()
    {
        base.OnButton1Up();

        if (onButton1Up != null)
        {
            onButton1Up.Invoke();
        }
    }

    public override void OnButton2()
    {
        base.OnButton2();

        if (onButton2 != null)
        {
            onButton2.Invoke();
        }
    }

    public override void OnButton2Down()
    {
        base.OnButton2Down();

        if (onButton2Down != null)
        {
            onButton2Down.Invoke();
        }
    }

    public override void OnButton2Up()
    {
        base.OnButton2Up();

        if (onButton2Up != null)
        {
            onButton2Up.Invoke();
        }
    }

    public override void OnSnapZoneEnter()
    {
        base.OnSnapZoneEnter();

        if (onSnapZoneEnter != null)
        {
            onSnapZoneEnter.Invoke();
        }
    }

    public override void OnSnapZoneExit()
    {
        base.OnSnapZoneExit();

        if (onSnapZoneExit != null)
        {
            onSnapZoneExit.Invoke();
        }
    }
}