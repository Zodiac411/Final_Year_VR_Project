using UnityEngine.InputSystem;
using UnityEngine;

public class ToggleActiveOnInputAction : MonoBehaviour
{
    [SerializeField] private InputActionReference InputAction = default;
    [SerializeField] private GameObject ToggleObject = default;
    [SerializeField] private GameObject SettingsObject = default;

    private void OnEnable()
    {
        InputAction.action.performed += ToggleActive;
    }

    private void OnDisable()
    {
        InputAction.action.performed -= ToggleActive;
    }

    public void ToggleActive(InputAction.CallbackContext context)
    {
        if (ToggleObject)
        {
            ToggleObject.SetActive(!ToggleObject.activeSelf);
        }

        if (SettingsObject && SettingsObject.activeSelf)
        {
            SettingsObject.SetActive(false);
            ToggleObject.SetActive(false);
        }
    }
}

