using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class musicma : MonoBehaviour
{
    AudioSource source;
    [SerializeField] AudioClip[] clip;
    int i;
    int f;
    // Start is called before the first frame update
    void Start()
    {
        source = this.GetComponent<AudioSource>();
        source.clip = clip[0];
        source.Play();
        f = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(source.isPlaying == false)
        {
            i = Random.Range(0, clip.Length);
                        if(i == f)
            {
                i = Random.Range(0, clip.Length);
            }
            f = i;
            source.clip = clip[i];
            source.Play();
        }
    }
    void Pause()
    {
        source.Pause();
    }
    void unPause()
    {
        source.UnPause();
    }
}
