using UnityEngine;

public class PlaySoundOnGrab : GrabbableEvents
{
    public AudioClip SoundToPlay;

    public override void OnGrab(Grabber grabber)
    {
        if (SoundToPlay)
        {
            XRManager.Instance.PlaySpatialClipAt(SoundToPlay, transform.position, 1f, 1f);
        }

        base.OnGrab(grabber);
    }
}