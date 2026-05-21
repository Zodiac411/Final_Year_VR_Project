using UnityEngine;

public class ScaleMaterialHelper : MonoBehaviour
{
    [SerializeField] private Vector2 Tiling = new Vector2(1, 1);
    [SerializeField] private Vector2 Offset;

    private Renderer ren;

    private void Start()
    {
        ren = GetComponent<Renderer>();
        updateTexture();
    }

    private void updateTexture()
    {
        if (ren != null && ren.material != null)
        {
            ren.material.mainTextureScale = Tiling;
            ren.material.mainTextureOffset = Offset;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isEditor)
        {
            updateTexture();
        }
    }
}
