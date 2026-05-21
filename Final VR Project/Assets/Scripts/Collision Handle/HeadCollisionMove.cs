using System.Collections;
using UnityEngine;

public class HeadCollisionMove : MonoBehaviour
{
    [SerializeField] private GameObject cameraRig;
    [SerializeField] private Transform centerEyeAnchor;

    [SerializeField] private string worldTag = "World";

    [SerializeField] private bool CollisionEnabled = true;
    [SerializeField] private bool OnlyCollideAgainstWorld = true;

    private void OnCollisionStay(Collision collision)
    {

        if (!CollisionEnabled)
        {
            return;
        }

        if (OnlyCollideAgainstWorld && !collision.collider.CompareTag(worldTag))
        {
            return;
        }

        StartCoroutine(PushBackPlayer());
    }

    private void OnCollisionExit(Collision collision)
    {
        if (OnlyCollideAgainstWorld && !collision.collider.CompareTag(worldTag))
        {
            return;
        }

        StopCoroutine(PushBackPlayer());
    }

    private IEnumerator PushBackPlayer()
    {
        if (!CollisionEnabled)
        {
            yield break;
        }

        var delta = transform.position - centerEyeAnchor.position;
        delta.y = 0f;
        cameraRig.transform.position += delta;
    }
}
