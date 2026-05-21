using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class GrabbableHaptics : GrabbableEvents
{
    [SerializeField] private bool HapticsOnValidPickup = true;
    [SerializeField] private bool HapticsOnValidRemotePickup = true;
    [SerializeField] private bool HapticsOnCollision = true;
    [SerializeField] private bool HapticsOnGrab = true;

    [SerializeField] private float VibrateFrequency = 0.3f;
    [SerializeField] private float VibrateAmplitude = 0.1f;
    [SerializeField] private float VibrateDuration = 0.1f;

    Grabber currentGrabber;

    public override void OnGrab(Grabber grabber)
    {
        currentGrabber = grabber;

        if (HapticsOnGrab)
        {
            doHaptics(grabber.HandSide);
        }
    }

    public override void OnRelease()
    {
        currentGrabber = null;
    }

    public override void OnBecomesClosestGrabbable(ControllerHand touchingHand)
    {

        if (HapticsOnValidPickup)
        {
            doHaptics(touchingHand);
        }
    }

    public override void OnBecomesClosestRemoteGrabbable(ControllerHand touchingHand)
    {
        if (HapticsOnValidRemotePickup)
        {
            doHaptics(touchingHand);
        }
    }

    private void doHaptics(ControllerHand touchingHand)
    {
        if (input)
        {
            input.VibrateController(VibrateFrequency, VibrateAmplitude, VibrateDuration, touchingHand);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (HapticsOnCollision && currentGrabber != null && input != null)
        {
            if (grab != null && grab.BeingHeld)
            {
                input.VibrateController(0.1f, 0.1f, 0.1f, currentGrabber.HandSide);
            }
        }
    }
}