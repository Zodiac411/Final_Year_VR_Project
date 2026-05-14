using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class JoystickControl : MonoBehaviour
{
    [Header("Deadzone")]
    [Tooltip("Any values below this threshold will not be passed to events")]
    [SerializeField] private float DeadZone = 0.001f;
    [SerializeField] private float MinDegrees = -45f;
    [SerializeField] private float MaxDegrees = 45f;

    [Header("Angles")]
    [SerializeField] private float angleX;
    [SerializeField] private float angleY;

    [Header("Chance")]
    [SerializeField] private float LeverPercentageX = 0;
    [SerializeField] private float LeverPercentageY = 0;

    [SerializeField] private Vector2 LeverVector;

    [SerializeField] private float SmoothLookSpeed = 15f;
    [SerializeField] private bool UseSmoothLook = true;

    [SerializeField] private bool KinematicWhileInactive = false;

    [SerializeField] private FloatFloatEvent onJoystickChange;
    [SerializeField] private Vector2Event onJoystickVectorChange;

    private Grabbable grab;
    private Rigidbody rb;

    private Vector3 currentRotation;

    private void Start()
    {
        grab = GetComponent<Grabbable>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (rb)
        {
            rb.isKinematic = KinematicWhileInactive && !grab.BeingHeld;
        }

        doJoystickLook();

        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);

        currentRotation = transform.localEulerAngles;
        angleX = Mathf.Floor(currentRotation.x);
        angleX = (angleX > 180) ? angleX - 360 : angleX;

        angleY = Mathf.Floor(currentRotation.y);
        angleY = (angleY > 180) ? angleY - 360 : angleY;

        if (angleX > MaxDegrees)
        {
            transform.localEulerAngles = new Vector3(MaxDegrees, currentRotation.y, currentRotation.z);
        }
        else if (angleX < MinDegrees)
        {
            transform.localEulerAngles = new Vector3(MinDegrees, currentRotation.y, currentRotation.z);
        }

        if (angleY > MaxDegrees)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, currentRotation.y, MaxDegrees);
        }
        else if (angleY < MinDegrees)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, currentRotation.y, MinDegrees);
        }

        LeverPercentageX = (angleY - MinDegrees) / (MaxDegrees - MinDegrees) * 100;
        LeverPercentageY = (angleX - MinDegrees) / (MaxDegrees - MinDegrees) * 100;

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

    void FixedUpdate()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void doJoystickLook()
    {
        if (grab != null && grab.BeingHeld)
        {

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Quaternion originalRot = transform.rotation;

            Vector3 localTargetPosition = transform.InverseTransformPoint(grab.GetPrimaryGrabber().transform.position);

            Vector3 targetPosition = transform.TransformPoint(localTargetPosition);
            transform.LookAt(targetPosition, transform.up);

            if (UseSmoothLook)
            {
                Quaternion newRot = transform.rotation;
                transform.rotation = originalRot;
                transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.fixedDeltaTime * SmoothLookSpeed);
            }
        }
        else if (grab != null && !grab.BeingHeld && rb.isKinematic)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime * SmoothLookSpeed);

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
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