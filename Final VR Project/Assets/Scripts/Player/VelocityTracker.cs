using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine;

public class VelocityTracker : MonoBehaviour
{
    public enum VelocityTrackingType
    {
        Device,
        PerFrame
    }

    public ControllerHand controllerHand = ControllerHand.None;
    [SerializeField] private VelocityTrackingType trackingType = VelocityTrackingType.Device;

    public float AverageVelocityCount = 3;

    private Vector3 vel;
    private Vector3 angularVel;

    private Vector3 lastPos;
    private Quaternion lastRot;

    private List<Vector3> previousVels = new List<Vector3>();
    private List<Vector3> previousAngularVels = new List<Vector3>();

    private GameObject playSpace;
    private Vector3 axis;

    private float angle;

    private void Start()
    {
        playSpace = GameObject.Find("TrackingSpace");
    }

    private void FixedUpdate()
    {
        UpdateVelocities();

        lastPos = transform.position;
        lastRot = transform.rotation;
    }

    public virtual void UpdateVelocities()
    {
        UpdateVelocity();
        UpdateAngularVelocity();
    }

    public virtual void UpdateVelocity()
    {
        vel = (transform.position - lastPos) / Time.deltaTime;
        previousVels.Add(GetVelocity());

        if (previousVels.Count > AverageVelocityCount)
        {
            previousVels.RemoveAt(0);
        }
    }

    public virtual void UpdateAngularVelocity()
    {
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRot);
        deltaRotation.ToAngleAxis(out angle, out axis);
        angle *= Mathf.Deg2Rad;

        angularVel = axis * angle * (1.0f / Time.deltaTime);
        previousAngularVels.Add(GetAngularVelocity());

        if (previousAngularVels.Count > AverageVelocityCount)
        {
            previousAngularVels.RemoveAt(0);
        }
    }

    public virtual Vector3 GetVelocity()
    {
        if (trackingType == VelocityTrackingType.PerFrame)
        {
            return this.vel;
        }

        Vector3 vel = XRInput.Instance.GetControllerVelocity(controllerHand);

        if (vel == null || vel == Vector3.zero)
        {
            return this.vel;
        }
        else
        {
            if (playSpace != null)
            {
                return playSpace.transform.rotation * vel;
            }

            return vel;
        }
    }

    public virtual Vector3 GetAveragedVelocity()
    {
        return GetAveragedVector(previousVels);
    }

    public virtual Vector3 GetAngularVelocity()
    {
        return angularVel;
    }

    public virtual Vector3 GetAveragedAngularVelocity()
    {
        return GetAveragedVector(previousAngularVels);
    }

    public virtual Vector3 GetAveragedVector(List<Vector3> vectors)
    {

        if (vectors != null)
        {

            int count = vectors.Count;
            float x = 0;
            float y = 0;
            float z = 0;

            for (int i = 0; i < count; i++)
            {
                Vector3 v = vectors[i];
                x += v.x;
                y += v.y;
                z += v.z;
            }

            return new Vector3(x / count, y / count, z / count);
        }

        return Vector3.zero;
    }
}