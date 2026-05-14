using UnityEngine;

public class BowArm : MonoBehaviour
{

    [SerializeField] private Bow BowItem;

    [SerializeField] private float BowPercentStart = 50f;
    [SerializeField] private float RotateDegrees = 10f;
    [SerializeField] private float Speed = 50f;

    [SerializeField] private bool RotateX = true;
    [SerializeField] private bool RotateZ = false;

    private Quaternion startRot;
    private Quaternion endRot;

    private void Start()
    {
        startRot = Quaternion.Euler(transform.localEulerAngles);

        if (RotateX)
        {
            endRot = Quaternion.Euler(new Vector3(startRot.x + RotateDegrees, transform.localEulerAngles.y, transform.localEulerAngles.z));
        }

        if (RotateZ)
        {
            endRot = Quaternion.Euler(new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z + RotateDegrees));
        }
    }

    private void Update()
    {
        if (BowItem.DrawPercent >= BowPercentStart)
        {
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, endRot, Speed * Time.deltaTime);
        }
        else if (BowItem.DrawPercent < BowPercentStart && BowItem.DrawPercent > 5)
        {
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, startRot, Speed * Time.deltaTime);
        }
        else
        {
            transform.localRotation = startRot;
        }
    }
}

