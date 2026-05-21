using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class RaycastWeaponVfx : MonoBehaviour
{
    [SerializeField] private GameObject muzzleFlashObject;
    [SerializeField] private GameObject hitFxPrefab;
    [SerializeField] private GameObject bulletCasingPrefab;
    [SerializeField] private Transform ejectPointTransform;
    [SerializeField] private Transform slideTransform;
    [SerializeField] private float slideSpeed = 1f;
    [SerializeField] private float slideDistance = -0.028f;
    [SerializeField] private float bulletCasingForce = 3f;

    private float minSlideDistance = 0.001f;

    public void Configure(WeaponDefinition definition)
    {
        if (definition != null)
        {
            bulletCasingForce = definition.bulletCasingForce;
        }
    }

    public void PrepareMuzzleFlash()
    {
        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(false);
        }
    }

    public IEnumerator DoMuzzleFlash()
    {
        if (muzzleFlashObject == null)
        {
            yield break;
        }

        muzzleFlashObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        RandomizeMuzzleFlashScaleRotation();
        yield return new WaitForSeconds(0.05f);
        muzzleFlashObject.SetActive(false);
    }

    public IEnumerator AnimateSlideAndEject(bool slideForcedBack)
    {
        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(true);
        }

        Vector3 slideDestination = new Vector3(0, 0, slideDistance);
        if (slideTransform != null)
        {
            bool slideEndReached = false;
            int frames = 0;
            while (!slideEndReached)
            {
                slideTransform.localPosition = Vector3.MoveTowards(slideTransform.localPosition, slideDestination, Time.deltaTime * slideSpeed);
                if (Vector3.Distance(slideTransform.localPosition, slideDestination) <= minSlideDistance || frames >= 2)
                {
                    slideEndReached = true;
                    if (muzzleFlashObject != null)
                    {
                        muzzleFlashObject.SetActive(false);
                    }
                }

                frames++;
                yield return new WaitForEndOfFrame();
            }

            slideTransform.localPosition = slideDestination;
        }

        yield return new WaitForEndOfFrame();
        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(false);
        }

        EjectCasing();

        if (!slideForcedBack && slideTransform != null)
        {
            int frames = 0;
            bool slideBeginningReached = false;
            while (!slideBeginningReached)
            {
                slideTransform.localPosition = Vector3.MoveTowards(slideTransform.localPosition, Vector3.zero, Time.deltaTime * slideSpeed);
                if (Vector3.Distance(slideTransform.localPosition, Vector3.zero) <= minSlideDistance || frames > 2)
                {
                    slideBeginningReached = true;
                }

                frames++;
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public void EjectCasing()
    {
        if (bulletCasingPrefab == null || ejectPointTransform == null)
        {
            return;
        }

        GameObject shell = Instantiate(bulletCasingPrefab, ejectPointTransform.position, ejectPointTransform.rotation);
        Rigidbody rb = shell.GetComponentInChildren<Rigidbody>();
        if (rb)
        {
            rb.AddRelativeForce(Vector3.right * bulletCasingForce, ForceMode.VelocityChange);
        }

        Destroy(shell, 5);
    }

    public void ApplyParticleFX(Vector3 position, Quaternion rotation, Collider attachTo)
    {
        if (!hitFxPrefab)
        {
            return;
        }

        GameObject impact = Instantiate(hitFxPrefab, position, rotation);
        BulletHole hole = impact.GetComponent<BulletHole>();
        if (hole)
        {
            hole.TryAttachTo(attachTo);
        }
    }

    private void RandomizeMuzzleFlashScaleRotation()
    {
        if (muzzleFlashObject == null)
        {
            return;
        }

        muzzleFlashObject.transform.localScale = Vector3.one * Random.Range(0.75f, 1.5f);
        muzzleFlashObject.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0, 90f));
    }
}
