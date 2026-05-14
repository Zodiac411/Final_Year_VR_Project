using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine;

public class BulletInsert : MonoBehaviour
{
    [SerializeField] private RaycastWeapon Weapon;
    [SerializeField] private AudioClip InsertSound;
    [SerializeField] private Renderer insertRenderer;
    [SerializeField] private float flashDuration = .5f;

    private Grabbable grabbableComponent;

    public string AcceptBulletName = "Bullet";
    private Coroutine flashRoutine = null;

    private void Awake()
    {
        grabbableComponent = Weapon.GetComponent<Grabbable>();

        if (insertRenderer != null)
        {
            insertRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (grabbableComponent != null && grabbableComponent.BeingHeld &&
            Weapon.GetBulletCount() < Weapon.MaxInternalAmmo &&
            flashRoutine == null)
        {
            StartFlashing();
        }
        else if (grabbableComponent != null &&
                 (Weapon.GetBulletCount() >= Weapon.MaxInternalAmmo || !grabbableComponent.BeingHeld) &&
                 flashRoutine != null)
        {
            StopFlashing();
        }
    }

    private void UpdateFlashingStatus()
    {
        if (grabbableComponent != null && Weapon != null)
        {
            if (grabbableComponent.BeingHeld &&
                Weapon.GetBulletCount() < Weapon.MaxInternalAmmo &&
                flashRoutine == null)
            {
                StartFlashing();
            }
            else if ((!grabbableComponent.BeingHeld || Weapon.GetBulletCount() >= Weapon.MaxInternalAmmo) &&
                    flashRoutine != null)
            {
                StopFlashing();
            }
        }
        else
        {
            Debug.LogError("UpdateFlashingStatus: grabbableComponent or Weapon is not assigned.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Grabbable grab = other.GetComponent<Grabbable>();
        if (grab != null)
        {
            if (grab.transform.name.Contains(AcceptBulletName))
            {

                if (Weapon.GetBulletCount() >= Weapon.MaxInternalAmmo)
                {
                    return;
                }

                grab.DropItem(false, true);
                grab.transform.parent = null;
                GameObject.Destroy(grab.gameObject);

                GameObject b = new GameObject();
                b.AddComponent<Bullet>();
                b.transform.parent = Weapon.transform;

                if (InsertSound)
                {
                    XRManager.Instance.PlaySpatialClipAt(InsertSound, transform.position, 1f, 0.5f);
                }

                UpdateFlashingStatus();
            }
        }
    }

    private void StartFlashing()
    {
        flashRoutine = StartCoroutine(FlashIndicator());
    }

    private void StopFlashing()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        if (insertRenderer != null)
        {
            insertRenderer.enabled = false;
        }
    }

    private IEnumerator FlashIndicator()
    {
        while (true)
        {
            insertRenderer.enabled = !insertRenderer.enabled;
            yield return new WaitForSeconds(flashDuration);
        }
    }
}

