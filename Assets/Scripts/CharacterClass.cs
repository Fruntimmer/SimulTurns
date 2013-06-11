using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterClass : MonoBehaviour {
    public GameObject collisionMesh;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

public class Weapon
{
    public float fireRate;
    public int roundsPerBurst;
    public int damage;
    public int maxRounds;
    public int currentAmmo;
    public float fireTimer = 0.0f;
    public float spread;

    private GameObject pBullet;
    public Weapon(string type)
    {
        if(type == "rifle")
        {
            fireRate = .12f;
            roundsPerBurst = 1;
            damage = 1;
            spread = 0.4f;
            maxRounds = 30;
            currentAmmo = maxRounds;
            pBullet = (GameObject)Resources.Load("BulletPrefab");
        }
        if (type == "shotgun")
        {
            fireRate = 2f;
            roundsPerBurst = 10;
            damage = 1;
            maxRounds = 20;
            currentAmmo = maxRounds;
            spread = 1.8f;
        }
    }
    public void FireWeapon(Vector3 position, Character enemy)
    {
        for (int i = 0; i < roundsPerBurst; i++)
        {
            Vector3 randomDir = new Vector3(Random.Range(spread*-1, spread), Random.Range(spread*-1, spread),
                                         Random.Range(spread*-1, spread));
            currentAmmo -= 1;
            GameObject bullet = (GameObject) UnityEngine.Object.Instantiate(pBullet, position+(Vector3.up*0.3f), Quaternion.identity);
            bullet.transform.LookAt(enemy.position+randomDir);
            bullet.rigidbody.AddForce(bullet.transform.forward*10000);
        }
        fireTimer = 0.0f;
    }
}

public class Character
{
    //public Vector3 position;
    public Transform transform;
    public Vector3 position;
    public Path path = new Path();
    public Node charNode = new Node();
    public bool isDead = false;

    public bool lockAim = false;
    public bool engageOnSight = true;
    public GameObject charGeo;

    public float health = 10.0f;
    
    private Waypoint nextWaypoint = null;
    private Vector3 moveDirection;
    private Vector3 goingFromPos;
    private float distToNextNode;

    private float activeSpeed;
    private float walkSpeed = 1.0f;
    private float runSpeed = 2.0f;

    private string activeMode = "move";
    private Character enemy;

    private Weapon weapon;

    //public bool wait = false;
    private float waitTimer = 0.0f;
    private float waitMax = 0.0f;

    public string team;

    //List of instructions given by the waypoint.
    public List<Instruction> charInstructions = new List<Instruction>();
    
    public Character(Vector3 position, string team)
    {
        this.position = position;
        weapon = new Weapon("rifle");
        charGeo = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("CharPrefab"));
        ((CharacterPointer) charGeo.GetComponent<CharacterPointer>()).character = this;
        charGeo.transform.position = position;
        this.team = team;
        if(team == "team2")
        {
            charGeo.renderer.material.color = new Color(150,0,0,0.3f);
        }
        //Sets default speed
        activeSpeed = walkSpeed;
        //Sets a waypoint beneath character.
        AddCharacterWaypoint();
    }
    public void Update()
    {
        if(activeMode == "move")
            MoveCharacter();
        charGeo.transform.position = position;
        if(activeMode == "fire")
        {
            Fire();
        }
        if(activeMode == "wait")
        {
            Wait();
        }
        if(charInstructions.Count > 0 && charInstructions[0].isDone)
        {
            charInstructions.RemoveAt(0);
            if(charInstructions.Count>0)
                ReadCharInstructions();
        }
    }
    public void MoveCharacter()
    {
        if(path.waypoints.Count >=1)
        {
            /*
            if(nextWaypoint == null)
            {
                NextWaypoint();
            }
             */
            position += moveDirection*activeSpeed*Time.deltaTime;
            if(distToNextNode-Vector3.Distance(position, goingFromPos) <= 0)
            {
                NextWaypoint();
            }
        }
    }
    public void NextWaypoint()
    {
        ReadWaypointInstructions();

        if (path.waypoints.Count >= 1)
        {
            nextWaypoint = path.waypoints[0];
            moveDirection = (nextWaypoint.position - position).normalized;
            distToNextNode = Vector3.Distance(position, nextWaypoint.position);
            goingFromPos = position;
        }
    }
    private void Wait()
    {
        waitTimer += Time.deltaTime;
        if(waitTimer > waitMax)
        {
            activeMode = "move";
            if(charInstructions.Count > 0 && charInstructions[0] != null)
                charInstructions[0].isDone = true;
        }
    }
    private void Fire()
    {
        if(enemy.isDead)
        {
            activeMode = "move";
        }
        charGeo.transform.LookAt(enemy.position);
        weapon.fireTimer += Time.deltaTime;
        if (weapon.fireTimer > weapon.fireRate && weapon.currentAmmo >= 1)
        {
            weapon.FireWeapon(position, enemy);
            enemy.Hit(weapon.damage);
        }
    }
    public void Hit(float damage)
    {
        health -= damage;
        if (health < 0.01f)
        {
            Die();
        }
    }
    public void ReadWaypointInstructions()
    {
        Waypoint passingWaypoint = path.waypoints[0];
        //Does the node have aim instructions?

        charInstructions.Clear();
        charInstructions = passingWaypoint.wpInstructions;
        //Have to initiate reading somewhere, not ideal.
        if(charInstructions.Count > 0)
            ReadCharInstructions();

        passingWaypoint.DeleteMesh();
        path.waypoints.Remove(passingWaypoint);
    }
    private void ReadCharInstructions()
    {
        if (charInstructions[0].waitDuration > 0)
        {
            activeMode = "wait";
            waitMax = charInstructions[0].waitDuration;
            return;
        }
        if (charInstructions[0].run)
        {
            ToggleRun();
            charInstructions[0].isDone = true;
            return;
        }
        if (charInstructions[0].lockAim)
        {
            Debug.Log("woop");
            charGeo.transform.LookAt(position + (charInstructions[0].aimDirection * 10));
            lockAim = true;
            charInstructions[0].isDone = true;
        }
        else
        {
            //Look at next waypoint. Default if char hasn't been aimed.
            if (path.waypoints.Count >= 2)
                charGeo.transform.LookAt(path.waypoints[1].position);
            //We should only get here if there were no other instructions. All other instructions use return. Otherwise this isDone will mess up Wait instructions.
            charInstructions[0].isDone = true;
        }
        
        
        //Does the node have run instructions?

    }
    private void ToggleRun()
    {
        if (activeSpeed == walkSpeed)
            activeSpeed = runSpeed;
        else
            activeSpeed = walkSpeed;
    }
    public void EnemySighted(Character enemy)
    {
        this.enemy = enemy;
        if (engageOnSight && enemy.team != team)
            activeMode = "fire";       
    }
    public void EnemyOutOfSight()
    {
        activeMode = "move";
    }
    public void Die()
    {
        isDead = true;
        GameObject.FindGameObjectWithTag("gameLogic").SendMessage("AddToKillList", this);
        //Debug.Log(GameObject.FindGameObjectWithTag("gameLogic").transform.position);
    }
    public void AddCharacterWaypoint()
    {
       Waypoint wp = new Waypoint(position);
       path.waypoints.Insert(0,wp);
    }
    public void DrawChar()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(position, new Vector3(0.4f, 0.4f, 0.4f));
    }
    public void AddWaypoints(List<Waypoint> wpList)
    {
        if(path.waypoints.Count>0)
        {
            wpList.Remove(wpList[0]);
        }
        path.entryCount.Add(wpList.Count);
        path.waypoints.AddRange(wpList);
    }
}

public class Waypoint
{
    public Vector3 position;
    public GameObject mesh;
    public Vector3 endPos;

    public GameObject wpMesh;
    //Instruction variables
 //   public Vector3 aimDirection;
//    public bool lockAim = false;
  //  public bool run = false;
    //public float waitDuration = 0;

    public List<Instruction> wpInstructions = new List<Instruction>();

    public Waypoint(Vector3 position)
    {
        this.position = position;
        Instruction defaultInstruction = new Instruction();
        wpInstructions.Add(defaultInstruction);

   
    }
    public void DrawWaypoint()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(position, new Vector3(0.4f,0.4f,0.4f));
        /*
        if(lockAim)
        {
            Debug.DrawRay(position, aimDirection, Color.red);
        }
         */
    }
    //Should be obsolete.
    /*
    public void AdjustAim(Vector3 aimPos)
    {
        aimDirection = (aimPos - position).normalized;
        lockAim = true;
    }
    */
    public void CreateMesh()
    {
        wpMesh = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("WaypointPrefab"), position, Quaternion.identity);
    }
    public void DeleteMesh()
    {
        UnityEngine.Object.Destroy(wpMesh);
        UnityEngine.Object.Destroy(mesh);
    }
}

public class Path
{    
    public List<Waypoint> waypoints;
    public List<int> entryCount = new List<int>();
    public List<GameObject> collisionMeshes = new List<GameObject>();
    public Path()
    {
        this.waypoints = new List<Waypoint>();
    }
    public Path(List<Waypoint> waypoints)
    {
        this.waypoints = waypoints;
    }
    public void UndoPath()
    {
        int index = waypoints.Count - entryCount[entryCount.Count - 1];
        waypoints[index-1].DeleteMesh();
        for (int i = index; i <= waypoints.Count - 1; i++ )
        {
            waypoints[i].DeleteMesh();
        }
            waypoints.RemoveRange(index, entryCount[entryCount.Count - 1]);
        entryCount.RemoveAt(entryCount.Count-1);
    }
    public void DisplayPath()
    {
        for (int i = 0; i <= waypoints.Count - 1; i++)
        {
            waypoints[i].DrawWaypoint();
            if (i + 1 <= waypoints.Count - 1)
                Debug.DrawLine(waypoints[i].position, waypoints[i + 1].position, Color.magenta);
        }
    }
    public void OptimizePath()
    {
        for (int i = 1; i <= waypoints.Count - 2; i++)
        {
            if ((waypoints[i - 1].position - waypoints[i].position).normalized == (waypoints[i].position - waypoints[i + 1].position).normalized)
            {
                waypoints.Remove(waypoints[i]);
                i--;
            }
        }
        LookForShortcut();
    }
    public bool CheckWaypointReachable(Waypoint wp)
    {
        //Find wp index
        int index = waypoints.IndexOf(wp)-1;
        int stopIndex = index + 2;
        RaycastHit hit;
        int wallLayerMask = 1 << 9;
        for(int i = index; i < stopIndex; i++)
        {
            if(i+1 == waypoints.Count)
                continue;
            Vector3 dir = (waypoints[i + 1].position - waypoints[i].position).normalized;
            float dist = Vector3.Distance(waypoints[i + 1].position, waypoints[i].position);
            if(Physics.Raycast(waypoints[i].position, dir, out hit, dist, wallLayerMask))
            {
                return false;
            }
        }
        return true;
    }
    public void LookForShortcut()
    {
        int wallLayerMask = 1 << 9;
        RaycastHit hit;
        int hitIndex = 1;
        for(int i = 0; i < waypoints.Count-2;i++)
        {
            bool isHit = false;
            for (int u = i + 2; u <= waypoints.Count - 1;u++ )
            {
                Vector3 dir = (waypoints[u].position - waypoints[i].position).normalized;
                float dist = Vector3.Distance(waypoints[i].position, waypoints[u].position);
                if (!Physics.Raycast(waypoints[i].position, dir, out hit, dist, wallLayerMask))
                {
                    isHit = true;
                    hitIndex = u;
                }
            }
            if (isHit)
                RemoveRange(i, hitIndex);
        }
    }
    private void RemoveRange(int start, int end)
    {
        int removeCount = end - start;
        waypoints.RemoveRange(start+1,removeCount-1);
    }
    public void InsertWaypoint(Vector3 newPos, Vector3 fromPos)
    {
        Waypoint fromWP;
        for(int i = 0; i <= waypoints.Count-1; i++)
        {

            if (waypoints[i].position == fromPos)
            {
                fromWP = waypoints[i];
                Waypoint newWP = new Waypoint(newPos);
                waypoints.Insert(i+1, newWP);

                foreach (Waypoint wp in waypoints)
                {
                    Debug.Log(wp.position);
                }
                break;
            }
        }
    }
    public void CreatePathMesh()
    {
        if (collisionMeshes.Count > 0)
            DeleteMeshes();
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            GameObject path =(GameObject)UnityEngine.Object.Instantiate(Resources.Load("PathMesh"), waypoints[i].position, Quaternion.identity);
            path.transform.LookAt(waypoints[i+1].position);
            path.transform.localScale = new Vector3(0.5f, 0.2f, Vector3.Distance(waypoints[i].position, waypoints[i+1].position));
            waypoints[i].mesh = path;
            waypoints[i].endPos = waypoints[i + 1].position;
            collisionMeshes.Add(path);
            if(waypoints[i].wpMesh == null)
                waypoints[i].CreateMesh();
        }
        if (waypoints[waypoints.Count - 1].wpMesh == null)
            waypoints[waypoints.Count-1].CreateMesh();
    }
    void DeleteMeshes()
    {
        foreach(GameObject path in collisionMeshes)
        {
            UnityEngine.Object.Destroy(path);
        }
    }
}

public class Instruction
{
    public Vector3 aimDirection;
    public bool lockAim = false;
    public bool run = false;
    public float waitDuration = 0;

    public bool isDone = false;

    public Vector3 AdjustAim(Vector3 aimPos, Vector3 position)
    {
        aimDirection = (aimPos - position).normalized;
        lockAim = true;

        return aimDirection;
    }
}
