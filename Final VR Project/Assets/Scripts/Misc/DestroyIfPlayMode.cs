using UnityEngine;

public class DestroyIfPlayMode : MonoBehaviour
{
    private void Start()
    {
        Destroy(this.gameObject);
    }
}

