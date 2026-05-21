using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HandPoseDefinition
{
    [Header("Wrist")]
    public FingerJoint WristJoint;

    [Header("Thumb")]
    public List<FingerJoint> ThumbJoints;

    [Header("Index")]
    public List<FingerJoint> IndexJoints;

    [Header("Middle")]
    public List<FingerJoint> MiddleJoints;

    [Header("Ring")]
    public List<FingerJoint> RingJoints;

    [Header("Pinky")]
    public List<FingerJoint> PinkyJoints;

    [Header("Other")]
    public List<FingerJoint> OtherJoints;
}
