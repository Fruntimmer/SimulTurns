using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour {
	// Use this for initialization
    public Node[,] map;

    private Node startNode = null;
    private List<Path> allPaths = new List<Path>();
    private List<Character> allChars = new List<Character>();

    private bool displayGrid = false;
    private Vector2 gridSize = new Vector2(10,10);
    private Node selectedNode;
    private float snapPoint = 0.2f; //How close to node to be able to select it
    private List<Node> nodeList = new List<Node>();

    private bool isCharSelected = false;
    private Character selectedChar = null;

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
        
        if(allChars.Count >= 1)
        {
            foreach (Character character in allChars)
            {
                character.Update();
            }
        }
	}

    private void SetCharWaypoint(Vector3 position)
    {
        if (startNode == null)
        {
            startNode = FindClosestNode(selectedChar.position);
        }
        Node goalNode = FindClosestNode(position);
        AStarMap astar = new AStarMap(nodeList, goalNode, startNode);
        astar.AStarPath();

        Path path = new Path(astar.result);
        allPaths.Add(path);
        path.OptimizePath();

        //Character newChar = new Character(startNode.position);
        selectedChar.path = path;
        //allChars.Add(newChar);

        startNode = null;
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
