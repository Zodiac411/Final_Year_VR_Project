using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(RaycastWeapon))]
public class RaycastWeaponTrigger : MonoBehaviour
{
    [FormerlySerializedAs("FiringMethod")]
    [SerializeField] private FiringType firingMethod = FiringType.Semi;
    [FormerlySerializedAs("TriggerTransform")]
    [SerializeField] private Transform triggerTransform;

    private RaycastWeapon weapon;
    private bool readyToShoot = true;
    private bool playedEmptySound;

    private void Awake()
    {
        weapon = GetComponent<RaycastWeapon>();
    }

    public void Configure(WeaponDefinition definition, Transform trigger)
    {
        if (definition != null)
        {
            firingMethod = definition.firingMethod;
        }

        triggerTransform = trigger;
    }

    public void OnTrigger(float triggerValue)
    {
        triggerValue = Mathf.Clamp01(triggerValue);

        if (triggerTransform)
        {
            triggerTransform.localEulerAngles = new Vector3(triggerValue * 15, 0, 0);
        }

        if (triggerValue <= 0.5f)
        {
            readyToShoot = true;
            playedEmptySound = false;
        }

        if (readyToShoot && triggerValue >= 0.75f)
        {
            weapon.Shoot(ref playedEmptySound);
            readyToShoot = firingMethod == FiringType.Automatic;
        }
    }
}
