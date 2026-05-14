using UnityEngine;

public interface IWeaponSpawnFactory
{
    GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent);
}

public sealed class DefaultWeaponSpawnFactory : IWeaponSpawnFactory
{
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (prefab == null || parent == null)
        {
            return null;
        }

        GameObject spawnedWeapon = Object.Instantiate(prefab, position, rotation);
        spawnedWeapon.transform.SetParent(parent);
        spawnedWeapon.transform.SetParent(null);

        Rigidbody rb = spawnedWeapon.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 forceDirection = -parent.up;
            float forceMagnitude = 10.0f;
            rb.isKinematic = false;
            rb.AddForce(forceDirection * forceMagnitude, ForceMode.VelocityChange);
        }

        return spawnedWeapon;
    }
}
