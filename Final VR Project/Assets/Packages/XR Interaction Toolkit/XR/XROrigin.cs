// ENABLE_VR is not defined on Game Core but the assembly is available with limited features when the XR module is enabled.
#if ENABLE_VR || UNITY_GAMECORE
#define XR_MODULE_AVAILABLE
#endif

using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.XR.CoreUtils 
{
    /// <summary>
    /// The XR Origin represents the session-space origin (0, 0, 0) in an XR scene.
    /// </summary>
    /// <remarks>
    /// The XR Origin component is typically attached to the base object of the XR Origin,
    /// and stores the <see cref="GameObject"/> that will be manipulated via locomotion.
    /// It is also used for offsetting the camera.
    /// </remarks>
    [AddComponentMenu("XR/XR Origin")]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.core-utils@2.0/api/Unity.XR.CoreUtils.XROrigin.html")]
    public class XROrigin : MonoBehaviour
    {
        /// <summary>
        /// the <see cref="GameObject"/> containing this <c>XROrigin</c> component.
        /// </summary>
        /// <remarks>
        /// You can add a <see cref="UnityEngine.InputSystem.XR.TrackedPoseDriver"/> component to the <see cref="Camera"/> 
        /// GameObject to update its position and rotation using tracking data from the XR device.
        /// You must update the <see cref="Camera"/> position and rotation using tracking data from the XR device.
        /// </remarks>

        /// <summary>
        /// The parent <c>Transform</c> for all "trackables" (for example, planes and feature points).
        /// </summary>
        /// <remarks>
        /// See [Trackables](xref:arfoundation-managers) for more information.
        /// </remarks>
        public Transform TrackablesParent { get; private set; }

        /// <summary>
        /// Invoked during
        /// [Application.onBeforeRender](xref:UnityEngine.Application.onBeforeRender(UnityEngine.Events.UnityAction))
        /// whenever the <see cref="TrackablesParent"/> [transform](xref:UnityEngine.Transform) changes.
        /// Sets which Tracking Origin Mode to use when initializing the input device.
        /// </summary>
        /// <seealso cref="RequestedTrackingOriginMode"/>
        /// <seealso cref="TrackingOriginModeFlags"/>
        /// <seealso cref="XRInputSubsystem.TrySetTrackingOriginMode"/>
        public enum TrackingOriginMode
        {
            /// <summary>
            /// Uses the default Tracking Origin Mode of the input device.
            /// </summary>
            /// <remarks>
            /// When changing to this value after startup, the Tracking Origin Mode will not be changed.
            /// </remarks>
            NotSpecified,

            /// <summary>
            /// Sets the Tracking Origin Mode to <see cref="TrackingOriginModeFlags.Device"/>.
            /// Input devices will be tracked relative to the first known location.
            /// </summary>
            /// <remarks>
            /// Represents a device-relative tracking origin. A device-relative tracking origin defines a local origin
            /// at the position of the device in space at some previous point in time, usually at a recenter event,
            /// power-on, or AR/VR session start. Pose data provided by the device will be in this space relative to
            /// the local origin. This means that poses returned in this mode will not include the user height (for VR)
            /// or the device height (for AR) and any camera tracking from the XR device will need to be manually offset accordingly.
            /// </remarks>
            /// <seealso cref="TrackingOriginModeFlags.Device"/>
            Device,

            /// <summary>
            /// Sets the Tracking Origin Mode to <see cref="TrackingOriginModeFlags.Floor"/>.
            /// Input devices will be tracked relative to a location on the floor.
            /// </summary>
            /// <remarks>
            /// Represents the tracking origin whereby (0, 0, 0) is on the "floor" or other surface determined by the
            /// XR device being used. The pose values reported by an XR device in this mode will include the height
            /// of the XR device above this surface, removing the need to offset the position of the camera tracking
            /// the XR device by the height of the user (VR) or the height of the device above the floor (AR).
            /// </remarks>
            /// <seealso cref="TrackingOriginModeFlags.Floor"/>
            Floor,
        }

        [Header("Enable / Disable : ")]
        [Tooltip("Use Emulator if true and HMDIsActive is false")]
        public bool EmulatorEnabled = true;

        [Tooltip("Set to false if you want to use in standalone builds as well as the editor")]
        public bool EditorOnly = true;

        [Tooltip("If true the game window must have focus for the emulator to be active")]
        public bool RequireGameFocus = true;

        [Header("Input : ")]
        [SerializeField]
        [Tooltip("Action set used specifically to mimic or supplement a vr setup")]
        public InputActionAsset EmulatorActionSet;

        [Header("Player Teleportation")]
        [Tooltip("Will set the PlayerTeleport component's ForceStraightArrow = true while the emulator is active.")]
        public bool ForceStraightTeleportRotation = true;

        [Header("Move Player Up / Down")]
        [Tooltip("If true, move the player eye offset up / down whenever PlayerUpAction / PlayerDownAction is called.")]
        public bool AllowUpDownControls = true;

        [Tooltip("Unity Input Action used to move the player up")]
        public InputActionReference PlayerUpAction;

        [Tooltip("Unity Input Action used to move the player down")]
        public InputActionReference PlayerDownAction;

        [Tooltip("Minimum height in meters the player can shrink to when using the PlayerDownAction")]
        public float MinPlayerHeight = 0.2f;

        [Tooltip("Maximum height in meters the player can grow to when using the PlayerUpAction")]
        public float MaxPlayerHeight = 5f;

        [Header("Head Look")]
        [Tooltip("Unity Input Action used to lock the camera in game mode to look around")]
        public InputActionReference LockCameraAction;

        [Tooltip("Unity Input Action used to lock the camera in game mode to look around")]
        public InputActionReference CameraLookAction;

        [Tooltip("Multiply the CameraLookAction by this amount")]
        public float CameraLookSensitivityX = 0.1f;

        [Tooltip("Multiply the CameraLookAction by this amount")]
        public float CameraLookSensitivityY = 0.1f;

        [Tooltip("Minimum local Eulers degrees the camera can rotate")]
        public float MinimumCameraY = -90f;

        [Tooltip("Minimum local Eulers degrees the camera can rotate")]
        public float MaximumCameraY = 90f;

        [Header("Controller Emulation")]
        [Tooltip("Unity Input Action used to mimic holding the Left Grip")]
        public InputActionReference LeftGripAction;

        [Tooltip("Unity Input Action used to mimic holding the Left Trigger")]
        public InputActionReference LeftTriggerAction;

        [Tooltip("Unity Input Action used to mimic having your thumb near a button")]
        public InputActionReference LeftThumbNearAction;

        [Tooltip("Unity Input Action used to move mimic holding the Right Grip")]
        public InputActionReference RightGripAction;

        [Tooltip("Unity Input Action used to move mimic holding the Right Grip")]
        public InputActionReference RightTriggerAction;

        [Tooltip("Unity Input Action used to mimic having your thumb near a button")]
        public InputActionReference RightThumbNearAction;

        float mouseRotationX;
        float mouseRotationY;

        Transform mainCameraTransform;
        Transform leftControllerTranform;
        Transform rightControllerTranform;

        Transform leftHandAnchor;
        Transform rightHandAnchor;

        PlayerController player;
        SmoothLocomotion smoothLocomotion;
        bool didFirstActivate = false;

        Grabber grabberLeft;
        Grabber grabberRight;

        private float originalPlayerYOffset = 1.65f;

        [Header("Shown for Debug : ")]
        public bool HMDIsActive;

        public Vector3 LeftControllerPosition = new Vector3(-0.2f, -0.2f, 0.5f);
        public Vector3 RightControllerPosition = new Vector3(0.2f, -0.2f, 0.5f);

        bool priorStraightSetting;

        void Start()
        {

            if (GameObject.Find("CameraRig"))
            {
                mainCameraTransform = GameObject.Find("CameraRig").transform;
            }

            else if (GameObject.Find("OVRCameraRig"))
            {
                mainCameraTransform = GameObject.Find("OVRCameraRig").transform;
            }

            leftHandAnchor = GameObject.Find("LeftHandAnchor").transform;
            rightHandAnchor = GameObject.Find("RightHandAnchor").transform;

            leftControllerTranform = GameObject.Find("LeftControllerAnchor").transform;
            rightControllerTranform = GameObject.Find("RightControllerAnchor").transform;

            player = FindObjectOfType<PlayerController>();

            if (player)
            {
                player.ElevateCameraIfNoHMDPresent = true;
                originalPlayerYOffset = player.ElevateCameraHeight;

                smoothLocomotion = player.GetComponentInChildren<SmoothLocomotion>(true);

                if (smoothLocomotion != null && !smoothLocomotion.isActiveAndEnabled)
                {
                    smoothLocomotion.CheckControllerReferences();
                }


                if (smoothLocomotion == null)
                {
                    //LEAVE IF NULL
                }
                else if (smoothLocomotion.MoveAction == null)
                {
                    //LEAVE IF NULL
                }
            }
        }

        public void OnBeforeRender()
        {
            HMDIsActive = XRInput.Instance.HMDActive;

            if (EmulatorEnabled && !HMDIsActive)
            {
                UpdateControllerPositions();
            }
        }

        void onFirstActivate()
        {
            UpdateControllerPositions();

            didFirstActivate = true;
        }

        void Update()
        {
            HMDIsActive = XRInput.Instance.HMDActive;

            if (EmulatorEnabled && !HMDIsActive)
            {

                if (!didFirstActivate)
                {
                    onFirstActivate();
                }

                if (HasRequiredFocus())
                {
                    CheckHeadControls();

                    UpdateControllerPositions();

                    CheckPlayerControls();
                }
            }

            if (EmulatorEnabled && didFirstActivate && HMDIsActive)
            {
                ResetAll();
            }
        }

        public virtual bool HasRequiredFocus()
        {

            if (EditorOnly == false || RequireGameFocus == false)
            {
                return true;
            }

            return Application.isFocused;
        }

        public void CheckHeadControls()
        {
            if (LockCameraAction != null)
            {
                if (LockCameraAction.action.ReadValue<float>() == 1)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;

                    Vector3 mouseLook = Vector2.zero;
                    if (CameraLookAction != null)
                    {
                        mouseLook = CameraLookAction.action.ReadValue<Vector2>();
                    }
                    else
                    {
                        mouseLook = Mouse.current.delta.ReadValue();
                    }
                    mouseRotationY += mouseLook.y * CameraLookSensitivityY;

                    mouseRotationY = Mathf.Clamp(mouseRotationY, MinimumCameraY, MaximumCameraY);
                    mainCameraTransform.localEulerAngles = new Vector3(-mouseRotationY, mainCameraTransform.localEulerAngles.y, 0);

                    player.transform.Rotate(0, mouseLook.x * CameraLookSensitivityX, 0);
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        float prevVal;

        public void UpdateInputs()
        {

            if (EmulatorEnabled == false || HMDIsActive)
            {
                return;
            }

            if (!HasRequiredFocus())
            {
                return;
            }

            checkGrabbers();

            if (LeftTriggerAction != null)
            {
                prevVal = XRInput.Instance.LeftTrigger;
                XRInput.Instance.LeftTrigger = LeftTriggerAction.action.ReadValue<float>();
                XRInput.Instance.LeftTriggerDown = prevVal < XRInput.Instance.DownThreshold && XRInput.Instance.LeftTrigger >= XRInput.Instance.DownThreshold;
                XRInput.Instance.LeftTriggerUp = prevVal > XRInput.Instance.DownThreshold && XRInput.Instance.LeftTrigger < XRInput.Instance.DownThreshold;
            }

            if (LeftGripAction != null)
            {
                prevVal = XRInput.Instance.LeftGrip;
                XRInput.Instance.LeftGrip = LeftGripAction.action.ReadValue<float>();
                XRInput.Instance.LeftGripDown = prevVal < XRInput.Instance.DownThreshold && XRInput.Instance.LeftGrip >= XRInput.Instance.DownThreshold;
            }

            if (LeftThumbNearAction != null)
            {
                XRInput.Instance.LeftThumbNear = LeftThumbNearAction.action.ReadValue<float>() == 1;
            }

            if (RightTriggerAction != null)
            {
                float rightTriggerVal = RightTriggerAction.action.ReadValue<float>();

                prevVal = XRInput.Instance.RightTrigger;
                XRInput.Instance.RightTrigger = RightTriggerAction.action.ReadValue<float>();
                XRInput.Instance.RightTriggerDown = prevVal < XRInput.Instance.DownThreshold && XRInput.Instance.RightTrigger >= XRInput.Instance.DownThreshold;
                XRInput.Instance.RightTriggerUp = prevVal > XRInput.Instance.DownThreshold && XRInput.Instance.RightTrigger < XRInput.Instance.DownThreshold;
            }

            if (RightGripAction != null)
            {
                prevVal = XRInput.Instance.RightGrip;
                XRInput.Instance.RightGrip = RightGripAction.action.ReadValue<float>();
                XRInput.Instance.RightGripDown = prevVal < XRInput.Instance.DownThreshold && XRInput.Instance.RightGrip >= XRInput.Instance.DownThreshold;
            }

            if (RightThumbNearAction)
            {
                XRInput.Instance.RightThumbNear = RightThumbNearAction.action.ReadValue<float>() == 1;
            }
        }

        public void CheckPlayerControls()
        {

            if (EditorOnly && !Application.isEditor)
            {
                return;
            }

            if (AllowUpDownControls)
            {
                if (PlayerUpAction != null && PlayerUpAction.action.ReadValue<float>() == 1)
                {
                    player.ElevateCameraHeight = Mathf.Clamp(player.ElevateCameraHeight + Time.deltaTime, MinPlayerHeight, MaxPlayerHeight);
                }
                else if (PlayerDownAction != null && PlayerDownAction.action.ReadValue<float>() == 1)
                {
                    player.ElevateCameraHeight = Mathf.Clamp(player.ElevateCameraHeight - Time.deltaTime, MinPlayerHeight, MaxPlayerHeight);
                }
            }

            if (smoothLocomotion != null && smoothLocomotion.enabled == false)
            {
                smoothLocomotion.CheckControllerReferences();
                smoothLocomotion.UpdateInputs();

                if (smoothLocomotion.ControllerType == PlayerControllerType.CharacterController)
                {
                    smoothLocomotion.MoveCharacter();
                }
                else if (smoothLocomotion.ControllerType == PlayerControllerType.Rigidbody)
                {
                    smoothLocomotion.MoveRigidCharacter();
                }
            }
        }

        public virtual void UpdateControllerPositions()
        {
            leftControllerTranform.transform.localPosition = LeftControllerPosition;
            leftControllerTranform.transform.localEulerAngles = Vector3.zero;

            rightControllerTranform.transform.localPosition = RightControllerPosition;
            rightControllerTranform.transform.localEulerAngles = Vector3.zero;
        }

        void checkGrabbers()
        {
            if (grabberLeft == null || !grabberLeft.isActiveAndEnabled)
            {
                Grabber[] grabbers = FindObjectsOfType<Grabber>();

                for (var x = 0; x < grabbers.Length; x++)
                {
                    if (grabbers[x] != null && grabbers[x].isActiveAndEnabled && grabbers[x].HandSide == ControllerHand.Left)
                    {
                        grabberLeft = grabbers[x];
                    }
                }
            }

            if (grabberRight == null || !grabberRight.isActiveAndEnabled)
            {
                Grabber[] grabbers = FindObjectsOfType<Grabber>();
                for (var x = 0; x < grabbers.Length; x++)
                {
                    if (grabbers[x] != null && grabbers[x].isActiveAndEnabled && grabbers[x].HandSide == ControllerHand.Right)
                    {
                        grabberRight = grabbers[x];
                    }
                }
            }
        }

        public virtual void ResetHands()
        {
            leftControllerTranform.transform.localPosition = Vector3.zero;
            leftControllerTranform.transform.localEulerAngles = Vector3.zero;

            rightControllerTranform.transform.localPosition = Vector3.zero;
            rightControllerTranform.transform.localEulerAngles = Vector3.zero;
        }

        public virtual void ResetAll()
        {
            ResetHands();

            mainCameraTransform.localEulerAngles = Vector3.zero;

            if (player)
            {
                player.ElevateCameraHeight = originalPlayerYOffset;
            }

            didFirstActivate = false;
        }

        void OnEnable()
        {
            if (EmulatorActionSet != null)
            {
                foreach (var map in EmulatorActionSet.actionMaps)
                {
                    foreach (var action in map)
                    {
                        if (action != null)
                        {
                            action.Enable();
                        }
                    }
                }
            }

            XRInput.OnInputsUpdated += UpdateInputs;
            Application.onBeforeRender += OnBeforeRender;
        }

        void OnDisable()
        {

            if (EmulatorActionSet != null)
            {
                foreach (var map in EmulatorActionSet.actionMaps)
                {
                    foreach (var action in map)
                    {
                        if (action != null)
                        {
                            action.Disable();
                        }
                    }
                }
            }

            Application.onBeforeRender -= OnBeforeRender;

            if (isQuitting)
            {
                return;
            }

            ResetAll();

            XRInput.OnInputsUpdated -= UpdateInputs;
        }

        bool isQuitting = false;

        void OnApplicationQuit()
        {
            isQuitting = true;
        }
    }
}
