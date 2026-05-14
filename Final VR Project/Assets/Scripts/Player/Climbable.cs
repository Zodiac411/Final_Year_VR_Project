using UnityEngine.XR.Interaction.Toolkit;

public class Climbable : Grabbable
{
    private PlayerClimbing playerClimbing;

    private void Start()
    {
        SecondaryGrabBehavior = OtherGrabBehavior.DualGrab;

        GrabPhysics = GrabPhysics.None;
        CanBeSnappedToSnapZone = false;
        TwoHandedDropBehavior = TwoHandedDropMechanic.None;

        if (BreakDistance == 1)
        {
            BreakDistance = 0;
        }

        if (player != null)
        {
            playerClimbing = player.gameObject.GetComponentInChildren<PlayerClimbing>();
        }
    }

    public override void GrabItem(Grabber grabbedBy)
    {

        if (playerClimbing)
        {
            playerClimbing.AddClimber(this, grabbedBy);
        }

        base.GrabItem(grabbedBy);
    }

    public override void DropItem(Grabber droppedBy)
    {
        if (droppedBy != null && playerClimbing != null)
        {
            playerClimbing.RemoveClimber(droppedBy);
        }

        base.DropItem(droppedBy);
    }
}