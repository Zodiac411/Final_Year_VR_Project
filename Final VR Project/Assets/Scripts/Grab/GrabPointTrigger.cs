using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class GrabPointTrigger : MonoBehaviour
{
    public enum HandMovement
    {
        Instant,
        Lerp,
        None
    }

    [Header("Hand Movement")]
    public HandMovement MoveInStyle = HandMovement.Instant;
    public HandMovement MoveOutStyle = HandMovement.Instant;

    public float HandSpeed = 20f;
    public bool LiveUpdateNearestGrabPoint = true;

    [Header("Grabbable Options")]
    public Grabbable GrabObject;
    public Grabbable OtherGrabbableMustBeHeld;

    public List<GrabPoint> GrabPoints;

    Grabber currentGrabber;
    Grabbable dummyGrabbable;
    GrabPoint closestPoint;
    Grabber grabberInTrigger;

    void Start()
    {
        if (dummyGrabbable == null)
        {
            var go = new GameObject("Dummy Grabbable");
            dummyGrabbable = go.AddComponent<Grabbable>();
            dummyGrabbable.transform.parent = transform;
            dummyGrabbable.transform.localPosition = Vector3.zero;
            dummyGrabbable.transform.localRotation = Quaternion.identity;

            List<Transform> grabs = new List<Transform>();
            for (int x = 0; x < GrabPoints.Count; x++)
            {
                GrabPoint g = GrabPoints[x];
                grabs.Add(g.transform);
            }
            dummyGrabbable.GrabPoints = grabs;
            dummyGrabbable.GrabMechanic = GrabType.Snap;
            dummyGrabbable.ParentHandModel = false;
            dummyGrabbable.CanBeDropped = false;

            if (GrabObject != null)
            {
                dummyGrabbable.GrabButton = GrabObject.GrabButton;
                dummyGrabbable.Grabtype = GrabObject.Grabtype;
            }
        }
    }

    void Update()
    {
        if (dummyGrabbable != null && currentGrabber != null)
        {
            if (OtherGrabbableMustBeHeld != null && !OtherGrabbableMustBeHeld.BeingHeld)
            {
                ReleaseGrabber();
                return;
            }

            if (LiveUpdateNearestGrabPoint)
            {
                Transform closestGrab = dummyGrabbable.GetClosestGrabPoint(currentGrabber);

                if (closestGrab != null)
                {
                    var newPoint = closestGrab.GetComponent<GrabPoint>();
                    if (newPoint != null && newPoint != closestPoint)
                    {
                        UpdateGrabPoint(newPoint);
                    }
                }
            }

            if (MoveInStyle == HandMovement.Lerp)
            {
                currentGrabber.HandsGraphics.localPosition = Vector3.Lerp(currentGrabber.HandsGraphics.localPosition, currentGrabber.handsGraphicsGrabberOffset, Time.deltaTime * HandSpeed);
                currentGrabber.HandsGraphics.localRotation = Quaternion.Slerp(currentGrabber.HandsGraphics.localRotation, Quaternion.identity, Time.deltaTime * HandSpeed);
            }

            if (currentGrabber.GetInputDownForGrabbable(dummyGrabbable))
            {

                if (GrabObject != null)
                {
                    var prevGrabber = currentGrabber;
                    ReleaseGrabber();
                    prevGrabber.GrabGrabbable(GrabObject);
                }
            }
        }

        if (grabberInTrigger != null && !grabberInTrigger.HoldingItem && currentGrabber == null)
        {
            setGrabber(grabberInTrigger);
        }
    }

    public virtual void UpdateGrabPoint(GrabPoint newPoint)
    {
        closestPoint = newPoint;

        dummyGrabbable.handPoseType = newPoint.handPoseType;
        dummyGrabbable.SelectedHandPose = newPoint.SelectedHandPose;

        if (currentGrabber != null && currentGrabber.HandsGraphics != null)
        {

            if (MoveInStyle != HandMovement.None)
            {
                currentGrabber.HandsGraphics.parent = closestPoint.transform;
            }

            if (MoveInStyle == HandMovement.Instant)
            {
                currentGrabber.HandsGraphics.localPosition = currentGrabber.handsGraphicsGrabberOffset;
                currentGrabber.HandsGraphics.localEulerAngles = Vector3.zero;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {

        if (OtherGrabbableMustBeHeld != null && !OtherGrabbableMustBeHeld.BeingHeld)
        {
            return;
        }

        if (grabberInTrigger != null)
        {
            return;
        }

        if (!this.isActiveAndEnabled || dummyGrabbable == null)
        {
            return;
        }

        Grabber grab = other.GetComponent<Grabber>();
        if (grab != null && !grab.HoldingItem && currentGrabber == null)
        {

            Transform closestGrab = dummyGrabbable.GetClosestGrabPoint(grab);

            if (closestGrab != null)
            {
                closestPoint = closestGrab.GetComponent<GrabPoint>();
            }
            else
            {
                closestPoint = null;
            }

            grabberInTrigger = grab;

            if (closestPoint != null)
            {
                dummyGrabbable.ActiveGrabPoint = closestPoint;
                setGrabber(grab);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Grabber grab = other.GetComponent<Grabber>();

        if (grab != null && grab == grabberInTrigger)
        {
            grabberInTrigger = null;
        }

        if (grab != null && grab == currentGrabber)
        {
            ReleaseGrabber();
        }
    }

    void setGrabber(Grabber theGrabber)
    {
        currentGrabber = theGrabber;

        dummyGrabbable.CanBeDropped = false;
        dummyGrabbable.BeingHeld = true;

        currentGrabber.HeldGrabbable = dummyGrabbable;

        UpdateGrabPoint(closestPoint);
    }

    public virtual void ReleaseGrabber()
    {

        if (currentGrabber != null)
        {
            dummyGrabbable.CanBeDropped = true;
            dummyGrabbable.BeingHeld = false;

            currentGrabber.HeldGrabbable = null;

            if (MoveOutStyle == HandMovement.Instant)
            {
                currentGrabber.ResetHandGraphics();
            }
            else if (MoveOutStyle == HandMovement.Lerp)
            {
                currentGrabber.ResetHandGraphics();
            }

            currentGrabber = null;
        }
    }
}