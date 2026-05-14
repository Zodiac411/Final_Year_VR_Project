using UnityEngine;
using UnityEngine.UI;

public class XRManager : MonoBehaviour 
{
    public static XRManager Instance 
    {
        get 
        {
            if (inst == null) 
            {
                inst = FindObjectOfType<XRManager>();
                if (inst == null) 
                {
                    inst = new GameObject("XR Manager").AddComponent<XRManager>();
                }
            }
            return inst;
        }
    }

    private static XRManager inst;

    private Color LogTextColor = Color.cyan;
    private Color WarnTextColor = Color.yellow;
    private Color ErrTextColor = Color.red;

    [SerializeField] Transform DebugTextHolder;
    [SerializeField] private string LastDebugMsg;
        
    private float MaxTextEntries = 10;

    private int lastDebugMsgCount;

    private void Awake() 
    {
        if (inst != null && inst != this) 
        {
            Destroy(this);
            return;
        }

        inst = this;
    }                    
        
    public void Log(string message) 
    {
        Debug.Log(message, gameObject);
        XR_DebugLog(message, LogTextColor);
    }

    public void Warn(string message) 
    {
        Debug.LogWarning(message, gameObject);
        XR_DebugLog(message, WarnTextColor);
    }

    public void Error(string message) 
    {
        Debug.LogError(message, gameObject);
        XR_DebugLog(message, ErrTextColor);
    }

    public void XR_DebugLog(string msg, Color logColor) 
    {
        if (DebugTextHolder != null) 
        {
            if (msg == LastDebugMsg) 
            {
                GameObject lastChild = DebugTextHolder.GetChild(DebugTextHolder.childCount - 1).gameObject;
                Text lastChildLine = lastChild.GetComponent<Text>();
                lastDebugMsgCount++;

                lastChildLine.text = $"({lastDebugMsgCount}) {msg}";
            }
            else 
            {
                GameObject go = new GameObject();
                go.transform.parent = DebugTextHolder;
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.transform.name = "Debug Text";

                Text textLine = go.AddComponent<Text>();
                textLine.text = msg;
                textLine.horizontalOverflow = HorizontalWrapMode.Wrap;
                textLine.verticalOverflow = VerticalWrapMode.Overflow;
                textLine.color = logColor;
                textLine.fontSize = 32;
                textLine.font = Resources.GetBuiltinResource(typeof(Font), "LegacyRuntime.ttf") as Font;
                textLine.raycastTarget = false;

                RectTransform rect = go.GetComponent<RectTransform>();
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;

                lastDebugMsgCount = 1;
            }

            CullDebugPanel();
            LastDebugMsg = msg;
        }
    }

    public void CullDebugPanel() 
    {
        for (int i = DebugTextHolder.childCount; i > MaxTextEntries; i--) 
        {
            Destroy(DebugTextHolder.GetChild(0).gameObject);
        }
    }

    public AudioSource PlaySpatialClipAt(AudioClip clip, Vector3 pos, float volume, float spatialBlend = 1f, float randomizePitch = 0) {

        if(clip == null) 
        {
            return null;
        }

        GameObject go = new GameObject("SpatialAudio - Temp");
        go.transform.position = pos;

        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;

        source.pitch = getRandomizedPitch(randomizePitch);
        source.spatialBlend = spatialBlend;
        source.volume = volume;
        source.Play();

        Destroy(go, clip.length);

        return source;
    }

    float getRandomizedPitch(float randomAmount) {

        if(randomAmount != 0) {
            float randomPitch = Random.Range(-randomAmount, randomAmount);
            return Time.timeScale + randomPitch;
        }

        return Time.timeScale;
    }
}