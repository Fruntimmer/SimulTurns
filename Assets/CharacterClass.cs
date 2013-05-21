using UnityEngine;
using System.Collections;

public class CharacterClass : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

public class Character
{
    //public Vector3 position;
    public Transform transform;
    private Node charNode;
    public Vector3 position;
    public Path path = new Path();

    private Node nextNode = null;
    private Vector3 moveDirection;
    private float speed = 1.0f;
    public Character(Vector3 position)
    {
        this.position = position;
        charNode = new Node(position.x, position.z);
    }
    public void Update()
    {
        MoveCharacter();   
    }
    public void MoveCharacter()
    {
        if(path.waypoints.Count >1)
        {
            if(nextNode == null)
            {
                nextNode = path.waypoints[0];
                moveDirection = (nextNode.position - position).normalized;
            }
            position += moveDirection*speed*Time.deltaTime;
            
            if(Vector3.Distance(position, nextNode.position) < 0.2f) //This should be based on the percantage of how far the char has come not units.
            {
                path.waypoints.Remove(nextNode);
                nextNode = null;
            }
        }
    }
    public void DrawChar()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(position, new Vector3(0.4f, 0.4f, 0.4f));
    }
}
