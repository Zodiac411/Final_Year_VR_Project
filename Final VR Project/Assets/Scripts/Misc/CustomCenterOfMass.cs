using UnityEngine;

public class CustomCenterOfMass : MonoBehaviour
{
    [Header("Define Center of Mass")]
    [SerializeField] private Vector3 CenterOfMass = Vector3.zero;
    [SerializeField] private Transform CenterOfMassTransform;

    private Rigidbody rb;

    private bool ShowGizmo = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        SetCenterOfMass(getThisCenterOfMass());
    }

    public virtual void SetCenterOfMass(Vector3 center)
    {
        if (rb)
        {
            rb.centerOfMass = center;
        }
    }

    protected virtual Vector3 getThisCenterOfMass()
    {
        if (CenterOfMassTransform != null)
        {
            return CenterOfMassTransform.localPosition;
        }
        else
        {
            return CenterOfMass;
        }
    }

    private void OnDrawGizmos()
    {
        if (ShowGizmo)
        {
            Gizmos.color = Color.red;
            if (rb)
            {
                Gizmos.DrawSphere(rb.worldCenterOfMass, 0.02f);
            }
            else
            {
                Gizmos.DrawSphere(transform.position + transform.TransformVector(getThisCenterOfMass()), 0.02f);
            }
        }
    }
}

