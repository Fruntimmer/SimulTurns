using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour {
	// Use this for initialization
    public Node[,] map;

    private Node startNode = null;
    private Node goalNode = null;
    
    private List<Path> allPaths = new List<Path>();
    private List<Character> allChars = new List<Character>();
    private List<Character> charsToBeKilled = new List<Character>();

    private Vector2 gridSize = new Vector2(30,30);
    private int gridScale = 3;
    private float snapPoint = 0.3f;
    private List<Node> nodeList = new List<Node>();

    private Character selectedChar = null;
    private Waypoint selectedWaypoint = null;
    private bool pathAltered = false;
    private Waypoint ghostPoint;

    private string gameMode = "paused";
    private float turnLength = 5.0f;
    private float turnTimer = 0.0f;
    private int characterCycle = 0; //Used to tabcycle between characters.

    void Start ()
    {
        GameSetup();
    }
	
	// Update is called once per frame
	void Update ()
	{
	    RaycastHit hit;
        Ray ray = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);
	    int groundLayerMask = 1 << 8;

        //Cycles characters
        if (gameMode == "paused")
        {
            if(Input.GetKeyDown(KeyCode.Backspace) && selectedChar != null && selectedChar.path.entryCount.Count>=1)
            {
                selectedChar.path.UndoPath();
            }
            
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CycleCharacters();
            }
            if (Input.GetKeyDown(KeyCode.Delete) && selectedWaypoint != null)
            {
                //This doesnt really work. Not entirely sure why. Also need to check that the new path doesnt go through walls before deleting.
                selectedChar.path.waypoints.Remove(selectedWaypoint);
                selectedChar.path.CreatePathMesh();
                selectedWaypoint = null;
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
                {
                    //First priority: Waypoints
                    if (selectedChar != null && selectedChar.path.waypoints.Count >0 && FindClosestWaypoint(hit.point) != null)
                    {
                        //This selects a waypoint, the selection is done in the function run in the if statement
                        if(selectedWaypoint != selectedChar.path.waypoints[0])
                            gameMode = "drag";
                    }
                    //Second priority paths
                    else if (hit.collider.tag == "path")
                    {
                        PathClick(hit);
                    }
                    else
                    {
                        Character temp = FindClosestCharacter(hit.point);
                        if (temp == null)
                        {
                            SetCharWaypoint(hit.point);
                        }
                        else
                        {
                            
                            //Last priority characters
                            SelectCharacter(hit.point);
                        }
                    }
                }
            }
            int charLayerMask = 1 << 10;
            if(Input.GetMouseButtonDown(1) && selectedChar != null)
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask) && FindClosestWaypoint(hit.point) != null)
                { 
                    gameMode = "aim";
                }
            }
            if (Input.GetButtonDown("Jump"))
            {
                StartTurn();
            }
            //Crap buttons, these should have GUI buttons
            if(Input.GetKeyDown(KeyCode.Keypad1) && selectedWaypoint != null)
            {
                Debug.Log("run!");
                Instruction newInstruction = new Instruction();
                newInstruction.run = true;
                selectedWaypoint.wpInstructions.Add(newInstruction);
                //selectedWaypoint.run = true;
            }
            if(Input.GetKeyDown(KeyCode.X) && selectedChar != null)
            {
                foreach(Waypoint wp in selectedChar.path.waypoints)
                {
                    Debug.Log(wp.position);
                }
            }
        }
	    //Game modes
        if (gameMode == "active")
        {
            turnTimer += Time.deltaTime;
            UpdateCharacters();
            if(charsToBeKilled.Count>0)
            {
                EmptyKillList();
            }
            if (turnTimer >= turnLength)
                EndTurn();
        }

        if (gameMode == "aim" && selectedWaypoint != null)
        {
            if (Input.GetMouseButton(1))
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
                {

                    Debug.DrawRay(selectedWaypoint.position, (GetMousePosition()-selectedWaypoint.position).normalized, Color.red);
                    //selectedWaypoint.AdjustAim(hit.point);
                }
            }
            if (Input.GetMouseButtonUp(1))
            {
                Instruction newInstruction = new Instruction();
                newInstruction.AdjustAim(GetMousePosition(), selectedWaypoint.position); 
                selectedWaypoint.wpInstructions.Add(newInstruction);
                gameMode = "paused";
            }
        }
        if(gameMode == "drag")
        {
            if (Input.GetMouseButton(0) && selectedWaypoint != null)
            {
                Vector3 mousePos = GetMousePosition();
                if (Vector3.Distance(mousePos, selectedWaypoint.position) < 1.0f)
                {
                    if (ghostPoint == null)
                    {
                        ghostPoint = new Waypoint(selectedWaypoint.position);
                    }
                    selectedWaypoint.position = mousePos;
                    selectedWaypoint.wpMesh.transform.position = selectedWaypoint.position;
                    pathAltered = true;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (pathAltered)
                {
                    if (selectedChar.path.CheckWaypointReachable(selectedWaypoint))
                    {
                        selectedChar.path.CreatePathMesh();
                        selectedWaypoint.wpMesh.transform.position = selectedWaypoint.position;
                        pathAltered = false;
                    }
                    else
                    {
                        Debug.Log("bam!");
                        selectedWaypoint.position = ghostPoint.position;
                        selectedWaypoint.wpMesh.transform.position = ghostPoint.position;
                    }
                    ghostPoint = null;
                }
                gameMode = "paused";
            }
        }
	}

    private void StartTurn()
    {
        foreach (Character character in allChars)
        {
            character.NextWaypoint();
        }
        gameMode = "active";
        turnTimer = 0.0f;
        selectedChar = null;
        selectedWaypoint = null;
    }

    private void EndTurn()
    {
        gameMode = "paused";
        foreach (Character character in allChars)
        {
            character.AddCharacterWaypoint();
            character.path.CreatePathMesh();
        }
    }

    private void PathClick(RaycastHit hit)
    {
        GameObject obj = hit.collider.transform.parent.gameObject;
        Vector3 closestP = ClosestPointOnLine(obj.transform.position,
                                              obj.transform.position +
                                              (hit.collider.transform.forward*obj.transform.localScale.z),
                                              hit.point);

        selectedChar.path.InsertWaypoint(closestP, hit.collider.transform.parent.position);
        selectedChar.path.CreatePathMesh();
    }

    private void CycleCharacters()
    {
        characterCycle += 1;
        if (characterCycle >= allChars.Count)
            characterCycle = 0;
        selectedChar = allChars[characterCycle];
    }

    void UpdateCharacters()
    {
        if (allChars.Count >= 1)
        {
            foreach (Character character in allChars)
            {
                if(!character.isDead)
                    character.Update();
            }
        }
    }

    void SetCharWaypoint(Vector3 position)
    {
        if(selectedChar.path.waypoints.Count <1)
            startNode = FindClosestNode(selectedChar.position);
        else
        {
            startNode = FindClosestNode(selectedChar.path.waypoints[selectedChar.path.waypoints.Count-1].position);
        }
        goalNode = FindClosestNode(position);
        
        if (goalNode != null && startNode != null && startNode != goalNode)
        {
            AStarMap astar = new AStarMap(nodeList, goalNode, startNode);
            Path newPath = astar.AStarPath();
            newPath.OptimizePath();
            
            selectedChar.AddWaypoints(newPath.waypoints);
            selectedChar.path.CreatePathMesh();
            PurgeAllNodes();
        }
        else
        {
            Debug.Log("start and goal node is probably the same");
        }

        startNode = null;
        goalNode = null;
    }
    private void PurgeAllNodes()
    {
        foreach (Node node in map)
        {
            if(node != null)
                node.PurgeNode();
        }
    }
    private void SelectCharacter(Vector3 position)
    {
        selectedChar = FindClosestCharacter(position);
        if(selectedChar != null)
        {
            Debug.Log("Selected Character");
        }
        else
        {
            selectedChar = null;
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
                Node newNode = new Node(column, row, gridScale);
                if (Physics.Raycast(new Vector3((float)column/gridScale,3,(float)row/gridScale), -Vector3.up, out hit, 4, wallLayerMask))
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
    Waypoint FindClosestWaypoint(Vector3 position)
    {
        Waypoint closestWaypoint = null;
        foreach (Waypoint waypoint in selectedChar.path.waypoints)
        {
            if (Vector3.Distance(position, waypoint.position) < snapPoint)
            {
                closestWaypoint = waypoint;
                selectedWaypoint = waypoint;
                return closestWaypoint;
            }
        }
        closestWaypoint = null;
        return null;
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
        Node closestNode = null;
        foreach (Node node in map)
        {
            if (node != null)
            {
                if (Vector3.Distance(position, node.position) < snapPoint)
                {
                    closestNode = node;
                    return closestNode;
                }
            }
        }
        return null;
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

    Vector3 GetMousePosition()
    {
        RaycastHit hit;
        Ray ray = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);
        int groundLayerMask = 1 << 8;
        if(Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
        {
            return hit.point;
        }
        return Vector3.zero;
    }
    void OnGUI()
    {
        if(selectedWaypoint != null)
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(selectedWaypoint.position);
            screenPos = new Vector2(screenPos.x,Screen.height - screenPos.y);
            if(GUI.Button(new Rect(screenPos.x-60, screenPos.y-50,35,20),"Wait" ))
            {
                Instruction newInstruction = new Instruction();
                newInstruction.waitDuration = 2.0f;
                selectedWaypoint.wpInstructions.Add(newInstruction);
                Debug.Log(selectedWaypoint.wpInstructions.Count);
                //selectedWaypoint.waitDuration = 2.0f;
                Debug.Log("waiting 2 sec");
            }
            if(GUI.Button(new Rect(screenPos.x-25, screenPos.y-50,35,20), "Run"))
            {
                
            }
        }
    }
    void OnDrawGizmos()
    {
        /*
       //DISPLAYS PATHFINDING NODES.
       if (map != null)
       {
           foreach (Node node in map)
           {
               if(node != null)
                   node.DisplayNode();
           }
       }

       if (allChars.Count >= 1)
       {
           foreach (Character character in allChars)
           {
               //character.DrawChar();
               if(character.path.waypoints.Count>=1)
               {
                   character.path.DisplayPath();
               }
           }
       }
        */
    }
    private void SpawnCharacter(Vector3 position, string team)
    {
        Character newChar = new Character(position, team);
        allChars.Add(newChar);
    }
    private void AddToKillList(Character character)
    {
        charsToBeKilled.Add(character);
    }
    private void EmptyKillList()
    {
        foreach (Character character in charsToBeKilled)
        {
            allChars.Remove(character);
            Destroy(character.charGeo);
        }
    }
    private void GameSetup()
    {
        GenerateGrid(gridSize);
        foreach (Node node in map)
        {
            if (node != null)
                nodeList.Add(node);
        }
        SpawnCharacter(new Vector3(9, 0, 9), "team1");
        SpawnCharacter(new Vector3(9, 0, 8), "team1");
        SpawnCharacter(new Vector3(8, 0, 9), "team1");
        
        SpawnCharacter(new Vector3(0, 0, 0), "team2");
        SpawnCharacter(new Vector3(1, 0, 0), "team2");
        SpawnCharacter(new Vector3(0, 0, 1), "team2");
    }
}
