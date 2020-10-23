using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexHighlighter : MonoBehaviour
{

    [SerializeField]
    Sprite[] sprites;

    public static PathType showingPath;
    public static GridType showingType;

    public static HexHighlighter[,] highlighterGrid;

    public static List<HexHighlighter> changed = new List<HexHighlighter>();
    public static HexHighlight defaultHighlight = HexHighlight.grid;

    SpriteRenderer sprRenderer;

    // Start is called before the first frame update
    void Awake()
    {
        showingPath = PathType.none;
        showingType = GridType.grid;
        sprRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Set(HexHighlight type)
    {
        if (sprRenderer == null)
        {
            sprRenderer = GetComponent<SpriteRenderer>();
        }
        if (sprRenderer == null)
            return;
        sprRenderer.sprite = sprites[(int)type];
  
    }

    public static void CreateGrid(int gridX, int gridZ, GameObject hexHighlighter, GameObject highlightParent)
    {
        changed = new List<HexHighlighter>();
        highlighterGrid = new HexHighlighter[gridX, gridZ];
        for (int x = 0; x < gridX; x ++)
        {
            for (int z = 0; z < gridZ; z ++)
            {
                GameObject newHighlighter;
                newHighlighter = Instantiate(hexHighlighter);
                Vector3 hexPos = TerrainGen.GetHexPosition(x, z);
                hexPos.y += TerrainGen.hexSize / 2;
                newHighlighter.transform.position = hexPos;
                HexHighlighter hexHighlighterScr = newHighlighter.GetComponent<HexHighlighter>();
                if (hexPos.y > 0)
                    hexHighlighterScr.Set(HexHighlight.grid);
                else
                {
                    hexHighlighterScr.Set(HexHighlight.none);
                    newHighlighter.SetActive(false);
                }
                highlighterGrid[x, z] = hexHighlighterScr;
                newHighlighter.transform.parent = highlightParent.transform;
            }
        }
    }

    public static void ResetGrid()
    {
        while (changed.Count > 0)
        {
            changed[0].Set(defaultHighlight);
            changed.RemoveAt(0);
        }
        showingPath = PathType.none;
        showingType = GridType.none;
        
        if (TerrainGen.mapGenState == MapGen.terrainCon)
        {
            TerrainGen.ShowEdgeHexes();
        }
    }

    public static void ShowTowersRange(bool showBad = false)
    {
        TowerBase[] towers = TerrainGen.instance.GetComponentsInChildren<TowerBase>();
        List<Vector2Int> goodHexes = new List<Vector2Int>();
        List<Vector2Int> badHexes = new List<Vector2Int>();
        List<Vector2Int> neutralHexes = new List<Vector2Int>();

        foreach (TowerBase tower in towers)
        {
            foreach (Vector2Int hex in tower.GetHexes())
            {
                if (!goodHexes.Contains(hex))
                    goodHexes.Add(hex);
            }
            if (showBad)
            {
                foreach (Vector2Int hex in tower.GetBlockedHexes())
                {
                    if (!badHexes.Contains(hex))
                        badHexes.Add(hex);
                }
            }
        }
        if (showBad)
        {
            foreach (Vector2Int hex in badHexes)
            {
                if (goodHexes.Contains(hex))
                {
                    neutralHexes.Add(hex);
                    goodHexes.Remove(hex);
                }
                    
            }
            if (neutralHexes.Count > 0)
            {
                foreach (Vector2Int hex in neutralHexes)
                {
                    badHexes.Remove(hex);
                }
            }
        }
        foreach(Vector2Int hex in goodHexes)
        {
            Set(hex, HexHighlight.positive);
        }
        if (showBad)
        {
            foreach (Vector2Int hex in badHexes)
            {
                Set(hex, HexHighlight.negative);
            }
            foreach (Vector2Int hex in neutralHexes)
            {
                Set(hex, HexHighlight.neutral);
            }
        }
    }

    public static void ShowHeightMap(float startY = 0.5f)
    {
        float stepSize = (TerrainGen.gridY) * TerrainGen.hexSize / 2;
        for (int x = 0; x < TerrainGen.gridX; x ++)
        {
            for (int z = 0; z < TerrainGen.gridZ; z ++)
            {
                //
                Vector2Int hex = new Vector2Int(x, z);
                float hexHeight = TerrainGen.GetHexHeight(hex);
                int steps = Mathf.RoundToInt( (hexHeight - startY)/ (TerrainGen.hexSize / 2));
                steps = 5 + Mathf.Abs(steps % 10);
                Set(hex, (HexHighlight)steps);
            }
        }
    }

    public static void Set(Vector2Int pos, HexHighlight type)
    {
        if (!MyMath.IsWithin(pos.x, -1, TerrainGen.gridX) || !MyMath.IsWithin(pos.y, -1, TerrainGen.gridZ))
            return;
        highlighterGrid[pos.x, pos.y].Set(type);
        if (type != defaultHighlight)
            changed.Add(highlighterGrid[pos.x, pos.y]);
        else if (changed.Contains(highlighterGrid[pos.x, pos.y]))
            changed.Remove(highlighterGrid[pos.x, pos.y]);
    }
}



public enum HexHighlight
{
    selected,
    positive,
    negative,
    neutral,
    grid,
    height_0,
    height_1,
    height_2,
    height_3,
    height_4,
    height_5,
    height_6,
    height_7,
    height_8,
    heghtt_9,
    arrow_0,
    arrow_1,
    arrow_2,
    arrow_3,
    arrow_4,
    arrow_5,
    none

}

public enum GridType
{
    none,
    grid,
    path,
    range,
    height
}
