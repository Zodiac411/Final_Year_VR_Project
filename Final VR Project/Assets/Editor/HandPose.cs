using UnityEngine;

[System.Serializable]
public class HandPose : ScriptableObject
{
    [Header("Pose Name")]
    public string PoseName;

    [Header("Joint Definitions")]
    public HandPoseDefinition Joints;
}
