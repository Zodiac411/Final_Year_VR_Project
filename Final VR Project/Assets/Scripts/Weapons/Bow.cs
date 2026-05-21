using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;

public class DrawDefinition
{
    public float DrawPercentage { get; set; }
    public float HapticAmplitude { get; set; }
    public float HapticFrequency { get; set; }
}

public class Bow : GrabbableEvents
{
    [Header("Bow Settings")]
    [SerializeField] private Transform BowModel;
    [SerializeField] private float BowForce = 50f;
    [SerializeField] private bool AlignBowToArrow = true;
    [SerializeField] private Text PercentageUI;

    [Header("Arrow Settings")]
    [SerializeField] private Transform ArrowRestLeftHanded;
    [SerializeField] private Transform ArrowRest;
    [SerializeField] private Transform ArrowKnock;
    [SerializeField] private bool CanGrabArrowFromKnock = true;

    private string ArrowPrefabName = "Arrow2";

    [Header("Arrow Positioning")]
    [SerializeField] private bool IgnoreXPosition = false;
    [SerializeField] private bool IgnoreYPosition = false;
    [SerializeField] private bool AllowNegativeZ = true;

    [Header("Arrow Grabbing")]
    public bool CanGrabArrow = false;
    private bool holdingArrow = false;

    [HideInInspector]
    public Grabber ClosestGrabber;
    [HideInInspector]
    public Arrow GrabbedArrow;
    Grabbable arrowGrabbable;
    [HideInInspector]
    public Vector3 LastValidPosition;

    [Header("String Settings")]
    public float MaxStringDistance = 0.3f;
    public float StringDistance = 0;

    public float DrawPercent { get; private set; } = 0;
    private float lastDrawPercent;
    private float lastDrawHaptic;
    private float lastDrawHapticTime;
    private bool playedDrawSound = false;

    private Grabber arrowGrabber;
    private Grabbable bowGrabbable;
    private Vector3 initialKnockPosition;

    private List<DrawDefinition> drawDefs;
    private AudioSource audioSource;

    private void Start()
    {
        initialKnockPosition = ArrowKnock.localPosition;
        bowGrabbable = GetComponent<Grabbable>();
        audioSource = GetComponent<AudioSource>();

        drawDefs = new List<DrawDefinition>() 
        {
            { new DrawDefinition() { DrawPercentage = 30f, HapticAmplitude = 0.1f, HapticFrequency = 0.1f } },
            { new DrawDefinition() { DrawPercentage = 40f, HapticAmplitude = 0.1f, HapticFrequency = 0.1f } },
            { new DrawDefinition() { DrawPercentage = 50f, HapticAmplitude = 0.1f, HapticFrequency = 0.1f } },
            { new DrawDefinition() { DrawPercentage = 60f, HapticAmplitude = 0.1f, HapticFrequency = 0.1f } },
            { new DrawDefinition() { DrawPercentage = 70f, HapticAmplitude = 0.1f, HapticFrequency = 0.1f } },
            { new DrawDefinition() { DrawPercentage = 80f, HapticAmplitude = 0.1f, HapticFrequency = 0.1f } },
            { new DrawDefinition() { DrawPercentage = 90f, HapticAmplitude = 0.1f, HapticFrequency = 0.9f } },
            { new DrawDefinition() { DrawPercentage = 100f, HapticAmplitude = 0.1f, HapticFrequency = 1f } },
        };
    }

    private void Update()
    {
        updateDrawDistance();
        checkBowHaptics();

        if (!bowGrabbable.BeingHeld)
        {

            if (holdingArrow)
            {
                ReleaseArrow();
            }

            resetStringPosition();
            return;
        }

        holdingArrow = GrabbedArrow != null;

        if (canGrabArrowFromKnock())
        {
            GameObject arrow = Instantiate(Resources.Load(ArrowPrefabName, typeof(GameObject))) as GameObject;
            arrow.transform.position = ArrowKnock.transform.position;
            arrow.transform.LookAt(getArrowRest());

            Grabbable g = arrow.GetComponent<Grabbable>();
            g.GrabButton = GrabButton.Trigger;
            g.AddControllerVelocityOnDrop = false;

            GrabArrow(arrow.GetComponent<Arrow>());
        }

        if (GrabbedArrow == null)
        {
            resetStringPosition();
        }

        if (arrowGrabber != null)
        {
            StringDistance = Vector3.Distance(transform.position, arrowGrabber.transform.position);
        }
        else
        {
            StringDistance = 0;
        }

        if (holdingArrow)
        {
            setKnockPosition();
            alignArrow();
            checkDrawSound();
            checkBowHaptics();

            if (getGrabArrowInput() <= 0.2f)
            {
                ReleaseArrow();
            }
        }

        alignBow();
    }

    private Transform getArrowRest()
    {
        if (bowGrabbable.GetPrimaryGrabber() != null && bowGrabbable.GetPrimaryGrabber().HandSide == ControllerHand.Right && ArrowRestLeftHanded != null)
        {
            return ArrowRestLeftHanded;
        }

        return ArrowRest;
    }

    private bool canGrabArrowFromKnock()
    {
        if (!CanGrabArrowFromKnock)
        {
            return false;
        }

        ControllerHand hand = bowGrabbable.GetControllerHand(bowGrabbable.GetPrimaryGrabber()) == ControllerHand.Left ? ControllerHand.Right : ControllerHand.Left;

        return CanGrabArrow && getTriggerInput(hand) > 0.75f && !holdingArrow;
    }

    private float getGrabArrowInput()
    {
        if (arrowGrabber != null && arrowGrabbable != null)
        {

            GrabButton grabButton = arrowGrabber.GetGrabButton(arrowGrabbable);

            if (grabButton == GrabButton.Grip)
            {
                return getGripInput(arrowGrabber.HandSide);
            }
            else if (grabButton == GrabButton.Trigger)
            {
                return getTriggerInput(arrowGrabber.HandSide);
            }
        }

        return 0;
    }

    float getGripInput(ControllerHand handSide)
    {
        if (handSide == ControllerHand.Left)
        {
            return input.LeftGrip;
        }
        else if (handSide == ControllerHand.Right)
        {
            return input.RightGrip;
        }

        return 0;
    }

    float getTriggerInput(ControllerHand handSide)
    {
        if (handSide == ControllerHand.Left)
        {
            return input.LeftTrigger;
        }
        else if (handSide == ControllerHand.Right)
        {
            return input.RightTrigger;
        }

        return 0;
    }

    void setKnockPosition()
    {
        if (StringDistance <= MaxStringDistance)
        {
            ArrowKnock.position = arrowGrabber.transform.position;
        }
        else
        {
            ArrowKnock.localPosition = initialKnockPosition;
            ArrowKnock.LookAt(arrowGrabber.transform, ArrowKnock.forward);
            ArrowKnock.position += ArrowKnock.forward * (MaxStringDistance * 0.65f);
        }

        if (IgnoreXPosition)
        {
            ArrowKnock.localPosition = new Vector3(getArrowRest().localPosition.x, ArrowKnock.localPosition.y, ArrowKnock.localPosition.z);
        }
        if (IgnoreYPosition)
        {
            ArrowKnock.localPosition = new Vector3(ArrowKnock.localPosition.x, 0, ArrowKnock.localPosition.z);
        }

        if (!AllowNegativeZ && ArrowKnock.localPosition.z > initialKnockPosition.z)
        {
            ArrowKnock.localPosition = new Vector3(ArrowKnock.localPosition.x, ArrowKnock.localPosition.y, initialKnockPosition.z);
        }
    }

    void checkDrawSound()
    {
        if (holdingArrow && !playedDrawSound && DrawPercent > 30f)
        {
            playBowDraw();
            playedDrawSound = true;
        }
    }

    void updateDrawDistance()
    {
        lastDrawPercent = DrawPercent;

        float knockDistance = Math.Abs(Vector3.Distance(ArrowKnock.localPosition, initialKnockPosition));
        DrawPercent = (knockDistance / MaxStringDistance) * 100;

        if (PercentageUI != null)
        {
            PercentageUI.text = (int)DrawPercent + "%";
        }
    }

    void checkBowHaptics()
    {
        if (DrawPercent < lastDrawPercent)
        {
            return;
        }

        if (Time.time - lastDrawHapticTime < 0.11)
        {
            return;
        }

        if (drawDefs == null)
        {
            return;
        }

        DrawDefinition d = drawDefs.FirstOrDefault(x => x.DrawPercentage <= DrawPercent && x.DrawPercentage != lastDrawHaptic);
        if (d != null && arrowGrabber != null)
        {
            input.VibrateController(d.HapticFrequency, d.HapticAmplitude, 0.1f, arrowGrabber.HandSide);
            lastDrawHaptic = d.DrawPercentage;
            lastDrawHapticTime = Time.time;
        }

    }

    void resetStringPosition()
    {
        ArrowKnock.localPosition = Vector3.Lerp(ArrowKnock.localPosition, initialKnockPosition, Time.deltaTime * 100);
    }

    protected virtual void alignArrow()
    {
        GrabbedArrow.transform.parent = this.transform;
        GrabbedArrow.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
        GrabbedArrow.GetComponent<Rigidbody>().isKinematic = true;

        GrabbedArrow.transform.position = ArrowKnock.transform.position;
        GrabbedArrow.transform.LookAt(getArrowRest());
    }

    public Vector3 BowUp = Vector3.forward;

    public float AlignBowSpeed = 20f;
    protected virtual void alignBow()
    {
        if (AlignBowToArrow == false || BowModel == null || grab == null || !grab.BeingHeld)
        {
            return;
        }

        if (grab != null && grab.BeingHeld)
        {
            if (holdingArrow)
            {

                if (GrabbedArrow != null)
                {
                    BowModel.transform.rotation = GrabbedArrow.transform.rotation;
                }
                else
                {
                    BowModel.transform.localRotation = Quaternion.Slerp(BowModel.transform.localRotation, Quaternion.identity, Time.deltaTime * AlignBowSpeed);
                }
                Vector3 eulers = BowModel.transform.localEulerAngles;
                eulers.z = 0;

                BowModel.transform.localEulerAngles = eulers;
            }
            else
            {
                BowModel.transform.localRotation = Quaternion.Slerp(BowModel.transform.localRotation, Quaternion.identity, Time.deltaTime * AlignBowSpeed);
            }
        }
    }

    public virtual void ResetBowAlignment()
    {
        if (BowModel != null)
        {
            BowModel.localEulerAngles = Vector3.zero;
        }
    }

    public void GrabArrow(Arrow arrow)
    {
        arrowGrabber = ClosestGrabber;

        GrabbedArrow = arrow.GetComponent<Arrow>();
        GrabbedArrow.ShaftCollider.enabled = false;

        arrowGrabbable = arrow.GetComponent<Grabbable>();

        if (arrowGrabbable)
        {
            arrowGrabbable.GrabItem(arrowGrabber);
            arrowGrabber.HeldGrabbable = arrowGrabbable;
            arrowGrabbable.AddControllerVelocityOnDrop = false;
        }


        Collider playerCollder = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<Collider>();
        if (playerCollder)
        {
            Physics.IgnoreCollision(GrabbedArrow.ShaftCollider, playerCollder);
        }

        holdingArrow = true;
    }

    public void ReleaseArrow()
    {
        playBowRelease();

        if (arrowGrabbable)
        {
            arrowGrabbable.GrabButton = GrabButton.Grip;
            arrowGrabbable.DropItem(false, true);

            arrowGrabbable.AddControllerVelocityOnDrop = true;
        }

        float shotForce = BowForce * StringDistance;
        GrabbedArrow.ShootArrow(GrabbedArrow.transform.forward * shotForce);

        arrowGrabber.ResetHandGraphics();

        resetArrowValues();
    }

    public override void OnRelease()
    {
        ResetBowAlignment();
        resetStringPosition();
    }

    void resetArrowValues()
    {
        GrabbedArrow = null;
        arrowGrabbable = null;
        arrowGrabber = null;
        holdingArrow = false;
        playedDrawSound = false;
    }

    void playSoundInterval(float fromSeconds, float toSeconds, float volume)
    {
        if (audioSource)
        {

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.pitch = Time.timeScale;
            audioSource.time = fromSeconds;
            audioSource.volume = volume;
            audioSource.Play();
            audioSource.SetScheduledEndTime(AudioSettings.dspTime + (toSeconds - fromSeconds));
        }
    }

    void playBowDraw()
    {
        playSoundInterval(0, 1.66f, 0.4f);
    }

    void playBowRelease()
    {
        playSoundInterval(1.67f, 2.2f, 0.3f);
    }
}