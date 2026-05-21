using System.Collections.Generic;
using UnityEngine;

public class UICanvasGroup : MonoBehaviour
{
    [SerializeField] private List<GameObject> CanvasObjects;

    public void ActivateCanvas(int CanvasIndex)
    {
        for (int i = 0; i < CanvasObjects.Count; i++)
        {
            if (CanvasObjects[i] != null)
            {
                CanvasObjects[i].SetActive(i == CanvasIndex);
            }
        }
    }
}

