using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private Color FadeColor = Color.black;

    [SerializeField] private float SceneFadeInDelay = 1f;
    [SerializeField] private float FadeOutSpeed = 6f;
    public float FadeInSpeed = 6f;
    
    [SerializeField] bool FadeOnSceneLoaded = true;

    private GameObject fadeObject;
    private RectTransform fadeObjectRect;
    private Canvas fadeCanvas;
    private CanvasGroup canvasGroup;
    private Image fadeImage;
    private IEnumerator fadeRoutine;

    private string faderName = "ScreenFader";

    void Awake()
    {
        initialize();
    }

    protected virtual void initialize()
    {
        if (fadeObject == null)
        {
            Canvas childCanvas = GetComponentInChildren<Canvas>();

            if (childCanvas != null && childCanvas.transform.name == faderName)
            {
                GameObject.Destroy(this.gameObject);
                return;
            }

            fadeObject = new GameObject();
            fadeObject.transform.parent = Camera.main.transform;
            fadeObject.transform.localPosition = new Vector3(0, 0, 0.03f);
            fadeObject.transform.localEulerAngles = Vector3.zero;
            fadeObject.transform.name = faderName;

            fadeCanvas = fadeObject.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.WorldSpace;
            fadeCanvas.sortingOrder = 100;

            canvasGroup = fadeObject.AddComponent<CanvasGroup>();
            canvasGroup.interactable = false;

            fadeImage = fadeObject.AddComponent<Image>();
            fadeImage.color = FadeColor;
            fadeImage.raycastTarget = false;

            fadeObjectRect = fadeObject.GetComponent<RectTransform>();
            fadeObjectRect.anchorMin = new Vector2(1, 0);
            fadeObjectRect.anchorMax = new Vector2(0, 1);
            fadeObjectRect.pivot = new Vector2(0.5f, 0.5f);
            fadeObjectRect.sizeDelta = new Vector2(0.2f, 0.2f);
            fadeObjectRect.localScale = new Vector2(2f, 2f);

        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        if (FadeOnSceneLoaded && fadeObject != null)
        {
            updateImageAlpha(FadeColor.a);

            StartCoroutine(fadeOutWithDelay(SceneFadeInDelay));
        }
    }

    private IEnumerator fadeOutWithDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        DoFadeOut();
    }

    public virtual void DoFadeIn()
    {

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        if (canvasGroup != null)
        {
            fadeRoutine = doFade(canvasGroup.alpha, 1);
            StartCoroutine(fadeRoutine);
        }
    }

    public virtual void DoFadeOut()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = doFade(canvasGroup.alpha, 0);
        StartCoroutine(fadeRoutine);
    }

    public virtual void SetFadeLevel(float fadeLevel)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        if (canvasGroup == null)
        {
            return;
        }

        fadeRoutine = doFade(canvasGroup.alpha, fadeLevel);
        StartCoroutine(fadeRoutine);
    }

    private IEnumerator doFade(float alphaFrom, float alphaTo)
    {

        float alpha = alphaFrom;

        updateImageAlpha(alpha);

        while (alpha != alphaTo)
        {

            if (alphaFrom < alphaTo)
            {
                alpha += Time.deltaTime * FadeInSpeed;
                if (alpha > alphaTo)
                {
                    alpha = alphaTo;
                }
            }
            else
            {
                alpha -= Time.deltaTime * FadeOutSpeed;
                if (alpha < alphaTo)
                {
                    alpha = alphaTo;
                }
            }

            updateImageAlpha(alpha);

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForEndOfFrame();

        updateImageAlpha(alphaTo);
    }

    protected virtual void updateImageAlpha(float alphaValue)
    {

        if (canvasGroup == null)
        {
            return;
        }

        if (!canvasGroup.gameObject.activeSelf)
        {
            canvasGroup.gameObject.SetActive(true);
        }

        canvasGroup.alpha = alphaValue;

        if (alphaValue == 0 && canvasGroup.gameObject.activeSelf)
        {
            canvasGroup.gameObject.SetActive(false);
        }
    }
}