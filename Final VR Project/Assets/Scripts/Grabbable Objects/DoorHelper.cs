using UnityEngine;

public class DoorHelper : MonoBehaviour
{
    [SerializeField] private Transform HandleFollower;
    [SerializeField] private Transform DoorLockTransform;

    [SerializeField] private AudioClip DoorOpenSound;
    [SerializeField] private AudioClip DoorCloseSound;

    [Header("Door Settings")]
    [SerializeField] private float DegreesTurned;
    [SerializeField] private float DegreesTurnToOpen = 10f;
    [SerializeField] private float AngularVelocitySnapDoor = 0.2f;
    [SerializeField] private float angle;
    [SerializeField] private float AngularVelocity = 0.2f;

    [SerializeField] private bool DoorIsLocked = false;
    [SerializeField] private bool RequireHandleTurnToOpen = false;

    private float moveLockAmount, rotateAngles, ratio;
    private float initialLockPosition;
    public float lockPos;

    private HingeJoint hinge;
    private Rigidbody rb;
    private Vector3 currentRotation;

    private bool playedOpenSound = false;
    private bool handleLocked = false;
    private bool readyToPlayCloseSound = false;

    private void Start()
    {
        hinge = GetComponent<HingeJoint>();
        rb = GetComponent<Rigidbody>();

        if (DoorLockTransform)
        {
            initialLockPosition = DoorLockTransform.transform.localPosition.x;
        }
    }

    private void Update()
    {
        AngularVelocity = rb.angularVelocity.magnitude;

        currentRotation = transform.localEulerAngles;
        angle = Mathf.Floor(currentRotation.y);

        if (angle >= 180)
        {
            angle -= 180;
        }
        else
        {
            angle = 180 - angle;
        }

        if (angle > 10)
        {
            if (!playedOpenSound)
            {
                XRManager.Instance.PlaySpatialClipAt(DoorOpenSound, transform.position, 1f, 1f);
                playedOpenSound = true;
            }
        }

        if (angle > 30)
        {
            readyToPlayCloseSound = true;
        }

        if (angle < 2 && playedOpenSound)
        {
            playedOpenSound = false;
        }

        if (angle < 1 && AngularVelocity <= AngularVelocitySnapDoor)
        {
            if (!rb.isKinematic)
            {
                rb.angularVelocity = Vector3.zero;
            }
        }

        if (readyToPlayCloseSound && angle < 2)
        {
            XRManager.Instance.PlaySpatialClipAt(DoorCloseSound, transform.position, 1f, 1f);
            readyToPlayCloseSound = false;
        }

        if (HandleFollower)
        {
            DegreesTurned = Mathf.Abs(HandleFollower.localEulerAngles.y - 270);
        }

        if (DoorLockTransform)
        {
            moveLockAmount = 0.025f;
            rotateAngles = 55;
            ratio = rotateAngles / (rotateAngles - Mathf.Clamp(DegreesTurned, 0, rotateAngles));
            lockPos = initialLockPosition - (ratio * moveLockAmount) + moveLockAmount;
            lockPos = Mathf.Clamp(lockPos, initialLockPosition - moveLockAmount, initialLockPosition);

            DoorLockTransform.transform.localPosition = new Vector3(lockPos, DoorLockTransform.transform.localPosition.y, DoorLockTransform.transform.localPosition.z);
        }

        if (RequireHandleTurnToOpen)
        {
            handleLocked = DegreesTurned < DegreesTurnToOpen;
        }

        if (angle < 0.02f && (handleLocked || DoorIsLocked))
        {
            if (rb.collisionDetectionMode == CollisionDetectionMode.Continuous || rb.collisionDetectionMode == CollisionDetectionMode.ContinuousDynamic)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }

            rb.isKinematic = true;
        }
        else
        {
            if (rb.collisionDetectionMode == CollisionDetectionMode.ContinuousSpeculative)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            rb.isKinematic = false;
        }
    }
}