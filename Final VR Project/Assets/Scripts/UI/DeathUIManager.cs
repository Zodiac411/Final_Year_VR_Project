using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject deathNotificationUI;

    private void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath.AddListener(HandlePlayerDeath);
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath.RemoveListener(HandlePlayerDeath);
        }
    }

    private void HandlePlayerDeath()
    {
        Time.timeScale = 0;
        deathNotificationUI.SetActive(true);
    }

    public void PlayAgain()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Time.timeScale = 1;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
