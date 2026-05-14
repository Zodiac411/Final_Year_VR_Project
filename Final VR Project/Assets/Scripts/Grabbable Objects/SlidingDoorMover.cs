using UnityEngine;

public class SlidingDoorMover : MonoBehaviour
{
    [SerializeField] private float DoorSpeed = 5f;
    [SerializeField] private float OpenXValue = -1f;

    float targetXPosition = 0;
    float smoothedPosition = 0;

    private void Update()
    {
        smoothedPosition = Mathf.Lerp(smoothedPosition, targetXPosition, Time.deltaTime * DoorSpeed);
        transform.localPosition = new Vector3(smoothedPosition, 0, 0);
    }

    public void SetTargetPosition(float targetValue)
    {
        targetXPosition = OpenXValue * targetValue; // Ex: 0.5 means the door is halfway open, or at -0.25 local X position
    }
}

