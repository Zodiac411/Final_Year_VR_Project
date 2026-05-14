using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine;
using UnityEditor;
using System;

public class SteeringWheel : GrabbableEvents
{
    [Header("Rotation Limits")]
    [SerializeField] private float MinAngle = -360f;
    [SerializeField] private float MaxAngle = 360f;

    [Header("Rotation Object")]
    [SerializeField] private Transform RotatorObject;

    [Header("Rotation Speed")]
    [SerializeField] private float RotationSpeed = 0f;

    [Header("Two-Handed Option")]
    [SerializeField] private bool AllowTwoHanded = true;

    [Header("Return to Center")]
    [SerializeField] private bool ReturnToCenter = false;
    [SerializeField] private float ReturnToCenterSpeed = 45;

    [Header("Debug Options")]
    [SerializeField] private Text DebugText;

    [Header("Events")]
    [SerializeField] private FloatEvent onAngleChange;
    [SerializeField] private FloatEvent onValueChange;

    [Header("Editor Option")]
    [SerializeField] private bool ShowEditorGizmos = true;

    public float Angle
    {
        get
        {
            return Mathf.Clamp(smoothedAngle, MinAngle, MaxAngle);
        }
    }

    public float RawAngle
    {
        get
        {
            return targetAngle;
        }
    }

    public float ScaleValue
    {
        get
        {
            return GetScaledValue(Angle, MinAngle, MaxAngle);
        }
    }

    public float ScaleValueInverted
    {
        get
        {
            return ScaleValue * -1;
        }
    }

    public float AngleInverted
    {
        get
        {
            return Angle * -1;
        }
    }

    public Grabber PrimaryGrabber
    {
        get
        {
            return GetPrimaryGrabber();
        }
    }
    public Grabber SecondaryGrabber
    {
        get
        {
            return GetSecondaryGrabber();
        }
    }

    protected Vector3 rotatePosition;
    protected Vector3 previousPrimaryPosition;
    protected Vector3 previousSecondaryPosition;

    protected float targetAngle;
    protected float previousTargetAngle;

    protected float smoothedAngle;

    void Update()
    {

        if (grab.BeingHeld)
        {
            UpdateAngleCalculations();
        }
        else if (ReturnToCenter)
        {
            ReturnToCenterAngle();
        }

        ApplyAngleToSteeringWheel(Angle);
        CallEvents();
        UpdatePreviewText();
        UpdatePreviousAngle(targetAngle);
    }

    public virtual void UpdateAngleCalculations()
    {

        float angleAdjustment = 0f;

        if (PrimaryGrabber)
        {
            rotatePosition = transform.InverseTransformPoint(PrimaryGrabber.transform.position);
            rotatePosition = new Vector3(rotatePosition.x, rotatePosition.y, 0);

            angleAdjustment += GetRelativeAngle(rotatePosition, previousPrimaryPosition);

            previousPrimaryPosition = rotatePosition;
        }

        if (AllowTwoHanded && SecondaryGrabber != null)
        {
            rotatePosition = transform.InverseTransformPoint(SecondaryGrabber.transform.position);
            rotatePosition = new Vector3(rotatePosition.x, rotatePosition.y, 0);

            angleAdjustment += GetRelativeAngle(rotatePosition, previousSecondaryPosition);

            previousSecondaryPosition = rotatePosition;
        }

        if (PrimaryGrabber != null && SecondaryGrabber != null)
        {
            angleAdjustment *= 0.5f;
        }

        targetAngle -= angleAdjustment;

        if (RotationSpeed == 0)
        {
            smoothedAngle = targetAngle;
        }
        else
        {
            smoothedAngle = Mathf.Lerp(smoothedAngle, targetAngle, Time.deltaTime * RotationSpeed);
        }

        if (MinAngle != 0 && MaxAngle != 0)
        {
            targetAngle = Mathf.Clamp(targetAngle, MinAngle, MaxAngle);
            smoothedAngle = Mathf.Clamp(smoothedAngle, MinAngle, MaxAngle);
        }
    }

    public float GetRelativeAngle(Vector3 position1, Vector3 position2)
    {

        // Are we turning left or right?
        if (Vector3.Cross(position1, position2).z < 0)
        {
            return -Vector3.Angle(position1, position2);
        }

        return Vector3.Angle(position1, position2);
    }

    public virtual void ApplyAngleToSteeringWheel(float angle)
    {
        RotatorObject.localEulerAngles = new Vector3(0, 0, angle);
    }

    public virtual void UpdatePreviewText()
    {
        if (DebugText)
        {
            DebugText.text = String.Format("{0}\n{1}", (int)AngleInverted, (ScaleValueInverted).ToString("F2"));
        }
    }

    public virtual void CallEvents()
    {
        // Call events
        if (targetAngle != previousTargetAngle)
        {
            onAngleChange.Invoke(targetAngle);
        }

        onValueChange.Invoke(ScaleValue);
    }

    public override void OnGrab(Grabber grabber)
    {
        if (grabber == SecondaryGrabber)
        {
            previousSecondaryPosition = transform.InverseTransformPoint(SecondaryGrabber.transform.position);

            previousSecondaryPosition = new Vector3(previousSecondaryPosition.x, previousSecondaryPosition.y, 0);
        }
        else
        {
            previousPrimaryPosition = transform.InverseTransformPoint(PrimaryGrabber.transform.position);

            previousPrimaryPosition = new Vector3(previousPrimaryPosition.x, previousPrimaryPosition.y, 0);
        }
    }

    public virtual void ReturnToCenterAngle()
    {

        bool wasUnderZero = smoothedAngle < 0;

        if (smoothedAngle > 0)
        {
            smoothedAngle -= Time.deltaTime * ReturnToCenterSpeed;
        }
        else if (smoothedAngle < 0)
        {
            smoothedAngle += Time.deltaTime * ReturnToCenterSpeed;
        }

        if (wasUnderZero && smoothedAngle > 0)
        {
            smoothedAngle = 0;
        }
        else if (!wasUnderZero && smoothedAngle < 0)
        {
            smoothedAngle = 0;
        }

        if (smoothedAngle < 0.02f && smoothedAngle > -0.02f)
        {
            smoothedAngle = 0;
        }

        targetAngle = smoothedAngle;
    }

    public Grabber GetPrimaryGrabber()
    {
        if (grab.HeldByGrabbers != null)
        {
            for (int x = 0; x < grab.HeldByGrabbers.Count; x++)
            {
                Grabber g = grab.HeldByGrabbers[x];
                if (g.HandSide == ControllerHand.Right)
                {
                    return g;
                }
            }
        }

        return null;
    }

    public Grabber GetSecondaryGrabber()
    {
        if (grab.HeldByGrabbers != null)
        {
            for (int x = 0; x < grab.HeldByGrabbers.Count; x++)
            {
                Grabber g = grab.HeldByGrabbers[x];
                if (g.HandSide == ControllerHand.Left)
                {
                    return g;
                }
            }
        }

        return null;
    }

    public virtual void UpdatePreviousAngle(float angle)
    {
        previousTargetAngle = angle;
    }

    public virtual float GetScaledValue(float value, float min, float max)
    {
        float range = (max - min) / 2f;
        float returnValue = ((value - min) / range) - 1;

        return returnValue;
    }

    
}