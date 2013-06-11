using UnityEngine;
using System.Collections;

public class WallTransparency : MonoBehaviour
{
    private Color defaultColor;
    // Use this for initialization
	void Start ()
	{
	    defaultColor = renderer.material.color;
	}
	
	// Update is called once per frame
	void Update () 
    {
    
	}
    void OnMouseOver()
    {
        renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 0.5f);
    }
    void OnMouseExit()
    {
        renderer.material.color = defaultColor;
    }
}
