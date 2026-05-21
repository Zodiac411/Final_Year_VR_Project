using UnityEngine;

public class PlayerGravity : MonoBehaviour
{
    [SerializeField] private bool GravityEnabled = true;
    [SerializeField] private Vector3 Gravity = Physics.gravity;

    private CharacterController characterController;
    private SmoothLocomotion smoothLocomotion;

    private Rigidbody rb;

    private float _movementY;
    private Vector3 _initialGravityModifier;

    private bool _validRigidBody = false;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        smoothLocomotion = GetComponentInChildren<SmoothLocomotion>();
        rb = GetComponent<Rigidbody>();

        _validRigidBody = rb != null;
        _initialGravityModifier = Gravity;
    }

    private void LateUpdate()
    {
        if (GravityEnabled && characterController != null && characterController.enabled)
        {
            _movementY += Gravity.y * Time.deltaTime;

            if (smoothLocomotion)
            {
                smoothLocomotion.MoveCharacter(new Vector3(0, _movementY, 0) * Time.deltaTime);
            }
            else if (characterController)
            {
                characterController.Move(new Vector3(0, _movementY, 0) * Time.deltaTime);
            }

            if (characterController.isGrounded)
            {
                _movementY = 0;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_validRigidBody && GravityEnabled)
        {
            if (smoothLocomotion && smoothLocomotion.ControllerType == 
                PlayerControllerType.Rigidbody && smoothLocomotion.GroundContacts < 1)
            {
                // LEAVE TO BE NULLLL
            }

            rb.AddForce(Gravity * rb.mass);
        }
    }

    public void ToggleGravity(bool gravityOn)
    {
        GravityEnabled = gravityOn;

        if (gravityOn)
        {
            Gravity = _initialGravityModifier;
        }
        else
        {
            Gravity = Vector3.zero;
        }
    }
}