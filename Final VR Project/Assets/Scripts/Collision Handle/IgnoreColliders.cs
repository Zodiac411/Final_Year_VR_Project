using System.Collections.Generic;
using UnityEngine;

public class IgnoreColliders : MonoBehaviour
{
    [SerializeField] private List<Collider> CollidersToIgnore;

    private void Start()
    {
        var thisCol = GetComponent<Collider>();

        if (CollidersToIgnore != null)
        {
            foreach (var col in CollidersToIgnore)
            {
                if (col && col.enabled)
                {
                    Physics.IgnoreCollision(thisCol, col, true);
                }
            }
        }
    }
}