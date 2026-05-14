using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine;

public class VRUISystem : BaseInputModule
{
    [Header("XR Controller Options : ")]
    public Transform LeftPointerTransform;
    public Transform RightPointerTransform;

    [SerializeField] private ControllerHand SelectedHand = ControllerHand.Right;
    [SerializeField] private List<ControllerBinding> ControllerInput = new List<ControllerBinding>() { ControllerBinding.RightTrigger };
    [SerializeField] private InputActionReference UIInputAction;
    [SerializeField] private LayerMask PhysicsRaycasterEventMask;

    [SerializeField] private bool AddPhysicsRaycaster = false;
    [SerializeField] private bool RightThumbstickScroll = true;

    [SerializeField] private GameObject PressingObject;
    [SerializeField] private GameObject DraggingObject;
    [SerializeField] private GameObject ReleasingObject;

    public PointerEventData EventData { get; private set; }

    private Camera cameraCaster;
    private GameObject _initialPressObject;

    private bool _lastInputDown;
    private bool inputDown;

    private static VRUISystem _instance;

    public static VRUISystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<VRUISystem>();

                if (_instance == null)
                {
                    EventSystem eventSystem = EventSystem.current;
                    if (eventSystem == null)
                    {
                        eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>(); ;
                    }

                    _instance = eventSystem.gameObject.AddComponent<VRUISystem>();
                }
            }

            return _instance;
        }
    }

    protected override void Awake()
    {
        initEventSystem();
    }

    protected virtual void initEventSystem()
    {
        UpdateControllerHand(SelectedHand);
        AssignCameraToAllCanvases(cameraCaster);

        EventData = new PointerEventData(eventSystem);
        EventData.position = new Vector2(cameraCaster.pixelWidth / 2, cameraCaster.pixelHeight / 2);
    }

    protected override void Start()
    {
        base.Start();
        AssignCameraToAllCanvases(cameraCaster);
    }

    private void CameraSetup()
    {
        if (cameraCaster == null)
        {
            var go = new GameObject("CameraCaster");
            cameraCaster = go.AddComponent<Camera>();
            cameraCaster.stereoTargetEye = StereoTargetEyeMask.None;
            cameraCaster.fieldOfView = 5f;
            cameraCaster.nearClipPlane = 0.01f;
            cameraCaster.clearFlags = CameraClearFlags.Nothing;
            cameraCaster.enabled = false;

            if (AddPhysicsRaycaster)
            {
                var pr = go.AddComponent<PhysicsRaycaster>();
                pr.eventMask = PhysicsRaycasterEventMask;
            }
        }
    }

    public override void Process()
    {
        DoProcess();
    }

    public void DoProcess()
    {
        if (EventData == null || !CameraCasterReady())
        {
            return;
        }

        EventData.position = new Vector2(cameraCaster.pixelWidth / 2, cameraCaster.pixelHeight / 2);

        eventSystem.RaycastAll(EventData, m_RaycastResultCache);

        EventData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        m_RaycastResultCache.Clear();

        HandlePointerExitAndEnter(EventData, EventData.pointerCurrentRaycast.gameObject);

        ExecuteEvents.Execute(EventData.pointerDrag, EventData, ExecuteEvents.dragHandler);

        if (RightThumbstickScroll)
        {
            EventData.scrollDelta = XRInput.Instance.RightThumbstickAxis;
            if (!Mathf.Approximately(EventData.scrollDelta.sqrMagnitude, 0))
            {
                ExecuteEvents.Execute(ExecuteEvents.GetEventHandler<IScrollHandler>(EventData.pointerCurrentRaycast.gameObject), EventData, ExecuteEvents.scrollHandler);
            }
        }

        inputDown = InputReady();

        if (inputDown && _lastInputDown == false)
        {
            PressDown();
        }
        else if (inputDown)
        {
            Press();
        }
        else
        {
            Release();
        }

        _lastInputDown = inputDown;
    }

    public virtual bool InputReady()
    {
        if (!CameraCasterReady())
        {
            return false;
        }

        if (UIInputAction != null && UIInputAction.action.ReadValue<float>() == 1f)
        {
            return true;
        }

        for (int i = 0; i < ControllerInput.Count; i++)
        {
            if (XRInput.Instance.GetControllerBindingValue(ControllerInput[i]))
            {
                return true;
            }
        }

        return false;
    }

    public virtual bool CameraCasterReady()
    {
        if (cameraCaster != null && !cameraCaster.gameObject.activeInHierarchy)
        {
            return false;
        }

        return true;
    }

    public virtual void PressDown()
    {
        EventData.pointerPressRaycast = EventData.pointerCurrentRaycast;

        if (_initialPressObject != null)
        {
            _initialPressObject = null;
        }

        _initialPressObject = ExecuteEvents.GetEventHandler<IPointerClickHandler>(EventData.pointerPressRaycast.gameObject);

        SetPressingObject(_initialPressObject);
        ExecuteEvents.Execute(EventData.pointerPress, EventData, ExecuteEvents.pointerDownHandler);

        SetDraggingObject(ExecuteEvents.GetEventHandler<IDragHandler>(EventData.pointerPressRaycast.gameObject));
        ExecuteEvents.Execute(EventData.pointerDrag, EventData, ExecuteEvents.beginDragHandler);
    }

    public virtual void Press()
    {
        EventData.pointerPressRaycast = EventData.pointerCurrentRaycast;

        SetPressingObject(ExecuteEvents.GetEventHandler<IPointerClickHandler>(EventData.pointerPressRaycast.gameObject));
        ExecuteEvents.Execute(EventData.pointerPress, EventData, ExecuteEvents.pointerDownHandler);

        SetDraggingObject(ExecuteEvents.GetEventHandler<IDragHandler>(EventData.pointerPressRaycast.gameObject));
        ExecuteEvents.Execute(EventData.pointerDrag, EventData, ExecuteEvents.beginDragHandler);
    }

    public virtual void Release()
    {
        SetReleasingObject(ExecuteEvents.GetEventHandler<IPointerClickHandler>(EventData.pointerCurrentRaycast.gameObject));

        if (EventData.pointerPress == ReleasingObject)
        {
            ExecuteEvents.Execute(EventData.pointerPress, EventData, ExecuteEvents.pointerClickHandler);
        }

        ExecuteEvents.Execute(EventData.pointerPress, EventData, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(EventData.pointerDrag, EventData, ExecuteEvents.endDragHandler);

        ClearAll();
    }

    public virtual void ClearAll()
    {
        SetPressingObject(null);
        SetDraggingObject(null);

        EventData.pointerCurrentRaycast.Clear();
    }

    public virtual void SetPressingObject(GameObject pressing)
    {
        EventData.pointerPress = pressing;
        PressingObject = pressing;
    }

    public virtual void SetDraggingObject(GameObject dragging)
    {
        EventData.pointerDrag = dragging;
        DraggingObject = dragging;
    }

    public virtual void SetReleasingObject(GameObject releasing)
    {
        ReleasingObject = releasing;
    }

    public virtual void AssignCameraToAllCanvases(Camera cam)
    {
        Canvas[] allCanvas = FindObjectsOfType<Canvas>();
        for (int x = 0; x < allCanvas.Length; x++)
        {
            AddCanvasToCamera(allCanvas[x], cam);
        }
    }

    public virtual void AddCanvas(Canvas canvas)
    {
        AddCanvasToCamera(canvas, cameraCaster);
    }

    public virtual void AddCanvasToCamera(Canvas canvas, Camera cam)
    {
        if (cam != null)
        {
            canvas.worldCamera = cam;
        }
    }

    public virtual void UpdateControllerHand(ControllerHand hand)
    {
        CameraSetup();

        if (hand == ControllerHand.Left && LeftPointerTransform != null)
        {
            cameraCaster.transform.parent = LeftPointerTransform;
            cameraCaster.transform.localPosition = Vector3.zero;
            cameraCaster.transform.localEulerAngles = Vector3.zero;
        }
        else if (hand == ControllerHand.Right && RightPointerTransform != null)
        {
            cameraCaster.transform.parent = RightPointerTransform;
            cameraCaster.transform.localPosition = Vector3.zero;
            cameraCaster.transform.localEulerAngles = Vector3.zero;
        }
    }
}