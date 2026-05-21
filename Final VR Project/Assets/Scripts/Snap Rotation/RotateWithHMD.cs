using UnityEngine;

public class RotateWithHMD : MonoBehaviour
{
    [SerializeField] private Transform FollowTransform;
    [SerializeField] private CharacterController Character;
    [SerializeField] private Vector3 Offset = new Vector3(0, -0.25f, 0);

    [SerializeField] private float RotateSpeed = 5f;
    [SerializeField] private float MovementSmoothing = 0;

    [SerializeField] private bool ParentToCharacter = false;

    private Transform originalParent;
    private Transform followTransform;
    private Transform camTransform;
    
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        originalParent = transform.parent;
        followTransform = new GameObject().transform;
        followTransform.name = "RotateReferenceObject";
        followTransform.position = transform.position;
        followTransform.rotation = transform.rotation;

        if (ParentToCharacter)
        {
            transform.parent = Character.transform;
        }

        if (FollowTransform)
        {
            followTransform.parent = FollowTransform;
        }
        else if (Character)
        {
            followTransform.parent = Character.transform;
        }
        else
        {
            followTransform.parent = originalParent;
        }
    }

    private void LateUpdate()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {

        if (camTransform == null && GameObject.FindGameObjectWithTag("MainCamera") != null)
        {
            camTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
            followTransform.position = camTransform.position;
            followTransform.localEulerAngles = Vector3.zero;
        }

        if (camTransform == null)
        {
            return;
        }

        Vector3 worldOffset = Vector3.zero;
        if (FollowTransform)
        {
            worldOffset = FollowTransform.position - FollowTransform.TransformVector(Offset);
        }
        else if (Character)
        {
            worldOffset = Character.transform.position - Character.transform.TransformVector(Offset);
        }

        Vector3 moveToPosition = new Vector3(worldOffset.x, camTransform.position.y - Offset.y, worldOffset.z);
        transform.position = Vector3.SmoothDamp(transform.position, moveToPosition, ref velocity, MovementSmoothing);
        transform.rotation = Quaternion.Lerp(transform.rotation, followTransform.rotation, Time.deltaTime * RotateSpeed);
    }
}
