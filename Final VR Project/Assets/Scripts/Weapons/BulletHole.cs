using UnityEngine;

public class BulletHole : MonoBehaviour
{
    [SerializeField] private Transform BulletHoleDecal;

    public float MaxScale = 1f;
    public float MinScale = 0.75f;

    [SerializeField] private float DestroyTime = 10f;

    private bool RandomYRotation = true;

    private void Start()
    {
        transform.localScale = Vector3.one * Random.Range(0.75f, 1.5f);

        if (BulletHoleDecal != null && RandomYRotation)
        {
            Vector3 currentRotation = BulletHoleDecal.transform.localEulerAngles;
            BulletHoleDecal.transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, Random.Range(0, 90f));
        }

        AudioSource audio = GetComponent<AudioSource>();
        audio.pitch = Time.timeScale;

        Invoke("DestroySelf", DestroyTime);
    }

    public void TryAttachTo(Collider col)
    {
        if (transformIsEqualScale(col.transform))
        {
            BulletHoleDecal.parent = col.transform;
            Destroy(BulletHoleDecal.gameObject, DestroyTime);
        }
        else if (col.gameObject.isStatic)
        {
            Destroy(BulletHoleDecal.gameObject, DestroyTime);
        }
        else
        {
            Destroy(BulletHoleDecal.gameObject, 0.1f);
        }
    }

    private bool transformIsEqualScale(Transform theTransform)
    {
        return theTransform.localScale.x == theTransform.localScale.y && theTransform.localScale.x == theTransform.localScale.z;
    }

    private void DestroySelf()
    {
        transform.parent = null;
        Destroy(this.gameObject);
    }
}