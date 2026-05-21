using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class NewBehaviourScript : MonoBehaviour
{
    AudioSource source;
    [SerializeField] AudioClip[] track;
    int i;
    //Start is called before the first frame update
    void Start()
    {
       source = gameObject.GetComponent<AudioSource>();
       
       source.clip = track[Random.Range(0, track.Length)];
       source.PlayDelayed(Random.Range(15, 100));
    }

    // Update is called once per frame

 
    void Sound()
    {
    
    }
}
