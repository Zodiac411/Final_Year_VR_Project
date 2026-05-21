using UnityEngine.UI;
using UnityEngine;
using System;

public class FPSText : MonoBehaviour
{
    private Text text;
    private float deltaTime = 0.0f;

    private void Start()
    {
        text = GetComponent<Text>();
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        text.text = Math.Ceiling(1.0f / deltaTime) + " FPS";
    }

    private void OnGUI()
    {
        text.text = Math.Ceiling(1.0f / deltaTime) + " FPS";
    }
}