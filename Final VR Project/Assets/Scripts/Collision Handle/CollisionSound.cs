using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class CollisionSound : MonoBehaviour
{
    [SerializeField] private AudioClip CollisionAudio;

    private AudioSource audioSource;
    private Collider col;
    private Grabbable grab;

    [SerializeField] private float MinimumVolume = 0.25f;
    [SerializeField] private float MaximumVolume = 1f;

    [SerializeField] private float LastRelativeVelocity = 0;

    public bool RecentlyPlayedSound = false;

    private float startTime;
    private float lastPlayedSound;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
        }

        startTime = Time.time;
        col = GetComponent<Collider>();
        grab = GetComponent<Grabbable>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - startTime < 0.2f)
        {
            return;
        }

        if (col == null || !col.enabled)
        {
            return;
        }

        float colVelocity = collision.relativeVelocity.magnitude;

        bool otherColliderPlayedSound = false;
        var colSound = collision.collider.GetComponent<CollisionSound>();
        if (colSound)
        {
            otherColliderPlayedSound = colSound.RecentlyPlayedSound && colSound.CollisionAudio == CollisionAudio;
        }

        float soundVolume = Mathf.Clamp(collision.relativeVelocity.magnitude / 10, MinimumVolume, MaximumVolume);
        bool minVelReached = colVelocity > 0.1f;

        if (!minVelReached && grab != null && grab.BeingHeld)
        {
            minVelReached = true;
            soundVolume = 0.1f;
        }

        bool audioValid = audioSource != null && CollisionAudio != null;

        if (minVelReached && audioValid && !otherColliderPlayedSound)
        {
            LastRelativeVelocity = colVelocity;

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.clip = CollisionAudio;
            audioSource.pitch = Time.timeScale;
            audioSource.volume = soundVolume;
            audioSource.Play();

            RecentlyPlayedSound = true;

            Invoke("resetLastPlayedSound", 0.1f);
        }
    }

    void resetLastPlayedSound()
    {
        RecentlyPlayedSound = false;
    }
}