using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEngine;

public class TrackedDevice : MonoBehaviour
{
    [SerializeField] private TrackableDevice Device = TrackableDevice.HMD;

    protected InputDevice deviceToTrack;

    protected Vector3 initialLocalPosition;
    protected Quaternion initialLocalRotation;

    protected Vector3 currentLocalPosition;
    protected Quaternion currentLocalRotation;

    protected virtual void Awake()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    protected virtual void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }

    protected virtual void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }

    protected virtual void Update()
    {
        RefreshDeviceStatus();
        UpdateDevice();
    }

    protected virtual void FixedUpdate()
    {
        UpdateDevice();
    }

    public virtual void RefreshDeviceStatus()
    {
        if (!deviceToTrack.isValid)
        {

            if (Device == TrackableDevice.HMD)
            {
                deviceToTrack = XRInput.Instance.GetHMD();
            }
            else if (Device == TrackableDevice.LeftController)
            {
                deviceToTrack = XRInput.Instance.GetLeftController();
            }
            else if (Device == TrackableDevice.RightController)
            {
                deviceToTrack = XRInput.Instance.GetRightController();
            }
        }
    }

    public virtual void UpdateDevice()
    {

        if (deviceToTrack.isValid)
        {
            if (Device == TrackableDevice.HMD)
            {
                transform.localPosition = currentLocalPosition = XRInput.Instance.GetHMDLocalPosition();
                transform.localRotation = currentLocalRotation = XRInput.Instance.GetHMDLocalRotation();
            }
            else if (Device == TrackableDevice.LeftController)
            {
                transform.localPosition = currentLocalPosition = XRInput.Instance.GetControllerLocalPosition(ControllerHand.Left);
                transform.localRotation = currentLocalRotation = XRInput.Instance.GetControllerLocalRotation(ControllerHand.Left);
            }
            else if (Device == TrackableDevice.RightController)
            {
                transform.localPosition = currentLocalPosition = XRInput.Instance.GetControllerLocalPosition(ControllerHand.Right);
                transform.localRotation = currentLocalRotation = XRInput.Instance.GetControllerLocalRotation(ControllerHand.Right);
            }
        }
    }

    protected virtual void OnBeforeRender()
    {
        UpdateDevice();
    }
}

public enum TrackableDevice
{
    HMD,
    LeftController,
    RightController
}