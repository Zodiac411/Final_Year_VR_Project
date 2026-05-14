using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;

public class PlayTimeTracker : MonoBehaviour
{
    [SerializeField] private Text playTimeText;
    [SerializeField] private Image notificationImage;

    [SerializeField] private GameObject notificationUI;

    [SerializeField] private int notificationIntervalMinutes = 20;

    public UnityAction OnNotificationTimeReached;

    private float playTime = 0f;
    private float notificationInterval;

    private bool notificationVisible = false;

    private void Start()
    {
        notificationInterval = notificationIntervalMinutes * 60;
    }

    private void Update()
    {
        playTime += Time.deltaTime;
        UpdatePlayTimeDisplay();

        if (playTime >= notificationInterval && !notificationVisible)
        {
            OnNotificationTimeReached?.Invoke();
            notificationInterval += notificationIntervalMinutes * 60;
        }
    }

    private void UpdatePlayTimeDisplay()
    {
        int hours = (int)(playTime / 3600);
        int minutes = (int)((playTime % 3600) / 60);
        int seconds = (int)(playTime % 60);

        if (playTimeText != null)
        {
            playTimeText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
    }

    public void ToggleNotificationUI(bool show)
    {
        notificationVisible = show;
        if (notificationUI != null)
        {
            notificationUI.SetActive(show);
        }
    }

    public bool NotificationReached()
    {
        return playTime >= notificationInterval;
    }

    public bool IsNotificationVisible()
    {
        return notificationVisible;
    }
}
