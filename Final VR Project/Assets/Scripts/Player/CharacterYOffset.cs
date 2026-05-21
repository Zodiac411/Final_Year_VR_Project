using UnityEngine;

public class CharacterYOffset : MonoBehaviour
{
    private void LateUpdate()
    {
        float yOffset = transform.parent.localPosition.y;
        transform.localPosition = new Vector3(transform.localPosition.x, -1 - yOffset, transform.localPosition.z);
    }
}
