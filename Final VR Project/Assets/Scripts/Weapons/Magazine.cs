using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class Magazine : MonoBehaviour
{
    private Grabbable grabbable;
    private float destructionCooldown = 10f;
    private float timeSinceUngrabbed = 0f;
    private bool startCountdown = false;

    private void Awake()
    {
        grabbable = GetComponent<Grabbable>();
    }

    private void Update()
    {
        if (grabbable.BeingHeld || HasAnyBullets())
        {
            startCountdown = false;
            timeSinceUngrabbed = 0f;
        }
        else if (!startCountdown)
        {
            startCountdown = true;
        }

        if (startCountdown && GetComponent<FixedJoint>() == null)
        {
            timeSinceUngrabbed += Time.deltaTime;

            if (timeSinceUngrabbed >= destructionCooldown)
            {
                Destroy(gameObject);
            }
        }
    }

    private bool HasAnyBullets()
    {
        Bullet[] bullets = GetComponentsInChildren<Bullet>();
        return bullets.Length > 0;
    }
}
