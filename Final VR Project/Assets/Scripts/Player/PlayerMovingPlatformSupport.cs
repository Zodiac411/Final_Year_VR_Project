using UnityEngine;
using System;

public class PlayerMovingPlatformSupport : MonoBehaviour
{
    [SerializeField] private LayerMask GroundedLayers;
    [SerializeField] private MovingPlatform CurrentPlatform;

    [SerializeField] private float DistanceFromGround;

    protected RaycastHit groundHit;

    private SmoothLocomotion smoothLocomotion;
    private CharacterController characterController;

    private Transform _initialCharacterParent;


    private bool wasOnPlatform;
    private bool requiresReparent;

    private GameObject _lastHitObject;

    private void Start()
    {
        smoothLocomotion = GetComponentInChildren<SmoothLocomotion>();
        characterController = GetComponentInChildren<CharacterController>();

        _initialCharacterParent = transform.parent;
    }

    private void Update()
    {
        CheckMovingPlatform();
    }

    private void FixedUpdate()
    {
        UpdateDistanceFromGround();
    }

    public virtual void CheckMovingPlatform()
    {
        bool onMovingPlatform = false;

        if (groundHit.collider != null && DistanceFromGround < 0.01f)
        {
            UpdateCurrentPlatform();

            if (CurrentPlatform)
            {
                onMovingPlatform = true;

                if (CurrentPlatform.MovementMethod == MovingPlatformMethod.PositionDifference && CurrentPlatform != null && CurrentPlatform.PositionDelta != Vector3.zero)
                {
                    if (smoothLocomotion)
                    {
                        if (smoothLocomotion.ControllerType == PlayerControllerType.Rigidbody)
                        {

                        }
                        else
                        {
                            smoothLocomotion.MoveCharacter(CurrentPlatform.PositionDelta);
                        }

                    }
                    else if (characterController)
                    {
                        characterController.Move(CurrentPlatform.PositionDelta);
                    }
                }
                if (CurrentPlatform.MovementMethod == MovingPlatformMethod.ParentToPlatform)
                {
                    if (characterController != null)
                    {
                        if (onMovingPlatform)
                        {
                            characterController.transform.parent = groundHit.collider.transform;
                            requiresReparent = true;
                        }
                    }
                    else if (smoothLocomotion != null && smoothLocomotion.ControllerType == PlayerControllerType.Rigidbody)
                    {
                        if (onMovingPlatform)
                        {
                            transform.parent = groundHit.collider.transform;
                            requiresReparent = true;
                        }
                    }
                }
            }
        }
        else
        {
            if (CurrentPlatform != null)
            {
                CurrentPlatform = null;
            }
        }

        if (!onMovingPlatform && wasOnPlatform && requiresReparent)
        {
            if (characterController)
            {
                characterController.transform.parent = _initialCharacterParent;
            }
            else
            {
                transform.parent = _initialCharacterParent;
            }
        }

        wasOnPlatform = onMovingPlatform;
    }

    public virtual void UpdateCurrentPlatform()
    {
        if (_lastHitObject != groundHit.collider.gameObject)
        {
            _lastHitObject = groundHit.collider.gameObject;
            CurrentPlatform = _lastHitObject.GetComponent<MovingPlatform>();
        }
    }

    public virtual void UpdateDistanceFromGround()
    {

        if (characterController)
        {
            if (Physics.Raycast(characterController.transform.position, -characterController.transform.up, out groundHit, 20, GroundedLayers, QueryTriggerInteraction.Ignore))
            {
                DistanceFromGround = Vector3.Distance(characterController.transform.position, groundHit.point);
                DistanceFromGround += characterController.center.y;
                DistanceFromGround -= (characterController.height * 0.5f) + characterController.skinWidth;

                DistanceFromGround = (float)Math.Round(DistanceFromGround * 1000f) / 1000f;
            }
            else
            {
                DistanceFromGround = 9999f;
            }
        }
        else
        {
            if (Physics.Raycast(transform.position, -transform.up, out groundHit, 20, GroundedLayers, QueryTriggerInteraction.Ignore))
            {
                DistanceFromGround = Vector3.Distance(transform.position, groundHit.point);
                DistanceFromGround = (float)Math.Round(DistanceFromGround * 1000f) / 1000f;
            }
            else
            {
                DistanceFromGround = 9999f;
            }
        }
    }
}