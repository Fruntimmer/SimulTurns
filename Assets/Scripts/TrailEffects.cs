using UnityEngine;
using System.Collections;

public class TrailEffects : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	renderer.material.mainTextureScale = new Vector2(1, transform.localScale.z/2);
    renderer.material.mainTextureOffset += new Vector2(0, 0.35f)*Time.deltaTime;
	}
}
