using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour {
	// Use this for initialization
    public Node[,] map;

    private Node startNode = null;
    private Node dragNode = null;
    private List<Path> allPaths = new List<Path>();
    private List<Character> allChars = new List<Character>();

    private bool displayGrid = false;
    private Vector2 gridSize = new Vector2(10,10);
    private Node selectedNode;
    private float snapPoint = 0.2f; //How close to node to be able to select it
    private List<Node> nodeList = new List<Node>();

    private bool isCharSelected = false;
    private Character selectedChar = null;

    private string gameMode = "paused";
    private float turnLength = 5.0f;
    private float turnTimer = 0.0f;

    void Start () 
    {
	    GenerateGrid(gridSize);
        foreach (Node node in map)
        {
            if(node != null)
                nodeList.Add(node);
        }
        Character newChar = new Character(Vector3.zero);
        allChars.Add(newChar);
        //AStarMap astar = new AStarMap(nodeList, map[7,3], map[0,0]);
        //astar.AStarPath();
    }
	
	// Update is called once per frame
	void Update ()
	{
	    RaycastHit hit;
        Ray ray = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);
	    const int groundLayerMask = 1 << 8;
        
        if(Input.GetMouseButtonDown(0))
	    {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
            {
                if (isCharSelected)
                {
                    SetCharWaypoint(hit.point);
                }
                else
                {
                    SelectCharacter(hit.point);
                }
            }
	    }
        if (Input.GetButtonDown("Jump"))
        {
            gameMode = "active";
            turnTimer = 0.0f;
        }

        if (gameMode == "active")
        {
            turnTimer += Time.deltaTime;
            UpdateCharacters();
            if (turnTimer >= turnLength)
                gameMode = "pause";
        }
	}
    void UpdateCharacters()
    {
        if (allChars.Count >= 1)
        {
            foreach (Character character in allChars)
            {
                character.Update();
            }
        }
    }
    void SetCharWaypoint(Vector3 position)
    {
        if (startNode == null)
        {
            startNode = FindClosestNode(selectedChar.position);
        }
        Node goalNode = FindClosestNode(position);
        if (goalNode != null)
        {
            AStarMap astar = new AStarMap(nodeList, goalNode, startNode);
            astar.AStarPath();

            Path path = new Path(astar.result);
            allPaths.Add(path);
            path.OptimizePath();
            selectedChar.path = path;
        }
        else
        {
            if (selectedChar.path.waypoints.Count >= 1)
            {
                for (int i = 0; i < selectedChar.path.waypoints.Count - 2; i++)
                {
                    Vector3 closestPoint = ClosestPointOnLine(selectedChar.path.waypoints[i].position, selectedChar.path.waypoints[i + 1].position, position);
                    if (Vector3.Distance(closestPoint, position) < 0.5f)
                    {
                        Node newNode = new Node(closestPoint.x, closestPoint.z);
                        nodeList.Add(newNode);
                        break;
                    }
                }
            }
        }

       startNode = null;
       goalNode = null;
    }

    private void SelectCharacter(Vector3 position)
    {
        selectedChar = FindClosestCharacter(position);
        if(selectedChar != null)
        {
            isCharSelected = true;
        }
        else
        {
            isCharSelected = false;
        }
    }

    void GenerateGrid(Vector2 size)
    {
        map = new Node[(int)size.x, (int)size.y];
        int wallLayerMask = 1 << 9;
        RaycastHit hit;
        for (int column = 0; column < size.x; column++)
        {
            for (int row = 0; row < size.y; row++)
            {
                Node newNode = new Node(column, row);
                if (Physics.Raycast(new Vector3(column,2,row), -Vector3.up, out hit, 3, wallLayerMask))
                {
                    newNode = null;
                }
                else
                {
                    if (column >= 1)
                    {
                        if (map[column - 1, row] != null)
                        {
                            newNode.AddNeighbor(map[column - 1, row]);
                        }
                    }
                    if (row >= 1)
                    {
                        if (map[column, row - 1] != null)
                        {
                            newNode.AddNeighbor(map[column, row - 1]);
                        }
                    }
                    if(column >=1 && row >= 1)
                    {
                        if(map[column-1, row -1] != null)
                        {
                            newNode.AddNeighbor(map[column -1, row - 1]);
                        }
                        if (row < size.y - 1 && map[column - 1, row + 1] != null)
                        {
                            newNode.AddNeighbor(map[column - 1, row + 1]);
                        }
                         
                    }
                    map[column, row] = newNode;
                }
            }
        }
    }
    
    Character FindClosestCharacter(Vector3 position)
    {
        Character closestChar = null;
        foreach (Character character in allChars)
        {
            if (Vector3.Distance(position, character.position) < snapPoint)
            {
                closestChar = character;
                return closestChar;
            }      
        }
        return null;
    }
    
    Node FindClosestNode(Vector3 position)
    { 
        Node closestNode = new Node();
        foreach (Node node in map)
        {
            if (node != null)
            {
                if (Vector3.Distance(position, node.position) < snapPoint)
                {
                    closestNode = node;
                    break;
                }
            }
        }
        return closestNode;
    }

    Vector3 ClosestPointOnLine(Vector3 p1, Vector3 p2, Vector3 target_point)
    {
        Vector3 vVector1 = target_point - p1;
        Vector3 vVector2 = (p2 - p1).normalized;

        float d = Vector3.Distance(p1, p2);
        float t = Vector3.Dot(vVector2, vVector1);

        if (t <= 0)
        {
            return p1;
        }

        if (t >= d)
        {
            return p2;
        }

        Vector3 vVector3 = vVector2 * t;
        Vector3 vClosestPoint = p1 + vVector3;
        return vClosestPoint;
    }

    void OnDrawGizmos()
    {
        if (map != null)
        {
            foreach (Node node in map)
            {
                if(node != null)
                    node.DisplayNode();
            }
        }
        
        if (allPaths.Count > 0)
        {
            foreach (Path path in allPaths)
            {
                path.DisplayPath();
            }
        }
        if (allChars.Count >= 1)
        {
            foreach (Character character in allChars)
            {
                character.DrawChar();
            }
        }
    }
}
