using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using UnityEngine;

[RequireComponent(typeof(Grabbable))]
public abstract class GrabbableEvents : MonoBehaviour
{
    protected Grabbable grab;
    protected Grabber thisGrabber;

    protected XRInput input;

    protected virtual void Awake()
    {
        grab = GetComponent<Grabbable>();
        input = XRInput.Instance;
    }

    public virtual void OnGrab(Grabber grabber)
    {
        thisGrabber = grabber;
    }

    public virtual void OnRelease()
    {
        // HAS BEEN DROPPED FROM THE GRABBER
    }

    public virtual void OnBecomesClosestGrabbable(ControllerHand touchingHand)
    {
        // CALLED IF THIS IS THE CLOSEST GRABBABLE BUT WASN'T IN THE PREVIOUS FRAME
    }

    public virtual void OnBecomesClosestGrabbable(Grabber touchingGrabber)
    {
        // CALLED IF THIS IS THE CLOSEST GRABBABLE BUT WASN'T IN THE PREVIOUS FRAME
    }

    public virtual void OnNoLongerClosestGrabbable(ControllerHand touchingHand)
    {
        // NO LONGER CLOSEST GRABBABLE. MAY NEED TO DISABLE HIGHLIGHT, RING AND ETC
    }

    public virtual void OnNoLongerClosestGrabbable(Grabber touchingGrabber)
    {
        // NO LONGER CLOSEST GRABBABLE. MAY NEED TO DISABLE HIGHLIGHT, RING AND ETC
    }

    public virtual void OnBecomesClosestRemoteGrabbable(ControllerHand touchingHand)
    {
        // FIRES IF THIS IS THE CLOSEST REMOTE GRABBABLE BUT WASN'T IN THE PREVIOUS FRAME
    }

    public virtual void OnBecomesClosestRemoteGrabbable(Grabber theGrabber)
    {
        // FIRES IF THIS IS THE CLOSEEST REMOTE GRABBABLE BUT WASN'T IN THE PREVIOUS FRAME
    }

    public virtual void OnNoLongerClosestRemoteGrabbable(ControllerHand touchingHand)
    {
        // FIRES IF THIS WAS THE CLOSEST REMOTE GRABBABLE LAST FRAME, BUT NOT THIS FRAME
    }

    public virtual void OnNoLongerClosestRemoteGrabbable(Grabber theGrabber)
    {
        // FIRES IF THIS WAS THE CLOEST REMOTE GRABBABLE LAST FRAME, BUT NOT THIS FRAME
    }

    public virtual void OnGrip(float gripValue)
    {
        //AMOUNT OF GRIP (0-1). ONLY FIRED IF OBJECT IS BEING HELD
    }

    public virtual void OnTrigger(float triggerValue)
    {
        // AMOUNT OF TRIGGER BEING HELD DOWN ON THE GRABBED ITEMS CONTROLLER. ONLY FIRED OBECT IS BEING HELD.
    }

    public virtual void OnTriggerDown()
    {
        // FIRES IF TRIGGER WAS PRESSED DOWN ON THIS CONTROLLER THIS FRAME, BUT WAS NOT PRESSED LAST FRAME. ONLY FIRED
        // IF OBJECT IS BEING HELD
    }

    public virtual void OnTriggerUp()
    {
        //FIRES IF TRIGGER IS NOT HELD DOWN THIS FRAME
    }

    public virtual void OnButton1()
    {
        // BUTTON 1 IS BEING HELD DOWN THIS FRAME BUT NOT LAST
        // BUTTON 1 = 'A' IF HELD IN RIGHT CONTROLLER. 'X' IF HELD IN THE LEFT CONTROLLER
    }

    public virtual void OnButton1Down()
    {
        // BUTTON 1 PRESSED DOWN THIS FRAME
        // BUTTON 1 = 'A' IF HELD IN THE RIGHT CONTROLLER. 'X' IF HELD IN THE LEFT CONTROLLER
    }

    public virtual void OnButton1Up()
    {
        // BUTTON 1 RELEASED THIS FRAME
        // BUTTON 1 = 'A' IF HELD IN RIGHT CONTROLLER. 'X' IF HELD IN THE LEFT CONTROLLER
    }


    public virtual void OnButton2()
    {
        // BUTTON 2 IS BEING HELD DOWN THIS FRAME BUT NOT LAST
        // BUTTON = 'B' IF HELD IN THE RIGHT CONTROLLER. 'Y' IF HELD IN THE LEFT CONTROLLER
    }

    public virtual void OnButton2Down()
    {
        //BUTTON 2 PRESSED DOWN THIS FRAME
        //BUTTON 2 = 'B' IF HELD IN THE RIGHT CONTROLLER. 'Y' IF HELD IN THE LEFT CONTROLLER
    }

    public virtual void OnButton2Up()
    {
        // BUTTON 2 RELEASED THIS FRAME
        // BUTTON 2 = 'B' IF HELD IN THE RIGHT CONTROLLER. 'Y' IF HELD IN THE LEFT CONTROLLER
    }

    public virtual void OnSnapZoneEnter()
    {
        //GRABBABLE HAS BEEN SUCCESSFULLY INSERTED INTO A SNAPZONE
    }

    public virtual void OnSnapZoneExit()
    {
        //GRABBABLE HAS BEEN REMOVED FROM A SNAPZONE
    }
}

[System.Serializable]
public class FloatEvent : UnityEvent<float> { }

[System.Serializable]
public class FloatFloatEvent : UnityEvent<float, float> { }

[System.Serializable]
public class GrabberEvent : UnityEvent<Grabber> { }

[System.Serializable]
public class GrabbableEvent : UnityEvent<Grabbable> { }

[System.Serializable]
public class RaycastHitEvent : UnityEvent<RaycastHit> { }

[System.Serializable]
public class Vector2Event : UnityEvent<Vector2> { }

[System.Serializable]
public class Vector3Event : UnityEvent<Vector3> { }

[System.Serializable]
public class PointerEventDataEvent : UnityEvent<UnityEngine.EventSystems.PointerEventData> { }