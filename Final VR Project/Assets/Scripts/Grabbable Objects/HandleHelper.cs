using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine;

public class HandleHelper : MonoBehaviour
{
    [SerializeField] private Rigidbody ParentRigid;
    [SerializeField] private Transform HandleTransform;

    private Grabbable thisGrab;
    private Vector3 lastAngularVelocity;
    private Rigidbody rb;
    private Collider col;

    private bool didRelease = false;

    private void Start()
    {
        thisGrab = GetComponent<Grabbable>();
        thisGrab.CanBeSnappedToSnapZone = false;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (col != null && ParentRigid != null && ParentRigid.GetComponent<Collider>() != null)
        {
            Physics.IgnoreCollision(ParentRigid.GetComponent<Collider>(), col, true);
        }
    }

    private void FixedUpdate()
    {
        if (!thisGrab.BeingHeld)
        {
            if (!didRelease)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                if (ParentRigid)
                {
                    ParentRigid.angularVelocity = lastAngularVelocity * 20;
                }
                col.enabled = true;
                StartCoroutine(doRelease());

                didRelease = true;
            }
        }
        else
        {
            didRelease = false;

            if (thisGrab.BreakDistance > 0 && Vector3.Distance(transform.position, HandleTransform.position) > thisGrab.BreakDistance)
            {
                thisGrab.DropItem(false, false);
            }

            lastAngularVelocity = rb.angularVelocity * -1;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Physics.IgnoreCollision(col, collision.collider, true);
    }

    IEnumerator doRelease()
    {
        yield return new WaitForSeconds(1f);
        col.enabled = true;
    }
}

