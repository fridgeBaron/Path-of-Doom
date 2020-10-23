using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeGenerator : MonoBehaviour
{


    [SerializeField]
    int numShapes;
    [SerializeField]
    int forestShapes;
    [SerializeField]
    Vector2 forestScale;
    [SerializeField]
    int lakeShapes;
    [SerializeField]
    Vector2 lakeScale;
    [SerializeField]
    float randomSize;

    [SerializeField]
    Texture2D shapeMap;
    Texture2D[] shapes;



    int frames;


    Texture2D islandTex;

    public MeshRenderer display;

    // Start is called before the first frame update
    void Start()
    {
        islandTex = new Texture2D(TerrainGen.gridX, TerrainGen.gridZ);
        SplitTextureMap();
        frames = 0;

    }

    public void GenerateForDisplay()
    {
        FillTexture(islandTex, Color.white);
        BlitShapes(islandTex, numShapes);
        GenerateLake(islandTex, lakeShapes);
        GenerateForests(islandTex, forestShapes);
        
        display.material.mainTexture = islandTex;
    }

    public void GenerateLake(Texture2D texture, int count)
    {
        Vector2Int lakePos = new Vector2Int();
        lakePos.x = Random.Range(texture.width/4,(int) (texture.width * 0.75f));
        lakePos.y = Random.Range(texture.height / 4, (int)(texture.height * 0.75f));
        for (int i = 0; i < count; i++)
        {
            Vector2Int offSet = Vector2Int.zero;
            Vector2 scale = Vector2.one / 2;
            scale *= Random.Range(lakeScale.x, lakeScale.y);
            offSet.x = (int)(Random.Range(-texture.width/10, texture.width / 10) * scale.x + lakePos.x);
            offSet.y = (int)(Random.Range(-texture.height/10,texture.height /10) * scale.y + lakePos.y);
            BlitShape(shapes[Random.Range(0, shapes.Length)], texture, scale, offSet, Color.blue, false);
        }//
    }

    public void GenerateForests(Texture2D texture, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2Int offSet = Vector2Int.zero;
            Vector2 scale = Vector2.one / 2;
            scale *= Random.Range(lakeScale.x, lakeScale.y);
            offSet.x = Random.Range(0, (int)(texture.width - (64 * scale.x)));
            offSet.y = Random.Range(0, (int)(texture.height - (64 * scale.y)));
            BlitShape(shapes[Random.Range(0, shapes.Length)], texture, scale, offSet, Color.green, false);
        }//
    }

    public Texture2D GenerateClipMap()
    {
        Texture2D clipMap = new Texture2D(TerrainGen.gridX, TerrainGen.gridZ);
        FillTexture(clipMap, Color.white);
        BlitShapes(clipMap, numShapes);
        return clipMap;
    }

    private void FillTexture(Texture2D tex, Color color)
    {
        for (int x = 0; x < tex.width; x ++)
        {
            for (int y = 0; y < tex.height; y ++)
            {
                tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();
    }

    private void BlitShapes(Texture2D texture,int numShapes)
    {
        for(int i = 0; i < numShapes; i ++)
        {
            Vector2Int offSet = Vector2Int.zero;
            Vector2 scale = Vector2.one/2;
            scale *= Random.Range(0.4f, 1.3f);
            offSet.x = Random.Range(0, (int)(texture.width - (64 * scale.x)));
            offSet.y = Random.Range(0,(int)(texture.height - (64 * scale.y)));
            BlitShape(shapes[Random.Range(0, shapes.Length)], texture, scale, offSet, Color.black, true);
        }//
        
    }

    private void BlitShape(Texture2D source, Texture2D target, Vector2 scale, Vector2Int offSet,Color colour, bool blitAny)
    {
        for(int x = 0; x < source.width * scale.x; x ++)
        {
            for(int y = 0; y < source.height * scale.y; y ++)
            {
                if (x + offSet.x < target.width && y + offSet.y < target.height)
                {
                    if (source.GetPixel(Mathf.RoundToInt(x/scale.x),Mathf.RoundToInt(y/scale.y)).grayscale == 1)
                    {
                        if (target.GetPixel(x + offSet.x,y + offSet.y).grayscale == 0 || blitAny)
                            target.SetPixel(x + offSet.x, y + offSet.y, colour);
                    }
                    
                }
            }
        }
        target.Apply();
    }


    private void SplitTextureMap()
    {
        int subSize = 64;
        int countX = shapeMap.width / subSize;
        int countY = shapeMap.height / subSize;
        int count = countX * countY;
        shapes = new Texture2D[count];
        int i = 0;
        for (int x = 0; x < countX; x ++)
        {
            for (int y = 0; y < countY; y ++)
            {
                shapes[i] = CopyTexturePart(shapeMap, x, y, subSize);
                i++;
            }
        }
    }
    private Texture2D CopyTexturePart(Texture2D texture, int xPos, int yPos, int size)
    {
        Texture2D retTex = new Texture2D(size, size);
        for (int x = 0; x < size; x ++)
        {
            for (int y = 0; y < size; y ++)
            {
                retTex.SetPixel(x, y, texture.GetPixel(xPos * size + x, yPos * size + y));
            }
        }
        retTex.Apply();

        return retTex;
    }

    // Update is called once per frame
    void Update()
    {
        frames++;
        if (frames > 60 && display != null)
        {
            GenerateForDisplay();
            frames = 0;
        }
    }
}
