using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GraphicRaycaster))]
[RequireComponent(typeof(Canvas))]
public class VRCanvas : MonoBehaviour
{
    private void Start()
    {
        VRUISystem.Instance.AddCanvas(GetComponent<Canvas>());
    }
}

