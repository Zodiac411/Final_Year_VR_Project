using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using System;

public enum LocomotionType
{
    Teleport,
    SmoothLocomotion,
    None
}

public class PlayerController : MonoBehaviour
{

    [Header("Camera Options : ")]
    public bool MoveCharacterWithCamera = true;
    public bool RotateCharacterWithCamera = true;
    public bool ResizeCharacterHeightWithCamera = true;

    [Header("Transform Setup ")]
    [SerializeField] private Transform TrackingSpace;
    public Transform CameraRig;
    public Transform CenterEyeAnchor;

    [Header("Ground checks : ")]
    [SerializeField] private LayerMask GroundedLayers;
    [SerializeField] private float DistanceFromGround = 0;
    public float DistanceFromGroundOffset = 0;

    [Header("Player Capsule Settings : ")]
    [SerializeField] private float MinimumCapsuleHeight = 0.4f;
    [SerializeField] private float MaximumCapsuleHeight = 3f;

    [HideInInspector]
    public float LastTeleportTime;

    [Header("Player Y Offset : ")]
    public float CharacterControllerYOffset = -0.025f;

    [HideInInspector]
    public float CameraHeight;

    [Header("Misc : ")]
    public bool ElevateCameraIfNoHMDPresent = true;

    public float ElevateCameraHeight = 1.65f;
    public float MinElevation = -6000f;
    public float MaxElevation = 6000f;

    [HideInInspector]
    public float LastPlayerMoveTime;

    protected CharacterController characterController;

    protected Rigidbody rb;
    protected CapsuleCollider capsuleCol;

    protected SmoothLocomotion smoothLocomotion;

    protected PlayerClimbing playerClimbing;
    protected bool isClimbing, wasClimbing = false;

    [SerializeField] private RaycastHit groundHit;

    protected RaycastHit hit;
    protected Transform mainCamera;

    private Vector3 _initialPosition;

    void Start()
    {
        characterController = GetComponentInChildren<CharacterController>();
        rb = GetComponent<Rigidbody>();
        capsuleCol = GetComponent<CapsuleCollider>();
        smoothLocomotion = GetComponentInChildren<SmoothLocomotion>();

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;

        if (characterController)
        {
            _initialPosition = characterController.transform.position;
        }
        else if (rb)
        {
            _initialPosition = rb.position;
        }
        else
        {
            _initialPosition = transform.position;
        }

        playerClimbing = GetComponentInChildren<PlayerClimbing>();
    }

    void Update()
    {
        if (mainCamera == null && Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }

        isClimbing = playerClimbing != null && playerClimbing.GrippingAtLeastOneClimbable();
        if (isClimbing != wasClimbing)
        {
            OnClimbingChange();
        }

        if (ResizeCharacterHeightWithCamera)
        {
            UpdateCharacterHeight();
        }

        UpdateCameraRigPosition();

        if (playerClimbing != null && playerClimbing.GrippingAtLeastOneClimbable() && characterController != null)
        {
            characterController.height = playerClimbing.ClimbingCapsuleHeight;
        }

        if (playerClimbing != null && playerClimbing.GrippingAtLeastOneClimbable() && rb != null)
        {
            capsuleCol.height = playerClimbing.ClimbingCapsuleHeight;
        }

        UpdateCameraHeight();

        CheckCharacterCollisionMove();

        // Align TrackingSpace with Camera
        if (RotateCharacterWithCamera)
        {
            RotateTrackingSpaceToCamera();
        }
    }

    void FixedUpdate()
    {
        UpdateDistanceFromGround();
        CheckPlayerElevationRespawn();
    }

    public virtual void CheckPlayerElevationRespawn()
    {
        if (MinElevation == 0 && MaxElevation == 0)
        {
            return;
        }

        if (characterController != null && (characterController.transform.position.y < MinElevation || characterController.transform.position.y > MaxElevation))
        {
            //Debug.Log("plr not in bounds.");
            characterController.transform.position = _initialPosition;
        }

        if (rb != null && (rb.transform.position.y < MinElevation || rb.transform.position.y > MaxElevation))
        {
            //Debug.Log("plr not in bounds.");
            rb.transform.position = _initialPosition;
        }
    }

    public virtual void UpdateDistanceFromGround()
    {

        if (characterController)
        {
            if (Physics.Raycast(characterController.transform.position, -characterController.transform.up, out groundHit, 20, GroundedLayers, QueryTriggerInteraction.Ignore))
            {
                DistanceFromGround = Vector3.Distance(characterController.transform.position, groundHit.point);
                DistanceFromGround += characterController.center.y;
                DistanceFromGround -= (characterController.height * 0.5f) + characterController.skinWidth;

                DistanceFromGround = (float)Math.Round(DistanceFromGround * 1000f) / 1000f;
            }
            else
            {
                DistanceFromGround = float.MaxValue;
            }
        }

        if (rb)
        {
            if (Physics.Raycast(capsuleCol.transform.position, -capsuleCol.transform.up, out groundHit, 20, GroundedLayers, QueryTriggerInteraction.Ignore))
            {
                DistanceFromGround = Vector3.Distance(capsuleCol.transform.position, groundHit.point);
                DistanceFromGround += capsuleCol.center.y;
                DistanceFromGround -= (capsuleCol.height * 0.5f);

                DistanceFromGround = (float)Math.Round(DistanceFromGround * 1000f) / 1000f;
            }
            else
            {
                DistanceFromGround = float.MaxValue;
            }
        }

        else
        {
            if (Physics.Raycast(transform.position, -transform.up, out groundHit, 20, GroundedLayers, QueryTriggerInteraction.Ignore))
            {
                DistanceFromGround = Vector3.Distance(transform.position, groundHit.point) - 0.0875f;
                DistanceFromGround = (float)Math.Round(DistanceFromGround * 1000f) / 1000f;
            }
            else
            {
                DistanceFromGround = float.MaxValue;
            }
        }

        if (DistanceFromGround != float.MaxValue)
        {
            DistanceFromGround -= DistanceFromGroundOffset;
        }

        if (DistanceFromGround < 0.001f && DistanceFromGround > -0.001f)
        {
            DistanceFromGround = 0;
        }
    }

    public virtual void RotateTrackingSpaceToCamera()
    {
        Vector3 initialPosition = TrackingSpace.position;
        Quaternion initialRotation = TrackingSpace.rotation;

        if (characterController)
        {
            characterController.transform.rotation = Quaternion.Euler(0.0f, CenterEyeAnchor.rotation.eulerAngles.y, 0.0f);

            TrackingSpace.position = initialPosition;
            TrackingSpace.rotation = initialRotation;
        }
        else if (rb)
        {
            rb.transform.rotation = Quaternion.Euler(0.0f, CenterEyeAnchor.rotation.eulerAngles.y, 0.0f);

            TrackingSpace.position = initialPosition;
            TrackingSpace.rotation = initialRotation;
        }
    }

    public virtual void UpdateCameraRigPosition()
    {

        float yPos = CharacterControllerYOffset;

        // Get character controller position based on the height and center of the capsule
        if (characterController != null)
        {
            yPos = -(0.5f * characterController.height) + characterController.center.y + CharacterControllerYOffset;
        }
        // Get character controller position based on the height and center of the capsule
        else if (rb != null)
        {
            yPos = -(0.5f * capsuleCol.height) + capsuleCol.center.y + CharacterControllerYOffset;
        }

        // Offset the capsule a bit while climbing. This allows the player to more easily hoist themselves onto a ledge / platform.
        if (playerClimbing != null && playerClimbing.GrippingAtLeastOneClimbable())
        {
            //yPos = yPos - (playerClimbing.ClimbingCapsuleHeight - playerClimbing.ClimbingCapsuleCenter);
        }

        // If no HMD is active, bump our rig up a bit so it doesn't sit on the floor
        if (!XRInput.Instance.HMDActive && ElevateCameraIfNoHMDPresent)
        {
            yPos += ElevateCameraHeight;
        }

        CameraRig.transform.localPosition = new Vector3(CameraRig.transform.localPosition.x, yPos, CameraRig.transform.localPosition.z);
    }

    public virtual void UpdateCharacterHeight()
    {
        float minHeight = MinimumCapsuleHeight;
        // Increase Min Height if no HMD is present. This prevents our character from being really small
        if (!XRInput.Instance.HMDActive && minHeight < 1f)
        {
            minHeight = 1f;
        }

        // Update Character Height based on Camera Height.
        if (characterController)
        {
            characterController.height = Mathf.Clamp(CameraHeight + CharacterControllerYOffset - characterController.skinWidth, minHeight, MaximumCapsuleHeight);

            // If we are climbing set the capsule center upwards
            if (playerClimbing != null && playerClimbing.GrippingAtLeastOneClimbable())
            {
                capsuleCol.height = playerClimbing.ClimbingCapsuleHeight;
                capsuleCol.center = new Vector3(0, playerClimbing.ClimbingCapsuleCenter * 2, 0);
            }
            else if (playerClimbing != null)
            {
                characterController.center = new Vector3(0, playerClimbing.ClimbingCapsuleCenter, 0);
            }
        }
        else if (rb && capsuleCol)
        {
            capsuleCol.height = Mathf.Clamp(CameraHeight + CharacterControllerYOffset, minHeight, MaximumCapsuleHeight);
            capsuleCol.center = new Vector3(0, capsuleCol.height / 2 + (SphereColliderRadius * 2), 0);
        }
    }

    public float SphereColliderRadius = 0.08f;

    public virtual void UpdateCameraHeight()
    {
        // update camera height
        if (CenterEyeAnchor)
        {
            CameraHeight = CenterEyeAnchor.localPosition.y;
        }
    }

    /// <summary>
    /// Move the character controller to new camera position
    /// </summary>
    public virtual void CheckCharacterCollisionMove()
    {

        if (!MoveCharacterWithCamera)
        {
            return;
        }

        Vector3 initialCameraRigPosition = CameraRig.transform.position;
        Vector3 cameraPosition = CenterEyeAnchor.position;
        Vector3 movePosition = new Vector3(cameraPosition.x, transform.position.y, cameraPosition.z);
        Vector3 delta = cameraPosition - transform.position;

        // Ignore Y position
        delta.y = 0;

        // Move Character Controller and Camera Rig to Camera's delta
        if (delta.magnitude > 0.0f)
        {

            if (smoothLocomotion && smoothLocomotion.ControllerType == PlayerControllerType.CharacterController)
            {
                smoothLocomotion.MoveCharacter(delta);
            }
            else if (smoothLocomotion && smoothLocomotion.ControllerType == PlayerControllerType.Rigidbody)
            {
                CheckRigidbodyCapsuleMove(movePosition);
            }
            else if (characterController)
            {
                characterController.Move(delta);
            }

            // Move Camera Rig back into position
            CameraRig.transform.position = initialCameraRigPosition;
        }
    }

    Vector3 moveTest;

    public virtual void CheckRigidbodyCapsuleMove(Vector3 movePosition)
    {

        bool noCollision = true;
        float capsuleRadius = 0.2f;
        moveTest = movePosition;

        // Cast capsule shape at the desired position to see if it is about to hit anything
        if (Physics.SphereCast(movePosition, capsuleRadius, transform.up, out hit, capsuleCol.height / 2, GroundedLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.Log(hit.collider);
            noCollision = false;
        }

        if (noCollision)
        {
            transform.position = movePosition;
        }
    }

    public virtual bool IsGrounded()
    {

        // Immediately check for a positive from a CharacterController if it's present
        if (characterController != null)
        {
            if (characterController.isGrounded)
            {
                return true;
            }
        }

        return DistanceFromGround <= 0.007f;
    }

    public virtual void OnClimbingChange()
    {
        if (playerClimbing.GrippingAtLeastOneClimbable())
        {

        }
        else
        {

        }
    }
}