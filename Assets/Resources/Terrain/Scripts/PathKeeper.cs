using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathKeeper : MonoBehaviour
{
    public static PathKeeper pathKeeper;

    TerrainGen terrainGen;

    [SerializeField]
    PathType displayType;

    int gridX;
    int gridZ;

    GUIStyle debugText;

    int walkCost;
    int climbCost;
    int dropCost;

    static Dictionary<PathType, PathNode[,]> pathGrids;
    public static bool generated;
    public static Vector3 goal;

    [SerializeField]
    public bool debug;
    [SerializeField]
    public bool debugCostText;

    Dictionary<PathType,List<Vector2Int>> updateNodes;
    Dictionary<PathType, List<PathNode>> criticalUpdateNodes;

    Vector3Int[] checkOffSets;

    PathType gridShowing;

    public static Dictionary<PathType, int> indexes;


    // Start is called before the first frame update
    void Awake()
    {
        generated = false;
        PathKeeper.pathKeeper = this;
        debugText = new GUIStyle();
        terrainGen = this.GetComponent<TerrainGen>();
        pathGrids = new Dictionary<PathType, PathNode[,]>();
        gridX = TerrainGen.gridX;
        gridZ = TerrainGen.gridZ;
        criticalUpdateNodes = new Dictionary<PathType, List<PathNode>>();
        criticalUpdateNodes.Add(PathType.normal, new List<PathNode>());
        updateNodes = new Dictionary<PathType, List<Vector2Int>>();
        updateNodes.Add(PathType.normal, new List<Vector2Int>());

        indexes = new Dictionary<PathType, int>();
        indexes[PathType.flight] = 0;
        indexes[PathType.normal] = 0;
        checkOffSets = new Vector3Int[]
        {
            //Odd
            new Vector3Int(1,0,0),
            new Vector3Int(1,0,-1),
            new Vector3Int(1,0,1),
            new Vector3Int(0,0,-1),
            new Vector3Int(0,0,1),
            new Vector3Int(-1,0,0),
            //Even
            new Vector3Int(1,0,0),
            new Vector3Int(0,0,-1),
            new Vector3Int(0,0,1),
            new Vector3Int(-1,0,0),
            new Vector3Int(-1,0,-1),
            new Vector3Int(-1,0,1)
        };

        walkCost = 3;
        climbCost = 7;
        dropCost = 5;
        gridShowing = PathType.none;
    }

    // Update is called once per frame
    void Update()
    {
        if (debug)
            Debug.DrawRay(goal, Vector3.up, Color.white);
        if (TerrainGen.terrainGenerated)
        {
            if (criticalUpdateNodes[PathType.normal].Count > 0)
            {
                foreach (PathNode node in criticalUpdateNodes[PathType.normal])
                {
                    UpdateNode(PathType.normal, node, true);
                }
                criticalUpdateNodes[PathType.normal].Clear();
            }
            else
            {
                if (updateNodes[PathType.normal].Count > 0)
                {
                    Vector2Int updateNode = updateNodes[PathType.normal][0];
                    FindGoal(PathType.normal, TerrainGen.GetGridPosition2D(goal), updateNode);
                    //TargetNode(ref updateNode, PathType.normal);
                    updateNodes[PathType.normal].RemoveAt(0);
                }
                else
                {
                    //updateNodes[PathType.normal].Add(pathGrids[PathType.normal][UnityEngine.Random.Range(0, gridX - 1), UnityEngine.Random.Range(0, gridZ - 1)]);
                    //updateNodes[PathType.normal].Add(pathGrids[PathType.normal][UnityEngine.Random.Range(0, gridX - 1), UnityEngine.Random.Range(0, gridZ - 1)]);
                    //updateNodes[PathType.normal].Add(pathGrids[PathType.normal][UnityEngine.Random.Range(0, gridX - 1), UnityEngine.Random.Range(0, gridZ - 1)]);
                }
            }
            if (debug)
            {
                if (HexHighlighter.showingPath != displayType)
                {
                    if (pathGrids.ContainsKey(displayType))
                    {
                        ShowOnGrid(displayType);
                        HexHighlighter.showingPath = displayType;
                        gridShowing = displayType;
                    }
                    
                }
            }
        }
       

    }

    private void OnGUI()
    {
        if (debugCostText)
        {
            Ray ray = MouseHook.mousehook.GetCamera().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            if (hit.collider != null)
            {
                Vector3Int hookPos = TerrainGen.GetGridPosition(hit.point);
                Vector2Int tilePos = Vector2Int.zero;
                tilePos.x = hookPos.x;
                tilePos.y = hookPos.z;
                List<Vector2Int> showHexes = TerrainGen.GetHexOffSetInRange(tilePos, 5);
                Vector2Int drawPos;
                string val;
                foreach (Vector2Int hex in showHexes)
                {
                    Handles.color = Color.red;
                    drawPos = hex + tilePos;
                    PathNode node = pathGrids[PathType.normal][drawPos.x, drawPos.y];
                    val = "" + node.cost.Value;
                    Handles.Label(TerrainGen.GetHexPosition(drawPos.x, drawPos.y), "" + val);
                    Vector3 drawEnd = TerrainGen.GetHexPosition(node.from);
                    
                    if (node.from != Vector2Int.zero)
                    {
                        Vector3 angle = -MyMath.GetDirectionRatio(TerrainGen.GetHexPosition(drawPos), drawEnd);
                        Quaternion lookat = Quaternion.LookRotation(angle, Vector3.up);
                        Handles.ArrowHandleCap(0, TerrainGen.GetHexPosition(drawPos), lookat, TerrainGen.hexSize, EventType.Repaint);
                    }
                    drawEnd = TerrainGen.GetHexPosition(node.target);
                    if (node.target != Vector2Int.zero)
                    {
                        Handles.color = Color.blue;
                        Vector3 angle = -MyMath.GetDirectionRatio(TerrainGen.GetHexPosition(drawPos), drawEnd);
                        Quaternion lookat = Quaternion.LookRotation(angle, Vector3.up);
                        Vector3 drawStart = TerrainGen.GetHexPosition(drawPos);
                        drawStart.y += 0.01f;
                        Handles.ArrowHandleCap(1, drawStart, lookat, TerrainGen.hexSize, EventType.Repaint);
                    }
                    
                        

                }
            }
            

        }
    }

    public void MakeReady()
    {

    }

        #region GridMaking

    public void GeneratePath(PathType type, Vector3Int destination)
    {
        goal = destination;
        PathNode[,] grid = new PathNode[gridX, gridZ];
        for (int x = 0; x < gridX; x ++)
        {
            for (int z = 0; z < gridZ; z ++)
            {
                if (TerrainGen.GetHexHeight(new Vector2Int(x,z)) != -1)
                    grid[x, z] = new PathNode(x,z);
            }
        }
        grid[destination.x, destination.z].cost = 0;
        grid[destination.x, destination.z].targetX = destination.x;
        grid[destination.x, destination.z].targetZ = destination.z;
        List<PathNode> calcingNodes = new List<PathNode>();
        calcingNodes.Add(new PathNode(destination.x, destination.z, -1));
        PathNode node;
        PathNode nextNode;
        float heightDiff;
        while (calcingNodes.Count > 0)
        {
            node = calcingNodes[0];
            Vector3Int checkPos = Vector3Int.zero;
            int offSet = 0;
            if (node.z % 2 == 0)
                offSet += 6;
            for(int i = 0; i < 6; i ++)
            {
                checkPos.x = Mathf.Clamp(node.x + checkOffSets[i + offSet].x, 0, gridX -1);
                checkPos.z = Mathf.Clamp(node.z + checkOffSets[i + offSet].z, 0, gridZ -1);
                nextNode = grid[checkPos.x, checkPos.z];
                if (nextNode.cost == null && !calcingNodes.Contains(nextNode))
                    calcingNodes.Add(nextNode);
                MakeTargetNode(ref node,ref nextNode, type);
            }

            calcingNodes.RemoveAt(0);
            calcingNodes.Sort();
            if (calcingNodes.Count > 1000)
            {
                Debug.LogError("Calc overload!");
                break;
            }
        }
        if (pathGrids.ContainsKey(type))
            pathGrids[type] = grid;
        else
            pathGrids.Add(type, grid);
        generated = true;

    }

    void UpdateNode(PathType type, PathNode node, bool critical)
    {
        PathNode nextNode;
        Vector3Int checkPos = Vector3Int.zero;
        int offSet = 0;
        if (node.z % 2 == 0)
            offSet += 6;
        TargetNode(ref node, type);
        for (int i = 0; i < 6; i++)
        {
            checkPos.x = Mathf.Clamp(node.x + checkOffSets[i + offSet].x, 0, gridX - 1);
            checkPos.z = Mathf.Clamp(node.z + checkOffSets[i + offSet].z, 0, gridZ - 1);
            nextNode = pathGrids[type][checkPos.x, checkPos.z];
            if (nextNode.cost == null && !updateNodes[type].Contains(nextNode.position) && !criticalUpdateNodes[type].Contains(nextNode))
                updateNodes[type].Add(nextNode.position);
            TargetNode(ref nextNode, type);
        }
        if (!critical && updateNodes[type].Contains(node.position))
        {
            updateNodes[type].Remove(node.position);
        }
    }

    public void RecalcArea(PathType type, Vector3Int position, int radius, bool critical)
    {
        List<PathNode> nodes = new List<PathNode>();
        List<PathNode> nodesEdit = new List<PathNode>();
        PathNode startNode = pathGrids[type][position.x,position.z];
        PathNode nextNode;
        Vector3Int checkPos = Vector3Int.zero;
        nodes.Add(pathGrids[type][position.x, position.z]);
        int offSet;
        for (int i = 0; i <= radius; i++)
        {
            offSet = 0;
            if (startNode.z % 2 == 0)
                offSet = 6;
            for (int o = 0; o < 6; o++)
            {
                
                checkPos.x = Mathf.Clamp(startNode.x + checkOffSets[i + offSet].x, 0, gridX - 1);
                checkPos.z = Mathf.Clamp(startNode.z + checkOffSets[i + offSet].z, 0, gridZ - 1);
                nextNode = pathGrids[type][checkPos.x, checkPos.z];
                if (!nodes.Contains(nextNode))
                    nodes.Add(nextNode);
            }
        }

        for (int i = 0; i <= radius; i ++)
        {
            
            nodesEdit.Clear();
            foreach (PathNode node in nodes)
            {
                nodesEdit.Add(node);
            }
            foreach (PathNode node in nodes)
            {
                if (!nodesEdit.Contains(node))
                    nodesEdit.Add(node);
                offSet = 0;
                if (node.z % 2 == 0)
                    offSet = 6;
                for (int o = 0; o < 6; o ++)
                {
                    checkPos.x = Mathf.Clamp(node.x + checkOffSets[i + offSet].x, 0, gridX - 1);
                    checkPos.z = Mathf.Clamp(node.z + checkOffSets[i + offSet].z, 0, gridZ - 1);
                    nextNode = pathGrids[type][checkPos.x, checkPos.z];
                    Debug.DrawLine(TerrainGen.GetHexPosition(node.positionVec), TerrainGen.GetHexPosition(nextNode.positionVec), Color.yellow, 4);
                    if (!nodesEdit.Contains(nextNode))
                        nodesEdit.Add(nextNode);
                }
            }
            nodes.Clear();
            foreach (PathNode node in nodesEdit)
            {
                nodes.Add(node);
            }
        }
        
        foreach (PathNode node in nodes)
        {
            if (critical)
            {
                criticalUpdateNodes[type].Add(node);
            }
                
            else
                updateNodes[type].Add(node.position);
        }

    }

    public void RecalcArea(PathType type, Vector2Int position, int radius)
    {
        List<Vector2Int> hexes;
        hexes = TerrainGen.GetHexInRange(position, radius);
        indexes[type]++;
        foreach (Vector2Int hex in hexes)
        {
            FindGoal(type, TerrainGen.GetGridPosition2D(goal), hex);
        }
    }

    bool MakeTargetNode(ref PathNode nodeA,ref PathNode nodeB, PathType type)
    {
        if (nodeA.cost == null)
            return true;
        RaycastHit hit;
        Vector3 rayPos = TerrainGen.GetHexPosition(nodeA.positionVec);
        rayPos.y += 1;
        Physics.Raycast(rayPos, Vector3.down,out hit, 5f);
        if (debug)
            Debug.DrawRay(rayPos, Vector3.down, Color.magenta, 0.5f);
        if (hit.collider != null)
        {
            //Debug.Log("Found Target:" + hit.collider.name);
            BuildingBase building = hit.collider.transform.parent.GetComponent<BuildingBase>();
            if (building != null)
            {
                Debug.Log("Found a building");
                if (nodeB.target == nodeA.position)
                {
                    nodeB.cost = 100000;
                    nodeB.target = Vector2Int.zero;
                    TargetNode(ref nodeB, type);
                    return false;
                }
            }
            
        }
        float heightDiff;
        heightDiff = TerrainGen.GetHexHeight(nodeB.positionVec) - TerrainGen.GetHexHeight(nodeA.positionVec);
        heightDiff = heightDiff / (TerrainGen.hexSize / 2 * TerrainGen.instance.yMult);

        //Debug.Log("HeightDif: " + heightDiff);

        switch (Mathf.Abs(Mathf.RoundToInt(heightDiff)))
        {
            case 0:
                if (nodeB.cost == null || nodeB.cost > nodeA.cost + walkCost)
                {
                    //Debug.Log("Adding level");
                    nodeB.target = nodeA.position;
                    if (nodeA.cost != null)
                        nodeB.cost = nodeA.cost + walkCost;
                    else
                        return true;
                }
                break;
            case 1:
                if (nodeB.cost == null || nodeB.cost > nodeA.cost + climbCost)
                {
                    //Debug.Log("Adding other");
                    nodeB.target = nodeA.position;
                    if (nodeA.cost != null)
                        nodeB.cost = nodeA.cost + climbCost;
                    else
                        return true;
                }
                break;
            default:
                if (nodeB.cost == null || nodeB.cost > nodeA.cost + 100)
                {
                    //Debug.Log("Adding other");
                    nodeB.target = nodeA.position;
                    if (nodeA.cost != null)
                        nodeB.cost = nodeA.cost + 10000;
                    else
                        nodeB.cost = 100;
                }
                break;
        }
        return false;
    }

    void TargetNode(ref PathNode nodeA, PathType type)
    {
        int offSet = 0;
        if (nodeA.z % 2 == 0)
            offSet = 6;
        PathNode nodeB;
        for (int i = 0; i < 6; i ++)
        {
            Vector3Int checkPos = Vector3Int.zero;
            checkPos.x = Mathf.Clamp(nodeA.x + checkOffSets[i + offSet].x, 0, gridX - 1);
            checkPos.z = Mathf.Clamp(nodeA.z + checkOffSets[i + offSet].z, 0, gridZ - 1);
            
            nodeB = pathGrids[type][checkPos.x, checkPos.z];
            if (nodeB.cost != null && nodeB.cost < nodeA.cost)
            {
                RaycastHit hit;
                Vector3 rayPos = TerrainGen.GetHexPosition(nodeB.positionVec);
                rayPos.y += 1;
                Physics.Raycast(rayPos, Vector3.down, out hit, 5f);
                if (debug)
                    Debug.DrawRay(rayPos, Vector3.down, Color.blue , 0.5f);
                if (hit.collider != null)
                {
                    BuildingBase buildingBase = null;
                    buildingBase = hit.collider.transform.parent.gameObject.GetComponent<BuildingBase>();
                    if (buildingBase != null && buildingBase.HasProperty(BuildingProperties.blocking))
                    {
                        Debug.DrawRay(hit.collider.transform.parent.gameObject.transform.position, Vector3.up, Color.magenta, 8f);
                        nodeB.cost = 100000;
                        if (nodeA.target == nodeB.position)
                        {
                            i = 0;
                            nodeA.target = Vector2Int.zero;
                            nodeA.cost = 10000;
                        }
                        continue;
                    }
                }
                float heightDif = TerrainGen.GetHexHeight(nodeA.positionVec) - TerrainGen.GetHexHeight(nodeB.positionVec);
                heightDif = heightDif / TerrainGen.hexSize / 2;
                if (type == PathType.flight)
                    heightDif = 0;
                switch (Mathf.RoundToInt(heightDif))
                {
                    case 0:
                        if (nodeB.cost + walkCost < nodeA.cost)
                        {
                            nodeA.target = nodeB.position;
                            nodeA.cost = nodeB.cost + walkCost;
                        }
                        break;
                    case 1:
                        if (nodeB.cost + climbCost < nodeA.cost)
                        {
                            nodeA.target = nodeB.position;
                            nodeA.cost = nodeB.cost + climbCost;
                        }
                        break;
                }
            }
        }
    }

    public bool FindGoal(PathType pathType, Vector2Int goal, Vector2Int start)
    {
        if (pathGrids[pathType][start.x, start.y].index == indexes[pathType])
        {
            return true;
        }
        Vector2Int pos = start;
        List<PathNode> checkNodes = new List<PathNode>();
        List<PathNode> doneNodes = new List<PathNode>();
        PathNode newNode;

        BuildingBase building;
        Vector3 rayPos;
        RaycastHit hit;

        newNode = new PathNode(pos.x, pos.y)
        {
            distance = Mathf.RoundToInt(TerrainGen.GetHexDistance(pos, TerrainGen.GetGridPosition2D(PathKeeper.goal)))
        };
        newNode.sortCost = newNode.distance;
        checkNodes.Add(newNode);
        
        Vector2Int checkStep = new Vector2Int();
        int rounds = 0;
        //
        while (pos != goal && checkNodes.Count > 0)
        {
            checkNodes.Sort();
            if (debug)
            {
                Vector3 startDraw = TerrainGen.GetHexPosition(pos);
                Vector3 endDraw = TerrainGen.GetHexPosition(checkNodes[0].position);
                startDraw.y += 0.01f;
                endDraw.y += 0.01f;
                Debug.DrawLine(startDraw, endDraw, Color.green, 3);
            }
            
            pos = checkNodes[0].position;
            
            int ind = 0;
            if (pos.y % 2 == 0)
                ind += 6;
            for (int i = 0; i < 6; i ++)
            {
                checkStep.x = pos.x + MyMath.hexOffSetGrid[ind + i].x;
                checkStep.y = pos.y + MyMath.hexOffSetGrid[ind + i].z;
                if (pathGrids[pathType][pos.x, pos.y].index == indexes[pathType])
                {
                    continue;
                }

                    
                newNode = new PathNode(checkStep.x, checkStep.y);
                if (debug)
                {
                    Debug.DrawLine(TerrainGen.GetHexPosition(pos), TerrainGen.GetHexPosition(checkStep), Color.red, 2);
                }
                
                newNode.distance = Mathf.RoundToInt(TerrainGen.GetHexDistance(checkStep, TerrainGen.GetGridPosition2D(PathKeeper.goal)));
                newNode.sortCost = CalcCost(pathType, pos, checkStep) + checkNodes[0].sortCost;
                newNode.cost = CalcCost(pathType, pos, checkStep) + newNode.distance;
                newNode.index = indexes[pathType];
                newNode.from = pos;
                rayPos = TerrainGen.GetHexPosition(checkStep);
                rayPos.y += 2;
                if (Physics.Raycast(rayPos, Vector3.down, out hit, 3))
                {
                    building = hit.collider.GetComponentInParent<BuildingBase>();
                    if (building != null)
                    {
                        if (pathType != PathType.flight && building.HasProperty(BuildingProperties.blocking))
                            newNode.sortCost += 100000;
                    }
                }
                int exists = checkNodes.FindIndex(a => a.position == newNode.position);
                if (exists == -1 && doneNodes.FindIndex(node => node.position == newNode.position) == -1)
                {
                    checkNodes.Add(newNode);
                }
                else if (exists != -1)
                {
                    PathNode existingNode = checkNodes[exists];
                    if (existingNode.sortCost > newNode.sortCost)
                    {
                        checkNodes.Remove(existingNode);
                        checkNodes.Add(newNode);
                    }
                }
            }

            
           

            doneNodes.Add(checkNodes[0]);
            checkNodes.RemoveAt(0);
            if (rounds > 500)
            {
                Debug.Log("Ending Pathfind with " + checkNodes.Count + " / " + doneNodes.Count);
                break;
            }
                
            rounds++;
        }
        doneNodes.Sort();
        PathNode thisNode;
        PathNode fromNode;
        List<PathNode> adjacentNodes;
        bool atStart = false;
        rounds = 0;

        thisNode = doneNodes[0];
        while (!atStart && rounds < 500 && thisNode != null)
        {
            fromNode = doneNodes.Find(node => node.position == thisNode.from);
            if (fromNode != null)
            {
                //fromNode.target = thisNode.position;
                //Find all nodes that came from fromNode
                adjacentNodes = doneNodes.FindAll(node => node.from == fromNode.position);
                foreach (PathNode node in adjacentNodes)
                {
                    //See if the node is adjacent to out new target
                    if (MyMath.hexOffSetGrid2D.Contains(thisNode.position - node.position))
                    {
                        if (debug)
                        {
                            Vector3 drawStart = TerrainGen.GetHexPosition(node.position);
                            Vector3 drawEnd = drawStart;
                            drawEnd.y += 0.1f;
                            Debug.DrawLine(drawStart, drawEnd, Color.magenta, 3);

                        }
                        if (node.target == node.position)
                            Debug.DrawRay(TerrainGen.GetHexPosition(node.position), Vector3.up, Color.red, 5);
                        node.target = thisNode.position;
                        pathGrids[pathType][thisNode.x, thisNode.z] = node;
                        
                        //doneNodes.Remove(node);
                    }
                    else
                    {
                        if (debug)
                        {
                            Vector3 drawStart = TerrainGen.GetHexPosition(node.position);
                            Vector3 drawEnd = drawStart;
                            drawEnd.y += 0.1f;
                            Debug.DrawLine(drawStart, drawEnd, Color.yellow, 3);

                        }
                        node.target = fromNode.position;
                        pathGrids[pathType][thisNode.x, thisNode.z] = node;
                        //doneNodes.Remove(node);
                    }
                }
                
            }
            else
            {
                Debug.Log("No node found");
                //pathGrids[pathType][thisNode.x, thisNode.z] = thisNode;
            }
            if (thisNode.target == Vector2Int.zero && thisNode.position != goal)
            {
                //Find best target by cost;
                Debug.Log("we got a lonly one");

            }
            pathGrids[pathType][thisNode.x, thisNode.z] = thisNode;
            rounds++;
            thisNode = fromNode;
        }

        foreach (PathNode node in checkNodes)
        {
            //updateNodes[pathType].Add(node.position);
        }


        return false;
    }

    int CalcCost(PathType type, Vector2Int pos, Vector2Int destination)
    {
        int cost = 0;
        float heightDif;
        Hex posHex = TerrainGen.GetHex(pos.x, pos.y);
        Hex destHex = TerrainGen.GetHex(destination.x, destination.y);

        if (type != PathType.flight)
        {
            if (posHex != null && destHex != null)
            {
                heightDif = posHex.height - destHex.height;
                heightDif /= TerrainGen.hexSize / 2;
                if (heightDif > 0)
                {
                    cost += Mathf.RoundToInt(heightDif);
                }
                cost += destHex.cost;
            }
            else
                cost += 10000;
        }

        return cost;
    }


    void ChokeCheck()
    {
        //to do
        //Check if that building was the last path
    }

    bool ChangerCheck()
    {
        return false;
        //Check if we should spawn a changer
    }

    #endregion

    public static Vector3 GetNextHex(Vector3Int pos, PathType type = PathType.normal)
    {
        return GetNextHex(pos.x, pos.z, type);
    }
    public static Vector3 GetNextHex(int x, int z, PathType type = PathType.normal)
    {
        Vector3 nextPos;
        nextPos = TerrainGen.GetHexPosition(pathGrids[type][x, z].targetVec);


        return nextPos;
    }


    #region Debug
    
    public static void ShowOnGrid(PathType type)
    {
        if (!generated)
            return;
        PathNode node;
        Vector3 facingDir;
        for (int x = 0; x < TerrainGen.gridX; x ++)
        {
            for (int z = 0; z < TerrainGen.gridZ; z ++)
            {
                node = pathGrids[type][x,z];
                if (node != null && node.target != null)
                {
                    Vector3 nodePos = TerrainGen.GetHexPosition(node.positionVec);
                    Vector3 targetPos = TerrainGen.GetHexPosition(node.targetVec);
                    nodePos.y = 0;
                    targetPos.y = 0;
                    facingDir = targetPos - nodePos;
                    if (facingDir.z > 0)
                    {
                        if (facingDir.x > 0)
                            HexHighlighter.Set(new Vector2Int(x, z), HexHighlight.arrow_1);
                        else
                            HexHighlighter.Set(new Vector2Int(x, z), HexHighlight.arrow_2);
                    }
                    else if (facingDir.z < 0)
                    {
                        if (facingDir.x > 0)
                            HexHighlighter.Set(new Vector2Int(x, z), HexHighlight.arrow_5);
                        else
                            HexHighlighter.Set(new Vector2Int(x, z), HexHighlight.arrow_4);
                    }
                    else if (facingDir.x > 0)
                    {
                        if (facingDir.z == 0)
                            HexHighlighter.Set(new Vector2Int(x, z), HexHighlight.arrow_0);
                    }
                    else if (facingDir.x < 0)
                    {
                        if (facingDir.z == 0)
                            HexHighlighter.Set(new Vector2Int(x, z), HexHighlight.arrow_3);
                    }
                }
            }
        }
        

        //
        HexHighlighter.showingType = GridType.path;
    }
    
    
    #endregion
    

}


public enum PathType
{
    normal,
    dumb,
    hover,
    flight,
    none
}


public class PathNode : IComparable, IEquatable<PathNode>
{
    public Vector2Int position;
    public int sortCost;
    public int? cost;
    public int index;
    public int distance;
    public Vector2Int target;
    public Vector2Int from;

    public PathNode()
    {
        this.cost = null;
        target.x = -1;
        target.y = -1;
    }

    public PathNode(int x, int z, int? cost = null)
    {
        position.x = x;
        position.y = z;
        distance = 0;
        index = 0;
        target.x = -1;
        target.y = -1;
        this.cost = cost;
    }

    
    public bool Equals(PathNode node)
    {
        if (position == node.position)
            return true;
        return false;
    }

    public int CompareTo(object obj)
    {
        if (obj == null) return -1;

        PathNode otherNode = obj as PathNode;
        if (otherNode.sortCost + otherNode.distance > this.sortCost + distance) // Precedes
            return -1;
        if (otherNode.sortCost + otherNode.distance < this.sortCost + distance) // Comes after
            return 1;
        else
            return 0;
    }

  

    public Vector3Int positionVec
    {
        get { return new Vector3Int(position.x, 0, position.y); }
    }

    public Vector3Int targetVec
    {
        get { return new Vector3Int(target.x, 0, target.y); }
    }

    public int x
    {
        get { return position.x; }
        set { position.x = value; }
    }
    public int z
    {
        get { return position.y; }
        set { position.y = value; }
    }

    public int targetX
    {
        get { return target.x; }
        set { target.x = value; }
    }
    public int targetZ
    {
        get { return target.y; }
        set { target.y = value; }
    }

}


