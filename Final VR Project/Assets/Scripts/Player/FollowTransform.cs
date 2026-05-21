using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] private Transform FollowTarget;

    [SerializeField] private bool MatchRotation = true;
    [SerializeField] private float YOffset = 0;

    private void Update()
    {
        if (FollowTarget)
        {
            transform.position = FollowTarget.position;

            if (YOffset != 0)
            {
                transform.position += new Vector3(0, YOffset, 0);
            }

            if (MatchRotation)
            {
                transform.rotation = FollowTarget.rotation;
            }
        }
    }
}