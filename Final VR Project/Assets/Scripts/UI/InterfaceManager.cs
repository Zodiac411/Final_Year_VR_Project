using UnityEngine;

public class InterfaceManager : MonoBehaviour
{
    public GameObject uiInterface;
    public GameObject settingsInterface;

    public void ShowSettings()
    {
        uiInterface.SetActive(false);
        settingsInterface.SetActive(true);
    }

    public void ShowMainUI()
    {
        settingsInterface.SetActive(false);
        uiInterface.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }
}
