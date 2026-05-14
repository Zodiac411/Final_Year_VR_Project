using UnityEngine;

public class Tooltip : MonoBehaviour
{
    [SerializeField] private Vector3 TipOffset = new Vector3(1.5f, 0.2f, 0);
    [SerializeField] private Transform DrawLineTo;

    [SerializeField] private float MaxViewDistance = 10f;

    private LineToTransform lineTo;
    private Transform childTransform;
    private Transform lookAt;

    private bool UseWorldYAxis = true;

    private void Start()
    {
        lookAt = Camera.main.transform;
        lineTo = GetComponentInChildren<LineToTransform>();

        childTransform = transform.GetChild(0);

        if (DrawLineTo && lineTo)
        {
            lineTo.ConnectTo = DrawLineTo;
        }
    }

    private void Update()
    {
        UpdateTooltipPosition();
    }

    public virtual void UpdateTooltipPosition()
    {
        if (lookAt)
        {
            transform.LookAt(Camera.main.transform);
        }
        else if (Camera.main != null)
        {
            lookAt = Camera.main.transform;
        }
        else if (Camera.main == null)
        {
            return;
        }

        transform.parent = DrawLineTo;
        transform.localPosition = TipOffset;

        if (UseWorldYAxis)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
            transform.position += new Vector3(0, TipOffset.y, 0);
        }

        if (childTransform)
        {
            childTransform.gameObject.SetActive(Vector3.Distance(transform.position, Camera.main.transform.position) <= MaxViewDistance);
        }
    }
}