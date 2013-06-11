using UnityEngine;
using System.Collections;

public class WaypointEffects : MonoBehaviour {

	Vector2 textureSize = new Vector2(0.4f,0.4f);
    // Use this for initialization
	void Start () {
        renderer.material.SetTextureScale("_MainTex", textureSize);
	}
	
	// Update is called once per frame
	void Update () {
	    transform.eulerAngles += new Vector3(0,10,0)*Time.deltaTime;
    }
}
