using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDefinition", menuName = "Weapons/Weapon Definition")]
public class WeaponDefinition : ScriptableObject
{
    public string displayName;
    public float maxRange = 25f;
    public float damage = 25f;
    public float firingRate = 0.2f;
    public float bulletImpactForce = 1000f;
    public float maxInternalAmmo = 10f;
    public float shotForce = 10f;
    public float bulletCasingForce = 3f;
    public float slowMoRateOfFire = 0.3f;
    public FiringType firingMethod = FiringType.Semi;
    public ReloadType reloadMethod = ReloadType.InfiniteAmmo;
    public bool autoChamberRounds = true;
    public bool mustChamberRounds = false;
    public bool alwaysFireProjectile = false;
    public bool fireProjectileInSlowMo = true;
    public Vector3 recoilForce = Vector3.zero;
    public float recoilDuration = 0.3f;
}
