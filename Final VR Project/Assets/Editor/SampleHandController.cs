using System.Collections.Generic;
using UnityEngine.XR;
using System.Linq;
using UnityEngine;

public class SampleHandController : MonoBehaviour
{
    public ControllerHandedness ControllerSide = ControllerHandedness.Right;

    public PoseableObject HeldObject;
    protected bool wasHoldingObject = false;

    private Animator handAnimator;
    private HandPoser handPoser;
    private AutoPoser autoPoser;

    private HandPoseBlender poseBlender;

    private void Start()
    {
        handAnimator = GetComponentInChildren<Animator>();
        handPoser = GetComponentInChildren<HandPoser>();
        autoPoser = GetComponentInChildren<AutoPoser>();

        poseBlender = GetComponentInChildren<HandPoseBlender>();

        if (poseBlender == null)
        {
            poseBlender = this.gameObject.AddComponent<HandPoseBlender>();
            poseBlender.Pose1 = Resources.Load<HandPose>("Open");
            poseBlender.Pose2 = Resources.Load<HandPose>("Closed");
        }

        poseBlender.UpdatePose = false;
    }

    public virtual void Update()
    {
        DoHandControllerUpdate();
    }

    public virtual void DoHandControllerUpdate()
    {
        UpdateXRDevices();
        UpdateFingerInputs();

        if (HoldingObject())
        {
            DoHeldItemPose();
        }
        else
        {
            DoIdlePose();
        }
    }

    public virtual void SetCurrentlyHeldObject(GameObject holdObject)
    {
        if (holdObject != null)
        {
            HeldObject = holdObject.GetComponent<PoseableObject>();
        }
        else
        {
            HeldObject = null;
        }
    }

    public virtual void ClearCurrentlyHeldObject()
    {
        if (HeldObject != null)
        {
            HeldObject = null;
            ResetToIdleComponents();
        }

        wasHoldingObject = false;
    }

    public virtual void ResetToIdleComponents()
    {
        handPoser.enabled = true;
        handPoser.CurrentPose = null;

        if (autoPoser)
        {
            autoPoser.enabled = false;
        }

        if (handAnimator)
        {
            handAnimator.enabled = false;
        }
    }

    public virtual void UpdateFingerInputs()
    {
        poseBlender.ThumbValue = Mathf.Lerp(poseBlender.ThumbValue, GetThumbIsNear() ? 1 : 0, Time.deltaTime * handPoser.AnimationSpeed);

        float targetIndexValue = correctValue(getFeatureUsage(controller, CommonUsages.trigger));

        if (targetIndexValue < 0.1f && GetIndexIsNear())
        {
            targetIndexValue = 0.1f;
        }

        poseBlender.IndexValue = Mathf.Lerp(poseBlender.IndexValue, targetIndexValue, Time.deltaTime * handPoser.AnimationSpeed);
        poseBlender.GripValue = correctValue(getFeatureUsage(controller, CommonUsages.grip));
    }

    public virtual void DoHeldItemPose()
    {
        if (!wasHoldingObject)
        {
            if ((HeldObject.poseType == PoseableObject.PoseType.AutoPoseContinuous || HeldObject.poseType == PoseableObject.PoseType.AutoPoseOnce) && autoPoser != null)
            {
                handAnimator.enabled = false;
                autoPoser.enabled = true;
                autoPoser.UpdateContinuously = true;
                handPoser.CurrentPose = null;

                Invoke("DisableContinousAutoPose", HeldObject.AutoPoseDuration);
            }

            else if (HeldObject.poseType == PoseableObject.PoseType.HandPose)
            {
                handAnimator.enabled = false;
                autoPoser.enabled = false;
                handPoser.CurrentPose = HeldObject.EquipHandPose;
            }
            else if (HeldObject.poseType == PoseableObject.PoseType.HandPose)
            {
                autoPoser.enabled = false;
                handPoser.enabled = false;
                handAnimator.enabled = true;
                handAnimator.SetInteger("HandPoseId", HeldObject.HandPoseID);
            }
        }

        wasHoldingObject = true;
    }

    public virtual void DisableContinousAutoPose()
    {
        if (autoPoser)
        {
            autoPoser.UpdateContinuously = false;
        }
    }

    public virtual void DoIdlePose()
    {
        poseBlender.DoIdleBlendPose();
    }

    public virtual bool HoldingObject()
    {
        return HeldObject != null;
    }

    private float correctValue(float inputValue)
    {
        return (float)System.Math.Round(inputValue * 1000f) / 1000f;
    }

    #region XRInputs

    static List<InputDevice> devices = new List<InputDevice>();
    InputDevice controller;

    public virtual void UpdateXRDevices()
    {
        InputDevices.GetDevices(devices);

        if (ControllerSide == ControllerHandedness.Right)
        {
            controller = GetRightController();
        }
        else
        {
            controller = GetLeftController();
        }
    }

    private float getFeatureUsage(InputDevice device, InputFeatureUsage<float> usage)
    {
        float val;
        device.TryGetFeatureValue(usage, out val);

        return Mathf.Clamp01(val);
    }

    private bool getFeatureUsage(InputDevice device, InputFeatureUsage<bool> usage)
    {
        bool val;
        if (device.TryGetFeatureValue(usage, out val))
        {
            return val;
        }

        return val;
    }

    public virtual InputDevice GetLeftController()
    {
        InputDevices.GetDevices(devices);

        var leftHandedControllers = new List<InputDevice>();
        var dc = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(dc, leftHandedControllers);
        return leftHandedControllers.FirstOrDefault();
    }

    public virtual InputDevice GetRightController()
    {
        InputDevices.GetDevices(devices);

        var rightHandedControllers = new List<InputDevice>();
        var dc = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(dc, rightHandedControllers);

        return rightHandedControllers.FirstOrDefault();
    }

    public virtual bool GetThumbIsNear()
    {
        #pragma warning disable 0618
        return getFeatureUsage(controller, CommonUsages.thumbTouch) > 0 ||
            getFeatureUsage(controller, CommonUsages.primaryTouch) ||
            getFeatureUsage(controller, CommonUsages.secondaryTouch) ||
            getFeatureUsage(controller, CommonUsages.primary2DAxisTouch);
            #pragma warning restore 0618
    }

    public virtual bool GetIndexIsNear()
    {
        #pragma warning disable 0618
        return getFeatureUsage(controller, CommonUsages.indexTouch) > 0;
        #pragma warning restore 0618
    }

    #endregion
}

public enum ControllerHandedness
{
    Left,
    Right,
    None
}