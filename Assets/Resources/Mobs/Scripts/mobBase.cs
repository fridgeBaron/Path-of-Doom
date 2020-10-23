using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;

public class mobBase : MonoBehaviour
{

    //Nav
    NavMeshAgent agent;
    bool destinationSet;

    [SerializeField]
    float health;
    [SerializeField]
    float healthMax;
    [SerializeField]
    float shields;
    [SerializeField]
    float shieldsMax;
    [SerializeField]
    float speed;
    [SerializeField]
    float climbMult;
    [SerializeField]
    float reductionFlat;
    [SerializeField]
    float reductionPercent;
    [SerializeField]
    float size;
    [SerializeField]
    float worldSize;
    [SerializeField]
    float attackDamage;
    [SerializeField]
    float attackSpeed;
    float attackCooldown;
    int spriteNum;
    int deathParticles;

    healthTracker healthOrb;
    //
    [SerializeField]
    bool debug;

    protected static LayerMask buildingMask = 1 << 8;
    protected static LayerMask mobMask = 1 << 9;

    float gold;

    Camera gameCam;
    SpriteRenderer spriteRenderer;
    CapsuleCollider collider;

    [SerializeField]
    MotionType motionType;
    PathType moveType;

    float flyHeight;

    static int moveMultiplier = 1;

    string mobType;

    [SerializeField]
    public GameObject goal;

    [SerializeField]
    Vector3? nextHex;
    public Vector3 thisHex;
    BuildingBase attackTarget;

    [SerializeField]
    Vector3 faceDir;

    List<MobEffect> effectList;
    List<mobBase> clumpMobs;

    MobSpawner spawner;

    public static int mobID = 0;

    Vector3 position;
    Vector3 isoPosition;

    public float WorldSize
    {
        get
        {
            return worldSize;
        }

    }

    public float Speed
    {
        get
        {
            return speed;
        }
    }

    public mobBase(float health, float healthMax, float shields, float shieldsMax, float speed, float climbMult, float reductionFlat, float reductionPercent)
    {
        this.health = health;
        this.healthMax = healthMax;
        this.shields = shields;
        this.shieldsMax = shieldsMax;
        this.speed = speed;
        this.climbMult = climbMult;
        this.reductionFlat = reductionFlat;
        this.reductionPercent = reductionPercent;
    }

    public mobBase()
    {
        health = 100;
        healthMax = 100;
        shields = 0;
        shieldsMax = 0;
        speed = 1;
        reductionFlat = 0;
        reductionPercent = 0;
    }

    public float Health
    {
        get
        {
            return health;
        }

        set
        {
            health = value;
        }
    }



    // Use this for initialization
    private void OnEnable()
    {

        nextHex = null;
        healthOrb = GetComponent<healthTracker>();
        //healthOrb.SetHealth(1);
        effectList = new List<MobEffect>();
        clumpMobs = new List<mobBase>();
        collider = GetComponent<CapsuleCollider>();
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        destinationSet = false;

    }

    void Awake()
    {
        gameCam = Camera.allCameras[0];
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    void Start()
    {
        gameCam = Camera.allCameras[0];
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MoveMob();
        EndEffects();
        //FaceCamera();

    }

    void FaceCamera()
    {
        Vector3 renderAxis = gameCam.ViewportPointToRay(this.transform.position).GetPoint(5);
        renderAxis = this.transform.position - renderAxis;
        //renderAxis.x = 0;
        renderAxis.z = 0;
        spriteRenderer.transform.rotation = Quaternion.LookRotation(Vector3.forward, renderAxis);
    }


    void MoveMob()
    {
        MouseHook.mousehook.GetCamera();
        spriteRenderer.transform.LookAt(MouseHook.mousehook.GetCamera().transform.position, Vector3.up);
        Vector3 pos = transform.position;
        float currentSpeed = speed;
        if (effectList.Count > 0)
        {
            float highestSlow = 1;
            foreach (MobEffect effect in effectList)
            {
                if (effect.moveSpeedMult > 0)
                {
                    if (highestSlow > effect.moveSpeedMult)
                    {
                        highestSlow = effect.moveSpeedMult;
                    }

                }
            }
            currentSpeed *= highestSlow;
        }

        if (!destinationSet)
        {
            agent.enabled = true;
            agent.SetDestination(TerrainGen.goal.transform.position);
            destinationSet = true;
        }
            

        /*
        switch (moveType)
        {

            case PathType.normal:
                switch (motionType)
                {
                    case MotionType.move:
                        faceDir.y = 0;
                        if (debug)
                            Debug.DrawRay(this.transform.position, faceDir / 10);
                        if (!nextHex.HasValue)
                            nextHex = PathKeeper.GetNextHex(TerrainGen.GetGridPosition(this.transform.position));
                        else if (TerrainGen.GetAsGridPosition(this.transform.position) == nextHex.Value)
                        {
                            //
                            MobLister.MoveMobOnGrid(this, TerrainGen.GetGridPosition2D(thisHex), TerrainGen.GetGridPosition2D(nextHex.Value));
                            GetClump();
                            thisHex = nextHex.Value;
                            if (goal != null)
                            {
                                if (TerrainGen.GetGridPosition(thisHex) == TerrainGen.GetGridPosition(goal.transform.position))
                                {
                                    MobLister.RemoveMob(this);
                                    Player.player.TakeLife(1);
                                    return;
                                }
                            }
                            nextHex = PathKeeper.GetNextHex(TerrainGen.GetGridPosition(this.transform.position));
                            if (thisHex.y > this.transform.position.y)
                                motionType = MotionType.climb;
                            else if (thisHex.y < this.transform.position.y)
                                motionType = MotionType.fall;
                            else
                            {
                                FindTarget();
                            }
                        }

                        faceDir = MyMath.GetDirectionRatio(nextHex.Value, transform.position);
                        bool pathAround = false;
                        Vector3 otherPos = Vector3.zero;

                        //float myDistance = MyMath.calcDistance(transform.position, nextHex.Value);
                        float myDistance = Vector3.Distance(transform.position, nextHex.Value);
                        float mobDistance = 0;
                        Profiler.BeginSample("ClumpCheck");
                        for (int i = 0; i < clumpMobs.Count; i++)
                        {

                            mobBase otherMob = clumpMobs[i];
                            if (otherMob.moveType != PathType.flight)
                            {
                                //float distance = MyMath.calcDistance(transform.position, otherMob.transform.position);
                                float distance = Vector3.Distance(transform.position, otherMob.transform.position);
                                if (distance < otherMob.WorldSize + worldSize)
                                {
                                    //mobDistance = MyMath.calcDistance(otherMob.transform.position, nextHex.Value);
                                    mobDistance = Vector3.Distance(otherMob.transform.position, nextHex.Value);
                                    float difference = myDistance - mobDistance;
                                    if (difference > 0)
                                    {
                                        if (distance < (otherMob.WorldSize + worldSize) / 1.9)
                                        {
                                            if (debug)
                                                Debug.DrawLine(transform.position, otherMob.transform.position, Color.red);
                                            currentSpeed = (currentSpeed * distance / (otherMob.WorldSize + worldSize))/2;
                                            i = clumpMobs.Count;
                                        }
                                        else
                                        {
                                            if (debug)
                                                Debug.DrawLine(transform.position, otherMob.transform.position, new Color(0.8f, 0.55f, 0.05f));
                                            otherPos = otherMob.transform.position;
                                            pathAround = true;
                                            i = clumpMobs.Count;
                                        }
                                    }
                                }
                            }


                        }
                        Profiler.EndSample();
                        if (pathAround)
                        {
                            Vector3 faceAngle = -MyMath.GetDirectionRatio(transform.position, nextHex.Value, new Vector3(1, 0, 1));
                            Vector3? checkAngle = FindPath(faceAngle, 30, 4);
                            if (checkAngle.HasValue)
                            {
                                pos.x += (speed / moveMultiplier * checkAngle.Value.x) * currentSpeed * Time.deltaTime;
                                pos.z += (speed / moveMultiplier * checkAngle.Value.z) * currentSpeed * Time.deltaTime;
                            }
                            else
                            {
                                pos.x += (speed / moveMultiplier * faceDir.x) * currentSpeed * Time.deltaTime;
                                pos.z += (speed / moveMultiplier * faceDir.z) * currentSpeed * Time.deltaTime;
                            }
                        }
                        else
                        {
                            pos.x += (speed / moveMultiplier * faceDir.x) * currentSpeed * Time.deltaTime;
                            pos.z += (speed / moveMultiplier * faceDir.z) * currentSpeed * Time.deltaTime;
                        }


                        transform.position = pos;
                        break;
                    case MotionType.climb:

                        pos.y += (speed * climbMult / moveMultiplier) * currentSpeed * Time.deltaTime;
                        if (pos.y >= thisHex.y)
                        {
                            FindTarget();

                        }
                        transform.position = pos;
                        break;
                    case MotionType.fall:
                        pos.y -= 0.1f * Time.deltaTime;
                        pos.x += (speed / moveMultiplier * faceDir.x) * currentSpeed * Time.deltaTime;
                        pos.z += (speed / moveMultiplier * faceDir.z) * currentSpeed * Time.deltaTime;
                        if (pos.y <= thisHex.y)
                        {
                            pos.y = thisHex.y;
                            FindTarget();

                        }
                        transform.position = pos;
                        break;


                    case MotionType.attack:
                        Attack();
                        break;
                }
                break;
            case PathType.flight:
                switch (motionType)
                {
                    case MotionType.move:
                        if (debug)
                            Debug.DrawRay(this.transform.position, faceDir / 10);
                        if (!nextHex.HasValue)
                            nextHex = PathKeeper.GetNextHex(TerrainGen.GetGridPosition(this.transform.position));
                        else if (TerrainGen.GetAsGridPosition(this.transform.position) == nextHex.Value)
                        {
                            MobLister.MoveMobOnGrid(this, TerrainGen.GetGridPosition2D(thisHex), TerrainGen.GetGridPosition2D(nextHex.Value));
                            GetClump();
                            thisHex = nextHex.Value;
                            if (goal != null)
                            {
                                if (TerrainGen.GetGridPosition(thisHex) == TerrainGen.GetGridPosition(goal.transform.position))
                                {
                                    MobLister.RemoveMob(this);
                                    Player.player.TakeLife(1);
                                    return;
                                }
                            }

                            nextHex = PathKeeper.GetNextHex(TerrainGen.GetGridPosition(this.transform.position));
                            FindTarget();
                        }
                        Vector3 nextPos = nextHex.Value;
                        nextPos.y += flyHeight;
                        faceDir = MyMath.GetDirectionRatio(nextPos, transform.position);




                        pos.x += (speed / moveMultiplier * faceDir.x) * currentSpeed * Time.deltaTime;
                        pos.z += (speed / moveMultiplier * faceDir.z) * currentSpeed * Time.deltaTime;
                        pos.y += (speed / moveMultiplier * faceDir.y) * currentSpeed * Time.deltaTime;

                        transform.position = pos;
                        break;
                    case MotionType.attack:
                        Attack();
                        break;
                }
                break;
        }
        */
    }

    void EndEffects()
    {
        float time = Time.time;
        List<MobEffect> endList = new List<MobEffect>();
        foreach (MobEffect effect in effectList)
        {
            if (effect.endTime < time)
            {
                endList.Add(effect);
            }

        }
        MobEffect endEffect;
        while (endList.Count > 0)
        {
            endEffect = endList[0];
            effectList.Remove(endEffect);
            endList.RemoveAt(0);
        }

    }

    void GetClump()
    {
        Vector2Int gridPos = TerrainGen.GetGridPosition2D(this.transform.position);
        clumpMobs = new List<mobBase>();
        for (int i = 0; i < 6; i++)
        {
            int ind = i;
            if ((gridPos.y & 1) == 1)
                ind += 6;
            Vector2Int offPos = gridPos;
            offPos.x += MyMath.hexOffSetGrid[ind].x;
            offPos.y += MyMath.hexOffSetGrid[ind].y;
            foreach (mobBase mob in MobLister.mobGrid[offPos.x, offPos.y])
            {
                if (!clumpMobs.Contains(mob))
                    clumpMobs.Add(mob);
            }
        }
    }



    public bool DamageMob(float damage)
    {
        damage *= 1 - reductionPercent;
        damage -= reductionFlat;

        float remainder = 0;
        if (shields > 0)
        {
            shields -= damage;
            damage = 0;
        }
        if (shields < 0)
        {
            remainder = shields;
            shields = 0;
        }
        if (damage > 0)
        {
            health -= damage;
        }
        else
        {
            health -= remainder;
        }
        if (health <= 0)
        {
            KillMob();
            return true;
        }
        else
            healthOrb.SetHealth(health / healthMax);
        return false;
    }

    public void Attack()
    {
        attackCooldown += Time.deltaTime;
        if (attackTarget != null)
        {
            if (attackCooldown > attackSpeed)
            {
                attackCooldown = 0;
                if (attackTarget.TakeDamage(attackDamage))
                {
                    motionType = MotionType.move;
                    attackTarget = null;
                }
            }
        }
        else
        {
            motionType = MotionType.move;
        }
    }

    public void FindTarget(MotionType fallBack = MotionType.move)
    {
        RaycastHit hit;
        Vector3 castPos = TerrainGen.GetAsGridPosition(this.transform.position);
        castPos.y += 1;
        if (Physics.Raycast(castPos, Vector3.down, out hit, 3, buildingMask))
        {
            if (!hit.collider.transform.parent.name.Contains("Copse"))
            {
                BuildingBase hitTarget;
                hitTarget = hit.collider.transform.GetComponentInParent<BuildingBase>();
                if (hitTarget != null)
                {
                    motionType = MotionType.attack;
                    attackTarget = hitTarget;
                }

            }
            else
                motionType = fallBack;
        }
        else
            motionType = fallBack;
    }

    public void AddMobEffects(List<MobEffect> mobEffects)
    {
        foreach (MobEffect effect in mobEffects)
        {
            effect.endTime = Time.time + effect.duration;
            //Debug.Log(Time.time);
            effectList.Add(effect);

        }
    }

    void KillMob()
    {
        Player.player.GiveGold((int)gold);

        if (deathParticles != 0)
        {
            ParticleSystem newSystem = ParticleMaster.MakeSystem((DeathParticles)deathParticles);
            newSystem.transform.position = this.transform.position;

        }
        if (spawner != null)
            spawner.RemoveMob(this);
        MobLister.RemoveMob(this);
    }

    Vector3? FindPath(Vector3 faceAngle, float angle, int steps = 3)
    {
        Profiler.BeginSample("Path raycasting");
        Vector3 checkAngle = Vector3.zero;
        angle = Mathf.Deg2Rad * angle;
        for (int i = 1; i <= steps; i++)
        {
            checkAngle.x = Mathf.Cos(angle * i) * faceAngle.x - Mathf.Sin(angle * i) * faceAngle.z;
            checkAngle.z = Mathf.Sin(angle * i) * faceAngle.x + Mathf.Cos(angle * i) * faceAngle.z;
            if (debug)
                Debug.DrawRay(transform.position, checkAngle, MyMath.rayColors[i]);
            RaycastHit hit;
            if (!Physics.Raycast(transform.position, checkAngle, out hit, TerrainGen.hexSize, mobMask))
            {
                if (hit.collider != null)
                    Debug.Log(hit.collider.gameObject.name);
                Profiler.EndSample();
                return checkAngle;
            }
            checkAngle.x = Mathf.Cos(-angle * i) * faceAngle.x - Mathf.Sin(-angle * i) * faceAngle.z;
            checkAngle.z = Mathf.Sin(-angle * i) * faceAngle.x + Mathf.Cos(-angle * i) * faceAngle.z;
            if (debug)
                Debug.DrawRay(transform.position, checkAngle, MyMath.rayColors[i]);
            if (!Physics.Raycast(transform.position, checkAngle, out hit, TerrainGen.hexSize, mobMask))
            {
                Profiler.EndSample();
                return checkAngle;
            }
        }
        Profiler.EndSample();

        return null;
    }


    public float SetMob(string mob)
    {
        if (mobType == null)
            mobType = mob;
        this.gameObject.name = mob + mobID;
        mobID++;
        health = 0;
        healthMax = 0;
        shields = -1;
        shieldsMax = 0;
        speed = 0;
        climbMult = 0.2f;
        size = 0;
        reductionFlat = 0;
        reductionPercent = 0;
        attackSpeed = 0;
        attackDamage = 0;

        float spawnPoints = 0;
        float yoffSet = 0;
        int properties = 0;
        float totalRandom = 0;
        //
        bool random = false;
        float randomVal = 1;
        float randomMax = 0;
        List<int> sprites = new List<int>();

        if (Mobs.instance.GetMob(mob).ContainsKey("randomAll"))
        {
            random = true;
            randomMax = Mobs.instance.GetMob(mob)["randomAll"];
        }

        foreach (KeyValuePair<string, float> val in Mobs.instance.GetMob(mob))
        {
            if (random)
            {
                randomVal = Random.Range(1 - randomMax, 1 + randomMax);
                if (randomVal <= 0)
                    randomVal = randomMax;
            }
            string key = val.Key;
            if (val.Key.Contains("SPR_"))
            {
                key = key.Replace("SPR_", "");
                key = key.Substring(2);
            }
            bool incRandom = false;
            switch (key)
            {
                case "health":
                    health = val.Value * randomVal;
                    incRandom = true; break;
                case "healthMax":
                    healthMax = val.Value * randomVal;
                    incRandom = true; break;
                case "shields":
                    shields = val.Value * randomVal;
                    incRandom = true; break;
                case "shieldsMax":
                    shieldsMax = val.Value * randomVal;
                    incRandom = true; break;
                case "speed":
                    speed = val.Value * randomVal;
                    incRandom = true; break;
                case "attack_damage":
                    attackDamage = val.Value; break;
                case "attack_speed":
                    attackSpeed = val.Value; break;
                case "climbmult":
                    climbMult = val.Value; break;
                case "size":
                    size = val.Value; break;
                case "world_size":
                    worldSize = val.Value; break;
                case "gold":
                    gold = val.Value; break;
                case "sprite":
                    sprites.Add((int)val.Value); break;
                case "deathParticle":
                    deathParticles = (int)val.Value; break;
                case "spawnPoints":
                    spawnPoints = val.Value; break;
                case "yoff":
                    yoffSet = val.Value; break;
                case "flight":
                    flyHeight = val.Value;
                    moveType = PathType.flight;
                    break;
                case "walk":
                    moveType = PathType.normal; break;

                default: break;
            }
            if (incRandom)
            {
                properties++;
                totalRandom += randomVal;
            }
        }

        float variance = 0;
        variance = totalRandom / properties;
        size = size * variance * 10;
        worldSize = worldSize * variance;
        collider.radius = worldSize;
        attackDamage = attackDamage * variance;
        gold = gold * variance;
        if (health == 0)
            health = healthMax;
        if (shields == -1)
            shields = shieldsMax;
        spriteNum = Random.Range(0, sprites.Count);
        spriteNum = sprites[spriteNum];
        spriteRenderer.sprite = Mobs.mobSprites[spriteNum];
        spriteRenderer.gameObject.transform.localScale = new Vector3(size, size, size) / 10;
        SetAgentSize(size/100);
        agent.speed = speed/10;
        Vector3 newPos = spriteRenderer.transform.localPosition;
        newPos.y = (size + yoffSet) / 100;
        spriteRenderer.transform.localPosition = newPos;
        speed = speed * TerrainGen.hexSize;
        return spawnPoints * totalRandom;
    }

    public void SetAgentPosition(Vector3 pos)
    {
        agent.Warp(pos);
    }

    public void SetAgentDestination(Vector3 pos)
    {
        agent.SetDestination(pos);
    }

    public void SetAgentSize(float size)
    {
        agent.radius = size;
    }

    public NavMeshPath GetAgentPath()
    {
        return agent.path;
    }


    public void SetSpawner(MobSpawner newSpawner)
    {
        spawner = newSpawner;
    }
}

public enum MotionType
{
    move,
    climb,
    fall,
    attack
}
