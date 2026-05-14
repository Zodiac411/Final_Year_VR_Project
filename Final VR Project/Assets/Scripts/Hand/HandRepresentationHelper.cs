using UnityEngine;

public class HandRepresentationHelper : MonoBehaviour
{

    [SerializeField] private Transform HandToToggle;
    [SerializeField] private Transform OtherHandTransform;

    [SerializeField] private float DistanceToShow = 0.1f;

    private void Update()
    {
        if (Vector3.Distance(HandToToggle.position, OtherHandTransform.position) >= DistanceToShow)
        {
            HandToToggle.gameObject.SetActive(true);
        }
        else
        {
            HandToToggle.gameObject.SetActive(false);
        }
    }
}

