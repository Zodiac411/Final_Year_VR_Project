using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using UnityEngine;

public class VRKeyboard : MonoBehaviour
{
    [SerializeField] private InputField AttachedInputField;
    [SerializeField] private bool UseShift = false;

    [Header("Sound FX")]
    [SerializeField] private AudioClip KeyPressSound;

    private List<VRKeyboardKey> KeyboardKeys;

    private void Awake()
    {
        KeyboardKeys = transform.GetComponentsInChildren<VRKeyboardKey>().ToList();
    }

    public void PressKey(string key)
    {

        if (AttachedInputField != null)
        {
            UpdateInputField(key);
        }
        else
        {
            Debug.Log("Pressed Key : " + key);
        }
    }

    public void UpdateInputField(string key)
    {
        string currentText = AttachedInputField.text;
        int caretPosition = AttachedInputField.caretPosition;
        int textLength = currentText.Length;
        bool caretAtEnd = AttachedInputField.isFocused == false || caretPosition == textLength;

        string formattedKey = key;
        if (key.ToLower() == "space")
        {
            formattedKey = " ";
        }

        if (formattedKey.ToLower() == "backspace")
        {
            if (caretPosition == 0)
            {
                PlayClickSound();
                return;
            }

            currentText = currentText.Remove(caretPosition - 1, 1);

            if (!caretAtEnd)
            {
                MoveCaretBack();
            }
        }
        else if (formattedKey.ToLower() == "enter")
        {
            // EventSystems.ExecuteEvents.Execute(AttachedInputField.gameObject, null, UnityEngine.EventSystems.ExecuteEvents.submitHandler);
        }
        else if (formattedKey.ToLower() == "shift")
        {
            ToggleShift();
        }
        else
        {
            if (caretAtEnd)
            {
                currentText += formattedKey;
                MoveCaretUp();
            }
            else
            {
                string preText = "";
                if (caretPosition > 0)
                {
                    preText = currentText.Substring(0, caretPosition);
                }
                MoveCaretUp();

                string postText = currentText.Substring(caretPosition, textLength - preText.Length);

                currentText = preText + formattedKey + postText;
            }
        }

        AttachedInputField.text = currentText;
        PlayClickSound();

        if (!AttachedInputField.isFocused)
        {
            AttachedInputField.Select();
        }
    }

    public virtual void PlayClickSound()
    {
        if (KeyPressSound != null)
        {
            XRManager.Instance.PlaySpatialClipAt(KeyPressSound, transform.position, 1f, 0.5f);
        }
    }

    public void MoveCaretUp()
    {
        StartCoroutine(IncreaseInputFieldCareteRoutine());
    }

    public void MoveCaretBack()
    {
        StartCoroutine(DecreaseInputFieldCareteRoutine());
    }

    public void ToggleShift()
    {
        UseShift = !UseShift;

        foreach (var key in KeyboardKeys)
        {
            if (key != null)
            {
                key.ToggleShift();
            }
        }
    }

    private IEnumerator IncreaseInputFieldCareteRoutine()
    {
        yield return new WaitForEndOfFrame();
        AttachedInputField.caretPosition = AttachedInputField.caretPosition + 1;
        AttachedInputField.ForceLabelUpdate();
    }

    private IEnumerator DecreaseInputFieldCareteRoutine()
    {
        yield return new WaitForEndOfFrame();
        AttachedInputField.caretPosition = AttachedInputField.caretPosition - 1;
        AttachedInputField.ForceLabelUpdate();
    }

    public void AttachToInputField(UnityEngine.UI.InputField inputField)
    {
        AttachedInputField = inputField;
    }
}