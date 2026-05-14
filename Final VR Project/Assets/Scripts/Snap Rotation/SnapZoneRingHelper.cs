using UnityEngine.UI;
using UnityEngine;

public class SnapZoneRingHelper : MonoBehaviour
{
    [SerializeField] private SnapZone Snap;

    [SerializeField] private Color RestingColor = Color.gray;
    [SerializeField] private Color ValidSnapColor = Color.white;

    [SerializeField] private float RestingScale = 1000f;
    [SerializeField] private float ValidSnapScale = 800f;

    [SerializeField] private float ScaleSpeed = 50f;

    private CanvasScaler ringCanvas;
    private Text ringText;
    private GrabbablesInTrigger nearbyGrabbables;

    bool validSnap = false;

    private void Start()
    {
        nearbyGrabbables = Snap.GetComponent<GrabbablesInTrigger>();
        ringCanvas = GetComponent<CanvasScaler>();
        ringText = GetComponent<Text>();
    }

    private void Update()
    {
        validSnap = checkIsValidSnap();

        float lerpTo = validSnap ? ValidSnapScale : RestingScale;
        ringCanvas.dynamicPixelsPerUnit = Mathf.Lerp(ringCanvas.dynamicPixelsPerUnit, lerpTo, Time.deltaTime * ScaleSpeed);

        ringText.color = validSnap ? ValidSnapColor : RestingColor;
    }

    private bool checkIsValidSnap()
    {
        if (nearbyGrabbables != null)
        {
            if (Snap.HeldItem != null)
            {
                return false;
            }
            if (Snap.ClosestGrabbable != null)
            {
                return true;
            }
        }

        return false;
    }
}

