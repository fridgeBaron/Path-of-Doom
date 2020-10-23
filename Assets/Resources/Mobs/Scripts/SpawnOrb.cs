using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOrb : MonoBehaviour
{


    Vector3 target;
    Vector3 moveDir;
    MobSpawner spawner;
    float spawnPoints;


    public SpawnOrb(Vector3 target, MobSpawner spawner)
    {
        this.target = target;
        moveDir = MyMath.GetDirectionRatio(target, this.transform.position);
        this.spawner = spawner;
    }

    public void SetOrb(Vector3 target, MobSpawner spawner, float spawnPoints)
    {
        this.target = target;
        moveDir = MyMath.GetDirectionRatio(target, this.transform.position);
        this.spawner = spawner;
        this.spawnPoints = spawnPoints;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }


    void Move()
    {
        this.transform.Translate(moveDir * TerrainGen.hexSize * Time.deltaTime, Space.World);
        if (MyMath.calcDistance(target, transform.position) < 0.001f)
        {
            spawner.SpawnFromOrb(this, spawnPoints);
        }
    }
}
