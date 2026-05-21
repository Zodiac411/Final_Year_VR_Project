using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using UnityEditor;
using UnityEngine;

public class Lever : MonoBehaviour
{

    [Header("Rotation Limits")]
    public float MinimumXRotation = -45f;
    public float MaximumXRotation = 45f;
    public float InitialXRotation = 0f;

    [Header("Audio")]
    [SerializeField] private AudioClip SwitchOnSound;
    [SerializeField] private AudioClip SwitchOffSound;

    [Header("Lever Stats")]
    [SerializeField] private float SwitchTolerance = 1.25f;
    [SerializeField] private float SmoothLookSpeed = 15f;
    [SerializeField] private float ReturnLookSpeed = 5f;
    [SerializeField] private float LeverPercentage;

    public bool AllowPhysicsForces = true;
    public bool ReturnToCenter = true;

    private bool UseSmoothLook = true;
    private bool SnapToGrabber = false;
    private bool DropLeverOnActivation = false;
    private bool ShowEditorGizmos = true;

    [SerializeField] private UnityEvent onLeverDown;
    [SerializeField] private UnityEvent onLeverUp;
    [SerializeField] private FloatEvent onLeverChange;

    private Grabbable grab;
    private Rigidbody rb;
    private AudioSource audioSource;
    private bool switchedOn;

    private ConfigurableJoint configJoint;
    private HingeJoint hingedJoint;

    private Vector3 _lastLocalAngle;

    private void Start()
    {
        grab = GetComponent<Grabbable>();
        rb = GetComponent<Rigidbody>();
        hingedJoint = GetComponent<HingeJoint>();
        configJoint = GetComponent<ConfigurableJoint>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (SwitchOnSound != null || SwitchOffSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Awake()
    {
        transform.localEulerAngles = new Vector3(InitialXRotation, 0, 0);
    }

    private void Update()
    {
        if (rb)
        {
            rb.isKinematic = AllowPhysicsForces == false && !grab.BeingHeld;
        }

        if (!grab.BeingHeld)
        {
            initialOffset = Quaternion.identity;
        }

        Vector3 currentRotation = transform.localEulerAngles;
        float angle = Mathf.Round(currentRotation.x);
        angle = (angle > 180) ? angle - 360 : angle;

        LeverPercentage = GetAnglePercentage(angle);

        OnLeverChange(LeverPercentage);

        if ((LeverPercentage + SwitchTolerance) > 99 && !switchedOn)
        {
            OnLeverUp();
        }
        else if ((LeverPercentage - SwitchTolerance) < 1 && switchedOn)
        {
            OnLeverDown();
        }

        _lastLocalAngle = transform.localEulerAngles;
    }

    public virtual float GetAnglePercentage(float currentAngle)
    {
        if (hingedJoint)
        {
            return (currentAngle - hingedJoint.limits.min) / (hingedJoint.limits.max - hingedJoint.limits.min) * 100;
        }

        if (configJoint)
        {
            return currentAngle / configJoint.linearLimit.limit * 100;
        }

        return 0;
    }

    private void FixedUpdate()
    {
        doLeverLook();
    }

    Quaternion initialOffset = Quaternion.identity;

    private void doLeverLook()
    {
        if (grab != null && grab.BeingHeld)
        {
            Transform target = grab.GetPrimaryGrabber().transform;
            Quaternion originalRot = transform.rotation;
            Vector3 localTargetPosition = transform.InverseTransformPoint(target.position);

            localTargetPosition.x = 0f;

            Vector3 targetPosition = transform.TransformPoint(localTargetPosition);
            transform.LookAt(targetPosition, transform.up);

            if (initialOffset == Quaternion.identity)
            {
                initialOffset = originalRot * Quaternion.Inverse(transform.rotation);
            }

            if (!SnapToGrabber)
            {
                transform.rotation = transform.rotation * initialOffset;
            }

            if (UseSmoothLook)
            {
                Quaternion newRot = transform.rotation;
                transform.rotation = originalRot;
                transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime * SmoothLookSpeed);
            }
        }
        else if (grab != null && !grab.BeingHeld)
        {
            if (ReturnToCenter && AllowPhysicsForces == false)
            {
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime * ReturnLookSpeed);
            }
        }
    }

    public virtual void SetLeverAngle(float angle)
    {
        transform.localEulerAngles = new Vector3(Mathf.Clamp(angle, MinimumXRotation, MaximumXRotation), 0, 0);
    }

    public virtual void OnLeverChange(float percentage)
    {
        if (onLeverChange != null)
        {
            onLeverChange.Invoke(percentage);
        }
    }

    public virtual void OnLeverDown()
    {
        if (SwitchOffSound != null)
        {
            audioSource.clip = SwitchOffSound;
            audioSource.Play();
        }

        if (onLeverDown != null)
        {
            onLeverDown.Invoke();
        }

        switchedOn = false;

        if (DropLeverOnActivation && grab != null)
        {
            grab.DropItem(false, false);
        }
    }
    public virtual void OnLeverUp()
    {
        if (SwitchOnSound != null)
        {
            audioSource.clip = SwitchOnSound;
            audioSource.Play();
        }

        if (onLeverUp != null)
        {
            onLeverUp.Invoke();
        }

        switchedOn = true;

        if (DropLeverOnActivation && grab != null)
        {
            grab.DropItem(false, false);
        }
    }
}