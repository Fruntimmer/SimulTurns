using UnityEngine;
using System.Collections;

public class BulletScript : MonoBehaviour
{
    private float timer = 0.0f;
	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	    timer += Time.deltaTime;
        if (timer > 0.3f)
        {
            Destroy(this.gameObject);
        }
	}
}
