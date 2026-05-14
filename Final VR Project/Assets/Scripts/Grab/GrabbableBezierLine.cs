using UnityEngine;

public class GrabbableBezierLine : GrabbableEvents
{
    [SerializeField] private LineRenderer LineToDraw;

    [SerializeField] private float LerpAmount = 0.5f;
    [SerializeField] private float HeightAdjustment = 0.5f;

    [SerializeField] private int SegmentCount = 100;

    private bool HighlightOnRemoteGrabbable = true;
    private bool HighlightOnGrabbable = true;

    Grabber lineToGrabber;
    Grabber lineRemoteGrabbing;

    void Start()
    {
        if (LineToDraw == null)
        {
            LineToDraw = transform.GetComponent<LineRenderer>();
        }

        UnhighlightItem();
    }

    private void LateUpdate()
    {

        if (lineToGrabber != null)
        {
            Vector3 midPoint = Vector3.Lerp(transform.position, lineToGrabber.transform.position, LerpAmount);
            midPoint.y += HeightAdjustment;

            DrawBezierCurve(transform.position, midPoint, lineToGrabber.transform.position, LineToDraw);
        }
        else if (grab != null && grab.RemoteGrabbing)
        {
            if (!LineToDraw.enabled)
            {
                LineToDraw.enabled = true;
            }

            Vector3 midPoint = Vector3.Lerp(transform.position, grab.FlyingToGrabber.transform.position, LerpAmount);
            midPoint.y += HeightAdjustment;

            DrawBezierCurve(transform.position, midPoint, grab.FlyingToGrabber.transform.position, LineToDraw);
        }
        else if (LineToDraw.enabled)
        {
            LineToDraw.enabled = false;
        }
    }

    public override void OnGrab(Grabber grabber)
    {
        UnhighlightItem();

        base.OnGrab(grabber);
    }

    public override void OnBecomesClosestGrabbable(Grabber touchingGrabber)
    {
        if (HighlightOnGrabbable)
        {
            HighlightItem(touchingGrabber);
        }

        base.OnBecomesClosestGrabbable(touchingGrabber);
    }

    public override void OnNoLongerClosestGrabbable(Grabber touchingGrabber)
    {
        if (HighlightOnGrabbable)
        {
            UnhighlightItem();
        }

        base.OnNoLongerClosestGrabbable(touchingGrabber);
    }

    public override void OnBecomesClosestRemoteGrabbable(Grabber touchingGrabber)
    {
        if (HighlightOnRemoteGrabbable)
        {
            HighlightItem(touchingGrabber);
        }

        base.OnBecomesClosestRemoteGrabbable(touchingGrabber);
    }

    public override void OnNoLongerClosestRemoteGrabbable(Grabber touchingGrabber)
    {
        if (HighlightOnRemoteGrabbable)
        {
            UnhighlightItem();
        }

        base.OnNoLongerClosestRemoteGrabbable(touchingGrabber);

    }
    public void HighlightItem(Grabber touchingGrabber)
    {

        if (LineToDraw == null)
        {
            return;
        }

        if (!LineToDraw.enabled)
        {
            LineToDraw.enabled = true;
        }

        lineToGrabber = touchingGrabber;
    }

    public void UnhighlightItem()
    {
        if (LineToDraw)
        {
            LineToDraw.enabled = false;
        }

        lineToGrabber = null;
    }

    public void DrawBezierCurve(Vector3 point0, Vector3 point1, Vector3 point2, LineRenderer lineRenderer)
    {

        lineRenderer.positionCount = SegmentCount;
        lineRenderer.useWorldSpace = true;

        float t = 0f;
        Vector3 b = new Vector3(0, 0, 0);
        for (int i = 0; i < SegmentCount; i++)
        {
            b = (1 - t) * (1 - t) * point0 + 2 * (1 - t) * t * point1 + t * t * point2;
            lineRenderer.SetPosition(i, b);
            t += (1 / (float)lineRenderer.positionCount);
        }
    }
}