using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class LaserSword : GrabbableEvents
{
    [SerializeField] private Transform BladeTransform;
    [SerializeField] private Transform RaycastTransform;
    [SerializeField] private LayerMask LaserCollision;
    [SerializeField] private ParticleSystem CollisionParticle;

    [SerializeField] private float LaserLength = 1f;
    [SerializeField] private float LaserActivateSpeed = 10f;

    [SerializeField] private AudioSource CollisionAudio;

    [SerializeField] private bool Colliding = false;
    [SerializeField] private bool BladeEnabled = false;

    Grabbable grabbable;

    bool SaberSwitchOn = false;

    private void Start()
    {
        grabbable = GetComponent<Grabbable>();

        if (CollisionParticle != null)
        {
            CollisionParticle.Stop();
        }
    }

    private void Update()
    {
        if (grabbable.BeingHeld && input.BButtonDown)
        {
            SaberSwitchOn = !SaberSwitchOn;
        }

        if (BladeEnabled || SaberSwitchOn)
        {
            BladeTransform.localScale = Vector3.Lerp(BladeTransform.localScale, Vector3.one, Time.deltaTime * LaserActivateSpeed);
        }
        else
        {
            BladeTransform.localScale = Vector3.Lerp(BladeTransform.localScale, new Vector3(1, 0, 1), Time.deltaTime * LaserActivateSpeed);
        }

        BladeTransform.gameObject.SetActive(BladeTransform.localScale.y >= 0.01);

        checkCollision();

        if (Colliding)
        {
            CollisionAudio.pitch = 2f;
        }
        else
        {
            CollisionAudio.pitch = 1f;
        }
    }

    public override void OnTrigger(float triggerValue)
    {
        BladeEnabled = triggerValue > 0.2f;

        base.OnTrigger(triggerValue);
    }

    private void checkCollision()
    {
        Colliding = false;

        if (BladeEnabled == false && !SaberSwitchOn)
        {
            CollisionParticle.Pause();
            return;
        }

        RaycastHit hit;
        Physics.Raycast(RaycastTransform.position, RaycastTransform.up, out hit, LaserLength, LaserCollision, QueryTriggerInteraction.Ignore);

        if (hit.collider != null)
        {
            if (CollisionParticle != null)
            {

                float distance = Vector3.Distance(hit.point, RaycastTransform.transform.position);
                float percentage = distance / LaserLength;
                BladeTransform.localScale = new Vector3(BladeTransform.localScale.x, percentage, BladeTransform.localScale.z);

                CollisionParticle.transform.parent.position = hit.point;
                CollisionParticle.transform.parent.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                if (!CollisionParticle.isPlaying)
                {
                    CollisionParticle.Play();
                }

                input.VibrateController(0.2f, 0.1f, 0.1f, thisGrabber.HandSide);
                Colliding = true;
            }
        }
        else
        {
            if (CollisionParticle != null)
            {
                CollisionParticle.Pause();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (RaycastTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(RaycastTransform.position, RaycastTransform.position + RaycastTransform.up * LaserLength);
        }
    }
}
