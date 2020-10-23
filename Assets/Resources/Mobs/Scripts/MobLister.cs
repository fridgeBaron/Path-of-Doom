using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MobLister {

    static GameObject deadMobs = GameObject.Find("DeadMobs");
    static GameObject livingMobs = GameObject.Find("LivingMobs");
    static GameObject allMobs = GameObject.Find("Mobs");

    private static List<mobBase> mobList = new List<mobBase>();
    private static List<mobBase> deadMobList = new List<mobBase>();

    private static GameObject mobFab = Resources.Load("Mobs/basicMob") as GameObject;

    public static List<mobBase>[,] mobGrid;


    public static void MakeModGrid(int gridX, int gridZ)
    {
        mobGrid = new List<mobBase>[gridX, gridZ];

        for (int x = 0; x < gridX; x ++)
        {
            for (int z = 0; z < gridZ; z ++)
            {
                mobGrid[x, z] = new List<mobBase>();
            }
        }
    }

    public static void MoveMobOnGrid(mobBase mob, Vector2Int from, Vector2Int to)
    {
        mobGrid[from.x, from.y].Remove(mob);
        mobGrid[to.x, to.y].Add(mob);
    }

    public static void RemoveMobFromGrid(mobBase mob, Vector2Int from)
    {
        mobGrid[from.x, from.y].Remove(mob);
    }

    public static List<mobBase> GetMobList(Vector2Int pos)
    {
        if (pos.x > 0 && pos.x < TerrainGen.gridX && pos.y > 0 && pos.y < TerrainGen.gridZ)
            return mobGrid[pos.x, pos.y];
        return null;
    }


    public static void AddMob(mobBase newMob)
    {
        mobList.Add(newMob);
    }
    public static void RemoveMob(mobBase oldMob)
    {
        Vector2Int gridPos = TerrainGen.GetGridPosition2D(oldMob.thisHex);
        RemoveMobFromGrid(oldMob, gridPos);
        mobList.Remove(oldMob);
        oldMob.transform.SetParent(deadMobs.transform);
        oldMob.gameObject.SetActive(false);
        deadMobList.Add(oldMob);
        allMobs.name = "Mobs: (" + mobList.Count + " / " + deadMobList.Count + ") = " + (deadMobList.Count + mobList.Count);
        //Debug.Log("Dead mobs " + deadMobList.Count);
    }
    public static List<mobBase> GetMobsInRange(Vector3 position, float range)
    {
        List<mobBase> retList = new List<mobBase>();

        foreach (mobBase mob in mobList)
        {
            
            if (MyMath.calcDistance(position, mob.transform.position, new Vector3(1,0,1)) < range)
            {
                retList.Add(mob);
            }
        }

        return retList;
    }
    public static mobBase GetDeadMob()
    {
        mobBase retMob;
        
        if (deadMobList.Count > 0)
        {
            retMob = deadMobList[0];
            deadMobList.RemoveAt(0);
            mobList.Add(retMob);
            retMob.transform.SetParent(livingMobs.transform);
            allMobs.name = "Mobs: (" + mobList.Count + " / " + deadMobList.Count + ") = " + (deadMobList.Count + mobList.Count);
            return retMob;
        }
        GameObject newMob;
        newMob = GameObject.Instantiate<GameObject>(mobFab);
        retMob = newMob.GetComponent<mobBase>();
        mobList.Add(retMob);
        retMob.transform.SetParent(livingMobs.transform);
        allMobs.name = "Mobs: (" + mobList.Count + " / " + deadMobList.Count + ") = " + (deadMobList.Count + mobList.Count);
        return retMob;
    }
}
