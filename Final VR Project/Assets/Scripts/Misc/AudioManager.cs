using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [System.Serializable]
    public class Sound
    {
        [Tooltip("The name of the sound")]
        public string audioName;

        [Tooltip("The audio clip of the sound")]
        public AudioClip clip;

        [Tooltip("The type of the sound ")]
        public SoundType type;

        [Tooltip("The name of the sound")]
        public bool is3D;
    }

    public enum SoundType
    {
        Music,
        SoundEffect,
        UISound
    }

    public List<Sound> sounds;
    private Dictionary<string, AudioSource> audioSources;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitialiseAudioSource();
            LoadPlayerPrefs();
            AudioManager.instance.PlayAudios("Background Music", transform.position);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitialiseAudioSource()
    {
        audioSources = new Dictionary<string, AudioSource>();

        foreach (Sound sound in sounds)
        {
            GameObject soundObj = new GameObject($"AudioSource_{sound.audioName}");
            soundObj.transform.SetParent(transform);

            AudioSource source = soundObj.AddComponent<AudioSource>();
            source.clip = sound.clip;
            source.playOnAwake = false;
            source.spatialBlend = sound.is3D ? 1f : 0f;

            audioSources.Add(sound.audioName, source);
        }
    }

    private void LoadPlayerPrefs()
    {
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 0.75f));
        SetSoundEffectsVolume(PlayerPrefs.GetFloat("SoundEffectsVolume", 0.75f));
        SetUISoundsVolume(PlayerPrefs.GetFloat("UISoundsVolume", 0.75f));
    }

    public void PlayAudios(string soundName, Vector3 position = default)
    {
        if (audioSources.ContainsKey(soundName))
        {
            AudioSource source = audioSources[soundName];
            source.transform.position = position == default ? transform.position : position;
            source.Play();
        }
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        SetVolume(SoundType.Music, volume);
    }

    public void SetSoundEffectsVolume(float volume)
    {
        PlayerPrefs.SetFloat("SoundEffectsVolume", volume);
        SetVolume(SoundType.SoundEffect, volume);
    }

    public void SetUISoundsVolume(float volume)
    {
        PlayerPrefs.SetFloat("UISoundsVolume", volume);
        SetVolume(SoundType.UISound, volume);
    }

    public void PlayAudioArray(string[] audioNames, Vector3 position = default)
    {
        if (audioNames.Length > 0)
        {
            int randomindex = UnityEngine.Random.Range(0, audioNames.Length);
            string randomAudioName = audioNames[randomindex];
            PlayAudios(randomAudioName, position);
        }
    }

    private void SetVolume(SoundType type, float volume)
    {
        foreach (KeyValuePair<string, AudioSource> entry in audioSources)
        {
            if (sounds.Find(sound => sound.audioName == entry.Key).type == type)
            {
                entry.Value.volume = volume;
            }
        }
    }
}