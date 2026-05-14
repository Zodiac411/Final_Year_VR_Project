using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDism : MonoBehaviour
{
    Animator anim;
    List<Rigidbody> rigidbodiesInRagdoll;
    void Start()
    {
        anim = GetComponent<Animator>();
        rigidbodiesInRagdoll = new List<Rigidbody>(transform.GetComponentsInChildren<Rigidbody>());
        rigidbodiesInRagdoll.Remove(GetComponent<Rigidbody>()); 
        EndRagdoll();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartRagdoll()
    {
        anim.enabled = false;
        anim.enabled = false;
        GetComponent<NavMeshAgent>().enabled = false; 
        GetComponent<ZombieAI>().enabled = false;
        for (int i = 0; i < rigidbodiesInRagdoll.Count ; i++)
        {
            rigidbodiesInRagdoll[i].useGravity = true;
            rigidbodiesInRagdoll[i].isKinematic = false ;
        }
    }

    private void EndRagdoll()
    {
        anim.enabled = true;
        for (int i = 0; i < rigidbodiesInRagdoll.Count; i++)
        {
            rigidbodiesInRagdoll[i].useGravity = false;
            rigidbodiesInRagdoll[i].isKinematic = true;
        }
    }



}
