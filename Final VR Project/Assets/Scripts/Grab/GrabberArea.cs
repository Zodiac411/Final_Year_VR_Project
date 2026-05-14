using System.Collections.Generic;
using UnityEngine;

public class GrabberArea : MonoBehaviour
{
    [SerializeField] private List<Grabber> grabbersInArea;
    [SerializeField] private Grabber InArea;

    private void Update()
    {
        InArea = GetOpenGrabber();
    }

    public Grabber GetOpenGrabber()
    {
        if (grabbersInArea != null && grabbersInArea.Count > 0)
        {
            foreach (var g in grabbersInArea)
            {
                if (!g.HoldingItem)
                {
                    return g;
                }
            }
        }

        return null;
    }

    private void OnTriggerEnter(Collider other)
    {

        Grabber grab = other.GetComponent<Grabber>();
        if (grab != null)
        {

            if (grabbersInArea == null)
            {
                grabbersInArea = new List<Grabber>();
            }

            if (!grabbersInArea.Contains(grab))
            {
                grabbersInArea.Add(grab);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Grabber grab = other.GetComponent<Grabber>();
        if (grab != null)
        {

            if (grabbersInArea == null)
            {
                return;
            }

            if (grabbersInArea.Contains(grab))
            {
                grabbersInArea.Remove(grab);
            }
        }
    }
}
