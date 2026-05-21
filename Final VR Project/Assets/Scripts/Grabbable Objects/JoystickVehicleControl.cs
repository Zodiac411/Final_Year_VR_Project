using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class JoystickVehicleControl : MonoBehaviour
{
    [Header("Grab Object")]
    [SerializeField] private Grabbable JoystickGrabbable;

    [Header("Movement Speed")]
    [SerializeField] private bool UseSmoothLook = true;
    [SerializeField] private float SmoothLookSpeed = 15f;

    [Header("Hinge X")]
    [SerializeField] private Transform HingeXTransform;

    [SerializeField] private float MinXAngle = -45f;
    [SerializeField] private float MaxXAngle = 45f;

    [Header("Hinge Y")]
    [SerializeField] private Transform HingeYTransform;

    [SerializeField] private float MinYAngle = -45f;
    [SerializeField] private float MaxYAngle = 45f;

    [Header("Return To Center")]
    [SerializeField] private float ReturnToCenterSpeed = 5f;

    [Header("Deadzone")]
    [SerializeField] private float DeadZone = 0.001f;

    [SerializeField] private FloatFloatEvent onJoystickChange;
    [SerializeField] private Vector2Event onJoystickVectorChange;

    [SerializeField] private float LeverPercentageX = 0;
    [SerializeField] private float LeverPercentageY = 0;

    [SerializeField] private Vector2 LeverVector;
    [SerializeField] private float angleX;
    [SerializeField] private float angleY;

    Quaternion originalRot = Quaternion.identity;

    private void Update()
    {
        if (JoystickGrabbable != null)
        {
            if (JoystickGrabbable.BeingHeld)
            {
                Transform lookAt = JoystickGrabbable.GetPrimaryGrabber().transform;

                if (HingeXTransform)
                {
                    originalRot = HingeXTransform.rotation;

                    HingeXTransform.LookAt(lookAt, Vector3.left);

                    angleX = HingeXTransform.localEulerAngles.x;
                    if (angleX > 180)
                    {
                        angleX -= 360;
                    }

                    HingeXTransform.localEulerAngles = new Vector3(Mathf.Clamp(angleX, MinXAngle, MaxXAngle), 0, 0);

                    if (UseSmoothLook)
                    {
                        Quaternion newRot = HingeXTransform.rotation;
                        HingeXTransform.rotation = originalRot;
                        HingeXTransform.rotation = Quaternion.Lerp(HingeXTransform.rotation, newRot, Time.deltaTime * SmoothLookSpeed);
                    }
                }
                if (HingeYTransform)
                {

                    originalRot = HingeYTransform.rotation;

                    HingeYTransform.LookAt(lookAt, Vector3.left);

                    angleY = HingeYTransform.localEulerAngles.y;
                    if (angleY > 180)
                    {
                        angleY -= 360;
                    }

                    HingeYTransform.localEulerAngles = new Vector3(0, Mathf.Clamp(angleY, MinYAngle, MaxYAngle), 0);

                    if (UseSmoothLook)
                    {
                        Quaternion newRot = HingeYTransform.rotation;
                        HingeYTransform.rotation = originalRot;
                        HingeYTransform.rotation = Quaternion.Lerp(HingeYTransform.rotation, newRot, Time.deltaTime * SmoothLookSpeed);
                    }
                }
            }
            else if (ReturnToCenterSpeed > 0)
            {
                if (HingeXTransform)
                {
                    HingeXTransform.localRotation = Quaternion.Lerp(HingeXTransform.localRotation, Quaternion.identity, Time.deltaTime * ReturnToCenterSpeed);
                }
                if (HingeYTransform)
                {
                    HingeYTransform.localRotation = Quaternion.Lerp(HingeYTransform.localRotation, Quaternion.identity, Time.deltaTime * ReturnToCenterSpeed);
                }
            }

            CallJoystickEvents();
        }
    }

    public virtual void CallJoystickEvents()
    {
        angleX = HingeXTransform.localEulerAngles.x;

        if (angleX > 180)
        {
            angleX -= 360;
        }

        angleY = HingeYTransform.localEulerAngles.y;

        if (angleY > 180)
        {
            angleY -= 360;
        }

        LeverPercentageY = (angleX - MinXAngle) / (MaxXAngle - MinXAngle) * 100;
        LeverPercentageX = (angleY - MinYAngle) / (MaxYAngle - MinYAngle) * 100;

        OnJoystickChange(LeverPercentageX, LeverPercentageY);

        float xInput = Mathf.Lerp(-1f, 1f, LeverPercentageX / 100);
        float yInput = Mathf.Lerp(-1f, 1f, LeverPercentageY / 100);

        if (DeadZone > 0)
        {
            if (Mathf.Abs(xInput) < DeadZone)
            {
                xInput = 0;
            }
            if (Mathf.Abs(yInput) < DeadZone)
            {
                yInput = 0;
            }
        }

        LeverVector = new Vector2(xInput, yInput);

        OnJoystickChange(LeverVector);
    }

    public virtual void OnJoystickChange(float leverX, float leverY)
    {
        if (onJoystickChange != null)
        {
            onJoystickChange.Invoke(leverX, leverY);
        }
    }

    public virtual void OnJoystickChange(Vector2 joystickVector)
    {
        if (onJoystickVectorChange != null)
        {
            onJoystickVectorChange.Invoke(joystickVector);
        }
    }
}