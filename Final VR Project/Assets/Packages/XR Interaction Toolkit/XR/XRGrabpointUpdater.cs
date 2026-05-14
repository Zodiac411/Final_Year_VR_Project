using UnityEngine;

public class XRGrabpointUpdater : MonoBehaviour
{
    [Header("Right Hand Model")]
    public Vector3 PriorModelOffsetRightPosition = new Vector3(-0.024f, 0.051f, 0.001f);
    public Vector3 NewModelOffsetRightPosition = new Vector3(-0.006f, 0, -0.04f);
    public Vector3 PriorModelOffsetRightRotation = new Vector3(-12.041f, 13f, -90f);
    public Vector3 NewModelOffsetRightRotation = new Vector3(-6, 0.43f, -90f);

    [Header("Left Hand Model")]
    public Vector3 PriorModelOffsetLeftPosition = new Vector3(0.024f, 0.051f, 0.001f);
    public Vector3 NewModelOffsetLeftPosition = new Vector3(0.006f, 0, -0.04f);
    public Vector3 PriorModelOffsetLeftRotation = new Vector3(-12.041f, -13f, 90f);
    public Vector3 NewModelOffsetLeftRotation = new Vector3(-6, -0.43f, 90);

    private void Start()
    {
        ApplyGrabPointUpdate();
    }

    public void ApplyGrabPointUpdate()
    {
        GrabPoint[] points = GetComponentsInChildren<GrabPoint>();

        foreach (var grabPoint in points)
        {
            if (grabPoint.RightHandIsValid && grabPoint.LeftHandIsValid)
            {
                grabPoint.transform.localPosition = grabPoint.transform.localPosition + 
                    (PriorModelOffsetRightPosition - NewModelOffsetRightPosition);

                grabPoint.transform.localRotation *= Quaternion.Euler(PriorModelOffsetRightRotation) * 
                    Quaternion.Inverse(Quaternion.Euler(NewModelOffsetRightRotation));
            }
            else if (grabPoint.RightHandIsValid)
            {
                grabPoint.transform.localPosition = grabPoint.transform.localPosition + 
                    (PriorModelOffsetRightPosition - NewModelOffsetRightPosition);

                grabPoint.transform.localRotation *= Quaternion.Euler(PriorModelOffsetRightRotation) * 
                    Quaternion.Inverse(Quaternion.Euler(NewModelOffsetRightRotation));
            }
            else if (grabPoint.LeftHandIsValid)
            {
                grabPoint.transform.localPosition = grabPoint.transform.localPosition + 
                    (PriorModelOffsetLeftPosition - NewModelOffsetLeftPosition);

                grabPoint.transform.localRotation *= Quaternion.Euler(PriorModelOffsetLeftRotation) * 
                    Quaternion.Inverse(Quaternion.Euler(NewModelOffsetLeftRotation));
            }
        }
    }
}

