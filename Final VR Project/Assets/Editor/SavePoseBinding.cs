using UnityEngine.InputSystem;
using UnityEngine;

public class SavePoseBinding : MonoBehaviour
{
    [Header("Save Input")]
    public InputAction SaveInput;

    [Header("Save name prefix")]
    public string SaveNamePrefix = "HandPose";

    [Header("Debug")]
    public bool ShowKeybindingToolTip = true;

    private HandPoser handPoser;

    private void Start()
    {
        handPoser = GetComponent<HandPoser>();

        if (SaveInput != null)
        {
            SaveInput.Enable();
        }
    }

    private void Update()
    {
        if (SaveInput != null && SaveInput.triggered)
        {
            handPoser.CreateUniquePose(SaveNamePrefix);
            Debug.Log("Created Hand Pose with prefix " + SaveNamePrefix);
        }
    }

    private void OnGUI()
    {
        if (ShowKeybindingToolTip)
        {
            GUI.Box(new Rect(20, 20, 480, 24), "Press '<b>" + SaveInput.bindings[0].path + "</b>' to save the current hand pose");
        }
    }
}

