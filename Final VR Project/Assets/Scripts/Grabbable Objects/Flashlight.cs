using UnityEngine;

public class Flashlight : GrabbableEvents
{
    [SerializeField] private Light SpotLight;
    [SerializeField] private Transform LightSwitch;

    private Vector3 originalSwitchPosition;

    void Start()
    {
        originalSwitchPosition = LightSwitch.transform.localPosition;
    }

    public override void OnTrigger(float triggerValue)
    {

        SpotLight.enabled = triggerValue > 0.2f;

        LightSwitch.localPosition = new Vector3(originalSwitchPosition.x * triggerValue, originalSwitchPosition.y, originalSwitchPosition.z);

        base.OnTrigger(triggerValue);
    }

    public override void OnTriggerUp()
    {

        SpotLight.enabled = false;

        LightSwitch.localPosition = originalSwitchPosition;

        base.OnTriggerUp();
    }
}

