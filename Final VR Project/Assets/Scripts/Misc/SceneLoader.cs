using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private LoadSceneMode loadSceneMode = LoadSceneMode.Single;

    [FormerlySerializedAs("UseSceenFader")]
    public bool UseScreenFader = true;

    public float ScreenFadeTime = 0.5f;

    private ScreenFader sf;

    private string _loadSceneName = string.Empty;

    public void LoadScene(string SceneName)
    {
        _loadSceneName = SceneName;

        if (UseScreenFader)
        {
            StartCoroutine(FadeThenLoadScene());
        }
        else
        {
            SceneManager.LoadScene(_loadSceneName, loadSceneMode);
        }
    }

    public IEnumerator FadeThenLoadScene()
    {

        if (UseScreenFader)
        {
            if (sf == null)
            {
                sf = FindObjectOfType<ScreenFader>();
                if (sf != null)
                {
                    sf.DoFadeIn();
                }
            }
        }

        if (ScreenFadeTime > 0)
        {
            yield return new WaitForSeconds(ScreenFadeTime);
        }

        SceneManager.LoadScene(_loadSceneName, loadSceneMode);
    }
}
