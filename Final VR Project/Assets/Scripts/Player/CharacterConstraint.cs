using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class CharacterConstraint : MonoBehaviour
{
    PlayerController playerController;
    CharacterController character;

    void Awake()
    {
        character = GetComponent<CharacterController>();
        playerController = transform.GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        CheckCharacterCollisionMove();
    }

    public virtual void CheckCharacterCollisionMove()
    {

        var initialCameraRigPosition = playerController.CameraRig.transform.position;
        var cameraPosition = playerController.CenterEyeAnchor.position;
        var delta = cameraPosition - transform.position;

        delta.y = 0;

        if (delta.magnitude > 0)
        {
            character.Move(delta);
            playerController.CameraRig.transform.position = initialCameraRigPosition;
        }
    }
}