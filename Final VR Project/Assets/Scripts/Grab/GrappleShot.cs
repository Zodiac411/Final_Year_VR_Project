using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class GrappleShot : GrabbableEvents
{
    [Header("Range")]
    [SerializeField] private float MaxRange = 100f;
    [SerializeField] private float currentGrappleDistance = 0;

    [Header("CharacterController Grapple Settings")]
    [SerializeField] private float GrappleReelForce = 0.5f;
    [SerializeField] private float MinReelDistance = 0.25f;

    [Header("Rigidbody Grapple Settings")]
    [SerializeField] private float GrappleForce = 3f;
    [SerializeField] private ForceMode GrappleForceMode = ForceMode.Acceleration;
    [SerializeField] private Climbable ClimbHelper;

    [Header("Raycast Layers")]
    [SerializeField] private LayerMask GrappleLayers;

    [Header("Component definitions")]
    [SerializeField] private Transform MuzzleTransform;
    [SerializeField] private Transform HitTargetPrefab;
    [SerializeField] private LineRenderer GrappleLine;
    [SerializeField] private LineRenderer HelperLine;

    [SerializeField] private AudioClip GrappleShotSound;

    private bool grappling = false;
    private bool wasGrappling = false;
    private bool validTargetFound = false;
    private bool isDynamic = false;
    private bool requireRelease = false;
    private bool climbing = false;

    private CharacterController characterController;
    private SmoothLocomotion smoothLocomotion;
    private PlayerGravity playerGravity;
    private PlayerClimbing playerClimbing;
    private Rigidbody rb;
    private Rigidbody grappleTargetRigid;
    private Collider grappleTargetCollider;
    private Transform grappleTargetParent;
    private AudioSource audioSource;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            characterController = player.GetComponentInChildren<CharacterController>();
            smoothLocomotion = player.GetComponentInChildren<SmoothLocomotion>();
            playerGravity = player.GetComponentInChildren<PlayerGravity>();
            playerClimbing = player.GetComponentInChildren<PlayerClimbing>();
            rb = player.GetComponent<Rigidbody>();
        }
        else
        {
            Debug.Log("No player object found.");
        }

        audioSource = GetComponent<AudioSource>();
    }

    private void LateUpdate()
    {
        if (!grappling && grab.BeingHeld && !requireRelease)
        {
            drawGrappleHelper();
        }
        else
        {
            hideGrappleHelper();
        }

        if (grappling && validTargetFound && grab.BeingHeld)
        {
            drawGrappleLine();
        }
        else
        {
            hideGrappleLine();
        }
    }

    public override void OnTrigger(float triggerValue)
    {

        updateGrappleDistance();

        // Fire gun if possible
        if (triggerValue >= 0.25f)
        {
            if (grappling)
            {
                reelInGrapple(triggerValue);
            }
            else
            {
                shootGrapple();
            }
        }
        else
        {
            grappling = false;
            requireRelease = false;
        }

        // User was grappling previous frame, but not now
        if (!grappling && wasGrappling)
        {
            onReleaseGrapple();
        }

        base.OnTrigger(triggerValue);
    }

    private void updateGrappleDistance()
    {
        // Update Distance
        if (grappling)
        {
            currentGrappleDistance = Vector3.Distance(MuzzleTransform.position, HitTargetPrefab.position);
        }
        else
        {
            currentGrappleDistance = 0;
        }
    }

    public override void OnGrab(Grabber grabber)
    {
        base.OnGrab(grabber);
    }

    public override void OnRelease()
    {
        onReleaseGrapple();

        base.OnRelease();
    }

    // Called when grappling previous frame, but not this one
    private void onReleaseGrapple()
    {

        // Reset gravity back to normal
        changeGravity(true);

        if (grappleTargetRigid && isDynamic)
        {
            grappleTargetRigid.useGravity = true;
            grappleTargetRigid.isKinematic = false;
            grappleTargetRigid.transform.parent = grappleTargetParent;

            // More reliable method of resetting parent :
            if (grappleTargetRigid.GetComponent<Grabbable>())
            {
                grappleTargetRigid.GetComponent<Grabbable>().ResetParent();
            }
        }

        // Reset Climbing
        ClimbHelper.transform.localPosition = Vector3.zero;
        playerClimbing.RemoveClimber(thisGrabber);
        climbing = false;

        grappling = false;
        validTargetFound = false;
        isDynamic = false;
        wasGrappling = false;
    }

    // Draw area where Grapple will land
    private void drawGrappleHelper()
    {

        if (HitTargetPrefab)
        {

            RaycastHit hit;
            if (Physics.Raycast(MuzzleTransform.position, MuzzleTransform.forward, out hit, MaxRange, GrappleLayers, QueryTriggerInteraction.Ignore))
            {

                // Ignore other grapple shots
                if (hit.transform.name.StartsWith("Grapple"))
                {
                    hideGrappleHelper();
                    validTargetFound = false;
                    isDynamic = false;
                    return;
                }

                showGrappleHelper(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));

                grappleTargetRigid = hit.collider.GetComponent<Rigidbody>();
                grappleTargetCollider = hit.collider;
                isDynamic = grappleTargetRigid != null && !grappleTargetRigid.isKinematic && grappleTargetRigid.useGravity;

                // Parent the helper to the object we hit so it will be moved with it
                // Only allow uniform scale so it doesn't warp
                bool isUniformVector = hit.collider.transform.localScale.x == hit.collider.transform.localScale.y;
                if (isUniformVector || isDynamic)
                {
                    HitTargetPrefab.parent = null;
                    HitTargetPrefab.localScale = Vector3.one;
                    HitTargetPrefab.transform.parent = hit.collider.transform;

                }
                else
                {
                    HitTargetPrefab.parent = null;
                    HitTargetPrefab.localScale = Vector3.one;
                }

                validTargetFound = true;

                if (isDynamic)
                {
                    grappleTargetParent = grappleTargetRigid.transform.parent;
                }
                else
                {
                    grappleTargetParent = null;
                }
            }
            else
            {
                hideGrappleHelper();
                validTargetFound = false;
                isDynamic = false;
            }
        }
    }

    private void drawGrappleLine()
    {
        GrappleLine.gameObject.SetActive(true);
        GrappleLine.SetPosition(0, MuzzleTransform.position);
        GrappleLine.SetPosition(1, HitTargetPrefab.position);
    }

    private void hideGrappleLine()
    {
        if (GrappleLine && GrappleLine.gameObject.activeSelf)
        {
            GrappleLine.gameObject.SetActive(false);
        }
    }

    private void showGrappleHelper(Vector3 pos, Quaternion rot)
    {

        HitTargetPrefab.gameObject.SetActive(true);

        HitTargetPrefab.position = pos;
        HitTargetPrefab.rotation = rot;
        HitTargetPrefab.localScale = Vector3.one;

        if (HelperLine)
        {
            HelperLine.gameObject.SetActive(true);
            HelperLine.SetPosition(0, MuzzleTransform.position);
            HelperLine.SetPosition(1, pos);
        }
    }

    private void hideGrappleHelper()
    {
        if (HitTargetPrefab && HitTargetPrefab.gameObject.activeSelf)
        {
            HitTargetPrefab.gameObject.SetActive(false);
        }

        if (HelperLine && HelperLine.gameObject.activeSelf)
        {
            HelperLine.gameObject.SetActive(false);
        }
    }

    private void reelInGrapple(float triggerValue)
    {

        // Has the collider been destroyed or disabled?
        if (validTargetFound && grappleTargetCollider != null && !grappleTargetCollider.enabled)
        {
            dropGrapple();
            return;
        }

        if (validTargetFound && currentGrappleDistance > MinReelDistance)
        {

            // Move object towards our hand
            if (isDynamic)
            {
                grappleTargetRigid.isKinematic = false;
                grappleTargetRigid.transform.parent = grappleTargetParent;
                grappleTargetRigid.useGravity = false;
                grappleTargetRigid.AddForce((MuzzleTransform.position - grappleTargetRigid.transform.position) * 0.1f, ForceMode.VelocityChange);

                //r.MovePosition(MuzzleTransform.position * Time.deltaTime);
            }
            // Move character towards Hit location
            else
            {
                Vector3 moveDirection = (HitTargetPrefab.position - MuzzleTransform.position) * GrappleReelForce;

                // Turn off gravity before we move
                changeGravity(false);

                // Use smooth loco method if available
                if (smoothLocomotion)
                {
                    if (smoothLocomotion.ControllerType == PlayerControllerType.CharacterController)
                    {
                        smoothLocomotion.MoveCharacter(moveDirection * Time.deltaTime * triggerValue);
                    }
                    else if (smoothLocomotion.ControllerType == PlayerControllerType.Rigidbody)
                    {
                        if (GrappleForceMode == ForceMode.VelocityChange)
                        {
                            rb.velocity = Vector3.MoveTowards(rb.velocity, (moveDirection * GrappleForce) * Time.fixedDeltaTime, 1f);
                        }
                        else
                        {
                            rb.AddForce(moveDirection * GrappleForce, GrappleForceMode);
                        }
                    }
                }
                // Fall back to character controller
                else if (characterController)
                {
                    characterController.Move(moveDirection * Time.deltaTime * triggerValue);
                }
            }
        }
        else if (validTargetFound && currentGrappleDistance <= MinReelDistance)
        {

            if (isDynamic)
            {
                //grappleTargetRigid.useGravity = true;
                grappleTargetRigid.velocity = Vector3.zero;
                grappleTargetRigid.isKinematic = true;
                grappleTargetRigid.transform.parent = transform;
            }

            if (!climbing && !isDynamic)
            {
                // Add climbable / grabber
                ClimbHelper.transform.localPosition = Vector3.zero;
                playerClimbing.AddClimber(ClimbHelper, thisGrabber);
                climbing = true;
            }

        }
    }

    // Shoot the valid out if valid target
    private void shootGrapple()
    {

        if (validTargetFound)
        {

            // Play Grapple Sound
            if (GrappleShotSound && audioSource)
            {
                audioSource.clip = GrappleShotSound;
                audioSource.pitch = Time.timeScale;
                audioSource.Play();
            }

            grappling = true;
            wasGrappling = true;
            requireRelease = true;
        }
    }

    private void dropGrapple()
    {
        grappling = false;
        validTargetFound = false;
        isDynamic = false;
        wasGrappling = false;
    }

    private void changeGravity(bool gravityOn)
    {
        if (playerGravity)
        {
            playerGravity.ToggleGravity(gravityOn);
        }
    }
}