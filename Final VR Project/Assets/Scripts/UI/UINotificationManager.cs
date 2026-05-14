using UnityEngine;
using UnityEngine.UI;

public class UINotificationManager : MonoBehaviour
{
    [SerializeField] private PlayTimeTracker playTimeTracker;
    [SerializeField] private GameObject notificationUI;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button ignoreButton;

    private void Start()
    {
        quitButton.onClick.AddListener(QuitGame);
        ignoreButton.onClick.AddListener(IgnoreNotification);

        playTimeTracker.OnNotificationTimeReached += PauseGame;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        notificationUI.SetActive(true);
    }

    public void IgnoreNotification()
    {
        Time.timeScale = 1;
        notificationUI.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
