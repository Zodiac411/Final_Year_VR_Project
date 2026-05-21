using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class JointBreaker : MonoBehaviour
{
    [SerializeField] private float BreakDistance = 0.25f;
    [SerializeField] private float JointDistance;

    [SerializeField] private bool DestroyJointOnBreak = true;

    [SerializeField] private GrabberEvent OnBreakEvent;

    private Joint theJoint;
    private Vector3 startPos;

    private bool brokeJoint = false;

    private void Start()
    {
        startPos = transform.localPosition;
        theJoint = GetComponent<Joint>();
    }

    private void Update()
    {
        JointDistance = Vector3.Distance(transform.localPosition, startPos);

        if (!brokeJoint && JointDistance > BreakDistance)
        {
            BreakJoint();
        }
    }

    public void BreakJoint()
    {

        if (DestroyJointOnBreak && theJoint)
        {
            Destroy(theJoint);
        }

        if (OnBreakEvent != null)
        {

            var heldGrabbable = GetComponent<Grabbable>();
            if (heldGrabbable && heldGrabbable.GetPrimaryGrabber())
            {
                brokeJoint = true;
                OnBreakEvent.Invoke(heldGrabbable.GetPrimaryGrabber());
            }
        }
    }
}

