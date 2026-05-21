using UnityEngine;

public class CharacterIK : MonoBehaviour
{
    [SerializeField] private Transform FollowLeftController;
    [SerializeField] private Transform FollowRightController;

    [SerializeField] private Transform FollowLeftFoot;
    [SerializeField] private Transform FollowRightFoot;
    [SerializeField] private Transform FollowHead;

    [SerializeField] private Transform HipsJoint;
    [SerializeField] private CharacterController FollowPlayer;

    [SerializeField] private float FootYPosition = 0;

    Transform headBone;
    Transform leftShoulderJoint;
    Transform rightShoulderJoint;
    Transform leftHandJoint;
    Transform rightHandJoint;

    Animator animator;

    private bool IKActive = true;
    private bool IKFeetActive = true;

    private bool HideHead = true;
    private bool HideLeftArm = false;
    private bool HideRightArm = false;
    private bool HideLeftHand = false;
    private bool HideRightHand = false;
    private bool HideLegs = false;

    public float HipOffset = 0;
    void Start()
    {
        animator = GetComponent<Animator>();

        headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        leftHandJoint = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        rightHandJoint = animator.GetBoneTransform(HumanBodyBones.RightHand);
        leftShoulderJoint = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
        rightShoulderJoint = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
    }

    public Vector3 hideBoneScale = new Vector3(0.0001f, 0.0001f, 0.0001f);

    void Update()
    {
        if (headBone != null)
        {
            headBone.localScale = HideHead ? Vector3.zero : Vector3.one;
        }
        if (leftShoulderJoint != null)
        {
            leftShoulderJoint.localScale = HideLeftArm ? hideBoneScale : Vector3.one;
        }
        if (rightShoulderJoint != null)
        {
            rightShoulderJoint.localScale = HideRightArm ? hideBoneScale : Vector3.one;
        }
        if (leftHandJoint != null)
        {
            leftHandJoint.localScale = HideLeftHand ? Vector3.zero : Vector3.one;
        }
        if (rightHandJoint != null)
        {
            rightHandJoint.localScale = HideRightHand ? Vector3.zero : Vector3.one;
        }
        if (HipsJoint)
        {
            HipsJoint.localScale = HideLegs ? Vector3.zero : Vector3.one;
        }

        Transform hipJoint = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
    }

    void OnAnimatorIK()
    {
        if (animator)
        {
            if (IKActive)
            {
                if (FollowHead != null)
                {
                    animator.SetLookAtWeight(1);
                    animator.SetLookAtPosition(FollowHead.position);
                }
                if (FollowLeftController != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, FollowLeftController.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, FollowLeftController.rotation);
                }
                if (FollowRightController != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, FollowRightController.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, FollowRightController.rotation);
                }
                if (IKFeetActive)
                {
                    if (FollowLeftFoot != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                        animator.SetIKPosition(AvatarIKGoal.LeftFoot, new Vector3(FollowLeftFoot.position.x, FootYPosition, FollowLeftFoot.position.z));
                    }
                    if (FollowRightFoot != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                        animator.SetIKPosition(AvatarIKGoal.RightFoot, new Vector3(FollowRightFoot.position.x, FootYPosition, FollowRightFoot.position.z));
                    }
                }
                else
                {
                    if (FollowLeftFoot != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
                    }
                    if (FollowRightFoot != null)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
                    }
                }
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);

                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);

                animator.SetLookAtWeight(0);
            }
        }
    }
}