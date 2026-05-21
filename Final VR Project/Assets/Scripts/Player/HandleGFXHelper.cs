using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class HandleGFXHelper : MonoBehaviour
{
    [SerializeField] private Grabbable HandleGrabbable;
    [SerializeField] private Transform LookAt;

    [SerializeField] private float Speed = 5f;

    [SerializeField] private float LocalYMin = 215f;
    [SerializeField] private float LocalYMax = 270f;

    private Vector3 initialRot;

    private void Start()
    {
        initialRot = transform.localEulerAngles;
    }

    private void Update()
    {
        if (HandleGrabbable != null && HandleGrabbable.BeingHeld)
        {
            Quaternion rot = Quaternion.LookRotation
                (HandleGrabbable.GetPrimaryGrabber().transform.position - transform.position);

            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10);

            Vector3 currentPos = transform.localEulerAngles;
            float constrainedY = Mathf.Clamp(currentPos.y, LocalYMin, LocalYMax);

            transform.localEulerAngles = new Vector3(initialRot.x, constrainedY, initialRot.z);
        }
        else
        {
            Quaternion rot = Quaternion.LookRotation(LookAt.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * Speed);

            Vector3 currentPos = transform.localEulerAngles;
            float constrainedY = Mathf.Clamp(currentPos.y, LocalYMin, LocalYMax);

            transform.localEulerAngles = new Vector3(initialRot.x, constrainedY, initialRot.z);
        }
    }
}