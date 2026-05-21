using UnityEngine;

public class HandJet : GrabbableEvents
{
    [SerializeField] private float JetForce = 10f;
    [SerializeField] private ParticleSystem JetFX;
    [SerializeField] private bool DisableGravityWhileHeld = true;

    private CharacterController characterController;
    private SmoothLocomotion smoothLocomotion;
    private PlayerGravity playerGravity;
    private Rigidbody playerRigid;

    private AudioSource audioSource;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player)
        {
            characterController = player.GetComponentInChildren<CharacterController>();
            playerGravity = player.GetComponentInChildren<PlayerGravity>();
            smoothLocomotion = player.GetComponentInChildren<SmoothLocomotion>();
            playerRigid = player.GetComponent<Rigidbody>();
        }
        else
        {
            Debug.Log("No player object found.");
        }

        audioSource = GetComponent<AudioSource>();
    }

    public override void OnTrigger(float triggerValue)
    {

        if (triggerValue > 0.25f)
        {
            doJet(triggerValue);
        }
        else
        {
            stopJet();
        }

        base.OnTrigger(triggerValue);
    }

    public void FixedUpdate()
    {
        if (grab != null && grab.BeingHeld && addRigidForce != null && addRigidForce != Vector3.zero)
        {
            if (playerRigid)
            {
                playerRigid.AddForce(addRigidForce, ForceMode.Force);
            }
        }
    }

    public override void OnGrab(Grabber grabber)
    {
        if (DisableGravityWhileHeld)
        {
            ChangeGravity(false);
        }

    }

    public void ChangeGravity(bool gravityOn)
    {
        if (playerGravity)
        {
            playerGravity.ToggleGravity(gravityOn);
        }
    }

    public override void OnRelease()
    {
        stopJet();

        if (DisableGravityWhileHeld)
        {
            ChangeGravity(true);
        }
    }

    Vector3 moveDirection;
    Vector3 addRigidForce;

    private void doJet(float triggerValue)
    {
        moveDirection = transform.forward * JetForce;

        if (smoothLocomotion)
        {
            if (smoothLocomotion.ControllerType == PlayerControllerType.CharacterController)
            {
                smoothLocomotion.MoveCharacter(moveDirection * Time.deltaTime * triggerValue);
            }
            else if (smoothLocomotion.ControllerType == PlayerControllerType.Rigidbody)
            {
                addRigidForce = moveDirection * triggerValue;
            }

        }
        else if (characterController)
        {
            characterController.Move(moveDirection * Time.deltaTime * triggerValue);
        }

        ChangeGravity(false);

        if (!audioSource.isPlaying)
        {
            audioSource.pitch = Time.timeScale;
            audioSource.Play();
        }

        if (JetFX != null && !JetFX.isPlaying)
        {
            JetFX.Play();
        }

        if (input && thisGrabber != null)
        {
            input.VibrateController(0.1f, 0.5f, 0.2f, thisGrabber.HandSide);
        }
    }

    private void stopJet()
    {

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        if (JetFX != null && JetFX.isPlaying)
        {
            JetFX.Stop();
        }

        if (DisableGravityWhileHeld == false)
        {
            ChangeGravity(true);
        }

        addRigidForce = Vector3.zero;
    }

    public override void OnTriggerUp()
    {
        stopJet();
        base.OnTriggerUp();
    }
}
