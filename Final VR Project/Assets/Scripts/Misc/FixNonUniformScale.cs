using UnityEngine;

public class FixNonUniformScale : MonoBehaviour
{
    private bool running = false;

    private void OnDrawGizmosSelected()
    {
        if (running)
        {
            return;
        }

        running = true;

        MakeUniform();
        DestroyImmediate(this);
    }

    public void MakeUniform()
    {
        Vector3 originalScale = transform.localScale;

        if (originalScale == Vector3.one)
        {
            return;
        }

        transform.localScale = Vector3.one;

        MeshRenderer ren = GetComponent<MeshRenderer>();
        MeshFilter filter = GetComponent<MeshFilter>();

        if (ren != null)
        {
            Transform renObject = new GameObject("Renderer").transform;

            if (filter)
            {
                MeshFilter mf = renObject.gameObject.AddComponent<MeshFilter>();
                mf.sharedMesh = filter.sharedMesh;
                DestroyImmediate(filter);
            }

            MeshRenderer newRenderer = renObject.gameObject.AddComponent<MeshRenderer>();
            newRenderer.sharedMaterial = ren.sharedMaterial;

            renObject.parent = transform;
            renObject.localPosition = Vector3.zero;
            renObject.localRotation = Quaternion.identity;
            renObject.localScale = originalScale;

            DestroyImmediate(ren);
        }
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            col.center = Vector3.zero;
            col.size = originalScale;
        }
    }
}