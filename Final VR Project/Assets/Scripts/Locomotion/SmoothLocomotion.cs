using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;

public enum MovementVector
{
    HMD,
    Controller
}

public class SmoothLocomotion : MonoBehaviour
{
    public PlayerControllerType ControllerType = PlayerControllerType.CharacterController;

    [Header("CharacterController Settings : ")]
    [SerializeField] private float MovementSpeed = 1.25f;

    [Header("Rigidbody Settings : ")]
    [SerializeField] private float MovementForce = 500f;
    [SerializeField] private float MaxHorizontalVelocity = 5f;
    [SerializeField] private float MaxVerticalVelocity = 10f;
    [SerializeField] private float StepHeight = 0.1f;
    [SerializeField] private float MaxSlopeAngle = 45f;

    [SerializeField] private PhysicMaterial MovementMaterial;
    [SerializeField] private PhysicMaterial FrictionMaterial;

    [SerializeField] private float MovementDrag = 1f;
    [SerializeField] private float StaticDrag = 5f;
    [SerializeField] private float AirDrag = 1f;

    [Header("Forward Direction : ")]
    [SerializeField] private Transform ForwardDirection;

    [Header("Input : ")]
    [SerializeField] private bool AllowInput = true;
    [SerializeField] private bool UpdateMovement = true;
    [SerializeField] private bool RequireAppFocus = true;

    [SerializeField] private List<InputAxis> inputAxis = new List<InputAxis>() { InputAxis.LeftThumbStickAxis };

    public InputActionReference MoveAction;

    [Header("Sprint : ")]
    [SerializeField] private float SprintSpeed = 1.5f;

    [SerializeField] private List<ControllerBinding> SprintInput = new List<ControllerBinding>() { ControllerBinding.None };
    [SerializeField] private InputActionReference SprintAction;

    [Header("Strafe : ")]
    [SerializeField] private float StrafeSpeed = 1f;
    [SerializeField] private float StrafeSprintSpeed = 1.25f;

    [Header("Jump : ")]
    [SerializeField] private float JumpForce = 3f;

    [SerializeField] private List<ControllerBinding> JumpInput = new List<ControllerBinding>() { ControllerBinding.None };
    [SerializeField] private InputActionReference JumpAction;

    [Header("Air Control : ")]
    [SerializeField] private bool AirControl = true;
    [SerializeField] private float AirControlSpeed = 1f;

    private PlayerController playerController;
    private CharacterController characterController;
    private Rigidbody playerRigid;
    private SphereCollider playerSphere;

    float movementX;
    float movementY;
    float movementZ;

    private float _verticalSpeed = 0;

    private Vector3 additionalMovement;

    public delegate void OnBeforeMoveAction();
    public static event OnBeforeMoveAction OnBeforeMove;

    public delegate void OnAfterMoveAction();
    public static event OnAfterMoveAction OnAfterMove;

    public virtual void Update()
    {
        CheckControllerReferences();
        UpdateInputs();

        if (UpdateMovement)
        {
            MoveCharacter();
        }
    }

    public virtual void FixedUpdate()
    {
        if (UpdateMovement && ControllerType == PlayerControllerType.Rigidbody)
        {
            MoveRigidCharacter();

            GroundContacts = 0;
        }
    }

    bool playerInitialized = false;

    public virtual void CheckControllerReferences()
    {
        if (playerController == null)
        {
            playerController = GetComponentInParent<PlayerController>();
        }

        if (playerInitialized == false)
        {

            if (ControllerType == PlayerControllerType.CharacterController && characterController == null)
            {
                SetupCharacterController();

                playerInitialized = true;
            }

            if (ControllerType == PlayerControllerType.Rigidbody && playerRigid == null)
            {
                playerRigid = GetComponent<Rigidbody>();
                playerSphere = GetComponent<SphereCollider>();

                if (playerRigid == null)
                {
                    SetupRigidbodyPlayer();
                }
            }
        }
    }


    public virtual void UpdateInputs()
    {
        movementX = 0;
        movementY = 0;
        movementZ = 0;

        if (AllowInput == false)
        {
            return;
        }

        Vector2 primaryAxis = GetMovementAxis();
        if (IsGrounded())
        {
            movementX = primaryAxis.x;
            movementZ = primaryAxis.y;
        }
        else if (AirControl)
        {
            movementX = primaryAxis.x * AirControlSpeed;
            movementZ = primaryAxis.y * AirControlSpeed;
        }

        if (CheckJump())
        {
            if (ControllerType == PlayerControllerType.CharacterController)
            {
                movementY += JumpForce;
            }
            else if (ControllerType == PlayerControllerType.Rigidbody)
            {
                DoRigidBodyJump();
            }
        }

        if (CheckSprint())
        {
            movementX *= StrafeSprintSpeed;
            movementZ *= SprintSpeed;
        }
        else
        {
            movementX *= StrafeSpeed;
            movementZ *= MovementSpeed;
        }
    }

    float lastJumpTime;
    float lastMoveTime;

    public virtual void DoRigidBodyJump()
    {

        if (Time.time - lastJumpTime > 0.2f)
        {

            playerRigid.AddForce(new Vector3(playerRigid.velocity.x, JumpForce, playerRigid.velocity.z), ForceMode.VelocityChange);

            lastJumpTime = Time.time;
        }
    }

    public virtual Vector2 GetMovementAxis()
    {
        Vector3 lastAxisValue = Vector3.zero;

        if (inputAxis != null)
        {
            for (int i = 0; i < inputAxis.Count; i++)
            {
                Vector3 axisVal = XRInput.Instance.GetInputAxisValue(inputAxis[i]);

                if (lastAxisValue == Vector3.zero)
                {
                    lastAxisValue = axisVal;
                }
                else if (axisVal != Vector3.zero && axisVal.magnitude > lastAxisValue.magnitude)
                {
                    lastAxisValue = axisVal;
                }
            }
        }

        bool hasRequiredFocus = RequireAppFocus == false || RequireAppFocus && Application.isFocused;
        if (MoveAction != null && hasRequiredFocus)
        {
            Vector3 axisVal = MoveAction.action.ReadValue<Vector2>();

            if (lastAxisValue == Vector3.zero)
            {
                lastAxisValue = axisVal;
            }
            else if (axisVal != Vector3.zero && axisVal.magnitude > lastAxisValue.magnitude)
            {
                lastAxisValue = axisVal;
            }
        }

        return lastAxisValue;
    }

    public virtual void MoveCharacter()
    {
        if (UpdateMovement == false)
        {
            return;
        }

        Vector3 moveDirection = new Vector3(movementX, movementY, movementZ);

        if (ForwardDirection != null)
        {
            moveDirection = ForwardDirection.TransformDirection(moveDirection);
        }
        else
        {
            moveDirection = transform.TransformDirection(moveDirection);
        }

        if (playerController != null && playerController.IsGrounded())
        {
            _verticalSpeed = 0;
            if (CheckJump())
            {
                _verticalSpeed = JumpForce;
            }
        }

        moveDirection.y = _verticalSpeed;

        if (playerController)
        {
            playerController.LastPlayerMoveTime = Time.time;
        }

        if (moveDirection != Vector3.zero)
        {
            MoveCharacter(moveDirection * Time.deltaTime);
        }
    }

    public virtual void MoveRigidCharacter(Vector3 moveTo) { }

    public virtual void MoveRigidCharacter()
    {
        float maxVelocityChange = 10f;

        if (playerRigid)
        {
            Vector3 moveDirection = new Vector3(movementX, movementY, movementZ);

            if (ForwardDirection != null)
            {
                moveDirection = ForwardDirection.TransformDirection(moveDirection);
            }
            else
            {
                moveDirection = transform.TransformDirection(moveDirection);
            }

            Vector3 targetVelocity = moveDirection * MovementForce;
            Vector3 movement = Vector3.zero;

            Vector3 currentVelocity = playerRigid.velocity;
            Vector3 velocityTarget = (targetVelocity - currentVelocity);
            bool recentlyJumped = Time.time - lastJumpTime < 0.1f;

            if (IsGrounded())
            {

                if (!recentlyJumped)
                {
                    if (maxVelocityChange > 0)
                    {
                        velocityTarget.x = Mathf.Clamp(velocityTarget.x, -maxVelocityChange, maxVelocityChange);
                        velocityTarget.z = Mathf.Clamp(velocityTarget.z, -maxVelocityChange, maxVelocityChange);
                    }

                    if (moveDirection.magnitude > 0.001f)
                    {
                        if (playerSphere)
                        {
                            playerSphere.material = MovementMaterial;
                        }

                        playerRigid.drag = MovementDrag;
                    }
                    else
                    {
                        if (playerSphere)
                        {
                            playerSphere.material = FrictionMaterial;
                        }

                        playerRigid.drag = StaticDrag;
                    }

                    if (moveDirection.magnitude > 0.001f)
                    {
                        playerRigid.AddForce(velocityTarget, ForceMode.VelocityChange);
                    }
                }
            }
            else
            {

                playerRigid.drag = AirDrag;

                if (AirControl)
                {

                    if (!recentlyJumped)
                    {
                        Vector3 move = (targetVelocity) * AirControlSpeed;
                        move.y = 0;

                        playerRigid.AddForce(move, ForceMode.Acceleration);
                    }
                }
            }

            var adjustedVelocity = new Vector3(playerRigid.velocity.x, 0, playerRigid.velocity.z);
            if (adjustedVelocity.magnitude > MaxHorizontalVelocity)
            {
                adjustedVelocity = Vector3.ClampMagnitude(adjustedVelocity, MaxHorizontalVelocity);
                adjustedVelocity = new Vector3(adjustedVelocity.x, playerRigid.velocity.y, adjustedVelocity.z);

                playerRigid.velocity = adjustedVelocity;
            }

            if (Mathf.Abs(playerRigid.velocity.y) > MaxVerticalVelocity)
            {
                playerRigid.velocity = new Vector3(playerRigid.velocity.x, Mathf.Clamp(playerRigid.velocity.y, -MaxVerticalVelocity, MaxVerticalVelocity), playerRigid.velocity.z);
            }
        }
    }

    public float Magnitude;

    public virtual void MoveCharacter(Vector3 motion)
    {
        if (motion == null || motion == Vector3.zero)
        {
            return;
        }

        Magnitude = (float)Math.Round(motion.magnitude * 1000f) / 1000f;

        bool callEvents = Magnitude > 0.0f;

        CheckControllerReferences();

        if (callEvents)
        {
            OnBeforeMove?.Invoke();
        }

        if (ControllerType == PlayerControllerType.CharacterController)
        {
            if (characterController && characterController.enabled)
            {
                characterController.Move(motion);
            }
        }
        else if (ControllerType == PlayerControllerType.Rigidbody) { }

        if (callEvents)
        {
            OnAfterMove?.Invoke();
        }
    }

    public virtual bool CheckJump()
    {
        if (!IsGrounded())
        {
            return false;
        }
        for (int x = 0; x < JumpInput.Count; x++)
        {
            if (XRInput.Instance.GetControllerBindingValue(JumpInput[x]))
            {
                return true;
            }
        }
        if (JumpAction != null && JumpAction.action.ReadValue<float>() > 0)
        {
            return true;
        }

        return false;
    }

    public virtual bool CheckSprint()
    {
        for (int x = 0; x < SprintInput.Count; x++)
        {
            if (XRInput.Instance.GetControllerBindingValue(SprintInput[x]))
            {
                return true;
            }
        }
        if (SprintAction != null)
        {
            return SprintAction.action.ReadValue<float>() == 1f;
        }

        return false;
    }

    public virtual bool IsGrounded()
    {
        if (playerController && playerController.IsGrounded())
        {
            return true;
        }
        else if (GroundContacts > 0)
        {
            return true;
        }

        return false;
    }

    public virtual void SetupCharacterController()
    {
        playerRigid = GetComponent<Rigidbody>();
        PlayerController bng = GetComponent<PlayerController>();
        if (bng)
        {
            bng.DistanceFromGroundOffset = 0f;
        }

        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.skinWidth = 0.001f;
            characterController.center = new Vector3(0, -0.25f, 0);
            characterController.radius = 0.1f;
            characterController.height = 1.5f;
        }

        playerInitialized = true;
    }

    public virtual void SetupRigidbodyPlayer()
    {
        playerRigid = gameObject.AddComponent<Rigidbody>();
        playerRigid.mass = 50f;
        playerRigid.drag = 1f;
        playerRigid.angularDrag = 0.05f;
        playerRigid.freezeRotation = true;

        CharacterController charController = GetComponent<CharacterController>();
        if (charController != null)
        {
            GameObject.Destroy(charController);
        }

        PlayerController bng = GetComponent<PlayerController>();
        if (bng)
        {
            bng.DistanceFromGroundOffset = -0.087f;
        }

        CapsuleCollider cc = gameObject.AddComponent<CapsuleCollider>();
        cc.radius = 0.1f;
        cc.center = new Vector3(0, 0.785f, 0);
        cc.height = 1.25f;

        playerSphere = gameObject.AddComponent<SphereCollider>();
        playerSphere.center = new Vector3(0, 0.079f, 0);
        playerSphere.radius = 0.08f;

        playerInitialized = true;
    }

    public virtual void EnableMovement()
    {
        UpdateMovement = true;
    }

    public virtual void DisableMovement()
    {
        UpdateMovement = false;
    }

    [Header("Shown for Debug : ")]
    public int GroundContacts = 0;

    [SerializeField] private float SurfaceAngle = 0f;
    [SerializeField] private float SurfaceHeight = 0f;

    void OnCollisionStay(Collision collisionInfo)
    {
        if (ControllerType == PlayerControllerType.Rigidbody)
        {
            for (int x = 0; x < collisionInfo.contacts.Length; x++)
            {
                ContactPoint contact = collisionInfo.contacts[x];
                SurfaceAngle = Vector3.Angle(contact.normal, transform.up);
                SurfaceHeight = Mathf.Abs(contact.point.y - transform.position.y);

                if (SurfaceHeight < 0.0001)
                {
                    SurfaceHeight = 0;
                }

                if (SurfaceHeight <= StepHeight)
                {
                    GroundContacts++;
                }
                else if (SurfaceAngle <= MaxSlopeAngle)
                {
                    GroundContacts++;
                }

                Debug.DrawLine(contact.point, transform.position);
            }
        }
    }
}

public enum PlayerControllerType
{
    CharacterController,
    Rigidbody
}