using UnityEngine;

public class UIButtonCollider : MonoBehaviour
{
    [Header("Hold Button Down Option")]
    public float InitialHoldDownDelay = 0.5f;
    public float HoldDownDelay = 0.1f;

    [Header("Animate Key")]
    public float PressedInZValue = 0.01f;
    public float PressInSpeed = 15f;

    public bool CanBeHeldDown = true;
    public bool AnimateKey = true;

    UnityEngine.UI.Button uiButton;

    protected bool readyForDownEvent = true;
    protected BoxCollider boxCollider;

    protected int itemsInTrigger = 0;
    protected int clickCount = 0;

    protected float lastPressTime = 0f;
    protected float colliderInitialCenterZ = 0;

    private void Awake()
    {
        uiButton = GetComponentInParent<UnityEngine.UI.Button>();
        boxCollider = GetComponent<BoxCollider>();

        if (boxCollider)
        {
            colliderInitialCenterZ = boxCollider.center.z;
        }
    }

    private void Update()
    {
        if (itemsInTrigger > 0)
        {
            if (AnimateKey)
            {
                transform.parent.localPosition = Vector3.Lerp(transform.parent.localPosition, new Vector3(transform.parent.localPosition.x, transform.parent.localPosition.y, PressedInZValue), Time.deltaTime * PressInSpeed);

                if (boxCollider)
                {
                    boxCollider.center = Vector3.Lerp(boxCollider.center, new Vector3(boxCollider.center.x, boxCollider.center.y, colliderInitialCenterZ - PressedInZValue), Time.deltaTime * PressInSpeed);
                }
            }

            float requiredDelay = clickCount == 1 ? InitialHoldDownDelay : HoldDownDelay;
            if (CanBeHeldDown && !readyForDownEvent && (Time.time - lastPressTime >= requiredDelay))
            {
                readyForDownEvent = true;
            }

            if (readyForDownEvent)
            {
                if (uiButton != null && uiButton.onClick != null)
                {
                    uiButton.onClick.Invoke();
                }

                clickCount++;
                lastPressTime = Time.time;

                readyForDownEvent = false;
            }
        }
        else
        {
            if (AnimateKey)
            {
                transform.parent.localPosition = Vector3.Lerp(transform.parent.localPosition, new Vector3(transform.parent.localPosition.x, transform.parent.localPosition.y, 0), Time.deltaTime * PressInSpeed);

                if (boxCollider)
                {
                    boxCollider.center = Vector3.Lerp(boxCollider.center, new Vector3(boxCollider.center.x, boxCollider.center.y, colliderInitialCenterZ), Time.deltaTime * PressInSpeed);
                }
            }

            clickCount = 0;
            readyForDownEvent = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<UITrigger>() != null)
        {
            itemsInTrigger++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<UITrigger>() != null)
        {
            itemsInTrigger--;
        }
    }
}