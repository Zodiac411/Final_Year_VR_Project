using UnityEngine;
using UnityEngine.Serialization;

public class RaycastWeaponRecoil : MonoBehaviour
{
    [SerializeField] private Vector3 recoilForce = Vector3.zero;
    [SerializeField] private float recoilDuration = 0.3f;
    [FormerlySerializedAs("MuzzlePointTransform")]
    [SerializeField] private Transform muzzlePointTransform;

    private Rigidbody weaponRigid;
    private Grabbable grabbable;

    private void Awake()
    {
        weaponRigid = GetComponent<Rigidbody>();
        grabbable = GetComponent<Grabbable>();
    }

    public void Configure(WeaponDefinition definition, Transform muzzle)
    {
        if (definition != null)
        {
            recoilForce = definition.recoilForce;
            recoilDuration = definition.recoilDuration;
        }

        muzzlePointTransform = muzzle;
    }

    public void ApplyRecoil()
    {
        if (weaponRigid == null || recoilForce == Vector3.zero || muzzlePointTransform == null)
        {
            return;
        }

        grabbable?.RequestSpringTime(recoilDuration);
        weaponRigid.AddForceAtPosition(
            muzzlePointTransform.TransformDirection(recoilForce),
            muzzlePointTransform.position,
            ForceMode.VelocityChange);
    }
}
