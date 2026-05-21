using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine;

public class SnapZoneChecker : MonoBehaviour
{
    [Header("UI Invetory Text")]
    [SerializeField] private Text uiLSText;
    [SerializeField] private Text uiLWText;
    [SerializeField] private Text uiRSText;
    [SerializeField] private Text uiRWText;

    [Header("Inventory SnapZone")]
    [SerializeField] private SnapZone holsterLeft;
    [SerializeField] private SnapZone holsterRight;
    [SerializeField] private SnapZone leftShoulder;
    [SerializeField] private SnapZone rightShoulder;

    private void Update()
    {
        UpdateSnapZoneDisplay(holsterLeft, uiLWText);
        UpdateSnapZoneDisplay(holsterRight, uiRWText);
        UpdateSnapZoneDisplay(leftShoulder, uiLSText);
        UpdateSnapZoneDisplay(rightShoulder, uiRSText);
    }

    private void UpdateSnapZoneDisplay(SnapZone snapZone, Text uiText)
    {
        if (snapZone != null && uiText != null)
        {
            Grabbable heldItem = snapZone.HeldItem;

            if (heldItem != null)
            {
                uiText.text = heldItem.gameObject.name;
                uiText.color = Color.white;
            }
            else
            {
                uiText.text = "Empty";
                uiText.color = Color.red;
            }
        }
    }

}
