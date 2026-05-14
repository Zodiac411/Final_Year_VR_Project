using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine;

public class HeadCollisionFade : MonoBehaviour
{
    [SerializeField] private Transform DistanceTransform;
    
    [SerializeField] private float FadeDistance = 0.1f;
    [SerializeField] private float FadeOutDistance = 0.045f;
    [SerializeField] private float MinFade = 0.5f;
    [SerializeField] private float MaxFade = 0.95f;
    [SerializeField] private float FadeSpeed = 1f;

    [SerializeField] private bool CheckOnlyIfHMDActive = false;

    bool IgnoreHeldGrabbables = true;

    public int cols = 0;
    
    private float currentFade = 0;
    private float lastFade = 0;

    private ScreenFader fader;
    public List<Collider> collisions;

    private void Start()
    {
        if (Camera.main)
        {
            fader = Camera.main.transform.GetComponent<ScreenFader>();
        }
    }

    private void LateUpdate()
    {
        bool headColliding = false;

        if (CheckOnlyIfHMDActive == false || XRInput.Instance.HMDActive)
        {
            for (int x = 0; x < collisions.Count; x++)
            {
                if (collisions[x] != null && collisions[x].enabled)
                {
                    headColliding = true;
                    break;
                }
            }
        }

        if (headColliding)
        {
            FadeDistance = Vector3.Distance(transform.position, DistanceTransform.position);
        }
        else
        {
            FadeDistance = 0;
        }

        if (fader)
        {
            if (FadeDistance > FadeOutDistance)
            {
                currentFade += Time.deltaTime * FadeSpeed;

                if (headColliding && currentFade < MinFade)
                {
                    currentFade = MinFade;
                }

                if (currentFade > MaxFade)
                {
                    currentFade = MaxFade;
                }

                if (currentFade != lastFade)
                {
                    fader.SetFadeLevel(currentFade);
                    lastFade = currentFade;
                }

            }
            else
            {
                currentFade -= Time.deltaTime * FadeSpeed;

                if (currentFade < 0)
                {
                    currentFade = 0;
                }

                if (currentFade != lastFade)
                {
                    fader.SetFadeLevel(currentFade);
                    lastFade = currentFade;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        if (collisions == null)
        {
            collisions = new List<Collider>();
        }

        bool ignorePhysics = IgnoreHeldGrabbables && col.gameObject.GetComponent<Grabbable>() != null && col.gameObject.GetComponent<Grabbable>().BeingHeld;

        if (!ignorePhysics && col.collider.GetComponent<Joint>())
        {
            ignorePhysics = true;
        }

        if (!ignorePhysics && col.gameObject.GetComponent<CharacterController>() != null)
        {
            ignorePhysics = true;
        }

        if (ignorePhysics)
        {
            Physics.IgnoreCollision(col.collider, GetComponent<Collider>(), true);
            return;
        }

        if (!collisions.Contains(col.collider))
        {
            collisions.Add(col.collider);
            cols++;
        }
    }

    private void OnCollisionExit(Collision col)
    {
        if (collisions == null)
        {
            collisions = new List<Collider>();
        }

        if (collisions.Contains(col.collider))
        {
            collisions.Remove(col.collider);
            cols--;
        }
    }
}