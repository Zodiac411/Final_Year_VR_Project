using UnityEngine;
using UnityEngine.UI;

public class VRKeyboardKey : MonoBehaviour
{
    public string KeycodeShift;
    public string Keycode;

    private UnityEngine.UI.Button thisButton;
    private Text thisButtonText;

    private VRKeyboard vrKeyboard;

    [HideInInspector]
    public bool UseShiftKey = false;

    private void Awake()
    {
        thisButton = GetComponent<UnityEngine.UI.Button>();
        thisButtonText = GetComponentInChildren<Text>();

        if (thisButton != null)
        {
            thisButton.onClick.AddListener(OnKeyHit);
        }

        vrKeyboard = GetComponentInParent<VRKeyboard>();
    }

    public virtual void ToggleShift()
    {
        UseShiftKey = !UseShiftKey;

        if (thisButtonText == null)
        {
            return;
        }

        if (UseShiftKey && !string.IsNullOrEmpty(KeycodeShift))
        {
            thisButtonText.text = KeycodeShift;
        }
        else
        {
            thisButtonText.text = Keycode;
        }
    }

    public virtual void OnKeyHit()
    {
        OnKeyHit(UseShiftKey && !string.IsNullOrEmpty(KeycodeShift) ? KeycodeShift : Keycode);
    }

    public virtual void OnKeyHit(string key)
    {
        if (vrKeyboard != null)
        {
            vrKeyboard.PressKey(key);
        }
        else
        {
            Debug.Log("Pressed key " + key + ", but no keyboard was found");
        }
    }
}

