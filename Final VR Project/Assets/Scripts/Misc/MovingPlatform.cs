using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public MovingPlatformMethod MovementMethod = MovingPlatformMethod.ParentToPlatform;

    [HideInInspector] public Vector3 PositionDelta;
    [HideInInspector] public Quaternion RotationDelta;

    protected Vector3 previousPosition;
    protected Quaternion previousRotation;

    protected void Update()
    {
        PositionDelta = transform.position - previousPosition;
        RotationDelta = transform.rotation * Quaternion.Inverse(previousRotation);

        previousPosition = transform.position;
        previousRotation = transform.rotation;
    }
}

public enum MovingPlatformMethod
{
    ParentToPlatform,
    PositionDifference
}
