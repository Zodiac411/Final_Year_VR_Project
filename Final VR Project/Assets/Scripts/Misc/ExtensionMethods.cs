using UnityEngine.XR.Interaction.Toolkit;

public static class ExtensionMethods
{
    public static bool GetDown(this ControllerBinding binding)
    {
        return XRInput.Instance.GetControllerBindingValue(binding);
    }
}

