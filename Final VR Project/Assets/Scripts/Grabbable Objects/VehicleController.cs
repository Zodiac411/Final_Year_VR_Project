using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

[System.Serializable]
public class WheelObject
{
    public WheelCollider Wheel;
    public Transform WheelVisual;
    public bool ApplyTorque;
    public bool ApplySteering;
}

public class VehicleController : MonoBehaviour
{
    [Header("Vehicle Properties")]
    [SerializeField] private float MotorTorque = 500f;
    [SerializeField] private float MaxSpeed = 30f;
    [SerializeField] private float MaxSteeringAngle = 45f;

    [Header("Steering Grabbable")]
    [SerializeField] private bool CheckTriggerInput = true;
    [SerializeField] private Grabbable SteeringGrabbable;

    [Header("Engine Status")]
    [SerializeField] private bool EngineOn = false;

    [SerializeField] private float CrankTime = 0.1f;

    [Header("Speedometer")]
    [SerializeField] private Text SpeedLabel;

    [Header("Audio Setup")]
    [SerializeField] private AudioSource EngineAudio;
    [SerializeField] private AudioClip IdleSound;
    [SerializeField] private AudioClip CrankSound;
    [SerializeField] private AudioClip CollisionSound;

    [HideInInspector] public float SteeringAngle = 0;
    [HideInInspector] public float MotorInput = 0;
    [HideInInspector] public float CurrentSpeed;


    [Header("Wheel Configuration")]
    public List<WheelObject> Wheels;

    private Vector3 initialPosition;
    private Rigidbody rb;

    bool wasHoldingSteering, isHoldingSteering;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.position;
    }

    private void Update()
    {

        isHoldingSteering = SteeringGrabbable != null && SteeringGrabbable.BeingHeld;

        if (CheckTriggerInput)
        {
            GetTorqueInputFromTriggers();
        }

        if (Mathf.Abs(MotorInput) > 0.01f && !EngineOn)
        {
            CrankEngine();
        }

        if (crankingEngine)
        {
            return;
        }

        UpdateEngineAudio();

        if (SpeedLabel != null)
        {
            SpeedLabel.text = CurrentSpeed.ToString("n0");
        }

        CheckOutOfBounds();

        wasHoldingSteering = isHoldingSteering;
    }

    public virtual void CrankEngine()
    {

        if (crankingEngine || EngineOn)
        {
            return;
        }

        StartCoroutine(crankEngine());
    }

    protected bool crankingEngine = false;

    private IEnumerator crankEngine()
    {
        crankingEngine = true;

        if (CrankSound != null)
        {
            EngineAudio.clip = CrankSound;
            EngineAudio.loop = false;
            EngineAudio.Play();
        }

        yield return new WaitForSeconds(CrankTime);

        if (IdleSound != null)
        {
            EngineAudio.clip = IdleSound;
            EngineAudio.loop = true;
            EngineAudio.Play();
        }

        yield return new WaitForEndOfFrame();

        crankingEngine = false;
        EngineOn = true;
    }

    public virtual void CheckOutOfBounds()
    {
        if (transform.position.y < -500f)
        {
            transform.position = initialPosition;
        }
    }

    public virtual void GetTorqueInputFromTriggers()
    {
        if (isHoldingSteering)
        {
            SetMotorTorqueInput(XRInput.Instance.RightTrigger - XRInput.Instance.LeftTrigger);
        }
        else if (wasHoldingSteering && !isHoldingSteering)
        {
            SetMotorTorqueInput(0);
        }
    }

    private void FixedUpdate()
    {
        CurrentSpeed = correctValue(rb.velocity.magnitude * 3.6f);
        UpdateWheelTorque();
    }

    public virtual void UpdateWheelTorque()
    {
        float torqueInput = EngineOn ? MotorInput : 0;

        for (int x = 0; x < Wheels.Count; x++)
        {
            WheelObject wheel = Wheels[x];

            if (wheel.ApplySteering)
            {
                wheel.Wheel.steerAngle = MaxSteeringAngle * SteeringAngle;
            }

            if (wheel.ApplyTorque)
            {
                wheel.Wheel.motorTorque = MotorTorque * torqueInput;
            }

            UpdateWheelVisuals(wheel);
        }
    }

    public virtual void SetSteeringAngle(float steeringAngle)
    {
        SteeringAngle = steeringAngle;
    }

    public virtual void SetSteeringAngleInverted(float steeringAngle)
    {
        SteeringAngle = steeringAngle * -1;
    }

    public virtual void SetSteeringAngle(Vector2 steeringAngle)
    {
        SteeringAngle = steeringAngle.x;
    }

    public virtual void SetSteeringAngleInverted(Vector2 steeringAngle)
    {
        SteeringAngle = -steeringAngle.x;
    }

    public virtual void SetMotorTorqueInput(float input)
    {
        MotorInput = input;
    }

    public virtual void SetMotorTorqueInputInverted(float input)
    {
        MotorInput = -input;
    }

    public virtual void SetMotorTorqueInput(Vector2 input)
    {
        MotorInput = input.y;
    }

    public virtual void SetMotorTorqueInputInverted(Vector2 input)
    {
        MotorInput = -input.y;
    }

    public virtual void UpdateWheelVisuals(WheelObject wheel)
    {
        if (wheel != null && wheel.WheelVisual != null)
        {
            Vector3 position;
            Quaternion rotation;
            wheel.Wheel.GetWorldPose(out position, out rotation);

            wheel.WheelVisual.transform.position = position;
            wheel.WheelVisual.transform.rotation = rotation;
        }
    }

    public virtual void UpdateEngineAudio()
    {
        if (EngineAudio && EngineOn)
        {
            EngineAudio.pitch = Mathf.Clamp(0.5f + (CurrentSpeed / MaxSpeed), -0.1f, 3f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        float colVelocity = collision.relativeVelocity.magnitude;

        if (colVelocity > 0.1f)
        {
            XRManager.Instance.PlaySpatialClipAt(CollisionSound, collision.GetContact(0).point, 1f);
        }
    }

    float correctValue(float inputValue)
    {
        return (float)System.Math.Round(inputValue * 1000f) / 1000f;
    }
}