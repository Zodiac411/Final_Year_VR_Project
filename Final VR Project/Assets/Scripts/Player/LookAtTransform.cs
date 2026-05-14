using UnityEngine;

public class LookAtTransform : MonoBehaviour
{
    [SerializeField] private Transform LookAt;

    [SerializeField] private float Speed = 20f;

    private bool UseLerp = true;
    private bool UseUpdate = false;
    private bool UseLateUpdate = true;

    private void Update()
    {
        if (UseUpdate)
        {
            lookAt();
        }
    }

    private void LateUpdate()
    {
        if (UseLateUpdate)
        {
            lookAt();
        }
    }

    private void lookAt()
    {
        if (LookAt != null)
        {
            if (UseLerp)
            {
                Quaternion rot = Quaternion.LookRotation(LookAt.position - transform.position);

                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * Speed);
            }
            else
            {
                transform.LookAt(LookAt, transform.forward);
            }
        }
    }
}