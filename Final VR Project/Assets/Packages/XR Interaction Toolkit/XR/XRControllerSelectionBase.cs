using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interprets feature values on a tracked input controller device using actions from the Input System
    /// into XR Interaction states, such as Select. Additionally, it applies the current Pose value
    /// of a tracked device to the transform of the GameObject.
    /// </summary>
    /// <remarks>
    /// This behavior requires that the Input System is enabled in the <b>Active Input Handling</b>
    /// setting in <b>Edit &gt; Project Settings &gt; Player</b> for input values to be read.
    /// Each input action must also be enabled to read the current value of the action. Referenced
    /// input actions in an Input Action Asset are not enabled by default.
    /// </remarks>
    /// <seealso cref="XRBaseController"/>
    [AddComponentMenu("XR/XR Controller (Selection-based)", 11)]
    public class XRControllerSelectionBase : MonoBehaviour
    {
        [SerializeField] private ControllerHand ControllerHand = ControllerHand.Right;
        [SerializeField] private ControllerOffset thisOffset;

        public Vector3 OffsetPosition;
        public Vector3 OffsetRotation;

        [SerializeField] private string thisControllerModel;
        [SerializeField] private List<ControllerOffset> ControllerOffsets;

        private void Start()
        {
            if (ControllerOffsets == null)
            {
                ControllerOffsets = new List<ControllerOffset>();
            }

            StartCoroutine(checkForController());
        }

        IEnumerator checkForController()
        {

            while (string.IsNullOrEmpty(thisControllerModel))
            {

                thisControllerModel = XRInput.Instance.GetControllerName();

                yield return new WaitForEndOfFrame();
            }

            OnControllerFound();
        }

        public virtual void OnControllerFound()
        {
            DefineControllerOffsets();

            thisOffset = GetControllerOffset(thisControllerModel);

            if (thisOffset != null)
            {
                if (ControllerHand == ControllerHand.Left)
                {
                    OffsetPosition = thisOffset.LeftControllerPositionOffset;
                    OffsetRotation = thisOffset.LeftControllerRotationOffset;

                    transform.localPosition += OffsetPosition;
                    transform.localEulerAngles += OffsetRotation;
                }
                else if (ControllerHand == ControllerHand.Right)
                {
                    OffsetPosition = thisOffset.RightControllerPositionOffset;
                    OffsetRotation = thisOffset.RightControlleRotationOffset;

                    transform.localPosition += OffsetPosition;
                    transform.localEulerAngles += OffsetRotation;
                }
            }
        }

        public virtual ControllerOffset GetControllerOffset(string controllerName)
        {
            var offset = ControllerOffsets.FirstOrDefault(x => thisControllerModel.StartsWith(x.ControllerName));

            if (offset == null && controllerName.EndsWith("OpenXR"))
            {
                return GetOpenXROffset();
            }

            return offset;
        }

        public virtual void DefineControllerOffsets()
        {
            ControllerOffsets = new List<ControllerOffset>();

            ControllerOffsets.Add(new ControllerOffset()
            {
                ControllerName = "Oculus Touch Controller OpenXR",
                LeftControllerPositionOffset = new Vector3(0.002f, -0.02f, 0.04f),
                RightControllerPositionOffset = new Vector3(-0.002f, -0.02f, 0.04f),
                LeftControllerRotationOffset = new Vector3(60.0f, 0.0f, 0.0f),
                RightControlleRotationOffset = new Vector3(60.0f, 0.0f, 0.0f)
            });

            ControllerOffsets.Add(new ControllerOffset()
            {
                ControllerName = "Index Controller OpenXR",
                LeftControllerPositionOffset = new Vector3(0.002f, -0.02f, 0.04f),
                RightControllerPositionOffset = new Vector3(-0.002f, -0.02f, 0.04f),
                LeftControllerRotationOffset = new Vector3(60.0f, 0.0f, 0.0f),
                RightControlleRotationOffset = new Vector3(60.0f, 0.0f, 0.0f)
            });

            // Oculus Touch on Oculus SDK is at correct orientation by default
            // Example  : "Oculus Touch Controller - Right"
            ControllerOffsets.Add(new ControllerOffset()
            {
                ControllerName = "Oculus Touch Controller",
            });

            // Oculus Quest Example : 
            ControllerOffsets.Add(new ControllerOffset()
            {
                ControllerName = "OpenVR Controller(Oculus Quest",
                LeftControllerPositionOffset = new Vector3(0.0075f, -0.005f, -0.0525f),
                RightControllerPositionOffset = new Vector3(-0.0075f, -0.005f, -0.0525f),
                LeftControllerRotationOffset = new Vector3(40.0f, 0.0f, 0.0f),
                RightControlleRotationOffset = new Vector3(40.0f, 0.0f, 0.0f)
            });

            // Default all other OpenVR Controllers to about a 40 degree angle
            ControllerOffsets.Add(new ControllerOffset()
            {
                ControllerName = "OpenVR Controller",
                LeftControllerPositionOffset = new Vector3(0.0075f, -0.005f, -0.0525f),
                RightControllerPositionOffset = new Vector3(-0.0075f, -0.005f, -0.0525f),
                LeftControllerRotationOffset = new Vector3(40.0f, 0.0f, 0.0f),
                RightControlleRotationOffset = new Vector3(40.0f, 0.0f, 0.0f)
            });
        }

        /// <summary>
        /// Returns a generic offset for OpenXR controllers not defined in DefineControllerOffsets(). 
        /// All OpenXR controllers appear to have about a 60 degree rotation in Unity, for example.
        /// Override this method if you need to specify a different offset (or none at all)
        /// </summary>
        /// <returns></returns>
        public virtual ControllerOffset GetOpenXROffset()
        {
            return new ControllerOffset()
            {
                ControllerName = "Controller OpenXR",
                LeftControllerPositionOffset = new Vector3(0.002f, -0.02f, 0.04f),
                RightControllerPositionOffset = new Vector3(-0.002f, -0.02f, 0.04f),
                LeftControllerRotationOffset = new Vector3(60.0f, 0.0f, 0.0f),
                RightControlleRotationOffset = new Vector3(60.0f, 0.0f, 0.0f)
            };
        }
    }

    public class ControllerOffset
    {
        public string ControllerName { get; set; }
        public Vector3 LeftControllerPositionOffset { get; set; }
        public Vector3 RightControllerPositionOffset { get; set; }
        public Vector3 LeftControllerRotationOffset { get; set; }
        public Vector3 RightControlleRotationOffset { get; set; }
    }
}