using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugshoot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if(Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))
            {
                if (hit.transform.GetComponent<BodyPart>())
                {
                    Debug.DrawRay(transform.position, transform.forward);
                    BodyPart part = hit.transform.GetComponent<BodyPart>();
                    part.hitBodyPart();
                }
            }
        }
    }
}
