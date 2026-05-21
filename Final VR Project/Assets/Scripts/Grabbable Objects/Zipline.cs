using UnityEngine;

public class Zipline : GrabbableEvents
{
    [SerializeField] private Transform ZiplineStart;
    [SerializeField] private Transform ZiplineEnd;

    [SerializeField] private float ZiplineSpeed = 1;

    AudioSource audioSource;

    private bool UseLinearMovement = true;
    private bool movingForward = true;
    
    private float lastMoveTime = -1f;

    private void Start()
    {
        if (ZiplineEnd != null)
        {
            transform.LookAt(ZiplineEnd.position);
        }

        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Time.time - lastMoveTime < 0.1f)
        {

            if (movingForward)
            {
                audioSource.pitch = Time.timeScale * 1f;
            }
            else
            {
                audioSource.pitch = Time.timeScale * 0.95f;
            }

            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (ZiplineStart != null && ZiplineEnd != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(ZiplineStart.position, ZiplineEnd.position);
        }
    }

    public override void OnTrigger(float triggerValue)
    {

        if (triggerValue > 0.5f)
        {
            moveTowards(ZiplineEnd.position, true);
        }

        base.OnTrigger(triggerValue);
    }

    public override void OnButton1()
    {

        moveTowards(ZiplineStart.position, false);

        base.OnButton1();
    }

    public override void OnButton2()
    {
        moveTowards(ZiplineEnd.position, true);

        base.OnButton2();
    }

    private void moveTowards(Vector3 pos, bool forwardDirection)
    {
        lastMoveTime = Time.time;
        movingForward = forwardDirection;

        if (forwardDirection)
        {
            transform.LookAt(pos);
        }
        else
        {
            transform.LookAt(2 * transform.position - pos);
        }

        if (UseLinearMovement)
        {
            transform.position = Vector3.MoveTowards(transform.position, pos, ZiplineSpeed * Time.fixedDeltaTime);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, pos, ZiplineSpeed * Time.deltaTime);
        }

        if (input && thisGrabber)
        {
            input.VibrateController(0.1f, 0.1f, 0.1f, thisGrabber.HandSide);
        }
    }
}