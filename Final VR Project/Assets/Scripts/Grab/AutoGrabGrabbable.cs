public class AutoGrabGrabbable : GrabbableEvents 
{
    public override void OnBecomesClosestGrabbable(Grabber touchingGrabber) 
    {
        touchingGrabber.GrabGrabbable(grab);
    }
}

