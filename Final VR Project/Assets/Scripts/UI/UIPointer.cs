using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

public class UIPointer : MonoBehaviour
{
    [SerializeField] private ControllerHand ControllerSide = ControllerHand.Right;
    [SerializeField] private GameObject cursor;

    [SerializeField] private bool AutoUpdateUITransforms = true;
    [SerializeField] private bool HidePointerIfNoObjectsFound = true;

    [SerializeField] private float FixedPointerLength = 0.5f;
    [SerializeField] private float CursorMinScale = 0.6f;
    [SerializeField] private float CursorMaxScale = 6.0f;

    public float LineDistanceModifier = 0.8f;

    public bool CursorScaling = true;

    private Vector3 _cursorInitialLocalScale;
    private GameObject _cursor;

    private VRUISystem uiSystem;
    private PointerEvents selectedPointerEvents;
    private PointerEventData data;

    [SerializeField] private LineRenderer lineRenderer;

    private void Awake()
    {

        if (cursor)
        {
            _cursor = GameObject.Instantiate(cursor);
            _cursor.transform.SetParent(transform);
            _cursorInitialLocalScale = transform.localScale;
        }

        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        uiSystem = VRUISystem.Instance;
    }

    private void OnEnable()
    {
        updateUITransforms();
    }

    private void updateUITransforms()
    {
        if (AutoUpdateUITransforms && ControllerSide == ControllerHand.Left)
        {
            uiSystem.LeftPointerTransform = this.transform;
        }
        else if (AutoUpdateUITransforms && ControllerSide == ControllerHand.Right)
        {
            uiSystem.RightPointerTransform = this.transform;
        }

        uiSystem.UpdateControllerHand(ControllerSide);
    }

    public void Update()
    {
        UpdatePointer();
    }

    public virtual void UpdatePointer()
    {
        data = uiSystem.EventData;

        if (data == null || data.pointerCurrentRaycast.gameObject == null)
        {

            HidePointer();

            return;
        }

        if (_cursor != null)
        {

            bool lookingAtUI = data.pointerCurrentRaycast.module.GetType() == typeof(GraphicRaycaster);
            selectedPointerEvents = data.pointerCurrentRaycast.gameObject.GetComponent<PointerEvents>();
            bool lookingAtPhysicalObject = selectedPointerEvents != null;

            if (lookingAtPhysicalObject)
            {
                if (data.pointerCurrentRaycast.distance > selectedPointerEvents.MaxDistance)
                {
                    HidePointer();
                    return;
                }
            }

            if (!lookingAtUI && !lookingAtPhysicalObject)
            {
                HidePointer();
                return;
            }

            float distance = Vector3.Distance(transform.position, data.pointerCurrentRaycast.worldPosition);
            _cursor.transform.localPosition = new Vector3(0, 0, distance - 0.0001f);
            _cursor.transform.rotation = Quaternion.FromToRotation(Vector3.forward, data.pointerCurrentRaycast.worldNormal);

            float cameraDist = Vector3.Distance(Camera.main.transform.position, _cursor.transform.position);
            _cursor.transform.localScale = _cursorInitialLocalScale * Mathf.Clamp(cameraDist, CursorMinScale, CursorMaxScale);

            _cursor.SetActive(data.pointerCurrentRaycast.distance > 0);
        }

        if (lineRenderer)
        {
            lineRenderer.useWorldSpace = false;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, new Vector3(0, 0, Vector3.Distance(transform.position, data.pointerCurrentRaycast.worldPosition) * LineDistanceModifier));
            lineRenderer.enabled = data.pointerCurrentRaycast.distance > 0;
        }
    }

    public virtual void HidePointer()
    {
        if (HidePointerIfNoObjectsFound)
        {
            _cursor.SetActive(false);
            lineRenderer.enabled = false;
        }
        else
        {
            if (_cursor)
            {
                _cursor.SetActive(false);
            }

            if (lineRenderer)
            {
                lineRenderer.useWorldSpace = false;
                lineRenderer.SetPosition(0, Vector3.zero);
                lineRenderer.SetPosition(1, new Vector3(0, 0, FixedPointerLength * LineDistanceModifier));
                lineRenderer.enabled = true;
            }
        }
    }
}
