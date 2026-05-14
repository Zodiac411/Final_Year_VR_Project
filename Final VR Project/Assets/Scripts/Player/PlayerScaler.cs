using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScaler : MonoBehaviour
{
    [SerializeField] private List<Transform> ScaleObjects;

    [SerializeField] private float Scale = 1f;
    public float scaleSpeed = 0.1f;

    private void Update()
    {
        if (XRInput.Instance.RightThumbstickAxis.y < -0.5f || Input.GetKey(KeyCode.H))
        {
            Scale -= Time.deltaTime * 1;
        }
        else if (XRInput.Instance.RightThumbstickAxis.y > 0.5f || Input.GetKey(KeyCode.J))
        {
            Scale += Time.deltaTime * 1;
        }

        Scale = Mathf.Clamp(Scale, 0.1f, 2f);

        foreach (var t in ScaleObjects)
        {
            t.localScale = new Vector3(Scale, Scale, Scale);
        }
    }
}

