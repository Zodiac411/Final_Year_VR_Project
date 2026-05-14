using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class ArmSwingLocomotion : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform ForwardDirection;

    [SerializeField] private float SpeedModifier = 5f;
    [SerializeField] private float MinInput = 0.1f;

    private bool MustBeHoldingLeftTrigger = true;
    private bool MustBeHoldingRightTrigger = true;

    private bool MustBeHoldingLeftGrip = false;
    private bool MustBeHoldingRightGrip = false;

    public float VelocitySum { get { return leftVelocity + rightVelocity; } }

    private float leftVelocity;
    private float rightVelocity;

    private void Start()
    {
        if (characterController == null)
        {
            characterController = GetComponentInChildren<CharacterController>();
        }

        if (ForwardDirection == null)
        {
            ForwardDirection = transform;
        }
    }

    private void Update()
    {
        UpdateVelocities();
        UpdateMovement();
    }

    public virtual void UpdateMovement()
    {
        if (characterController && VelocitySum > MinInput)
        {
            characterController.Move(ForwardDirection.forward * VelocitySum * SpeedModifier * Time.deltaTime);
        }
    }

    public void UpdateVelocities()
    {
        if (LeftInputReady())
        {
            leftVelocity = XRInput.Instance.GetControllerVelocity(ControllerHand.Left).magnitude;
        }
        else
        {
            leftVelocity = 0;
        }

        if (RightInputReady())
        {
            rightVelocity = XRInput.Instance.GetControllerVelocity(ControllerHand.Right).magnitude;
        }
        else
        {
            rightVelocity = 0;
        }
    }

    public virtual bool LeftInputReady()
    {

        if (MustBeHoldingLeftGrip && XRInput.Instance.LeftGrip < 0.1f)
        {
            return false;
        }

        if (MustBeHoldingLeftTrigger && XRInput.Instance.LeftTrigger < 0.1f)
        {
            return false;
        }

        return true;
    }

    public virtual bool RightInputReady()
    {

        if (MustBeHoldingRightGrip && XRInput.Instance.RightGrip < 0.1f)
        {
            return false;
        }

        if (MustBeHoldingRightTrigger && XRInput.Instance.RightTrigger < 0.1f)
        {
            return false;
        }

        return true;
    }
}