using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;

public enum RotationMechanic
{
    Snap,
    Smooth
}

public class PlayerRotation : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private bool AllowInput = true;
    [SerializeField] private List<InputAxis> inputAxis = new List<InputAxis>() { InputAxis.RightThumbStickAxis };
    [SerializeField] private InputActionReference RotateAction;

    [Header("Smooth / Snap Turning")]
    [SerializeField] private RotationMechanic RotationType = RotationMechanic.Snap;

    [Header("Snap Turn Settings")]
    public float SnapInputAmount = 0.75f;
    [SerializeField] private float SnapRotationAmount = 45f;

    [Header("Smooth Turn Settings")]
    [SerializeField] private float SmoothTurnSpeed = 40f;
    [SerializeField] private float SmoothTurnMinInput = 0.1f;

    private float recentSnapTurnTime;
    private float rotationAmount = 0;

    private float xAxis;
    private float previousXInput;

    public delegate void OnBeforeRotateAction();
    public static event OnBeforeRotateAction OnBeforeRotate;

    public delegate void OnAfterRotateAction();
    public static event OnAfterRotateAction OnAfterRotate;

    private void Update()
    {
        if (!AllowInput)
        {
            return;
        }

        xAxis = GetAxisInput();

        if (RotationType == RotationMechanic.Snap)
        {
            DoSnapRotation(xAxis);
        }

        else if (RotationType == RotationMechanic.Smooth)
        {
            DoSmoothRotation(xAxis);
        }

        previousXInput = xAxis;
    }

    public virtual float GetAxisInput()
    {

        float lastVal = 0;

        if (inputAxis != null)
        {
            for (int i = 0; i < inputAxis.Count; i++)
            {
                float axisVal = XRInput.Instance.GetInputAxisValue(inputAxis[i]).x;

                if (lastVal == 0)
                {
                    lastVal = axisVal;
                }
                else if (axisVal != 0 && axisVal > lastVal)
                {
                    lastVal = axisVal;
                }
            }
        }

        // Check Unity Input Action
        if (RotateAction != null)
        {
            float axisVal = RotateAction.action.ReadValue<Vector2>().x;
            // Always take this value if our last entry was 0. 
            if (lastVal == 0)
            {
                lastVal = axisVal;
            }
            else if (axisVal != 0 && axisVal > lastVal)
            {
                lastVal = axisVal;
            }
        }

        return lastVal;
    }

    public virtual void DoSnapRotation(float xInput)
    {

        // Reset rotation amount before retrieving inputs
        rotationAmount = 0;

        // Snap Right
        if (xInput >= 0.1f && previousXInput < 0.1f)
        {
            rotationAmount += SnapRotationAmount;
        }
        // Snap Left
        else if (xInput <= -0.1f && previousXInput > -0.1f)
        {
            rotationAmount -= SnapRotationAmount;
        }

        if (Math.Abs(rotationAmount) > 0)
        {

            // Call any Before Rotation Events
            OnBeforeRotate?.Invoke();

            // Apply rotation
            transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + rotationAmount, transform.eulerAngles.z));

            recentSnapTurnTime = Time.time;

            // Call any After Rotation Events
            OnAfterRotate?.Invoke();
        }
    }

    public virtual bool RecentlySnapTurned()
    {
        return Time.time - recentSnapTurnTime <= 0.1f;
    }

    public virtual void DoSmoothRotation(float xInput)
    {

        // Reset rotation amount before retrieving inputs
        rotationAmount = 0;

        // Smooth Rotate Right
        if (xInput >= SmoothTurnMinInput)
        {
            rotationAmount += xInput * SmoothTurnSpeed * Time.deltaTime;
        }
        // Smooth Rotate Left
        else if (xInput <= -SmoothTurnMinInput)
        {
            rotationAmount += xInput * SmoothTurnSpeed * Time.deltaTime;
        }

        // Apply rotation
        transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + rotationAmount, transform.eulerAngles.z));
    }
}

