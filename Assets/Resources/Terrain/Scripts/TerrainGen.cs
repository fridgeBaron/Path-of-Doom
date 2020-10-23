using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TerrainGen : MonoBehaviour
{
    public static float hexSize = 1f;
    public static float hexWidth = Mathf.Sqrt(3) * hexSize;

    public static int gridX = 30;
    public static int gridZ = 30;
    public static int gridY = 30;
    public float hexHeight;
    public NavMeshSurface navMesh;

    public static TerrainGen instance;
    [SerializeField]
    bool debug;

    [SerializeField]
    public int yMult;

    [SerializeField]
    int xOrigin;
    [SerializeField]
    int zOrigin;
    
    public static GameObject goal;
    [SerializeField]
    GameObject blocks;
    [SerializeField]
    GameObject doodads;
    [SerializeField]
    GameObject blockFab;
    [SerializeField]
    GameObject camHandle;
    //
    PathKeeper pathKeeper;
    static HexGrid hexGrid;
    static List<Vector2Int> edgeHexes;
    Texture2D noiseTex;


    [SerializeField]
    GameObject hexFab;
    [SerializeField]
    GameObject treeDoodadFab;
    [SerializeField]
    GameObject highlightFab;
    [SerializeField]
    GameObject highlighters;
    [SerializeField]
    GameObject spawnerParctilesFab;

    [SerializeField]
    public static MapGen mapGenState;

    List<MobSpawner> spawners;

    public static bool terrainGenerated = false;

    [SerializeField]
    MeshRenderer noisePreview;

    [SerializeField]
    public NoiseMap[] noiseMaps;

    [SerializeField]
    ShapeGenerator shapeGenerator;

    public int noiseMapLength;

    // Start is called before the first frame update
    void Awake()
    {
        pathKeeper = this.GetComponent<PathKeeper>();
        //
        if (TerrainGen.instance == null)
            instance = this;
        mapGenState = MapGen.none;
        hexGrid = new HexGrid(gridX, gridZ);
        edgeHexes = new List<Vector2Int>();
        noiseMaps = new NoiseMap[2];
        noiseMaps[0] = new NoiseMap()
        {
            scale = 2,
            weight = 30,
            offSet = new Vector2Int(Random.Range(0, 1000), Random.Range(0, 1000))
        };
        CalcNoise(noiseMaps[0]);
        noiseMaps[1] = new NoiseMap()
        {
            scale = 15,
            weight = 20,
            offSet = new Vector2Int(Random.Range(0, 1000), Random.Range(0, 1000))
        };
        CalcNoise(noiseMaps[1]);
        navMesh.buildHeightMesh = true;
    }


    private void Start()
    {
        mapGenState = MapGen.terrain;
        //spawner.GetComponent<MobSpawner>().GetParticleSystems();
    }

    // Update is called once per frame
    void Update()
    {
        if (mapGenState == MapGen.terrain)
        {
            Texture2D featureMap;
            foreach (NoiseMap noiseMap in noiseMaps)
            {
                noiseMap.offSet = new Vector2Int(Random.Range(0, 1000), Random.Range(0, 1000));
                CalcNoise(noiseMap);
            }
            featureMap = shapeGenerator.GenerateClipMap();
            shapeGenerator.GenerateLake(featureMap, 3);
            shapeGenerator.GenerateForests(featureMap, 7);
            noiseTex = new Texture2D(gridX, gridZ);
            CombineTextures(noiseTex, noiseMaps);
            ClipTexture(noiseTex, featureMap, Color.white);
            GenHexGrid(gridX, gridY, gridZ);
            CombineMeshes(20, 20);
            navMesh.BuildNavMesh();
            FillDoodads(featureMap);
            CalcEdgeHexes(25);
            SpreadOutterEdges(5);
            Vector3 camPos = GetHexPosition((int)(gridX / 2), (int)(gridZ / 2));
            camPos.y = 5f;
            camHandle.transform.position = camPos;
            mapGenState = MapGen.terrainCon;
            HexHighlighter.CreateGrid(gridX, gridZ, highlightFab, highlighters);
            ShowEdgeHexes();
            if (debug)
            {
                Vector3 goalPos = Vector3.zero;
                while (goalPos == Vector3.zero)
                {
                    Vector2Int gridPos = Vector2Int.zero;
                    gridPos.x = Random.Range(0, gridX - 1);
                    gridPos.y = Random.Range(0, gridZ - 1);
                    if (GetHex(gridPos.x, gridPos.y) != null)
                        goalPos = GetHexPosition(gridPos);
                }
                GameObject debugGoal = new GameObject();
                debugGoal.transform.position = goalPos;
                MobLister.MakeModGrid(gridX, gridZ);
                pathKeeper.GeneratePath(PathType.normal, TerrainGen.GetGridPosition(debugGoal.transform.position));
                PathKeeper.goal = debugGoal.transform.position;
                mapGenState = MapGen.done;
                terrainGenerated = true;
                MobSpawner.GetSpawners()[0].MakeReady(debugGoal);
            }

            //
            //
        }
        else if (mapGenState == MapGen.pathing)
        {
            
            MobLister.MakeModGrid(gridX, gridZ);
            pathKeeper.GeneratePath(PathType.normal, TerrainGen.GetGridPosition(goal.transform.position));
            mapGenState = MapGen.done;
            terrainGenerated = true;
            MobSpawner.GetSpawners()[0].MakeReady(goal);
            
        }
    }

    /// Handling textures for heightmaps and clipsmaps -----------------------------------------------------------------------------------------------
    #region MapTextureManipulation


    public void CalcNoise(NoiseMap noiseMap)
    {
        // For each pixel in the texture...
        int y = 0;

        while (y < noiseMap.noiseMap.height)
        {
            int x = 0;
            while (x < noiseMap.noiseMap.width)
            {
                float xCoord = ((float)x / noiseMap.noiseMap.width) * noiseMap.scale + noiseMap.offSet.x;
                float yCoord = ((float)y / noiseMap.noiseMap.height) * noiseMap.scale + noiseMap.offSet.y;
                float value = Mathf.PerlinNoise(xCoord, yCoord);
                noiseMap.noiseMap.SetPixel(x, y, new Color(value, value, value));

                x++;
            }
            y++;
        }

        noiseMap.noiseMap.Apply();
    }


    private void CombineTextures(Texture2D main, NoiseMap[] noiseMaps)
    {
        float totalWeight = 0;
        foreach (NoiseMap noiseMap in noiseMaps)
        {
            totalWeight += Mathf.Abs(noiseMap.weight);
        }
        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridZ; y++)
            {
                float pixValue = 0;
                foreach (NoiseMap noiseMap in noiseMaps)
                {
                    pixValue += noiseMap.noiseMap.GetPixel(x, y).grayscale * noiseMap.weight;
                }
                pixValue = pixValue / totalWeight;
                Color newPixel = new Color(pixValue, pixValue, pixValue);
                main.SetPixel(x, y, newPixel);
            }
        }
        main.Apply();
    }

    private void ClipTexture(Texture2D main, Texture2D clip, Color color)
    {
        for (int x = 0; x < main.width; x++)
        {
            for (int y = 0; y < main.height; y++)
            {
                if (clip.GetPixel(x, y).grayscale == color.grayscale)
                {
                    main.SetPixel(x, y, Color.black);
                }
            }
        }
    }

    #endregion
    /// Creating and solving info for hexes ----------------------------------------------------------------------------------------------------------
    #region GridMaking

    public void KillTerrain()
    {
        if (mapGenState != MapGen.done)
        {
            foreach (Transform block in blocks.GetComponentInChildren<Transform>())
            {
                Destroy(block.gameObject);
            }
            foreach (Transform doodad in doodads.GetComponentInChildren<Transform>())
            {
                Destroy(doodad.gameObject);
            }
            foreach (Transform hex in highlighters.GetComponentInChildren<Transform>())
            {
                Destroy(hex.gameObject);
            }
        }
        hexGrid = new HexGrid(gridX, gridZ);
        edgeHexes = new List<Vector2Int>();
    }

    public void TryGenHexGrid()
    {
        
    }

    public static void ShowEdgeHexes()
    {
        foreach (Vector2Int hex in edgeHexes)
        {
            if (GetHex(hex.x, hex.y).edge == EdgeType.outter)
                HexHighlighter.Set(hex, HexHighlight.height_0);
            else
                HexHighlighter.Set(hex, HexHighlight.height_6);
        }
    }

    private void GenHexGrid(int xSize, int ySize, int zSize)
    {
        Vector3 pos;
        pos = Vector3.zero;

        Hex newHex;
        for (int x = 0; x < xSize; x++)
        {
            pos.z = 0;
            for (int z = 0; z < zSize; z++)
            {
                if (z % 2 != 0)
                    pos.x = hexWidth / 2;
                else
                    pos.x = 0;

                pos.z = z * (hexSize + hexSize / 2);
                pos.x += x * hexWidth;
                float noise = noiseTex.GetPixel(x, z).grayscale;
                if (noise == 0)
                    continue;
                newHex = new Hex(Mathf.RoundToInt(noise * ySize) * hexSize / 2);
                pos.y = newHex.height;
                newHex.mesh = CreateHexMesh(pos, newHex, x, z);
                hexGrid[x, z] = newHex;
                newHex.gridX = x;
                newHex.gridZ = z;

            }
        }




    }

    private void FillDoodads(Texture2D map)
    {
        for (int x = 0; x < map.width; x ++)
        {
            for (int z = 0; z < map.height; z ++)
            {
                Color pixel = map.GetPixel(x, z);
                if (pixel == Color.blue)
                {

                }
                if (pixel == Color.green)
                {
                    GameObject newDoodad;
                    Vector3 newPos = GetHexPosition(x, z);
                    newDoodad = Instantiate(treeDoodadFab, doodads.transform);
                    newDoodad.transform.position = newPos;
                }
                
            }
        }
    }

    private void CombineMeshes(int xSize, int ySize)
    {
        List<Mesh> blockMeshes = new List<Mesh>();
        GameObject newBlock;
        CombineInstance[] combine;
        for (int xBlock = 0; xBlock < TerrainGen.gridX; xBlock += xSize)
        {
            for (int yBlock = 0; yBlock < gridZ; yBlock += ySize)
            {
                blockMeshes = new List<Mesh>();
                for (int x = 0; x < xSize; x ++)
                {
                    for (int y = 0; y < ySize; y ++)
                    {
                        Hex hex = TerrainGen.GetHex(xBlock + x, yBlock + y);
                        if (hex != null)
                            blockMeshes.Add(hex.mesh);
                    }
                }
                if (blockMeshes.Count > 0)
                {
                    newBlock = Instantiate(blockFab, blocks.transform);
                    newBlock.name = "Block: " + xBlock + " , " + yBlock;
                    combine = new CombineInstance[blockMeshes.Count];
                    int i = 0;
                    foreach (Mesh mesh in blockMeshes)
                    {
                        combine[i].mesh = mesh;
                        combine[i].transform = newBlock.transform.localToWorldMatrix;
                        i++;
                        //
                    }
                    newBlock.GetComponent<MeshFilter>().mesh = new Mesh();
                    newBlock.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true);
                    newBlock.GetComponent<MeshCollider>().sharedMesh = newBlock.GetComponent<MeshFilter>().mesh;
                    newBlock.SetActive(true);
                }
                
            }
        }
    }

    private Mesh CreateHexMesh(Vector3 pos, Hex shape = null, int x = 0, int z = 0)
    {
        //GameObject newHex;
        //newHex = Instantiate(hexFab, this.transform);
        //newHex.transform.parent = blocks.transform;
       // newHex.name = "Hex:" + x + ", " + z + "   y:" + pos.y;
        MeshFilter hexMesh;
        //hexMesh = newHex.GetComponent<MeshFilter>();
        Mesh newMesh = new Mesh();
        //MeshCollider hexColl = newHex.GetComponent<MeshCollider>();

      

 
        Vector3[] verticies = new Vector3[25]
        {
            //Hex top 0-6
            new Vector3(0, 0, hexSize),
            new Vector3(hexWidth/2, 0, hexSize/2),
            new Vector3(hexWidth/2, 0 ,-hexSize/2),
            new Vector3(0, 0, -hexSize),
            new Vector3(-hexWidth/2,0, -hexSize/2),
            new Vector3(-hexWidth/2,0, hexSize/2),
            new Vector3(0,0,0),
            //Hex Rise 7-12
            new Vector3(0, 0, hexSize),
            new Vector3(hexWidth/2, 0, hexSize/2),
            new Vector3(hexWidth/2, 0 ,-hexSize/2),
            new Vector3(0, 0, -hexSize),
            new Vector3(-hexWidth/2,0, -hexSize/2),
            new Vector3(-hexWidth/2,0, hexSize/2),
            //Hex Mids 13-18
            new Vector3(0, -hexSize, hexSize),
            new Vector3(hexWidth/2, -hexSize, hexSize/2),
            new Vector3(hexWidth/2, -hexSize ,-hexSize/2),
            new Vector3(0, -hexSize, -hexSize),
            new Vector3(-hexWidth/2,-hexSize, -hexSize/2),
            new Vector3(-hexWidth/2,-hexSize, hexSize/2),
            //Hex Bottoms 19-24
            new Vector3(0, -hexSize * 2, hexSize),
            new Vector3(hexWidth/2, -hexSize * 2, hexSize/2),
            new Vector3(hexWidth/2, -hexSize *2,-hexSize/2),
            new Vector3(0, -hexSize *2, -hexSize),
            new Vector3(-hexWidth/2,-hexSize *2, -hexSize/2),
            new Vector3(-hexWidth/2,-hexSize *2, hexSize/2),
        };
        int rightRaise = 0;
        int upRaise = 0;
        int downRaise = 0;
        int centerRaise = Mathf.RoundToInt(noiseTex.GetPixel(x, z).grayscale * gridY);



        if (x < gridX)
        {
            
            rightRaise = Mathf.RoundToInt(noiseTex.GetPixel(x + 1, z).grayscale * gridY);
               
        }


        if (z < gridZ)
        {

            if (z % 2 == 0)
            {
                upRaise = Mathf.RoundToInt(noiseTex.GetPixel(x, z + 1).grayscale * gridY);
                downRaise = Mathf.RoundToInt(noiseTex.GetPixel(x, z - 1).grayscale * gridY);
            }
            else
            {
                upRaise = Mathf.RoundToInt(noiseTex.GetPixel(x + 1, z + 1).grayscale * gridY);
                downRaise = Mathf.RoundToInt(noiseTex.GetPixel(x + 1, z - 1).grayscale * gridY);
            }

                
            //float leftUpRaise = Mathf.RoundToInt(noiseTex.GetPixel(x + 1, z + 1).grayscale * gridY);

        }
        
        
        if (rightRaise != centerRaise && (Mathf.Abs(rightRaise - centerRaise) <= 1))
        {
            if (rightRaise > centerRaise)
            {
                verticies[1] = new Vector3(hexWidth / 2, hexSize / 2, hexSize / 2);
                verticies[2] = new Vector3(hexWidth / 2, hexSize / 2, -hexSize / 2);

                verticies[8] = new Vector3(hexWidth / 2, hexSize / 2, hexSize / 2);
                verticies[9] = new Vector3(hexWidth / 2, hexSize / 2, -hexSize / 2);
            }
            else
            {
                verticies[1] = new Vector3(hexWidth / 2, -hexSize / 2, hexSize / 2);
                verticies[2] = new Vector3(hexWidth / 2, -hexSize / 2, -hexSize / 2);
                verticies[8] = new Vector3(hexWidth / 2, -hexSize / 2, hexSize / 2);
                verticies[9] = new Vector3(hexWidth / 2, -hexSize / 2, -hexSize / 2);
            }
        }
        if (downRaise != centerRaise && (Mathf.Abs(downRaise - centerRaise) <= 1))
        {
            if (downRaise > centerRaise)
            {
                verticies[3] = new Vector3(0, hexSize / 2, -hexSize);
                verticies[10] = new Vector3(0, hexSize / 2, -hexSize);
            }
            else
            {
                verticies[3] = new Vector3(0, -hexSize / 2, -hexSize);
                verticies[10] = new Vector3(0, -hexSize / 2, -hexSize);
            }
        }
        if (upRaise != centerRaise && (Mathf.Abs(upRaise - centerRaise) <= 1))
        {
            if (upRaise > centerRaise)
            {
                verticies[0] = new Vector3(0, hexSize / 2, hexSize);
                verticies[7] = new Vector3(0, hexSize /2, hexSize);
            }
            else
            {
                verticies[0] = new Vector3(0, -hexSize / 2, hexSize);
                verticies[7] = new Vector3(0, -hexSize / 2, hexSize);
            }
        }
        

        for (int i = 0; i < verticies.Length; i++)
        {
            verticies[i] += pos;
        }

        int[] tris = new int[90]
        {
            //12 - 2
            6, 0, 1,
            //2 - 4
            6, 1, 2,
            //4 - 6
            6, 2, 3,
            // 6 - 8
            6, 3, 4,
            // 8- 10
            6, 4, 5,
            // 10 - 12
            6, 5, 0,
            //12 - 2 Rise to mid
            7, 13, 14,
            14, 8, 7,
            //2 - 4 Rise to mid
            8, 14, 15,
            15, 9, 8,
            // 4 - 6 Rise to mid
            9, 15, 16,
            16, 10, 9,
            // 6- 8 Rise to mid
            10, 16, 17,
            17, 11, 10,
            // 8- 10 Rise to mid
            12, 11, 17,
            17, 18, 12,
            //10 - 12 Rise to mid
            12, 18, 13,
            13, 7, 12,

            //12 - 2 Mid to Lower
            13, 19, 20,
            20, 14, 13,
            //2 - 4 Mid to Lower
            14, 20, 21,
            21, 15, 14,
            // 4 - 6 Mid to Lower
            15, 21, 22,
            22, 16, 15,
            // 6- 8 Mid to Lower
            16, 22, 23,
            23, 17, 16,
            // 8- 10 Mid to Lower
            17, 23, 24,
            24, 18, 17,
            //10 - 12 Mid to Lower
            18, 24, 19,
            19, 13, 18


        };

        Vector3[] normals = new Vector3[25]
        {
            //Top Mesh
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,
            //Rise Mesh Top
            new Vector3(0, 0, hexSize),
            new Vector3(hexWidth/2, 0, hexSize/2),
            new Vector3(hexWidth/2, 0 ,-hexSize/2),
            new Vector3(0, 0, -hexSize),
            new Vector3(-hexWidth/2,0, -hexSize/2),
            new Vector3(-hexWidth/2,0, hexSize/2),
            //Rise Mesh Mid
            new Vector3(0, 0, hexSize),
            new Vector3(hexWidth/2, 0, hexSize/2),
            new Vector3(hexWidth/2, 0 ,-hexSize/2),
            new Vector3(0, 0, -hexSize),
            new Vector3(-hexWidth/2,0, -hexSize/2),
            new Vector3(-hexWidth/2,0, hexSize/2),
            //Rise Mesh Mid
            new Vector3(0, 0, hexSize),
            new Vector3(hexWidth/2, 0, hexSize/2),
            new Vector3(hexWidth/2, 0 ,-hexSize/2),
            new Vector3(0, 0, -hexSize),
            new Vector3(-hexWidth/2,0, -hexSize/2),
            new Vector3(-hexWidth/2,0, hexSize/2)
        };


        Vector2[] topCorners = MyMath.GetUVSubdivisionHex(new Vector2Int(0, 0), new Vector2Int(4, 3));
        Vector2[] firstCorners = MyMath.GetUVSubdivision(new Vector2Int(0, 1), new Vector2Int(4, 3));
        Vector2[] bottomCorners = MyMath.GetUVSubdivision(new Vector2Int(0, 2), new Vector2Int(4, 3));
        Vector2[] uvs = new Vector2[25]
              {
            //Top Mesh
            topCorners[0],
            topCorners[1],
            topCorners[2],
            topCorners[3],
            topCorners[4],
            topCorners[5],
            topCorners[6],
            //Rise Mesh upper hex verts
            firstCorners[0],
            firstCorners[1],
            firstCorners[0],
            firstCorners[1],
            firstCorners[0],
            firstCorners[1],
            //Mid Hex verts
            firstCorners[2],
            firstCorners[3],
            firstCorners[2],
            firstCorners[3],
            firstCorners[2],
            firstCorners[3],
            //Lower Hex verts
            bottomCorners[2],
            bottomCorners[3],
            bottomCorners[2],
            bottomCorners[3],
            bottomCorners[2],
            bottomCorners[3],
        };

        newMesh.vertices = verticies;
        newMesh.triangles = tris;
        newMesh.normals = normals; ;
        newMesh.uv = uvs;
        //hexMesh.mesh = newMesh;
        //hexColl.sharedMesh = newMesh;

        return newMesh;
    }

    private bool CreateRiseMesh(Hex hexOne, int dir)
    {



        switch (dir)
        {
            case 1:
                break;
        }


        return false;
    }

    private void CalcEdgeHexes(int checkDistance)
    {
        Hex hex;
        RaycastHit hit;
        for (int x = 0; x < gridX; x ++)
        {
            for (int z = 0;z < gridZ; z ++)
            {
                hex = GetHex(x, z);
                if (hex != null)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        int ind = i;
                        if (z % 2 == 0)
                            ind += 6;
                        if (GetHex(x + MyMath.hexOffSetGrid[ind].x, z + MyMath.hexOffSetGrid[ind].z) == null)
                        {
                            hex.edge = EdgeType.inner;
                            if (!edgeHexes.Contains(new Vector2Int(x, z)))
                                edgeHexes.Add(new Vector2Int(x, z));
                            Vector3 cubePos = ConvertToCubicPosition(new Vector2Int(x, z));
                            Vector3 cubeTarget = cubePos + (MyMath.cubeHexDirections[i] * checkDistance);

                            List<Vector2Int> hexes = TerrainGen.GetHexesInLine(cubePos, cubeTarget);

                            if (hexes != null && hexes.Count > 0)
                            {
                                hexes.RemoveAt(0);
                                if (hexes.Count > 0)
                                {
                                    bool hasHex = false;
                                    foreach (Vector2Int hexPos in hexes)
                                    {
                                        if (TerrainGen.GetHex(hexPos.x, hexPos.y) != null)
                                            hasHex = true;
                                    }
                                    if (!hasHex)
                                        hex.edge = EdgeType.outter;
                                   
                                    
                                }
                            }
                            
                        }
                    }
                }
            }
        }
    }

    private void SpreadOutterEdges(int times)
    {
        for (int i = 0; i < times; i ++)
        {
            foreach (Vector2Int hex in edgeHexes)
            {
                if (GetHex(hex.x,hex.y).edge == EdgeType.outter)
                {
                    for (int o = 0; o < 6; o++)
                    {
                        Hex spreadHex;
                        Vector2Int spreadPos = Vector2Int.zero;
                        int ind = o;
                        if (hex.y % 2 == 0)
                            ind += 6;
                        spreadPos.x = hex.x + MyMath.hexOffSetGrid[ind].x;
                        spreadPos.y = hex.y + MyMath.hexOffSetGrid[ind].z;
                        spreadHex = GetHex(spreadPos.x, spreadPos.y);
                        if (spreadHex != null)
                        {
                            if (spreadHex.edge == EdgeType.inner)
                                spreadHex.edge = EdgeType.outter;
                        }
                    }
                }
                
            }
        }
    }

    #endregion
    /// Functions to handle grid operations ----------------------------------------------------------------------------------------------------------
    #region GridHandling

    public static Hex GetHex(int x, int z)
    {
        x = Mathf.Clamp(x, 0, gridX - 1);
        z = Mathf.Clamp(z, 0, gridZ - 1);
        return hexGrid[x, z];
    }

    /// <summary>
    /// Gets the height of a hex tile from the given grid position
    /// </summary>
    public static float GetHexHeight(Vector3Int position)
    {
        if (position.x > 0 && position.x < gridX - 1 && position.z > 0 && position.z < gridZ - 1 && hexGrid[position.x, position.z] != null)
            return hexGrid[position.x, position.z].height;
        return -1;
    }

    /// <summary>
    /// Gets the height of a hex tile from the given grid position
    /// </summary>
    public static float GetHexHeight(Vector2Int position)
    {
        if (position.x > 0 && position.x < gridX - 1 && position.y > 0 && position.y < gridZ - 1 && hexGrid[position.x, position.y] != null)
            return hexGrid[position.x, position.y].height;
        return 0;
    }


    /// <summary>
    /// Gets the in world position of a grid value from X and Z values 
    /// </summary>
    public static Vector3 GetHexPosition(Vector3Int checkVec)
    {
        return GetHexPosition(checkVec.x, checkVec.z);
    }

    public static Vector3 GetHexPosition(Vector2Int checkVec)
    {
        return GetHexPosition(checkVec.x, checkVec.y);
    }

    /// <summary>
    ///  Gets the in world position of a grid value from X and Z values 
    /// </summary>
    public static Vector3 GetHexPosition(int x, int z)
    {
        Vector3 retVec = Vector3.zero;
        retVec.x = x * hexWidth;
        retVec.z = z * hexSize * 1.5f;
        if (hexGrid[x, z] != null)
            retVec.y = hexGrid[x, z].height;
        else
            retVec.y = -1;
        if (z % 2 != 0)
        {
            retVec.x += hexWidth / 2;
        }
        return retVec;
    }

    


    /// <summary>
    /// Gets the in grid values from a real world position 
    /// </summary>
    public static Vector3Int GetGridPosition(Vector3 position)
    {
        Vector3Int retVec = new Vector3Int();
        retVec.z = Mathf.RoundToInt(position.z / (hexSize * 1.5f));
        if (retVec.z % 2 != 0)
            position.x -= hexWidth / 2;
        retVec.x = Mathf.RoundToInt(position.x / hexWidth);
        retVec.x = Mathf.Clamp(retVec.x, 0, gridX - 1);
        retVec.z = Mathf.Clamp(retVec.z, 0, gridZ - 1);
        return retVec;
    }

    /// <summary>
    /// Gets the in grid values from a real world position returned as a Vector2Int
    /// </summary>
    public static Vector2Int GetGridPosition2D(Vector3 position)
    {
        Vector2Int retVec = new Vector2Int();
        retVec.y = Mathf.RoundToInt(position.z / (hexSize * 1.5f));
        if (retVec.y % 2 != 0)
            position.x -= hexWidth / 2;
        retVec.x = Mathf.RoundToInt(position.x / hexWidth);
        retVec.x = Mathf.Clamp(retVec.x, 0, gridX - 1);
        retVec.y = Mathf.Clamp(retVec.y, 0, gridZ - 1);
        return retVec;
    }


    /// <summary>
    /// Returns a realworld position rounded to fit with the hex grid 
    /// </summary>
    public static Vector3 GetAsGridPosition(Vector3 position)
    {
        Vector3Int gridPos;
        gridPos = GetGridPosition(position);
        return GetHexPosition(gridPos);
    }

    /// <summary>
    /// Converts from my lazy offset grid to functional cubic co-ords
    /// </summary>
    public static Vector3 ConvertToCubicPosition(Vector2Int gridPos)
    {
        int x = (int)(gridPos.x - (gridPos.y - (gridPos.y & 1)) / 2f);
        int z = gridPos.y;
        int y = -x - z;
        return new Vector3(x, y, z);
    }



    /// <summary>
    /// Converts from cubic co-ords to my lazy offset grid
    /// </summary>
    public static Vector2Int ConvertToOffSetPosition(Vector3 cubePos)
    {
        int x =Mathf.RoundToInt((cubePos.x + (cubePos.z - (Mathf.RoundToInt(cubePos.z) & 1)) / 2f));
        int y =Mathf.RoundToInt(cubePos.z);
        return new Vector2Int(x, y);
    }

    ///<summary>
    ///Gets a list of offset co-ords within range of a grid point
    /// </summary>
    public static List<Vector2Int> GetHexOffSetInRange(Vector2Int position, int range)
    {
        Vector3 cubePos = ConvertToCubicPosition(position);
        List<Vector2Int> hexList = new List<Vector2Int>();

        for (int x = -range; x <= range; x ++)
        {
            for (int y = Mathf.Max(-range, -x-range); y <= Mathf.Min(range, -x+range); y ++)
            {
                int z = -x - y;
                hexList.Add(ConvertToOffSetPosition(new Vector3(x, y, z)));
            }
        }
        return hexList;
    }

    /// <summary>
    /// Gets a list of hexes in range of the given position
    /// </summary>
    /// <param name="position"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public static List<Vector2Int> GetHexInRange(Vector2Int position, int range)
    {
        Vector3 cubePos = ConvertToCubicPosition(position);
        Vector3 worldPos = GetHexPosition(position);
        List<Vector2Int> hexList = new List<Vector2Int>();
        int nums = 0;
        for (int x = -range; x <= range; x++)
        {
            for (int y = Mathf.Max(-range, -x - range); y <= Mathf.Min(range, -x + range); y++)
            {
                int z = -x - y;

                nums++;
                //Debug.Log(cubeHexPos);
                Vector2Int hexPos = ConvertToOffSetPosition(new Vector3(cubePos.x + x,cubePos.y + y, cubePos.z + z));
                hexList.Add(hexPos);
            }
        }
        return hexList;
    }


    /// <summary>
    /// Gets the distance in units that two points are on the gird
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static float GetHexDistance(Vector2Int origin, Vector2Int target)
    {
        Vector3 cubeOrigin = ConvertToCubicPosition(origin);
        Vector3 cubeTarget = ConvertToCubicPosition(target);
        return GetHexDistance(cubeOrigin, cubeTarget);
    }

    public static float GetHexDistance(Vector3 origin, Vector3 target)
    {

        return Mathf.Max(Mathf.Abs(origin.x - target.x), Mathf.Abs(origin.y - target.y), Mathf.Abs(origin.z - target.z));
    }


    public static List<Vector2Int> GetHexesInLine(Vector3 origin, Vector3 target)
    {
        List<Vector2Int> lineHexes = new List<Vector2Int>();
        float distance = GetHexDistance(origin, target);
        for (int i = 0; i <= distance; i++)
        {
            Vector2Int pos;
            Vector3 cubePos = VectorLerp(origin, target, 1 / distance * i);
            pos = ConvertToOffSetPosition(cubePos);
            //Debug.Log("Cube: " + cubePos + "  Offset: " + pos);
            lineHexes.Add(pos);
        }
        return lineHexes;
    }

    public static List<Vector2Int> GetHexesInLine(Vector2Int origin, Vector2Int target)
    {
        
        Vector3 cubeOrigin = ConvertToCubicPosition(origin);
        Vector3 cubeTarget = ConvertToCubicPosition(target);

        return GetHexesInLine(cubeOrigin, cubeTarget);
    }




    public static List<Vector2Int> GetGridSide(GridSide side)
    {
        List<Vector2Int> sideList;
        sideList = new List<Vector2Int>();

        switch (side)
        {
            case GridSide.xMin:
                for (int i = 0; i < gridX; i ++)
                {
                    sideList.Add(new Vector2Int(i, 0));
                }   break;
            case GridSide.xMax:
                for (int i = 0; i < gridX; i ++)
                {
                    sideList.Add(new Vector2Int(i, gridZ - 1));
                }   break;
            case GridSide.zMin:
                for (int i = 0; i < gridZ; i ++)
                {
                    sideList.Add(new Vector2Int(0, i));
                }   break;
            case GridSide.zMax:
                for (int i = 0; i < gridZ; i ++)
                {
                    sideList.Add(new Vector2Int(gridX - 1, i));
                }   break;
            case GridSide.outter:
                foreach (Vector2Int hex in edgeHexes)
                {
                    if (GetHex(hex.x, hex.y).edge == EdgeType.outter)
                        sideList.Add(hex);
                }
                break;
        }
        return sideList;
    }

    public static bool IsGridEdge(Vector2Int pos)
    {
        if (pos.x == 0 || pos.y == 0)
            return true;
        if (pos.x == gridX - 1 || pos.y == gridZ - 1)
            return true;
        return false;
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    private static Vector3 VectorLerp(Vector3 a, Vector3 b, float t)
    {
        return new Vector3(Lerp(a.x, b.x, t), Lerp(a.y, b.y, t), Lerp(a.z, b.z, t));
    }

    #endregion

}


public class Hex
{
    public int gridX;
    public int gridZ;
    public int girdY;
    public int[] hexPoints;
    public float height;
    public Mesh mesh;
    public Transform combineTransform;
    public EdgeType edge;
    public int cost;

    public Hex()
    {
        hexPoints = new int[6];
        height = 0;
        cost = 0;
        edge = EdgeType.none;
    }

    public Hex(float height)
    {
        hexPoints = new int[6];
        this.height = height;
        edge = EdgeType.none;
        cost = 0;
    }
}




class HexGrid
{
    Hex[,] hexGrid;

    int gridX;
    int gridZ;

    int gridOffsetX;
    int gridOffsetZ;

    public HexGrid()
    {
        hexGrid = new Hex[0, 0];
        gridOffsetX = gridX / 2;
        gridOffsetZ = gridZ / 2;
    }

    public HexGrid(int gridX, int gridZ)
    {
        //Debug.Log("Grid x: " + gridX + " ? Grid y:" + gridZ);
        this.gridX = gridX;
        this.gridZ = gridZ;
        hexGrid = new Hex[gridX, gridZ];
        gridOffsetX = gridX / 2;
        gridOffsetZ = gridZ / 2;
    }

    public Hex this[int x, int z]
    {
        get
        {
            x = Mathf.Clamp(x, 0, gridX-1);
            z = Mathf.Clamp(z, 0, gridZ - 1);
            return hexGrid[x,z];
        }
        set
        {
            hexGrid[x,z] = value;
        }

    }

}


public class NoiseMap
{
    public int width;
    public int height;

    public float scale;
    public Vector2Int offSet;

    public float weight;

    public Texture2D noiseMap;
    public MeshRenderer noisePrieview;

    public NoiseMap()
    {
        width = TerrainGen.gridX;
        height = TerrainGen.gridZ;
        scale = 10;
        offSet = new Vector2Int(Random.Range(0, 100), Random.Range(0,100));
        weight = 1;
        noiseMap = new Texture2D(width, height);

    }

    

}


public enum EdgeType
{
    outter,
    inner,
    none
}

public enum GridSide
{
    xMin,
    xMax,
    zMin,
    zMax,
    outter,
    none
}

public enum GridPosition
{
    xCenter,
    x
}



public enum MapGen
{
    hold,
    none,
    start,
    terrain,
    terrainCon,
    pathing,
    started,
    done,
    
    redo
}