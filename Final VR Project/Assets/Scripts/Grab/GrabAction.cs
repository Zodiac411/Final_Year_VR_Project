using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public class GrabAction : GrabbableEvents
{
    [SerializeField] private GrabberEvent OnGrabEvent;

    Grabbable g;
    float lastGrabTime = 0;
    float minTimeBetweenGrabs = 0.2f;

    public override void OnGrab(Grabber grabber)
    {

        if (g == null)
        {
            g = GetComponent<Grabbable>();
        }

        g.DropItem(grabber, false, false);

        if (grabber.RemoteGrabbingItem || grabber.HoldingItem)
        {
            return;
        }

        if (OnGrabEvent != null)
        {

            if (Time.time - lastGrabTime >= minTimeBetweenGrabs)
            {
                OnGrabEvent.Invoke(grabber);
                lastGrabTime = Time.time;
            }
        }
    }
}
