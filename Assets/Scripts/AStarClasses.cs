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
    private Path result = new Path();

    private List<Node> openList = new List<Node>();
    private List<Node> closedList = new List<Node>();
    private List<Node> allNodes = new List<Node>();
    //public List<Waypoint> result = new List<Waypoint>();

    private bool foundPath = false;
    public AStarMap(List<Node> map , Node goalNode, Node startNode)
    {
        allNodes = new List<Node>(map);
        openList.Add(startNode);
        this.goalNode = goalNode;
        this.startNode = startNode;
        //CalculateAllHeuristics(goalNode);
        PopulateClosedList();
    }

    public Path AStarPath()
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
                {
                    activeNode = startNode;
                    CalculateAllHeuristics(goalNode);
                    RemoveFromOpen(activeNode);
                }
                previousNode = activeNode;
                activeNode = ChooseNextNode(activeNode);
                RemoveFromOpen(previousNode);
                AddAdjacent(activeNode);
                
                if (openList.Count == 0 && activeNode != goalNode)
                    break;
            }
        }
        return result;
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
        do
        {
            Waypoint wp = new Waypoint(node.position);
            result.waypoints.Add(wp);
            
            node = node.parent;
        } while (node != null);
        result.waypoints.Reverse();
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
            if(node == goalNode)
            {
                nextNode = node;
                return nextNode;
            }
                
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
                        //node.g = activeNode.g + Vector3.Distance(activeNode.position, node.position);
                        node.CalculateG(activeNode);
                        node.CalculateF();
                    }
                }
                else
                {
                    node.CalculateG(activeNode);
                    node.parent = activeNode;
                    openList.Add(node);
                }
                
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
    public Node(float x, float y, int gridScale)
    {
        this.X = x;
        this.Y = y;
        this.position = new Vector3(x/gridScale,0,y/gridScale);
    }
    
    public void PurgeNode()
    {
        h = 0;
        g = 0;
        f = 0;
        parent = null;
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
        //PrintNode();
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
        Debug.Log(position);
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

