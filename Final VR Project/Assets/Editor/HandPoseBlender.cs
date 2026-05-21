using UnityEngine;

public class HandPoseBlender : MonoBehaviour
{
    public HandPose Pose1;
    public HandPose Pose2;

    public bool UpdatePose = true;

    [Header("Inputs")]
    [Range(0, 1)] public float ThumbValue = 0f;
    [Range(0, 1)] public float IndexValue = 0f;
    [Range(0, 1)] public float MiddleValue = 0f;
    [Range(0, 1)] public float RingValue = 0f;
    [Range(0, 1)] public float PinkyValue = 0f;
    [Range(0, 1)] public float GripValue = 0f;

    private float _lastGripValue;

    protected HandPoser handPoser;

    void Start()
    {
        handPoser = GetComponent<HandPoser>();
    }

    void Update()
    {
        if (UpdatePose)
        {
            UpdatePoseFromInputs();
        }
    }

    public virtual void UpdatePoseFromInputs()
    {
        DoIdleBlendPose();
    }

    public void UpdateThumb(float amount)
    {
        handPoser.UpdateJoints(Pose2.Joints.ThumbJoints, handPoser.ThumbJoints, amount);
    }

    public void UpdateIndex(float amount)
    {
        handPoser.UpdateJoints(Pose2.Joints.IndexJoints, handPoser.IndexJoints, amount);
    }

    public void UpdateMiddle(float amount)
    {
        handPoser.UpdateJoints(Pose2.Joints.MiddleJoints, handPoser.MiddleJoints, MiddleValue);
    }

    public void UpdateRing(float amount)
    {
        handPoser.UpdateJoints(Pose2.Joints.RingJoints, handPoser.RingJoints, amount);
    }

    public void UpdatePinky(float amount)
    {
        handPoser.UpdateJoints(Pose2.Joints.PinkyJoints, handPoser.PinkyJoints, amount);
    }

    public void UpdateGrip(float amount)
    {
        MiddleValue = amount;
        RingValue = amount;
        PinkyValue = amount;

        UpdateMiddle(amount);
        UpdateRing(amount);
        UpdatePinky(amount);

        _lastGripValue = amount;
    }

    public virtual void DoIdleBlendPose()
    {
        if (Pose1)
        {
            handPoser.UpdateHandPose(Pose1, false);

            UpdateThumb(ThumbValue);
            UpdateIndex(IndexValue);

            if (GripValue != _lastGripValue)
            {
                UpdateGrip(GripValue);
            }
            else
            {
                UpdateMiddle(MiddleValue);
                UpdateRing(RingValue);
                UpdatePinky(PinkyValue);
            }
        }
    }
}