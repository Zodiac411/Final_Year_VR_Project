using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class VRButton : MonoBehaviour
{
    public float MinLocalY = 0.25f;
    public float MaxLocalY = 0.55f;

    public float actuationForce = 0.01f;

    private bool AllowPhysicsForces = true;

    private List<Grabber> grabbers = new List<Grabber>();
    private List<UITrigger> uiTriggers = new List<UITrigger>();
    private SpringJoint joint;

    [SerializeField] private AudioClip ButtonClick;
    [SerializeField] private AudioClip ButtonClickUp;

    [SerializeField] private UnityEvent onButtonDown;
    [SerializeField] private UnityEvent onButtonUp;

    private bool clickingDown = false;

    private AudioSource audioSource;
    private Rigidbody rigid;

    void Start()
    {
        joint = GetComponent<SpringJoint>();
        rigid = GetComponent<Rigidbody>();

        if (!AllowPhysicsForces)
        {
            rigid.isKinematic = true;
        }

        transform.localPosition = new Vector3(transform.localPosition.x, MaxLocalY, transform.localPosition.z);

        audioSource = GetComponent<AudioSource>();
    }

    float ButtonSpeed = 15f;
    float SpringForce = 1500f;
    
    Vector3 buttonDownPosition;
    Vector3 buttonUpPosition;

    void Update()
    {
        buttonDownPosition = GetButtonDownPosition();
        buttonUpPosition = GetButtonUpPosition();

        bool grabberInButton = false;
        bool UITriggerInButton = uiTriggers != null && uiTriggers.Count > 0;

        for (int x = 0; x < grabbers.Count; x++)
        {
            if (!grabbers[x].HoldingItem)
            {
                grabberInButton = true;
                break;
            }
        }
        if (grabberInButton || UITriggerInButton)
        {
            float speed = ButtonSpeed;
            transform.localPosition = Vector3.Lerp(transform.localPosition, buttonDownPosition, speed * Time.deltaTime);

            if (joint)
            {
                joint.spring = 0;
            }
        }
        else
        {
            if (AllowPhysicsForces)
            {
                if (joint)
                {
                    joint.spring = SpringForce;
                }
            }
            else
            {
                float speed = ButtonSpeed;
                transform.localPosition = Vector3.Lerp(transform.localPosition, buttonUpPosition, speed * Time.deltaTime);
                if (joint)
                {
                    joint.spring = 0;
                }
            }
        }

        if (transform.localPosition.y < MinLocalY)
        {
            transform.localPosition = buttonDownPosition;
        }
        else if (transform.localPosition.y > MaxLocalY)
        {
            transform.localPosition = buttonUpPosition;
        }

        float buttonDownDistance = transform.localPosition.y - buttonDownPosition.y;
        if (buttonDownDistance <= actuationForce && !clickingDown)
        {
            clickingDown = true;
            OnButtonDown();
        }
        float buttonUpDistance = buttonUpPosition.y - transform.localPosition.y;
        if (buttonUpDistance <= actuationForce && clickingDown)
        {
            clickingDown = false;
            OnButtonUp();
        }
    }

    public virtual Vector3 GetButtonUpPosition()
    {
        return new Vector3(transform.localPosition.x, MaxLocalY, transform.localPosition.z);
    }

    public virtual Vector3 GetButtonDownPosition()
    {
        return new Vector3(transform.localPosition.x, MinLocalY, transform.localPosition.z);
    }

    public virtual void OnButtonDown()
    {
        if (audioSource && ButtonClick)
        {
            audioSource.clip = ButtonClick;
            audioSource.Play();
        }

        if (onButtonDown != null)
        {
            onButtonDown.Invoke();
        }
    }

    public virtual void OnButtonUp()
    {
        if (audioSource && ButtonClickUp)
        {
            audioSource.clip = ButtonClickUp;
            audioSource.Play();
        }

        if (onButtonUp != null)
        {
            onButtonUp.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Grabber grab = other.GetComponent<Grabber>();
        if (grab != null)
        {
            if (grabbers == null)
            {
                grabbers = new List<Grabber>();
            }

            if (!grabbers.Contains(grab))
            {
                grabbers.Add(grab);
            }
        }

        UITrigger trigger = other.GetComponent<UITrigger>();
        if (trigger != null)
        {
            if (uiTriggers == null)
            {
                uiTriggers = new List<UITrigger>();
            }

            if (!uiTriggers.Contains(trigger))
            {
                uiTriggers.Add(trigger);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Grabber grab = other.GetComponent<Grabber>();
        if (grab != null)
        {
            if (grabbers.Contains(grab))
            {
                grabbers.Remove(grab);
            }
        }

        UITrigger trigger = other.GetComponent<UITrigger>();
        if (trigger != null)
        {
            if (uiTriggers.Contains(trigger))
            {
                uiTriggers.Remove(trigger);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        Vector3 upPosition = transform.TransformPoint(new Vector3(transform.localPosition.x, MaxLocalY, transform.localPosition.z));
        Vector3 downPosition = transform.TransformPoint(new Vector3(transform.localPosition.x, MinLocalY, transform.localPosition.z));

        Vector3 size = new Vector3(0.005f, 0.005f, 0.005f);

        Gizmos.DrawCube(upPosition, size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(downPosition, size);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(upPosition, downPosition);
    }
}
