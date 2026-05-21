using UnityEngine;

public class LaserPointer : MonoBehaviour
{
    [SerializeField] private LayerMask ValidLayers;
    [SerializeField] private Transform LaserEnd;

    [SerializeField] private float MaxRange = 25f;

    [SerializeField] private bool Active = true;

    private LineRenderer line;

    private void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    private void LateUpdate()
    {
        if (Active)
        {
            line.enabled = true;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, MaxRange, ValidLayers, QueryTriggerInteraction.Ignore))
            {
                line.useWorldSpace = true;
                line.SetPosition(0, transform.position);
                line.SetPosition(1, hit.point);

                LaserEnd.gameObject.SetActive(true);
                LaserEnd.position = hit.point;
                LaserEnd.rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);
            }
            else
            {
                line.useWorldSpace = false;
                line.SetPosition(0, transform.localPosition);
                line.SetPosition(1, new Vector3(0, 0, MaxRange));

                LaserEnd.localPosition = new Vector3(0, 0, MaxRange);

                LaserEnd.gameObject.SetActive(false);
            }
        }
        else
        {
            LaserEnd.gameObject.SetActive(false);

            if (line)
            {
                line.enabled = false;
            }
        }
    }
}