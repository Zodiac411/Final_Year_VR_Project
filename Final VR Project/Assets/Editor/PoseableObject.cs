using UnityEngine;

public class PoseableObject : MonoBehaviour
{
    [Header("Pose Type")]
    public PoseType poseType = PoseType.HandPose;

    [Header("Hand Pose Properties")]
    public HandPose EquipHandPose;

    [Header("Auto Pose Properties")]
    public float AutoPoseDuration = 0.15f;

    [Header("Animator Properties")]
    public int HandPoseID;
    public enum PoseType
    {
        HandPose,
        AutoPoseOnce,
        AutoPoseContinuous,
        Animator,
        Other,
        None
    }
}
