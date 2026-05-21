using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbing : MonoBehaviour
{
    [Header("Capsule Settings")]
    public float ClimbingCapsuleHeight = 0.5f;
    public float ClimbingCapsuleCenter = -0.25f;

    [Header("Climbing Transforms")]
    [SerializeField] private Transform LeftControllerTransform;
    [SerializeField] private Transform RightControllerTransform;

    [Header("Haptics")]

    [SerializeField] private float VibrateFrequency = 0.3f;
    [SerializeField] private float VibrateAmplitude = 0.1f;
    [SerializeField] private float VibrateDuration = 0.1f;

    [SerializeField] private bool ApplyHapticsOnGrab = true;

    private List<Grabber> climbers;

    private bool wasGrippingClimbable;

    private CharacterController characterController;
    private SmoothLocomotion smoothLocomotion;
    private PlayerGravity playerGravity;
    private Rigidbody rb;

    public bool IsRigidbodyPlayer
    {
        get
        {
            if (ensurePlayerRB)
            {
                return isPlayerRB;
            }
            else
            {
                isPlayerRB = smoothLocomotion != null && smoothLocomotion.ControllerType == 
                    PlayerControllerType.Rigidbody;

                ensurePlayerRB = true;
                return isPlayerRB;
            }
        }
    }

    private bool ensurePlayerRB = false;
    private bool isPlayerRB = false;

    public bool GrippingClimbable = false;

    private Vector3 moveDirection = Vector3.zero;

    private Vector3 previousLeftControllerPosition;
    private Vector3 previousRightControllerPosition;

    private Vector3 controllerMoveAmount;

    public void Start()
    {
        climbers = new List<Grabber>();
        characterController = GetComponentInChildren<CharacterController>();
        smoothLocomotion = GetComponentInChildren<SmoothLocomotion>();
        playerGravity = GetComponentInChildren<PlayerGravity>();
        rb = GetComponent<Rigidbody>();
    }

    public void LateUpdate()
    {
        checkClimbing();

        if (LeftControllerTransform != null)
        {
            previousLeftControllerPosition = LeftControllerTransform.position;
        }
        if (RightControllerTransform != null)
        {
            previousRightControllerPosition = RightControllerTransform.position;
        }
    }

    public virtual void AddClimber(Climbable climbable, Grabber grab)
    {

        if (climbers == null)
        {
            climbers = new List<Grabber>();
        }

        if (!climbers.Contains(grab))
        {

            if (grab.DummyTransform == null)
            {
                GameObject go = new GameObject();
                go.transform.name = "DummyTransform";
                go.transform.parent = grab.transform;
                go.transform.position = grab.transform.position;
                go.transform.localEulerAngles = Vector3.zero;

                grab.DummyTransform = go.transform;
            }

            grab.DummyTransform.parent = climbable.transform;
            grab.PreviousPosition = grab.DummyTransform.position;

            if (ApplyHapticsOnGrab)
            {
                XRInput.Instance.VibrateController(VibrateFrequency, VibrateAmplitude, VibrateDuration, grab.HandSide);
            }

            climbers.Add(grab);
        }
    }

    public virtual void RemoveClimber(Grabber grab)
    {
        if (climbers.Contains(grab))
        {
            grab.DummyTransform.parent = grab.transform;
            grab.DummyTransform.localPosition = Vector3.zero;

            climbers.Remove(grab);
        }
    }

    public virtual bool GrippingAtLeastOneClimbable()
    {

        if (climbers != null && climbers.Count > 0)
        {

            for (int i = 0; i < climbers.Count; i++)
            {
                if (climbers[i] != null && climbers[i].HoldingItem)
                {
                    return true;
                }
            }

            climbers = new List<Grabber>();
        }

        return false;
    }

    protected virtual void checkClimbing()
    {
        GrippingClimbable = GrippingAtLeastOneClimbable();

        if (GrippingClimbable && !wasGrippingClimbable)
        {
            onGrabbedClimbable();
        }

        if (wasGrippingClimbable && !GrippingClimbable)
        {
            onReleasedClimbable();
        }

        if (GrippingClimbable)
        {

            moveDirection = Vector3.zero;

            int count = 0;
            float length = climbers.Count;
            for (int i = 0; i < length; i++)
            {
                Grabber climber = climbers[i];
                if (climber != null && climber.HoldingItem)
                {

                    if (climber.HandSide == ControllerHand.Left)
                    {
                        controllerMoveAmount = previousLeftControllerPosition - LeftControllerTransform.position;
                    }
                    else
                    {
                        controllerMoveAmount = previousRightControllerPosition - RightControllerTransform.position;
                    }

                    if (count == length - 1)
                    {
                        moveDirection = controllerMoveAmount;

                        moveDirection -= climber.PreviousPosition - climber.DummyTransform.position;
                    }

                    count++;
                }
            }

            if (smoothLocomotion)
            {
                if (smoothLocomotion.ControllerType == PlayerControllerType.CharacterController)
                {
                    smoothLocomotion.MoveCharacter(moveDirection);
                }
                else if (smoothLocomotion.ControllerType == PlayerControllerType.Rigidbody)
                {
                    DoPhysicalClimbing();
                }
            }
            else if (characterController)
            {
                characterController.Move(moveDirection);
            }
        }

        for (int x = 0; x < climbers.Count; x++)
        {
            Grabber climber = climbers[x];
            if (climber != null && climber.HoldingItem)
            {
                if (climber.DummyTransform != null)
                {
                    climber.PreviousPosition = climber.DummyTransform.position;
                }
                else
                {
                    climber.PreviousPosition = climber.transform.position;
                }
            }
        }

        wasGrippingClimbable = GrippingClimbable;
    }

    private void DoPhysicalClimbing()
    {
        int count = 0;
        float length = climbers.Count;

        Vector3 movementVelocity = Vector3.zero;

        for (int i = 0; i < length; i++)
        {
            Grabber climber = climbers[i];
            if (climber != null && climber.HoldingItem)
            {

                Vector3 positionDelta = climber.transform.position - climber.DummyTransform.position;

                if (count == length - 1)
                {
                    movementVelocity = positionDelta;

                    movementVelocity -= climber.PreviousPosition - climber.DummyTransform.position;
                }

                count++;
            }
        }

        if (movementVelocity.magnitude > 0)
        {
            rb.velocity = Vector3.MoveTowards(rb.velocity, (-movementVelocity * 2000f) * Time.fixedDeltaTime, 1f);
        }
    }

    private void onGrabbedClimbable()
    {
        if (smoothLocomotion)
        {
            smoothLocomotion.DisableMovement();
        }

        if (playerGravity)
        {
            playerGravity.ToggleGravity(false);
        }
    }

    private void onReleasedClimbable()
    {
        if (smoothLocomotion)
        {
            smoothLocomotion.EnableMovement();
        }

        if (playerGravity)
        {
            playerGravity.ToggleGravity(true);
        }
    }
}