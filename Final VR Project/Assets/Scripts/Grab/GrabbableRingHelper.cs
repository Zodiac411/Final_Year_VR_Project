using System.Linq;
using UnityEngine;

public class GrabbableRingHelper : GrabbableEvents
{
    public bool RingOnValidPickup = true;
    public bool RingOnValidRemotePickup = true;

    public float RingHelperScale = 0.2f;

    void Start()
    {
        if (RingOnValidPickup)
        {
            RingHelper rh = GetComponentInChildren<RingHelper>();
            if (rh == null)
            {
                GameObject go = Instantiate(Resources.Load("RingHelper", typeof(GameObject))) as GameObject;
                go.transform.SetParent(this.transform, false);
                go.transform.name = "Ring Helper";
                go.transform.localPosition = grab.GrabPositionOffset;

                if (grab.GrabPoints != null && grab.GrabPoints.Count > 0)
                {
                    go.transform.localPosition = grab.GrabPoints.FirstOrDefault().localPosition;
                }

                RectTransform rt = go.GetComponent<RectTransform>();
                rt.localScale = new Vector3(RingHelperScale, RingHelperScale, RingHelperScale);
            }
        }
    }
}