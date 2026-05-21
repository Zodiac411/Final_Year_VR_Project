using System.Collections.Generic;
using UnityEngine;

public class HandCollision : MonoBehaviour 
{
    public HandController HandControl;
    public Grabber HandGrabber;

    //COLLISION DETECTION
    private bool EnableHandCollision = true;
    private bool EnableCollisionOnPoint = true;
    private bool EnableCollisionOnFist = true;
    private bool EnableCollisionOnAllPoses = false;
    private bool EnableCollisionDuringGrab = false;

    private float PointAmount;
    private float GripAmount;
    private bool MakingFist;

    List<Collider> handColliders;

    private void Start() 
    {
        handColliders = new List<Collider>();
        var tempColliders = GetComponentsInChildren<Collider>(true);

        foreach(var c in tempColliders) 
        {
            if(!c.isTrigger) 
            {
                handColliders.Add(c);
            }
        }
    }

    private void Update() 
    {
        if(!EnableHandCollision) 
        {
            return;
        }

        bool grabbing = HandGrabber != null && HandGrabber.HoldingItem;
           
        bool makingFist = HandControl != null && HandControl.GripAmount > 0.9f && (HandControl.PointAmount < 0.1 || HandControl.PointAmount > 1);
        MakingFist = makingFist;
        PointAmount = HandControl != null ? HandControl.PointAmount : 0;
        GripAmount = HandControl != null ? HandControl.GripAmount : 0;

        bool pointing = HandControl != null && HandControl.PointAmount > 0.9f && HandControl.GripAmount > 0.9f;

        for (int x = 0; x < handColliders.Count; x++) 
        {
            Collider col = handColliders[x];

            if (EnableCollisionDuringGrab == false && grabbing) 
            {
                col.enabled = false;
                continue;
            }

            if(HandGrabber != null && (Time.time - HandGrabber.LastDropTime < 0.5f )) 
            {
                col.enabled = false;
                continue;
            }

            bool enableCollider = false;
            if (EnableCollisionDuringGrab && grabbing) 
            {
                enableCollider = true;
            }
            else if (EnableCollisionOnPoint && pointing) 
            {
                enableCollider = true;
            }
            else if (EnableCollisionOnFist && makingFist) 
            {
                enableCollider = true;
            }
            else if (EnableCollisionOnAllPoses) 
            {
                enableCollider = true;
            }

            col.enabled = enableCollider;
        }
    }
}