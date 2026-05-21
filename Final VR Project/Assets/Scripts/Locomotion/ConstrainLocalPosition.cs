using UnityEngine;

public class ConstrainLocalPosition : MonoBehaviour 
{
    //CONSTRAINT X
    [SerializeField] private bool ConstrainLocalX = false;
    [SerializeField] private float LocalXMin = -1f;
    [SerializeField] private float LocalXMax = 1f;

    //CONSTRAINT Y
    [SerializeField] private bool ConstrainLocalY = false;
    [SerializeField] private float LocalYMin = -1f;
    [SerializeField] private float LocalYMax = 1f;

    //CONSTRAINT Z
    [SerializeField] private bool ConstrainLocalZ = false;
    [SerializeField] private float LocalZMin = -1f;
    [SerializeField] private float LocalZMax = 1f;

    private void Update() 
    {
        Constrain();
    }

    private void Constrain() 
    {
        if (!ConstrainLocalX && !ConstrainLocalY && !ConstrainLocalZ) 
        {
            return;
        }

        Vector3 currentPos = transform.localPosition;
        float newX = ConstrainLocalX ? Mathf.Clamp(currentPos.x, LocalXMin, LocalXMax) : currentPos.x;
        float newY = ConstrainLocalY ? Mathf.Clamp(currentPos.y, LocalYMin, LocalYMax) : currentPos.y;
        float newZ = ConstrainLocalZ ? Mathf.Clamp(currentPos.z, LocalZMin, LocalZMax) : currentPos.z;

        transform.localPosition = new Vector3(newX, newY, newZ);
    }
}