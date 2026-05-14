using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    public GameObject ProjectileObject;
    public AudioClip LaunchSound;
    public ParticleSystem LaunchParticles;
    public Transform MuzzleTransform;

    public float ProjectileForce = 15f;
    private float _initialProjectileForce;

    private void Start()
    {
        _initialProjectileForce = ProjectileForce;
    }

    public GameObject ShootProjectile(float projectileForce)
    {

        if (MuzzleTransform && ProjectileObject)
        {
            GameObject launched = Instantiate(ProjectileObject, MuzzleTransform.transform.position, MuzzleTransform.transform.rotation) as GameObject;
            launched.transform.position = MuzzleTransform.transform.position;
            launched.transform.rotation = MuzzleTransform.transform.rotation;

            launched.GetComponentInChildren<Rigidbody>().AddForce(MuzzleTransform.forward * projectileForce, ForceMode.VelocityChange);

            XRManager.Instance.PlaySpatialClipAt(LaunchSound, launched.transform.position, 1f);

            if (LaunchParticles)
            {
                LaunchParticles.Play();
            }

            return launched;
        }

        return null;
    }

    public void ShootProjectile()
    {
        ShootProjectile(ProjectileForce);
    }

    public void SetForce(float force)
    {
        ProjectileForce = force;
    }

    public float GetInitialProjectileForce()
    {
        return _initialProjectileForce;
    }
}

