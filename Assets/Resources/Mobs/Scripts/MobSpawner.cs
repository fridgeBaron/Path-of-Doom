using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobSpawner : MonoBehaviour {

    public int spawnNumber;
    public float spawnTimer;
    public float spawnCoolDown;
    float spawnPoints;
    public GameObject spawnType;
    GameObject goal;

    [SerializeField]
    bool debug;


    float firstSpawnTime;
    float spawnTime;
    public float coolDownTime;
    public float wavePoints;
    public float wavePointsMult;
    public bool mobsMustDie;

    List<mobBase> mobs;
    List<Vector2Int> spawnHexes;
    Dictionary<Vector2Int, ParticleSystem> hexParticles;
    Text timer;

    [SerializeField]
    public GameObject spawnParticleFab;
    [SerializeField]
    public GameObject spawnOrbFabHolder;
    public static GameObject spawnOrbFab;

    static List<SpawnOrb> spawnOrbs;
    static List<SpawnOrb> deadSpawnOrbs;

    public GridSide gridSide;

    static List<MobSpawner> spawners;

	// Use this for initialization
	void Start () {
        if (spawners == null)
        {
            spawners = new List<MobSpawner>();
            spawners.Add(this);
            deadSpawnOrbs = new List<SpawnOrb>();
            spawnOrbs = new List<SpawnOrb>();
            spawnOrbFab = spawnOrbFabHolder;
        }
        else
            spawners.Add(this);
        spawnTime = 0;
        spawnCoolDown = 0;
        spawnPoints = wavePoints;
        firstSpawnTime = 0;
        hexParticles = new Dictionary<Vector2Int, ParticleSystem>();
        mobs = new List<mobBase>();
        timer = GetComponentInChildren<Text>();
        mobsMustDie = false;
    }

    public void MakeReady(GameObject goal)
    {
        this.goal = goal;
        GetSpawnHexes();
        MakeParticleSystems();
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        if (TerrainGen.terrainGenerated && spawnHexes != null)
        {
            if (firstSpawnTime < 0)
            {


                if ((mobsMustDie && mobs.Count == 0) || !mobsMustDie)
                    spawnCoolDown += Time.deltaTime;
                if (spawnCoolDown >= coolDownTime)
                {
                    SpawnOrb(wavePoints);
                    spawnCoolDown = 0;
                    wavePoints *= wavePointsMult;
                }
                spawnTime += Time.deltaTime;
                if (spawnTime > spawnTimer)
                {
                    SpawnMobs();
                    spawnTime = 0;
                }
            }
            else
                firstSpawnTime -= Time.deltaTime;
        }
        //
    }

    public void GetSpawnHexes()
    {
        if (gridSide != GridSide.none)
        {
            spawnHexes = TerrainGen.GetGridSide(gridSide);
        }

    }

    
    public void MakeParticleSystems()
    {
        hexParticles = new Dictionary<Vector2Int, ParticleSystem>();
        if (spawnHexes != null && spawnHexes.Count > 0)
        {
            GameObject newSystem;
            foreach (Vector2Int hex in spawnHexes)
            {
                newSystem = Instantiate(spawnParticleFab, this.transform) as GameObject;
                hexParticles.Add(hex, newSystem.GetComponent<ParticleSystem>());
                newSystem.transform.position = TerrainGen.GetHexPosition(hex.x ,hex.y);
            }
        }
    }

    public void GetParticleSystems()
    {
        ParticleSystem[] spawnParticles = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem system in spawnParticles)
        {
            Vector2Int gridPos;
            gridPos = TerrainGen.GetGridPosition2D(system.transform.position);
            if (!hexParticles.ContainsKey(gridPos))
                hexParticles.Add(gridPos, system);
        }
    }

    public mobBase SpawnMob(ref float mobPoints,Vector2Int hex = new Vector2Int(), string nextMob = "")
    {
        Vector3 newPos;
        if (hex == new Vector2Int())
            newPos = TerrainGen.GetHexPosition(spawnHexes[Random.Range(0, spawnHexes.Count)]);
        else
            newPos = TerrainGen.GetHexPosition(hex);
        if (nextMob == "")
            nextMob = Mobs.instance.getRandomMob();
        mobBase newMob;
        newMob = MobLister.GetDeadMob();
        newMob.goal = goal;
        newMob.gameObject.SetActive(true);
        mobPoints -= newMob.SetMob(nextMob);
        mobs.Add(newMob);
        newMob.SetSpawner(this);
        newMob.goal = goal;
        //newMob.transform.position = newPos;
        newMob.SetAgentPosition(newPos);
        //newMob.SetAgentDestination(TerrainGen.goal.transform.position);
        Vector2Int gridPos = TerrainGen.GetGridPosition2D(newPos);
        if (hexParticles.ContainsKey(gridPos))
        {
            ParticleSystem spawnSystem;
            spawnSystem = hexParticles[gridPos];
            ParticleSystem.Burst burst;
            burst = new ParticleSystem.Burst(0, (int)mobPoints);
            spawnSystem.emission.SetBurst(0, burst);
            spawnSystem.time = 0;
        }
        return newMob;
    }

    public void SpawnMobs()
    {
        float spawnPoints = 0;
        if (spawnNumber > 0)
        {
            for (int i = 0; i < spawnNumber; i ++)
            {
                SpawnMob(ref spawnPoints);
            }
        }
    }

    public void SpawnOrb(float spawnPoints)
    {
        Vector3 target = TerrainGen.GetHexPosition(spawnHexes[Random.Range(0, spawnHexes.Count)]);
        Vector3 spawnPoint = target;
        spawnPoint.y += TerrainGen.hexSize * coolDownTime;
        SpawnOrb spawnOrb = GetOrb();
        spawnOrb.transform.position = spawnPoint;
        spawnOrb.SetOrb(target, this, spawnPoints);
    }

    public void SpawnFromOrb(SpawnOrb orb, float spawnPoints)
    {
        
        Vector2Int hex = TerrainGen.GetGridPosition2D(orb.transform.position);
        List<Vector2Int> hexes = TerrainGen.GetHexInRange(hex, 3);
        
        string mobType = Mobs.instance.getRandomMob();
        mobType = "slime";
        while (spawnPoints > 0)
        {
            hex = hexes[Random.Range(0, hexes.Count)];
            while (TerrainGen.GetHex(hex.x,hex.y) == null)
                hex = hexes[Random.Range(0, hexes.Count)];
            SpawnMob(ref spawnPoints, hex, mobType);
        }
  
        KillOrb(orb);
    }

    public Dictionary<string, float> GetDetails()
    {
        Dictionary<string, float> details = new Dictionary<string, float>();


        if (spawnTimer != 0)
            details.Add("spawn_time", spawnTimer);
        if (spawnNumber != 0)
            details.Add("spawn_number", spawnNumber);
        if (coolDownTime != 0)
            details.Add("spawn_cooldown", coolDownTime);
        if (wavePoints != 0)
            details.Add("wave_points", wavePoints);
       
        return details;
    }

    public static MobSpawner GetSpawner(Vector2Int hex)
    {
        MobSpawner retSpawner = null;
        if (spawners != null)
        {
            foreach (MobSpawner spawner in spawners)
            {
                if (spawner.spawnHexes != null)
                {
                    if (spawner.spawnHexes.Contains(hex))
                    {
                        return spawner;
                    }
                }
            }
        }
            
        return retSpawner;
    }

    public static List<MobSpawner> GetSpawners()
    {
        return spawners;
    }

    public  void RemoveMob(mobBase mob)
    {
        if (mobs.Contains(mob))
        {
            mobs.Remove(mob);
        }
    }

    public static SpawnOrb GetOrb()
    {
        SpawnOrb newOrb;
        if (deadSpawnOrbs.Count > 0)
        {
            newOrb = deadSpawnOrbs[0];
            newOrb.gameObject.SetActive(true);
            deadSpawnOrbs.RemoveAt(0);
            spawnOrbs.Add(newOrb);
            return newOrb;
        }

        newOrb = Instantiate(spawnOrbFab).GetComponent<SpawnOrb>();
        spawnOrbs.Add(newOrb);
        return newOrb;
    }

    public static void KillOrb(SpawnOrb orb)
    {
        if (spawnOrbs.Contains(orb))
        {
            spawnOrbs.Remove(orb);
        }
        else
        {
            if (!deadSpawnOrbs.Contains(orb))
                deadSpawnOrbs.Add(orb);
        }
        orb.gameObject.SetActive(false);
    }
}
