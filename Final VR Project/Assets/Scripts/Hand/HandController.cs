using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Setup : ")]
    public Animator HandAnimator;

    [SerializeField] private HandPoserType IdlePoseType = HandPoserType.HandPoser;
    [SerializeField] private Transform HandAnchor;
    [SerializeField] private HandPoser handPoser;
    [SerializeField] private AutoPoser autoPoser;
    [SerializeField] private Grabber grabber;

    [SerializeField] private int PoseId;

    private HandPoseBlender poseBlender;

    public bool UseIndexFingerTracking = true;
    public float HandAnimationSpeed = 20f;

    [Header("Override : ")]
    [SerializeField] private HandPose HandPoseOverride;

    [Range(0f, 1f)] public float GripAmount;
    private float previousGrip;

    [Range(0f, 1f)]
    public float PointAmount;
    private float _prevPoint;

    [Range(0f, 1f)]
    public float ThumbAmount;
    private float _prevThumb;

    private bool _thumbIsNear = false;
    private bool _indexIsNear = false;
    private float _triggerValue = 0f;
    private float _gripValue = 0f;

    private bool ResetHandAnchorPosition = true;

    private XRControllerSelectionBase offset;
    private XRInput input;
    private Rigidbody rb;
    private Transform offsetTransform;

    private Vector3 offsetPosition
    {
        get
        {
            if (offset)
            {
                return offset.OffsetPosition;
            }
            return Vector3.zero;
        }
    }

    private Vector3 offsetRotation
    {
        get
        {
            if (offset)
            {
                return offset.OffsetRotation;
            }
            return Vector3.zero;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        offset = GetComponent<XRControllerSelectionBase>();
        offsetTransform = new GameObject("OffsetHelper").transform;
        offsetTransform.parent = transform;

        if (HandAnchor)
        {
            transform.parent = HandAnchor;
            offsetTransform.parent = HandAnchor;

            if (ResetHandAnchorPosition)
            {
                transform.localPosition = offsetPosition;
                transform.localEulerAngles = offsetRotation;
            }
        }

        if (grabber == null)
        {
            grabber = GetComponentInChildren<Grabber>();
        }

        if (grabber != null)
        {
            grabber.onAfterGrabEvent.AddListener(OnGrabberGrabbed);
            grabber.onReleaseEvent.AddListener(OnGrabberReleased);
        }

        SetHandAnimator();

        input = XRInput.Instance;
    }

    public void Update()
    {
        CheckForGrabChange();
        UpdateFromInputs();

        if (HandPoseOverride != null)
        {
            UpdateHandPoser();
        }
        else if (HoldingObject())
        {
            UpdateHeldObjectState();
        }
        else
        {
            UpdateIdleState();
        }
    }

    public virtual void UpdateHeldObjectState()
    {
        if (IsAnimatorGrabbable())
        {
            UpdateAnimimationStates();
        }
        else if (IsHandPoserGrabbable())
        {
            UpdateHandPoser();
        }
        else if (IsAutoPoserGrabbable()) { }
    }

    public virtual void UpdateIdleState()
    {
        if (IdlePoseType == HandPoserType.Animator)
        {
            UpdateAnimimationStates();
        }
        else if (IdlePoseType == HandPoserType.HandPoser)
        {
            UpdateHandPoserIdleState();

        }
        else if (IdlePoseType == HandPoserType.AutoPoser)
        {
            EnableAutoPoser(true);
        }
    }

    public GameObject PreviousHeldObject;

    public virtual bool HoldingObject()
    {

        if (grabber != null && grabber.HeldGrabbable != null)
        {
            return true;
        }

        return false;
    }

    public virtual void CheckForGrabChange()
    {
        if (grabber != null)
        {
            if (grabber.HeldGrabbable == null && PreviousHeldObject != null)
            {
                OnGrabDrop();
            }
            else if (grabber.HeldGrabbable != null && 
                !ReferenceEquals(grabber.HeldGrabbable.gameObject, PreviousHeldObject))
            {
                OnGrabChange(grabber.HeldGrabbable.gameObject);
            }
        }
    }

    public virtual void OnGrabChange(GameObject newlyHeldObject)
    {
        if (HoldingObject())
        {
            if (HandPoseOverride != null)
            {
                // DO NOTHING
            }
            else if (grabber.HeldGrabbable.handPoseType == HandPoseType.AnimatorID)
            {
                EnableHandAnimator();
            }
            else if (grabber.HeldGrabbable.handPoseType == HandPoseType.AutoPoseOnce)
            {
                EnableAutoPoser(false);
            }
            else if (grabber.HeldGrabbable.handPoseType == HandPoseType.AutoPoseContinuous)
            {
                EnableAutoPoser(true);
            }
            else if (grabber.HeldGrabbable.handPoseType == HandPoseType.HandPose)
            {
                if (grabber.HeldGrabbable.SelectedHandPose != null)
                {
                    EnableHandPoser();

                    if (poseBlender != null)
                    {
                        poseBlender.UpdatePose = false;
                    }

                    if (handPoser != null)
                    {
                        handPoser.CurrentPose = grabber.HeldGrabbable.SelectedHandPose;
                    }
                }
            }
        }

        PreviousHeldObject = newlyHeldObject;
    }

    public virtual void OnGrabDrop()
    {
        if (IdlePoseType == HandPoserType.AutoPoser)
        {
            EnableAutoPoser(true);
        }
        else if (IdlePoseType == HandPoserType.HandPoser)
        {
            DisableAutoPoser();
        }
        else if (IdlePoseType == HandPoserType.Animator)
        {
            DisablePoseBlender();
            EnableHandAnimator();
            DisableAutoPoser();
        }

        PreviousHeldObject = null;
    }

    public virtual void SetHandAnimator()
    {
        if (HandAnimator == null || !HandAnimator.gameObject.activeInHierarchy)
        {
            HandAnimator = GetComponentInChildren<Animator>();
        }
    }
    public virtual void UpdateFromInputs()
    {
        if (grabber == null || !grabber.isActiveAndEnabled)
        {
            grabber = GetComponentInChildren<Grabber>();
            GripAmount = 0;
            PointAmount = 0;
            ThumbAmount = 0;
            return;
        }

        if (grabber.HandSide == ControllerHand.Left)
        {
            _indexIsNear = input.LeftTriggerNear;
            _thumbIsNear = input.LeftThumbNear;
            _triggerValue = input.LeftTrigger;
            _gripValue = input.LeftGrip;
        }
        else if (grabber.HandSide == ControllerHand.Right)
        {
            _indexIsNear = input.RightTriggerNear;
            _thumbIsNear = input.RightThumbNear;
            _triggerValue = input.RightTrigger;
            _gripValue = input.RightGrip;
        }

        GripAmount = _gripValue;
        ThumbAmount = _thumbIsNear ? 0 : 1;

        PointAmount = 1 - _triggerValue;
        PointAmount *= XRInput.Instance.InputSource == XRInputSource.SteamVR ? .25f : .5f;

        if (input.SupportsIndexTouch && _indexIsNear == false && PointAmount != 0)
        {
            PointAmount = 1f;
        }
        else if (!input.SupportsIndexTouch && _triggerValue == 0)
        {
            PointAmount = 1;
        }
    }

    public bool DoUpdateAnimationStates = true;
    public bool DoUpdateHandPoser = true;

    public virtual void UpdateAnimimationStates()
    {
        if (DoUpdateAnimationStates == false)
        {
            return;
        }

        if (IsAnimatorGrabbable() && !HandAnimator.isActiveAndEnabled)
        {
            EnableHandAnimator();
        }

        if (HandAnimator != null && HandAnimator.isActiveAndEnabled && HandAnimator.runtimeAnimatorController != null)
        {

            previousGrip = Mathf.Lerp(previousGrip, GripAmount, Time.deltaTime * HandAnimationSpeed);
            _prevThumb = Mathf.Lerp(_prevThumb, ThumbAmount, Time.deltaTime * HandAnimationSpeed);
            _prevPoint = Mathf.Lerp(_prevPoint, PointAmount, Time.deltaTime * HandAnimationSpeed);

            HandAnimator.SetFloat("Flex", previousGrip);

            HandAnimator.SetLayerWeight(1, _prevThumb);
            HandAnimator.SetLayerWeight(2, _prevPoint);

            if (grabber != null && grabber.HeldGrabbable != null)
            {
                HandAnimator.SetLayerWeight(0, 0);
                HandAnimator.SetLayerWeight(1, 0);
                HandAnimator.SetLayerWeight(2, 0);

                PoseId = (int)grabber.HeldGrabbable.CustomHandPose;

                if (grabber.HeldGrabbable.ActiveGrabPoint != null)
                {
                    HandAnimator.SetLayerWeight(0, 1);
                    HandAnimator.SetFloat("Flex", 1);

                    setAnimatorBlend
                        (grabber.HeldGrabbable.ActiveGrabPoint.IndexBlendMin, 
                        grabber.HeldGrabbable.ActiveGrabPoint.IndexBlendMax, PointAmount, 2);

                    setAnimatorBlend(grabber.HeldGrabbable.ActiveGrabPoint.ThumbBlendMin, 
                        grabber.HeldGrabbable.ActiveGrabPoint.ThumbBlendMax, ThumbAmount, 1);
                }
                else
                {
                    if (grabber.HoldingItem)
                    {
                        GripAmount = 1;
                        PointAmount = 0;
                        ThumbAmount = 0;
                    }
                }

                HandAnimator.SetInteger("Pose", PoseId);
            }
            else
            {
                HandAnimator.SetInteger("Pose", 0);
            }
        }
    }

    private void setAnimatorBlend(float min, float max, float input, int animationLayer)
    {
        HandAnimator.SetLayerWeight(animationLayer, min + (input) * max - min);
    }

    public virtual bool IsAnimatorGrabbable()
    {
        return HandAnimator != null && grabber != null && grabber.HeldGrabbable != null && 
            grabber.HeldGrabbable.handPoseType == HandPoseType.AnimatorID;
    }

    public virtual void UpdateHandPoser()
    {
        if (DoUpdateHandPoser == false)
        {
            return;
        }

        if (handPoser == null || !handPoser.isActiveAndEnabled)
        {
            handPoser = GetComponentInChildren<HandPoser>();
        }

        if (HandPoseOverride == null && (grabber == null || handPoser == null || grabber.HeldGrabbable == null ||
            grabber.HeldGrabbable.handPoseType != HandPoseType.HandPose))
        {
            return;
        }

        if (poseBlender != null && poseBlender.UpdatePose)
        {
            poseBlender.UpdatePose = false;
        }

        if (HandPoseOverride)
        {
            UpdateCurrentHandPose();
            if (handPoser.CurrentPose != HandPoseOverride)
            {

            }
        }
        else if (handPoser.CurrentPose == null || handPoser.CurrentPose != grabber.HeldGrabbable.SelectedHandPose)
        {
            UpdateCurrentHandPose();
        }
    }

    public virtual bool IsHandPoserGrabbable()
    {
        return handPoser != null && grabber != null && grabber.HeldGrabbable != null && 
            grabber.HeldGrabbable.handPoseType == HandPoseType.HandPose;
    }

    public virtual void UpdateHandPoserIdleState()
    {
        DisableHandAnimator();

        if (!SetupPoseBlender())
        {
            return;
        }

        poseBlender.UpdatePose = true;

        if (UseIndexFingerTracking && XRInput.Instance.IsValveIndexController)
        {
            return;
        }

        poseBlender.ThumbValue = Mathf.Lerp(poseBlender.ThumbValue, _thumbIsNear ? 1 : 0, Time.deltaTime * 
            handPoser.AnimationSpeed);

        float targetIndexValue = _triggerValue;

        if (targetIndexValue < 0.1f && _indexIsNear)
        {
            targetIndexValue = 0.1f;
        }

        poseBlender.IndexValue = Mathf.Lerp(poseBlender.IndexValue, targetIndexValue, Time.deltaTime * 
            handPoser.AnimationSpeed);

        poseBlender.GripValue = _gripValue;
    }

    public virtual bool SetupPoseBlender()
    {
        if (handPoser == null || !handPoser.isActiveAndEnabled)
        {
            handPoser = GetComponentInChildren<HandPoser>(false);
        }

        if (handPoser == null)
        {
            return false;
        }

        if (poseBlender == null || !poseBlender.isActiveAndEnabled)
        {
            poseBlender = handPoser.GetComponentInChildren<HandPoseBlender>();
        }

        if (poseBlender == null)
        {
            if (handPoser != null)
            {
                poseBlender = handPoser.gameObject.AddComponent<HandPoseBlender>();
            }
            else
            {
                poseBlender = this.gameObject.AddComponent<HandPoseBlender>();
            }

            poseBlender.UpdatePose = false;

            poseBlender.Pose1 = GetDefaultOpenPose();
            poseBlender.Pose2 = GetDefaultClosedPose();
        }

        return true;
    }

    public virtual HandPose GetDefaultOpenPose()
    {
        return Resources.Load<HandPose>("Open");
    }

    public virtual HandPose GetDefaultClosedPose()
    {
        return Resources.Load<HandPose>("Closed");
    }

    public virtual void EnableHandPoser()
    {
        if (handPoser != null)
        {
            DisableHandAnimator();
        }
    }

    public virtual void EnableAutoPoser(bool continuous)
    {
        if (autoPoser == null || !autoPoser.gameObject.activeInHierarchy)
        {

            if (handPoser != null)
            {
                autoPoser = handPoser.GetComponent<AutoPoser>();
            }
            else
            {
                autoPoser = GetComponentInChildren<AutoPoser>(false);
            }
        }

        if (autoPoser != null)
        {
            autoPoser.UpdateContinuously = continuous;

            if (!continuous)
            {
                autoPoser.UpdateAutoPoseOnce();
            }

            DisableHandAnimator();
            DisablePoseBlender();
        }
    }

    public virtual void DisablePoseBlender()
    {
        if (poseBlender != null)
        {
            poseBlender.UpdatePose = false;
        }
    }

    public virtual void DisableAutoPoser()
    {
        if (autoPoser != null)
        {
            autoPoser.UpdateContinuously = false;
        }
    }

    public virtual bool IsAutoPoserGrabbable()
    {
        return autoPoser != null && grabber != null && grabber.HeldGrabbable != null && 
            (grabber.HeldGrabbable.handPoseType == HandPoseType.AutoPoseOnce || grabber.HeldGrabbable.handPoseType == 
            HandPoseType.AutoPoseContinuous);
    }

    public virtual void EnableHandAnimator()
    {
        if (HandAnimator != null && HandAnimator.enabled == false)
        {
            HandAnimator.enabled = true;
        }

        if (handPoser != null)
        {
            handPoser.CurrentPose = null;
        }
    }

    public virtual void DisableHandAnimator()
    {
        if (HandAnimator != null && HandAnimator.enabled)
        {
            HandAnimator.enabled = false;
        }
    }

    public virtual void OnGrabberGrabbed(Grabbable grabbed)
    {
        if (grabbed.SelectedHandPose != null)
        {
            UpdateCurrentHandPose();
        }
        else if (grabbed.handPoseType == HandPoseType.HandPose && grabbed.SelectedHandPose == null)
        {
            grabbed.SelectedHandPose = GetDefaultClosedPose();
            UpdateCurrentHandPose();
        }
    }

    public virtual void UpdateCurrentHandPose()
    {
        if (handPoser != null)
        {
            if (HandPoseOverride != null)
            {
                handPoser.CurrentPose = HandPoseOverride;
            }
            else if (grabber != null && grabber.HeldGrabbable != null)
            {
                handPoser.CurrentPose = grabber.HeldGrabbable.SelectedHandPose;
            }

            handPoser.OnPoseChanged();
        }
    }

    public virtual void OnGrabberReleased(Grabbable released)
    {
        OnGrabDrop();
    }
}

public enum HandPoserType
{
    HandPoser,
    Animator,
    AutoPoser,
    None
}