using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine;

public class HingeHelper : GrabbableEvents
{
    [Header("Snap Options")]
    [SerializeField] private Transform SnapGraphics;
    [SerializeField] private AudioClip SnapSound;

    [SerializeField] private bool SnapToDegrees = false;

    [SerializeField] private float SnapDegrees = 5f;
    [SerializeField] private float RandomizePitch = 0.001f;
    [SerializeField] private float SnapHaptics = 0.5f;

    [SerializeField] private Text LabelToUpdate;

    [Header("Change Events")]
    [SerializeField] private FloatEvent onHingeChange;
    [SerializeField] private FloatEvent onHingeSnapChange;

    private Rigidbody rb;

    private float lastDeg = 0;
    private float lastSnapDeg = 0;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float degrees = getSmoothedValue(transform.localEulerAngles.y);

        if (degrees != lastDeg)
        {
            OnHingeChange(degrees);
        }

        lastDeg = degrees;

        float nearestSnap = getSmoothedValue(Mathf.Round(degrees / SnapDegrees) * SnapDegrees);

        if (SnapToDegrees)
        {

            if (nearestSnap != lastSnapDeg)
            {
                OnSnapChange(nearestSnap);
            }

            lastSnapDeg = nearestSnap;
        }

        if (LabelToUpdate)
        {
            float val = getSmoothedValue(SnapToDegrees ? nearestSnap : degrees);
            LabelToUpdate.text = val.ToString("n0");
        }
    }

    public void OnSnapChange(float yAngle)
    {
        if (SnapGraphics)
        {
            SnapGraphics.localEulerAngles = new Vector3(SnapGraphics.localEulerAngles.x, yAngle, SnapGraphics.localEulerAngles.z);
        }

        if (SnapSound)
        {
            XRManager.Instance.PlaySpatialClipAt(SnapSound, transform.position, 1f, 1f, RandomizePitch);
        }

        if (grab.BeingHeld && SnapHaptics > 0)
        {
            XRInput.Instance.VibrateController(0.5f, SnapHaptics, 0.01f, thisGrabber.HandSide);
        }

        if (onHingeSnapChange != null)
        {
            onHingeSnapChange.Invoke(yAngle);
        }
    }

    public override void OnRelease()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        base.OnRelease();
    }

    public void OnHingeChange(float hingeAmount)
    {
        if (onHingeChange != null)
        {
            onHingeChange.Invoke(hingeAmount);
        }
    }

    private float getSmoothedValue(float val)
    {
        if (val < 0)
        {
            val = 360 - val;
        }
        if (val == 360)
        {
            val = 0;
        }

        return val;
    }
}