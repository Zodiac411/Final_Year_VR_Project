using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class ReturnToSnapZone : MonoBehaviour
{
    public SnapZone ReturnTo;

    public float SnapDistance = 0.05f;
    public float LerpSpeed = 15f;
    public float ReturnDelay = 0.1f;

    float currentDelay = 0;

    Grabbable grab;
    Rigidbody rigid;

    bool useGravityInitial;

    private void Start()
    {
        grab = GetComponent<Grabbable>();
        rigid = GetComponent<Rigidbody>();
        useGravityInitial = rigid.useGravity;
    }

    private void Update()
    {
        returnSnapping();
    }

    void returnSnapping()
    {
        if (grab.BeingHeld)
        {
            currentDelay = 0;
        }

        bool validReturn = grab != null && ReturnTo != null && ReturnTo.HeldItem == null && !grab.BeingHeld;

        if (validReturn)
        {
            currentDelay += Time.deltaTime;
        }

        if (validReturn && currentDelay >= ReturnDelay)
        {
            moveToSnapZone();
        }
    }

    void moveToSnapZone()
    {

        rigid.useGravity = false;

        transform.position = Vector3.MoveTowards(transform.position, ReturnTo.transform.position, Time.deltaTime * LerpSpeed);

        if (Vector3.Distance(transform.position, ReturnTo.transform.position) < SnapDistance)
        {
            rigid.useGravity = useGravityInitial;
            ReturnTo.GrabGrabbable(grab);
        }
    }
}