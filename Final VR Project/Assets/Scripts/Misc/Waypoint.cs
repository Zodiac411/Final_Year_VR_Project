using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public Waypoint Destination;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.1f);

        if (Destination != null)
        {
            Gizmos.DrawLine(transform.position, Destination.transform.position);
            Gizmos.DrawSphere(Destination.transform.position, 0.1f);
        }
    }
}

