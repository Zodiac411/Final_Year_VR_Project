using UnityEngine;

public class ScaleBetweenPoints : MonoBehaviour
{
    [SerializeField] private Transform Begin;
    [SerializeField] private Transform End;

    private bool DoUpdate = true;
    private bool DoLateUpdate = false;

    private bool LookAtTarget = false;

    private void Update()
    {
        if (DoUpdate)
        {
            doScale();
        }
    }

    private void LateUpdate()
    {
        if (DoLateUpdate)
        {
            doScale();
        }
    }

    private void doScale()
    {

        if (LookAtTarget)
        {
            transform.position = Begin.position;
            transform.LookAt(End, transform.up);
        }

        Vector3 objectScale = transform.localScale;
        float distance = Vector3.Distance(Begin.position, End.position);

        Vector3 newScale = new Vector3(objectScale.x, objectScale.y, distance);
        transform.localScale = newScale;
        transform.position = (Begin.position + End.position) / 2;
    }
}
