using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class MyMath
{

    public static readonly Vector3Int[] hexOffSetGrid = new Vector3Int[]
        {
            //Odd
            new Vector3Int(1,0,0),
            new Vector3Int(1,0,1),
            new Vector3Int(0,0,1),
            new Vector3Int(-1,0,0),
            new Vector3Int(0,0,-1),
            new Vector3Int(1,0,-1),
            //Even
            new Vector3Int(1,0,0),
            new Vector3Int(0,0,1),
            new Vector3Int(-1,0,1),
            new Vector3Int(-1,0,0),
            new Vector3Int(-1,0,-1),
            new Vector3Int(0,0,-1)
        };
    public static readonly List<Vector2Int> hexOffSetGrid2D = new List<Vector2Int>
        {
            //Odd
            new Vector2Int(1,0),
            new Vector2Int(1,1),
            new Vector2Int(0,1),
            new Vector2Int(-1,0),
            new Vector2Int(0,-1),
            new Vector2Int(1,-1),
            //Even
            new Vector2Int(1,0),
            new Vector2Int(0,1),
            new Vector2Int(-1,1),
            new Vector2Int(-1,0),
            new Vector2Int(-1,-1),
            new Vector2Int(0,-1)
        };
    public static readonly Vector3[] hexDirections = new Vector3[]
    {
        new Vector3(1,0,0),
        new Vector3(0.5f, 0, -0.8666f),
        new Vector3(0.5f, 0,  0.8666f),
        new Vector3(0,1,0),
        new Vector3(0,1,0),
        new Vector3(0,1,0)
    };

    public static readonly Vector3[] cubeHexDirections = new Vector3[]
    {
        new Vector3(1,-1,0),
        new Vector3(0,-1,1),
        new Vector3(-1,0,1),
        new Vector3(-1,1,0),
        new Vector3(0,1,-1),
        new Vector3(1,0,-1)
    };
    //new Vector3(1,0,0),
    //    new Vector3(0,1,0),
     //   new Vector3(0,0,1),
     //   new Vector3(-1,0,0),
    //    new Vector3(0,-1,0),
     //   new Vector3(0,0,-1)

    public static readonly Color[] rayColors = new Color[]
    {
        Color.red,
        new Color(0.8f,0.55f, 0.05f),
        Color.yellow,
        Color.green,
        new Color(0.05f, 0.8f, 0.8f),
        Color.blue
    };

    

    static float UVHexWidth = Mathf.Sqrt(3);

    public static float calcDistance(Vector3 a, Vector3 b, Vector3 checkAxis)
    {
        float distance = 0;
        a.x *= checkAxis.x;
        a.y *= checkAxis.y;
        a.z *= checkAxis.z;

        b.x *= checkAxis.x;
        b.y *= checkAxis.y;
        a.z *= checkAxis.z;

        distance = calcDistance(a, b);
        return distance;
    }


    public static float CheapDistance(Vector3 a, Vector3 b, Vector3 checkAxis)
    {
        float distance = 0;
        a.x *= checkAxis.x;
        a.y *= checkAxis.y;
        a.z *= checkAxis.z;

        b.x *= checkAxis.x;
        b.y *= checkAxis.y;
        a.z *= checkAxis.z;

        distance = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);
        return distance;
    }

    public static float calcDistance(Vector2 a, Vector2 b)
    {
        float distance;
        distance = Mathf.Sqrt(Mathf.Pow((a.x - b.x), 2) + Mathf.Pow((a.y - b.y), 2));
        return distance;
    }

    public static float calcDistance(Vector3 a, Vector3 b)
    {
        float distance = 0;
        distance = Mathf.Pow(b.x - a.x, 2) + Mathf.Pow(b.y - a.y, 2) + Mathf.Pow(b.z - a.z, 2) / 2;

        return distance;
    }


    public static Vector3 GetDirectionRatio(Vector3 a, Vector3 b, Vector3 checkAxis = new Vector3())
    {

        Vector3 retVec;
        retVec = Vector3.zero;

        if (checkAxis != new Vector3())
        {
            a.x *= checkAxis.x;
            a.y *= checkAxis.y;
            a.z *= checkAxis.z;

            b.x *= checkAxis.x;
            b.y *= checkAxis.y;
            a.z *= checkAxis.z;
        }

        float cap;

        retVec.x = a.x - b.x;
        retVec.y = a.y - b.y;
        retVec.z = a.z - b.z;

        cap = Mathf.Abs(retVec.x) + Mathf.Abs(retVec.y) + Mathf.Abs(retVec.z);
        retVec.x = retVec.x / cap;
        retVec.y = retVec.y / cap;
        retVec.z = retVec.z / cap;

        return retVec;
    }

    public static Vector3 SnapToSub(int sub, Vector3 position)
    {
        position.x = Mathf.RoundToInt(position.x * sub);
        position.y = Mathf.RoundToInt(position.y * sub);
        position.x /= sub;
        position.y /= sub;

        //Debug.Log(position.x + " / " + position.y);

        return position;
    }

    public static Vector2 GetUVSubdivision(Vector2Int position, Vector2Int size, UVCorners corner)
    {
        float xFraction;
        float yFraction;
        xFraction = 1f / size.x;
        yFraction = 1f / size.y;
        Vector2 cornerPosition = Vector2.zero;
        cornerPosition.x = xFraction * position.x;
        cornerPosition.y = 1 - yFraction * position.y;

        switch (corner)
        {
            case UVCorners.TopRight:
                cornerPosition.x += xFraction; break;
            case UVCorners.BottomLeft:
                cornerPosition.y -= yFraction; break;
            case UVCorners.BottomRight:
                cornerPosition.x += xFraction;
                cornerPosition.y -= yFraction; break;
        }
        return cornerPosition;
    }

    public static Vector2 GetUVSubdivision(Vector2Int position, Vector2Int size, UVCornersHex corner)
    {
        float xFraction;
        float yFraction;
        xFraction = 1f / size.x;
        yFraction = 1f / size.y;
        Vector2 cornerPosition = Vector2.zero;
        cornerPosition.x = xFraction * position.x;
        cornerPosition.y = 1 - yFraction * position.y;

        switch (corner)
        {
            case UVCornersHex.Center:
                cornerPosition.x += xFraction / 2;
                cornerPosition.y -= yFraction / 2; break;
            case UVCornersHex.Top:
                cornerPosition.x += xFraction / 2;
                cornerPosition.y -= yFraction; break;
            case UVCornersHex.UpperRight:
                cornerPosition.y -= yFraction * 0.75f; break;
            case UVCornersHex.LowerRight:
                cornerPosition.y -= yFraction * 0.25f; break;
            case UVCornersHex.Bottom:
                cornerPosition.x += xFraction /2; break;
            case UVCornersHex.LowerLeft:
                cornerPosition.x += xFraction;
                cornerPosition.y -= yFraction * 0.25f; break;
            case UVCornersHex.UpperLeft:
                cornerPosition.x += xFraction;
                cornerPosition.y -= yFraction * 0.75f; break;
        }
    



        return cornerPosition;
    }

    /// <summary>
    /// Returns the 4 corners of a sprite in its atlas. Top left, top right, bottom left, bottom right.
    /// </summary>
    /// <param name="position">The position of the desired sprite in the atlas in (x,y)</param>
    /// <param name="size">The total number of sprites in each axis of the atlas</param>
    /// <returns></returns>
    public static Vector2[] GetUVSubdivision(Vector2Int position, Vector2Int size)
    {
        Vector2[] corners = new Vector2[4];
        corners[0] = GetUVSubdivision(position, size, UVCorners.TopLeft);
        corners[1] = GetUVSubdivision(position, size, UVCorners.TopRight);
        corners[2] = GetUVSubdivision(position, size, UVCorners.BottomLeft);
        corners[3] = GetUVSubdivision(position, size, UVCorners.BottomRight);

        return corners;
    }

    public static Vector2[] GetUVSubdivisionHex(Vector2Int position, Vector2Int size)
    {
        Vector2[] corners = new Vector2[7];
        corners[0] = GetUVSubdivision(position, size, UVCornersHex.Top);
        corners[1] = GetUVSubdivision(position, size, UVCornersHex.UpperRight);
        corners[2] = GetUVSubdivision(position, size, UVCornersHex.LowerRight);
        corners[3] = GetUVSubdivision(position, size, UVCornersHex.Bottom);
        corners[4] = GetUVSubdivision(position, size, UVCornersHex.LowerLeft);
        corners[5] = GetUVSubdivision(position, size, UVCornersHex.UpperLeft);
        corners[6] = GetUVSubdivision(position, size, UVCornersHex.Center);

        return corners;
    }



    


    public static bool IsWithin(int val, int min, int max)
    {
        if (val < max && val > min)
            return true;
        return false;
    }
}

public enum UVCorners
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public enum UVCornersHex
{
    Top,
    UpperRight,
    LowerRight,
    Bottom,
    LowerLeft,
    UpperLeft,
    Center
}
