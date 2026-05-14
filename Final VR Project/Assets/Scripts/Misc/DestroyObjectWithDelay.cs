using UnityEngine;

public class DestroyObjectWithDelay : MonoBehaviour {

    public float DestroySeconds = 0f;

    void Start() 
    {
        Destroy(this.gameObject, DestroySeconds);
    }
}
