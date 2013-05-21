using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AStarClasses : MonoBehaviour 
{

}

public class AStarMap
{
    //public List<Cell> gridMap = new List<Cell>(); 
    private int columnLength;
    private int rowLength;
    private Node goalNode;
    private Node startNode;

    private List<Node> openList = new List<Node>();
    private List<Node> closedList = new List<Node>();
    private List<Node> allNodes = new List<Node>();
    public List<Node> result = new List<Node>();

    private bool foundPath = false;
    public AStarMap(List<Node> map , Node goalNode, Node startNode)
    {
        allNodes.AddRange(map);
        openList .Add(startNode);
        this.goalNode = goalNode;
        this.startNode = startNode;
        CalculateAllHeuristics(goalNode);
        PopulateClosedList();
    }

    public void AStarPath()
    {
        Node activeNode = new Node();
        Node previousNode = new Node();
        while(!foundPath)
        {

            if (activeNode == goalNode)
            {
                LightPath(activeNode);
                foundPath = true;
            }
            else
            {
                if (activeNode == null)
                    activeNode = startNode;
                else
                {
                    previousNode = activeNode;
                    activeNode = ChooseNextNode(activeNode);
                }
                RemoveFromOpen(previousNode);
                AddAdjacent(activeNode);
            }
        }
    }
    void PopulateClosedList()
    {
        foreach (Node node in closedList)
        {
            if(node.closed)
            {
                closedList.Add(node);
            }
        }
    }
    void LightPath(Node node)
    {
        result.Add(startNode);
        while(node != null)
        {
            result.Add(node);
            node.Select();
            node = node.parent;
        }
        result.Add(startNode);
        result.Reverse();
    }
    void CalculateAllHeuristics(Node goalNode)
    {
        foreach (Node node in allNodes)
        {
            node.CalculateHeuristic(goalNode);
        }
    }
    Node ChooseNextNode(Node activeNode)
    {
        float lowestF = 1000;
        Node nextNode = null;
        foreach(Node node in openList)
        {
            if(nextNode == null)
            {
                nextNode = node;
                lowestF = node.CalculateF();
            }
            else if(node.CalculateF()<lowestF)
            {
                nextNode = node;
                lowestF = node.CalculateF();
            }
        }
        return nextNode;
    }
    private void AddAdjacent(Node activeNode)
    {
        foreach (Node node in activeNode.neighbors)
        {
            if(!closedList.Contains(node))
            {
                if(openList.Contains(node))
                {
                    if(node.g > activeNode.g + Vector3.Distance(activeNode.position, node.position))
                    {
                        node.parent = activeNode;
                        node.g = activeNode.g + Vector3.Distance(activeNode.position, node.position);
                        node.CalculateF();
                    }
                }
                openList.Add(node);
                node.parent = activeNode;
            }
        }
    }
    private void RemoveFromOpen(Node node)
    {
        openList.Remove(node);
        closedList.Add(node);
    }
}
    
public class Node
{
    public float X, Y;
    public Vector3 position;
    public List<Node> neighbors = new List<Node>();
    public Node parent = null;
    public bool closed = false;

    private float h;
    public float g = 0;
    private float f;

    public bool selected = false;
    public bool selectedNeighbor = false;
    public Node()
    {
    }
    public Node(float x, float y)
    {
        this.X = x;
        this.Y = y;
        this.position = new Vector3(x,0,y);
    }
    
    public void AddNeighbor(Node node)
    {
        if(!neighbors.Contains(node))
        {
            neighbors.Add(node);
        }
        if(!node.neighbors.Contains(this))
        {
            node.AddNeighbor(this);
        }
    }

    public void CalculateHeuristic(Node goal)
    {
        h = Mathf.Abs(this.X - goal.X) + Mathf.Abs(this.Y - goal.Y);
    }

    public void CalculateG(Node fromNode)
    {
        g = Vector3.Distance(fromNode.position, this.position) +fromNode.g;
    }
    public float CalculateF()
    {
        f = g + h;
        return f;
    }
    public void Select()
    {
        selected = true;
        if(this.parent!=null)
            Debug.DrawLine(this.position, this.parent.position, Color.red, 5.0f);
        //SelectedNeighbor();
        PrintNode();
    }
    public void Deselect()
    {
        selected = false;
        DeselectNeighbors();
    }
    public void SelectedNeighbor()
    {
        foreach (Node node in neighbors)
        {
            node.selectedNeighbor = true;
        }
    }
    public void DeselectNeighbors()
    {
        foreach (Node node in neighbors)
        {
            node.selectedNeighbor = false;
        }
    }
    public void PrintNode()
    {
        Debug.Log("g="+g);
        Debug.Log("h="+h);
    }
    public void DisplayNode()
    {
        if (selected)
        {
            Gizmos.color = Color.yellow;
        }
        else if (selectedNeighbor)
        {
            Gizmos.color = Color.magenta;
        }
        else
        {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawWireSphere(position, 0.1f);
    }
}

public class Path
{
    public List<Node> waypoints;
    
    public Path()
    {
        this.waypoints = new List<Node>();
    }
    public Path(List<Node> waypoints)
    {
        this.waypoints = waypoints;
    }
    
    public void DisplayPath()
    {
        for(int i = 0; i < waypoints.Count-1; i++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(waypoints[i].position, .2f);
            if(i+1 < waypoints.Count - 1)
            Debug.DrawLine(waypoints[i].position, waypoints[i+1].position);
        }
    }

    public void OptimizePath()
    {
        for(int i = 1; i < waypoints.Count - 2; i++)
        {
            //if(Vector3.Angle(waypoints[i].position, waypoints[i-1].parent.position) == Vector3.Angle(waypoints[i+1].position, waypoints[i].parent.position))
            if((waypoints[i-1].position - waypoints[i].position).normalized == (waypoints[i].position - waypoints[i+1].position).normalized)
            {  
                waypoints.Remove(waypoints[i]);
                i--;
            }
        }
    }
}

