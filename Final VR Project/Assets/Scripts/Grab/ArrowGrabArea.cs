using UnityEngine;

public class ArrowGrabArea : MonoBehaviour
{
    private Bow bow;

    private void Start()
    {
        bow = transform.parent.GetComponent<Bow>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Grabber grabObject = other.GetComponent<Grabber>();
        if (grabObject != null)
        {
            bow.ClosestGrabber = grabObject;

            if (!grabObject.HoldingItem)
            {
                bow.CanGrabArrow = true;
            }
            else if (grabObject.HoldingItem && grabObject.HeldGrabbable != null)
            {
                Arrow arrowObject = grabObject.HeldGrabbable.GetComponent<Arrow>();
                if (arrowObject != null && bow.GrabbedArrow == null)
                {
                    bow.GrabArrow(arrowObject);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Grabber grabObject = other.GetComponent<Grabber>();
        if (bow.ClosestGrabber != null && grabObject != null && bow.ClosestGrabber == grabObject)
        {
            bow.CanGrabArrow = false;
            bow.ClosestGrabber = null;
        }
    }
}
