using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine;

public class RingHelper : MonoBehaviour
{
    [SerializeField] private Grabbable grabbable;
    [SerializeField] private GrabPoint Grabpoint;

    [SerializeField] private Color RingColor = Color.white;
    [SerializeField] private Color RingSelectedColor = Color.white;
    [SerializeField] private Color RingSecondarySelectedColor = Color.white;

    [SerializeField] private float ringSizeInRange = 1500f;
    [SerializeField] private float ringSizeGrabbable = 1100f;
    [SerializeField] private float RingFadeSpeed = 5;

    public bool HideIfHandsAreFull = true;

    private Transform cam;

    private CanvasScaler scaler;
    private Canvas canvas;
    private Text text;

    private Grabber leftGrabber;
    private Grabber rightGrabber;
    private Grabber closestGrabber;

    private bool handsFull = false;

    private float initialOpac;
    private float currentOpac;


    private void Start()
    {
        AssignCamera();

        if (grabbable == null)
        {
            grabbable = transform.parent.GetComponent<Grabbable>();
        }

        canvas = GetComponent<Canvas>();
        scaler = GetComponent<CanvasScaler>();
        text = GetComponent<Text>();

        if (text == null)
        {
            Debug.LogWarning("need a text component for the ring");
            return;
        }

        initialOpac = text.color.a;
        currentOpac = initialOpac;

        AssignGrabbers();
    }

    private void Update()
    {
        AssignCamera();

        if (text == null || cam == null || grabbable == null)
        {
            return;
        }

        bool grabbersExist = leftGrabber != null && rightGrabber != null;
        handsFull = grabbersExist && leftGrabber.HoldingItem && rightGrabber.HoldingItem;

        if (grabbersExist && grabbable.GrabButton == GrabButton.Grip && !leftGrabber.FreshGrip && !rightGrabber.FreshGrip)
        {
            handsFull = true;
        }

        bool showRings = handsFull;

        if (grabbable.BeingHeld || !grabbable.isActiveAndEnabled)
        {
            canvas.enabled = false;
            return;
        }

        if (grabbable.OtherGrabbableMustBeGrabbed != null && grabbable.OtherGrabbableMustBeGrabbed.BeingHeld == false)
        {
            canvas.enabled = false;
            return;
        }

        float currentDistance = Vector3.Distance(transform.position, cam.position);
        if (!handsFull && currentDistance <= grabbable.RemoteGrabDistance)
        {
            showRings = true;
        }
        else
        {
            showRings = false;
        }

        if (showRings)
        {
            canvas.enabled = true;
            canvas.transform.LookAt(cam);

            text.text = "o";

            bool isClosest = grabbable.GetClosestGrabber() != null && grabbable.IsGrabbable();
            if (Grabpoint != null)
            {

            }

            if (isClosest)
            {
                scaler.dynamicPixelsPerUnit = ringSizeGrabbable;

                text.color = getSelectedColor();
            }
            else
            {
                scaler.dynamicPixelsPerUnit = ringSizeInRange;
                text.color = RingColor;
            }

            currentOpac += Time.deltaTime * RingFadeSpeed;
            if (currentOpac > initialOpac)
            {
                currentOpac = initialOpac;
            }

            Color colorCurrent = text.color;
            colorCurrent.a = currentOpac;
            text.color = colorCurrent;
        }
        else
        {

            currentOpac -= Time.deltaTime * RingFadeSpeed;
            if (currentOpac <= 0)
            {
                currentOpac = 0;
                canvas.enabled = false;
            }
            else
            {
                canvas.enabled = true;
                Color colorCurrent = text.color;
                colorCurrent.a = currentOpac;
                text.color = colorCurrent;
            }
        }
    }

    public virtual void AssignCamera()
    {
        if (cam == null)
        {
            if (GameObject.FindGameObjectWithTag("MainCamera") != null)
            {
                cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
            }
        }
    }

    public virtual void AssignGrabbers()
    {
        Grabber[] grabs;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            grabs = player.GetComponentsInChildren<Grabber>();
        }
        else
        {
            grabs = FindObjectsOfType<Grabber>();
        }

        for (int x = 0; x < grabs.Length; x++)
        {
            Grabber g = grabs[x];
            if (g.HandSide == ControllerHand.Left)
            {
                leftGrabber = g;
            }
            else if (g.HandSide == ControllerHand.Right)
            {
                rightGrabber = g;
            }
        }
    }

    private Color getSelectedColor()
    {
        closestGrabber = grabbable.GetClosestGrabber();
        if (grabbable != null && closestGrabber != null)
        {
            if (closestGrabber.HandSide == ControllerHand.Left)
            {
                return RingSecondarySelectedColor;
            }
        }
        return RingSelectedColor;
    }
}