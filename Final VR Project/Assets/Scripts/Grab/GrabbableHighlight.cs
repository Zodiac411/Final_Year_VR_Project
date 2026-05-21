using UnityEngine.XR.Interaction.Toolkit;

public class GrabbableHighlight : GrabbableEvents
{
    public bool HighlightOnGrabbable = true;
    public bool HighlightOnRemoteGrabbable = true;

    public override void OnGrab(Grabber grabber)
    {
        UnhighlightItem();
    }

    public override void OnBecomesClosestGrabbable(ControllerHand touchingHand)
    {
        if (HighlightOnGrabbable)
        {
            HighlightItem();
        }
    }

    public override void OnNoLongerClosestGrabbable(ControllerHand touchingHand)
    {
        if (HighlightOnGrabbable)
        {
            UnhighlightItem();
        }
    }

    public override void OnBecomesClosestRemoteGrabbable(ControllerHand touchingHand)
    {
        if (HighlightOnRemoteGrabbable)
        {
            HighlightItem();
        }
    }

    public override void OnNoLongerClosestRemoteGrabbable(ControllerHand touchingHand)
    {
        if (HighlightOnRemoteGrabbable)
        {
            UnhighlightItem();
        }
    }
    public void HighlightItem()
    {

    }

    public void UnhighlightItem()
    {

    }
}