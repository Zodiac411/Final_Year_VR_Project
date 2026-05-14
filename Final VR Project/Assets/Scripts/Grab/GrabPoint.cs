using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class GrabPoint : MonoBehaviour
{
    public HandPoseType handPoseType = HandPoseType.HandPose;
    public HandPose SelectedHandPose;

    public HandPoseId HandPose;

    [Header("Valid Hands")]
    public bool LeftHandIsValid = true;
    public bool RightHandIsValid = true;

    [Tooltip("If specified, the Hand Model will be parented here when snapped")]
    public Transform HandPosition;

    [Header("Angle Restriction")]
    [Range(0.0f, 360.0f)] public float MaxDegreeDifferenceAllowed = 360;

    [Header("Finger Blending")]
    [Range(0.0f, 1.0f)] public float IndexBlendMin = 0;
    [Range(0.0f, 1.0f)] public float IndexBlendMax = 0;
    [Range(0.0f, 1.0f)] public float ThumbBlendMin = 0;
    [Range(0.0f, 1.0f)] public float ThumbBlendMax = 0;

    Vector3 previewModelOffsetLeft = new Vector3(0.007f, -0.0179f, 0.0071f);
    Vector3 previewModelOffsetRight = new Vector3(-0.029f, 0.0328f, 0.044f);

    [Header("Editor")]
    [Tooltip("Show a green arc in the Scene view representing MaxDegreeDifferenceAllowed")]
    public bool ShowAngleGizmo = true;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        DrawEditorArc();
        UpdatePreviews();
    }

    public void UpdatePreviews()
    {
        UpdateChildAnimators();
        UpdatePreviewTransforms();
        UpdateHandPosePreview();
        UpdateAutoPoserPreview();
    }

    public void DrawEditorArc()
    {

        if (ShowAngleGizmo && MaxDegreeDifferenceAllowed != 0 && MaxDegreeDifferenceAllowed != 360)
        {
            Vector3 from = Quaternion.AngleAxis(-0.5f * MaxDegreeDifferenceAllowed, transform.up) * (-transform.forward - Vector3.Dot(-transform.forward, transform.up) * transform.up);

            UnityEditor.Handles.color = new Color(0, 1, 0, 0.1f);
            UnityEditor.Handles.DrawSolidArc(transform.position, transform.up, from, MaxDegreeDifferenceAllowed, 0.05f);
        }
    }
#endif

    bool offsetFound = false;

    public void UpdatePreviewTransforms()
    {
        Transform leftHandPreview = transform.Find("LeftHandModelsEditorPreview");
        Transform rightHandPreview = transform.Find("RightHandModelsEditorPreview");

        if (!offsetFound)
        {
            if (GameObject.Find("LeftController/Grabber") != null)
            {
                Grabber LeftGrabber = GameObject.Find("LeftController/Grabber").GetComponent<Grabber>();
                previewModelOffsetLeft = Vector3.zero - LeftGrabber.transform.localPosition;
            }

            if (GameObject.Find("RightController/Grabber") != null)
            {
                Grabber RightGrabber = GameObject.Find("RightController/Grabber").GetComponent<Grabber>();
                previewModelOffsetRight = Vector3.zero - RightGrabber.transform.localPosition;
            }
        }

        if (leftHandPreview)
        {
            leftHandPreview.localPosition = previewModelOffsetLeft;
            leftHandPreview.localEulerAngles = Vector3.zero;
        }

        if (rightHandPreview)
        {
            rightHandPreview.localPosition = previewModelOffsetRight;
            rightHandPreview.localEulerAngles = Vector3.zero;
        }
    }

    public void UpdateHandPosePreview()
    {
        if (handPoseType == HandPoseType.HandPose)
        {
            Transform leftHandPreview = transform.Find("LeftHandModelsEditorPreview");
            Transform rightHandPreview = transform.Find("RightHandModelsEditorPreview");

            if (leftHandPreview)
            {
                HandPoser hp = leftHandPreview.GetComponentInChildren<HandPoser>();
                if (hp != null)
                {
                    hp.CurrentPose = SelectedHandPose;
                }
            }

            if (rightHandPreview)
            {
                HandPoser hp = rightHandPreview.GetComponentInChildren<HandPoser>();
                if (hp != null)
                {
                    hp.CurrentPose = SelectedHandPose;
                }
            }
        }
    }

    public void UpdateAutoPoserPreview()
    {
        if (handPoseType == HandPoseType.AutoPoseContinuous || handPoseType == HandPoseType.AutoPoseOnce)
        {
            Transform leftHandPreview = transform.Find("LeftHandModelsEditorPreview");
            Transform rightHandPreview = transform.Find("RightHandModelsEditorPreview");

            if (leftHandPreview)
            {
                AutoPoser ap = leftHandPreview.GetComponentInChildren<AutoPoser>();
                if (ap != null)
                {
                    ap.UpdateContinuously = true;
                }
            }

            if (rightHandPreview)
            {
                AutoPoser ap = rightHandPreview.GetComponentInChildren<AutoPoser>();
                if (ap != null)
                {
                    ap.UpdateContinuously = true;
                }
            }
        }
        else
        {
            Transform leftHandPreview = transform.Find("LeftHandModelsEditorPreview");
            Transform rightHandPreview = transform.Find("RightHandModelsEditorPreview");

            if (leftHandPreview)
            {
                AutoPoser ap = leftHandPreview.GetComponentInChildren<AutoPoser>();
                if (ap != null)
                {
                    ap.UpdateContinuously = false;
                }
            }

            if (rightHandPreview)
            {
                AutoPoser ap = rightHandPreview.GetComponentInChildren<AutoPoser>();
                if (ap != null)
                {
                    ap.UpdateContinuously = false;
                }
            }
        }
    }

    public void UpdateChildAnimators()
    {
        var animators = transform.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            if (handPoseType == HandPoseType.AnimatorID)
            {
                animators[i].enabled = true;
                if (animators[i].isActiveAndEnabled)
                {
                    animators[i].Update(Time.deltaTime);
                }
            }
            else if (handPoseType == HandPoseType.HandPose && SelectedHandPose != null)
            {
                animators[i].enabled = false;
            }
            else if (handPoseType == HandPoseType.AutoPoseOnce || handPoseType == HandPoseType.AutoPoseContinuous)
            {
                animators[i].enabled = false;
            }
        }
    }
}