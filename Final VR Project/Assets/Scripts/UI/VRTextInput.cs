using UnityEngine.UI;
using UnityEngine;

public class VRTextInput : MonoBehaviour
{
    [SerializeField] private VRKeyboard AttachedKeyboard;

    public bool AttachToVRKeyboard = true;
    [SerializeField] private bool ActivateKeyboardOnSelect = true;
    [SerializeField] private bool DeactivateKeyboardOnDeselect = false;

    private InputField thisInputField;

    private bool isFocused, wasFocused = false;

    private float lastActivatedTime = 0;

    private void Awake()
    {
        thisInputField = GetComponent<InputField>();

        if (thisInputField && AttachedKeyboard != null)
        {
            AttachedKeyboard.AttachToInputField(thisInputField);
        }
    }

    private void Update()
    {
        isFocused = thisInputField != null && thisInputField.isFocused;

        if (isFocused == true && wasFocused == false)
        {
            OnInputSelect();
        }
        else if (isFocused == false && wasFocused == true)
        {
            OnInputDeselect();
        }

        wasFocused = isFocused;
    }

    public void OnInputSelect()
    {
        if (ActivateKeyboardOnSelect && AttachedKeyboard != null && !AttachedKeyboard.gameObject.activeInHierarchy)
        {
            AttachedKeyboard.gameObject.SetActive(true);

            AttachedKeyboard.AttachToInputField(thisInputField);

            lastActivatedTime = Time.time;
        }
    }

    public void OnInputDeselect()
    {
        if (DeactivateKeyboardOnDeselect && AttachedKeyboard != null && AttachedKeyboard.gameObject.activeInHierarchy && Time.time - lastActivatedTime >= 0.1f)
        {
            AttachedKeyboard.gameObject.SetActive(false);
        }
    }

    private void Reset()
    {
        var keyboard = GameObject.FindObjectOfType<VRKeyboard>();
        if (keyboard)
        {
            AttachedKeyboard = keyboard;
            Debug.Log("Found and attached Keyboard to " + AttachedKeyboard.transform.name);
        }
    }
}

