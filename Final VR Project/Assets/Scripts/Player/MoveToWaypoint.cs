using UnityEngine;

public class MoveToWaypoint : MonoBehaviour
{
    [SerializeField] private Waypoint Destination;
    [SerializeField] private Vector3 PositionDifference;

    [SerializeField] private bool IsActive = true;
    [SerializeField] private bool ReachedDestination = false;

    [SerializeField] private float MovementSpeed = 1f;
    [SerializeField] private float StartDelay = 0f;

    private Vector3 previousPosition;
    private Rigidbody rb;

    private float delayedTime = 0;

    private bool reachedDelay = false;
    private bool MoveInUpdate = true;
    private bool MoveInFixedUpdate = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void Update()
    {
        if (!reachedDelay)
        {
            delayedTime += Time.deltaTime;
            if (delayedTime >= StartDelay)
            {
                reachedDelay = true;
            }
        }

        if (MoveInUpdate)
        {
            movePlatform(Time.deltaTime);
        }

        PositionDifference = transform.position - previousPosition;

        previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (MoveInFixedUpdate)
        {
            movePlatform(Time.fixedDeltaTime);
        }
    }

    private void movePlatform(float timeDelta)
    {
        if (IsActive && !ReachedDestination && reachedDelay && Destination != null)
        {
            Vector3 direction = Destination.transform.position - transform.position;
            rb.MovePosition(transform.position + (direction.normalized * MovementSpeed * timeDelta));

            float dist = Vector3.Distance(transform.position, Destination.transform.position);
            if (Vector3.Distance(transform.position, Destination.transform.position) < 0.02f)
            {
                ReachedDestination = true;

                resetDelayStatus();

                if (Destination.Destination != null)
                {
                    Destination = Destination.Destination;
                    ReachedDestination = false;
                }
            }
        }
    }

    private void resetDelayStatus()
    {
        reachedDelay = false;
        delayedTime = 0;
    }
}
