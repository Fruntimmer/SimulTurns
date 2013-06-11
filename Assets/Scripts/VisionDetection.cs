using UnityEngine;
using System.Collections;

public class VisionDetection : MonoBehaviour {

	// Use this for initialization
    //private bool isInVision = false;
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerStay(Collider other)
    {
        if(other.collider.tag == "character")
        {
            RaycastHit hit;
            Vector3 dir = (other.collider.transform.position - transform.parent.position).normalized;
            int wallCharLayerMask = 1 << 9 | 1 << 10;
            float dist = Vector3.Distance(transform.parent.position, other.collider.transform.position);
            if(Physics.Raycast(transform.parent.position, dir, out hit, dist, wallCharLayerMask))
            {
                if(hit.collider.tag == "character")
                {
                    CharacterPointer poop = (CharacterPointer)hit.collider.gameObject.GetComponent(typeof(CharacterPointer));
                    SendMessageUpwards("EnemySighted", poop.character);
                }
            }
            else
            {
                SendMessageUpwards("EnemyOutOfSight");
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if(other.collider.tag == "character")
        SendMessageUpwards("EnemyOutOfSight");
    }
}
