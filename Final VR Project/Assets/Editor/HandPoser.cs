using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HandPoser : MonoBehaviour
{
    public string ResourcePath = "Assets/Resources";
    public string PoseName = "Default";

    public HandPose CurrentPose;

    [Header("Animation Properties")]
    public float AnimationSpeed = 15f;

    public bool UpdateJointRotations = true;
    public bool UpdateJointPositions = false;
    public bool UpdateWristPosition = true;

    public HandPoseDefinition HandPoseJoints
    {
        get
        {
            return GetHandPoseDefinition();
        }
    }

    public Transform WristJoint;
    public List<Transform> ThumbJoints;
    public List<Transform> IndexJoints;
    public List<Transform> MiddleJoints;
    public List<Transform> RingJoints;
    public List<Transform> PinkyJoints;
    public List<Transform> OtherJoints;

    private HandPose previousPose;
    private bool doSingleAnimation;

    private float editorAnimationTime = 0f;
    private float maxEditorAnimationTime = 2f;

    public bool ShowGizmos = true;
    public bool ContinuousUpdate = false;

    private void Start()
    {
        OnPoseChanged();
    }

    private void Update()
    {
        CheckForPoseChange();

        if (ContinuousUpdate || doSingleAnimation)
        {
            DoPoseUpdate();
        }
    }

    public void CheckForPoseChange()
    {
        if (previousPose == null || (CurrentPose != null && previousPose != null && previousPose.name != CurrentPose.name && CurrentPose != null))
        {
            OnPoseChanged();
            previousPose = CurrentPose;
        }
    }

    public void OnPoseChanged()
    {
        editorAnimationTime = 0;
        doSingleAnimation = true;
    }

    public FingerJoint GetWristJoint()
    {
        return GetJointFromTransform(WristJoint);
    }

    public List<FingerJoint> GetThumbJoints()
    {
        return GetJointsFromTransforms(ThumbJoints);
    }

    public List<FingerJoint> GetIndexJoints()
    {
        return GetJointsFromTransforms(IndexJoints);
    }

    public List<FingerJoint> GetMiddleJoints()
    {
        return GetJointsFromTransforms(MiddleJoints);
    }

    public List<FingerJoint> GetRingJoints()
    {
        return GetJointsFromTransforms(RingJoints);
    }

    public List<FingerJoint> GetPinkyJoints()
    {
        return GetJointsFromTransforms(PinkyJoints);
    }

    public List<FingerJoint> GetOtherJoints()
    {
        return GetJointsFromTransforms(OtherJoints);
    }

    public int GetTotalJointsCount()
    {

        if (ThumbJoints == null || IndexJoints == null)
        {
            return 0;
        }

        return ThumbJoints.Count + IndexJoints.Count + MiddleJoints.Count + RingJoints.Count + PinkyJoints.Count + OtherJoints.Count + (WristJoint != null ? 1 : 0);
    }

    public Transform GetTip(List<Transform> transforms)
    {
        if (transforms != null)
        {
            return transforms[transforms.Count - 1];
        }

        return null;
    }

    public Transform GetThumbTip() { return GetTip(ThumbJoints); }
    public Transform GetIndexTip() { return GetTip(IndexJoints); }
    public Transform GetMiddleTip() { return GetTip(MiddleJoints); }
    public Transform GetRingTip() { return GetTip(RingJoints); }
    public Transform GetPinkyTip() { return GetTip(PinkyJoints); }

    public static Vector3 GetFingerTipPositionWithOffset(List<Transform> jointTransforms, float tipRadius)
    {

        if (jointTransforms == null || jointTransforms.Count == 0)
        {
            return Vector3.zero;
        }

        if (jointTransforms[jointTransforms.Count - 1] == null)
        {
            return Vector3.zero;
        }

        Vector3 tipPosition = jointTransforms[jointTransforms.Count - 1].position;

        if (jointTransforms.Count == 1)
        {
            return tipPosition;
        }

        return tipPosition + (jointTransforms[jointTransforms.Count - 2].position - tipPosition).normalized * tipRadius;
    }

    public virtual List<FingerJoint> GetJointsFromTransforms(List<Transform> jointTransforms)
    {
        List<FingerJoint> joints = new List<FingerJoint>();

        if (jointTransforms == null)
        {
            return joints;
        }

        for (int x = 0; x < jointTransforms.Count; x++)
        {
            if (jointTransforms[x] != null)
            {
                joints.Add(GetJointFromTransform(jointTransforms[x]));
            }
        }

        return joints;
    }

    public virtual FingerJoint GetJointFromTransform(Transform jointTransform)
    {
        if (jointTransform == null)
        {
            return null;
        }

        return new FingerJoint()
        {
            TransformName = jointTransform.name,
            LocalPosition = jointTransform.localPosition,
            LocalRotation = jointTransform.localRotation
        };
    }

    public virtual void UpdateHandPose(HandPose handPose, bool lerp)
    {
        UpdateHandPose(handPose.Joints, lerp);
    }

    public virtual void UpdateHandPose(HandPoseDefinition pose, bool lerp)
    {
        UpdateJoint(pose.WristJoint, WristJoint, lerp, UpdateWristPosition, UpdateJointRotations);
        UpdateJoints(pose.ThumbJoints, ThumbJoints, lerp);
        UpdateJoints(pose.IndexJoints, IndexJoints, lerp);
        UpdateJoints(pose.MiddleJoints, MiddleJoints, lerp);
        UpdateJoints(pose.RingJoints, RingJoints, lerp);
        UpdateJoints(pose.PinkyJoints, PinkyJoints, lerp);
        UpdateJoints(pose.OtherJoints, OtherJoints, lerp);
    }

    public virtual void UpdateJoint(FingerJoint fromJoint, Transform toTransform, bool doLerp, bool updatePosition, bool updateRotation)
    {
        UpdateJoint(fromJoint, toTransform, doLerp ? Time.deltaTime * AnimationSpeed : 1, updatePosition, updateRotation);
    }

    public virtual void UpdateJoint(FingerJoint fromJoint, Transform toTransform, float lerpAmount, bool updatePosition, bool updateRotation)
    {
        if (fromJoint == null || toTransform == null)
        {
            return;
        }

        if (lerpAmount != 1)
        {
            if (updatePosition)
            {
                toTransform.localPosition = Vector3.Lerp(toTransform.localPosition, fromJoint.LocalPosition, lerpAmount);
            }
            if (UpdateJointRotations)
            {
                toTransform.localRotation = Quaternion.Lerp(toTransform.localRotation, fromJoint.LocalRotation, lerpAmount);
            }
        }
        else
        {
            if (UpdateJointPositions)
            {
                toTransform.localPosition = fromJoint.LocalPosition;
            }
            if (UpdateJointRotations)
            {
                toTransform.localRotation = fromJoint.LocalRotation;
            }
        }
    }

    public virtual void UpdateJoints(List<FingerJoint> joints, List<Transform> toTransforms, bool doLerp)
    {
        UpdateJoints(joints, toTransforms, doLerp ? Time.deltaTime * AnimationSpeed : 1);
    }

    public virtual void UpdateJoints(List<FingerJoint> joints, List<Transform> toTransforms, float lerpAmount)
    {
        if (joints == null || toTransforms == null)
        {
            return;
        }

        int jointCount = joints.Count;
        int transformsCount = toTransforms.Count;

        bool verifyTransformName = jointCount != transformsCount;
        for (int i = 0; i < jointCount; i++)
        {
            if (i < toTransforms.Count)
            {

                if (joints[i] == null || toTransforms[i] == null)
                {
                    continue;
                }

                if (verifyTransformName && joints[i].TransformName == toTransforms[i].name)
                {
                    UpdateJoint(joints[i], toTransforms[i], lerpAmount, UpdateJointPositions, UpdateJointRotations);
                }
                else if (verifyTransformName == false)
                {
                    UpdateJoint(joints[i], toTransforms[i], lerpAmount, UpdateJointPositions, UpdateJointRotations);
                }
            }
        }
    }

    public virtual HandPoseDefinition GetHandPoseDefinition()
    {
        return new HandPoseDefinition()
        {
            WristJoint = GetWristJoint(),
            ThumbJoints = GetThumbJoints(),
            IndexJoints = GetIndexJoints(),
            MiddleJoints = GetMiddleJoints(),
            RingJoints = GetRingJoints(),
            PinkyJoints = GetPinkyJoints(),
            OtherJoints = GetOtherJoints()
        };
    }

    public virtual void CreateUniquePose(string poseName)
    {
        if (string.IsNullOrEmpty(poseName))
        {
            poseName = "Pose";
        }

        string formattedPoseName = poseName;
        string fullPath = ResourcePath + formattedPoseName + ".asset";
        bool exists = System.IO.File.Exists(fullPath);
        int checkCount = 0;

        while (exists)
        {
            formattedPoseName = poseName + " " + checkCount;
            exists = System.IO.File.Exists(ResourcePath + formattedPoseName + ".asset");

            checkCount++;
        }
    }

    public virtual void DoPoseUpdate()
    {
        if (CurrentPose != null)
        {
            UpdateHandPose(CurrentPose.Joints, true);
        }

        if (doSingleAnimation)
        {
            editorAnimationTime += Time.deltaTime;

            if (editorAnimationTime >= maxEditorAnimationTime)
            {
                editorAnimationTime = 0;
                doSingleAnimation = false;
            }
        }
    }
}