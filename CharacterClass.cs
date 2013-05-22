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
    public Vector3 position;
    public Path path = new Path();

    private Node charNode;
    private Node nextNode = null;
    private Vector3 moveDirection;
    private Vector3 goingFromPos;
    private float distToNextNode;
    private float speed = 1.0f;

    private string activeMode = "pause";
    
    public Character(Vector3 position)
    {
        this.position = position;
        charNode = new Node(position.x, position.z);
    }
    public void Update()
    {
        //if(activeMode == "move")
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
                distToNextNode = Vector3.Distance(position, nextNode.position);
                goingFromPos = position;
            }
            position += moveDirection*speed*Time.deltaTime;
            if(distToNextNode-Vector3.Distance(position, goingFromPos) <= 0)
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
