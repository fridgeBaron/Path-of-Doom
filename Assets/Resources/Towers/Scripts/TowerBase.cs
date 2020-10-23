using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TowerBase : BuildingBase
{

    [SerializeField]
    private float attackSpeed;
    [SerializeField]
    private float attackRange;
    [SerializeField]
    private float attackDamage;
    [SerializeField]
    private float projectileSpeed;
    [SerializeField]
    private int attackProjectiles;
    [SerializeField]
    private float projectileArc;
    private bool projectileDoesArc;

    private int projectileParticles;
    private int projectileParticleExplosion;
    [SerializeField]
    private float projectileYOffSet;

    [SerializeField]
    private float projectileAoE;
    private bool projectileAoEFalloff;
    [SerializeField]
    private int projectileChain;
    [SerializeField]
    private float projectileChainDistanceMin;
    [SerializeField]
    private float projectileChainDistanceMax;
    [SerializeField]
    private float projectileChainLoss;
    [SerializeField]
    private int projectileForkCount;
    [SerializeField]
    private int projectileForkProjectiles;
    [SerializeField]
    private float projectileForkLoss;

    private List<Vector2Int> hexesInRange;
    private List<Vector2Int> blockedHexesInRange;
    private List<List<mobBase>> mobHexes;

    public float moveSpeedMult;
    public float moveSpeedStatic;
    public bool projectileHoming;
    public bool projectileInstant;

    public float damageMult;
    public float damageStatic;

    public float damageOverTime;
    public float damageOverTimeDecay;

    public float duration;
    public float endTime;


   

    

    



    public float attackTime;
    private bool attack;
    [SerializeField]
    TowerTypes towerType;

    [SerializeField]
    public MobEffect statusEffect;

    private static List<ProjectileBase> projectilesList = new List<ProjectileBase>();
    private static List<ProjectileBase> deadProjectileList = new List<ProjectileBase>();

    private static GameObject projectileFab;
    private static GameObject projectileParent;


    private Vector2 position;
    private Vector2 isoPosition;

    public mobBase target;
    public List<mobBase> targets;
    private bool hasTarget;

    public double lastFrame;

    #region Getters/Setters

    public float AttackSpeed
    {
        get
        {
            return attackSpeed;
        }

        set
        {
            attackSpeed = value;
        }
    }

    public float AttackRange
    {
        get
        {
            return attackRange;
        }

        set
        {
            attackRange = value;
        }
    }

    public float AttackDamage
    {
        get
        {
            return attackDamage;
        }

        set
        {
            attackDamage = value;
        }
    }

    public float ProjectileSpeed
    {
        get
        {
            return projectileSpeed;
        }

        set
        {
            projectileSpeed = value;
        }
    }

    public float ProjectileAoE
    {
        get
        {
            return projectileAoE;
        }

        set
        {
            projectileAoE = value;
        }
    }

    public int ProjectileChain
    {
        get
        {
            return projectileChain;
        }

        set
        {
            projectileChain = value;
        }
    }

    public float ProjectileChainDistanceMin
    {
        get
        {
            return projectileChainDistanceMin;
        }

        set
        {
            projectileChainDistanceMin = value;
        }
    }

    public float ProjectileChainDistanceMax
    {
        get
        {
            return projectileChainDistanceMax;
        }

        set
        {
            projectileChainDistanceMax = value;
        }
    }

    public void SetProjectileParent(GameObject newParent)
    {
        projectileParent = newParent;
    }


    #endregion
    #region InPlay

    void OnEnable()
    {
        if (projectileFab == null)
        {
            projectileFab = Resources.Load("Towers/basicProjectile") as GameObject;
        }
        
        mobHexes = new List<List<mobBase>>();
        hexesInRange = new List<Vector2Int>();
        buildingProperties = new List<BuildingProperties>();
        building = Building.no;
        modelObject = this.GetComponentInChildren<MeshRenderer>().gameObject;
        healthBar = GetComponent<healthTracker>();
        keepLinks = new List<GameObject>();
        //
        StartBuilding();
        autoLink = true;
    }

    private void Awake()
    {
        linksMax = 1;
    }

    void Start()
    {
        healthBar = GetComponent<healthTracker>();
        targets = new List<mobBase>();
        attackTime = 0;
        statusEffect = new MobEffect
        {
            duration = 2
        };

        updateDelay = 10;

    }

    void FixedUpdate()
    {

        if (updateLinks)
        {
            if (updateDelay <= 0)
            {
                UpdateSelf();
                if (autoLink)
                {
                    updateDelay = 480;
                }
                else
                {
                    updateLinks = false;
                    updateDelay = 10;
                }
                
            }
            updateDelay -= 1;
        }
        Tick();
    }

    void Tick()
    {
        if (building == Building.done)
        {
            attackTime += Time.deltaTime;
            if (attackTime > (1 / attackSpeed))
            {
                attackTime = 0;
                attack = true;
            }
            if (attack == true)
            {
                hasTarget = PickTarget();
                if (hasTarget)
                {
                    for (int i = 0; i < attackProjectiles; i ++)
                    {
                        if (targets.Count - 1 < i)
                            break;
                        target = targets[i];
                        ShootProjectile();
                        attack = false;
                    }
                }
            }
        }
        else
        {
            Build();
        }
        
    }
    #endregion
    #region Projectiles

    public static void AddProjectile(ProjectileBase newProjectile)
    {
        projectilesList.Add(newProjectile);
    }

    public static void RemoveProjectile(ProjectileBase oldProjectile)
    {
        projectilesList.Remove(oldProjectile);
        ParticleSystem particleSystem = oldProjectile.GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
            if (oldProjectile.killParticles)
                Destroy(particleSystem.gameObject);
            else
                particleSystem.transform.parent = null;
        }
        deadProjectileList.Add(oldProjectile);
        oldProjectile.gameObject.SetActive(false);
    }

    public static ProjectileBase GetNewProjectile()
    {
        ProjectileBase newProjectile;
        if (deadProjectileList.Count > 0)
        {
            newProjectile = deadProjectileList[0];
            deadProjectileList.RemoveAt(0);
            projectilesList.Add(newProjectile);
            return newProjectile;
        }

        GameObject newObject = Instantiate(projectileFab);
        newProjectile = newObject.GetComponent<ProjectileBase>();
        projectilesList.Add(newProjectile);
        newProjectile.transform.parent = projectileParent.transform;
        return newProjectile;
    }

    void ShootProjectile()
    {
        ProjectileBase shootingProjectile = GetNewProjectile();
        shootingProjectile.gameObject.SetActive(true);
        Vector3 projPos = this.transform.position;
        projPos.y += projectileYOffSet;
        shootingProjectile.transform.position = projPos;
        shootingProjectile.SetProjectile(target, projectileSpeed, attackDamage, projectileAoE, projectileChain, projectileChainDistanceMin, projectileChainDistanceMax, projectileForkCount, projectileForkProjectiles, projectileForkLoss, projectileHoming, projectileInstant);
        shootingProjectile.setBaseTower(this);
        shootingProjectile.addMobEffect(statusEffect);
        shootingProjectile.SetParticleSystems((ProjectileParticles)projectileParticles, (ProjectileParticles)projectileParticleExplosion);
        shootingProjectile.SpawnParticles();
        if (towerType == TowerTypes.Basic || towerType == TowerTypes.Earth)
        {
            shootingProjectile.killParticles = true;
        }
        


    }

    bool PickTarget()
    {

        GetTargetList();
        if (targets.Count > 0)
        {
            return true;
        }
        return false;
    }

    private void GetTargetList()
    {
        targets.Clear();
        
        foreach (Vector2Int hex in hexesInRange)
        {
            List<mobBase> mobs = MobLister.GetMobList(hex);
            if (mobs != null)
            {
                foreach (mobBase mob in mobs)
                {
                    targets.Add(mob);
                }
            }
            
        }
    }
    #endregion




    #region StatHandling

    public static void UpdateTowerHexes()
    {
        TowerBase aTower = FindObjectOfType<TowerBase>();
        if (aTower != null)
        {
            TowerBase[] towers = aTower.transform.parent.GetComponentsInChildren<TowerBase>();
            foreach (TowerBase tower in towers)
            {
                tower.UpdateHexes();
            }
        }
    }

    public void ShowHexes()
    {
        HexHighlighter.ResetGrid();
        foreach (Vector2Int gridPos in hexesInRange)
        {
            HexHighlighter.Set(gridPos, HexHighlight.positive);
        }
        foreach (Vector2Int gridPos in blockedHexesInRange)
        {
            HexHighlighter.Set(gridPos, HexHighlight.negative);
        }
        
    }

    public List<Vector2Int> GetHexes()
    {
        return hexesInRange;
    }
    public List<Vector2Int> GetBlockedHexes()
    {
        return blockedHexesInRange;
    }

    public void UpdateHexes()
    {
        Dictionary<string, List<Vector2Int>> hexes = GetHexesInRange(this.transform.position, (int)attackRange, projectileDoesArc, projectileYOffSet);
        if (hexes.ContainsKey("good") && hexes["good"] != null)
            hexesInRange = hexes["good"];
        if (hexes.ContainsKey("bad") && hexes["bad"] != null)
            blockedHexesInRange = hexes["bad"];
    }


    public override void UpdateSelf()
    {
        SetTower(towerType);
        base.UpdateSelf();
    }

    public static Dictionary<string,List<Vector2Int>> GetHexesInRange(Vector3 position, int range, bool arc = false, float offSet = 0.05f)
    {
        Dictionary<string, List<Vector2Int>> hexesInRange;
        hexesInRange = new Dictionary<string, List<Vector2Int>>();

        List<Vector2Int> hexInRange = new List<Vector2Int>();
        List<Vector2Int> goodHexes = new List<Vector2Int>();
        List<Vector2Int> blockedHexes = new List<Vector2Int>();
        Vector3Int hexPos = TerrainGen.GetGridPosition(position);
        Vector2Int gridPos = new Vector2Int(hexPos.x, hexPos.z);
        hexInRange = TerrainGen.GetHexInRange(gridPos, range);
        Vector3 towerPos = position;
        towerPos.y += offSet;

        foreach (Vector2Int newPos in hexInRange)
        {

            if (!MyMath.IsWithin(hexPos.x, -1, TerrainGen.gridX) || !MyMath.IsWithin(hexPos.y, -1, TerrainGen.gridZ))
                continue;
            RaycastHit[] hits;
            bool good = false;
            BuildingBase building;
            Vector3 castPos = TerrainGen.GetHexPosition(newPos.x, newPos.y);
            if (position.y > TerrainGen.GetHexHeight(newPos))
                castPos.y += 0.25f;
            else
                castPos.y += 0.05f;
            if (!arc)
            {
                //Physics.RayC
                float lineLenght = Vector3.Distance(towerPos, castPos);
                Vector3 faceDir = MyMath.GetDirectionRatio(towerPos, castPos);
                hits = Physics.RaycastAll(towerPos, -faceDir, lineLenght);
                good = true;
                foreach (RaycastHit hit in hits)
                {
                    building = hit.transform.GetComponentInParent<BuildingBase>();
                    if (building != null)
                    {
                        if (!building.Ethereal)
                        {
                            good = false;
                        }
                            
                    }
                    else
                        good = false;
                }
                if (good)
                    goodHexes.Add(newPos);
                else
                    blockedHexes.Add(newPos);
            }
            else
            {
                goodHexes.Add(newPos);
            }

        }
        hexesInRange.Add("good", goodHexes);
        hexesInRange.Add("bad", blockedHexes);

        return hexesInRange;
    }

    public void SetTower(TowerTypes tower)
    {
        gameObject.name = GetTowerTypeString(tower);
        //if (towerType == TowerTypes.none)
        towerType = tower;
        attackSpeed = 0;
        attackRange = 0;
        attackDamage = 0;
        attackProjectiles = 1;

        projectileSpeed = 0;
        projectileChain = 0;
        projectileChainDistanceMin = 0;
        projectileChainDistanceMax = 3;
        projectileForkCount = 0;
        projectileForkProjectiles = 0;
        projectileAoE = 0;
        projectileAoEFalloff = true;
        projectileHoming = true;
        projectileInstant = false;
        projectileDoesArc = false;

        moveSpeedMult = 0;
        moveSpeedStatic = 0;

        damageMult = 0;
        damageStatic = 0;

        damageOverTime = 0;
        damageOverTimeDecay = 0;

        buildingOffset = -0.5f;
        buildingSpeed = 5;

        duration = 0;
        endTime = 0;


        Dictionary<string, float> values;
        values = Towers.instance.GetTower(GetTowerTypeString(tower));
        LoadBuildingStats(values);
        LoadTowerStats(values);
       

        buildingSpeed = Mathf.Abs(buildingOffset / buildingSpeed);
        MeshFilter towerMesh = this.gameObject.GetComponentInChildren<MeshFilter>();
        if (towerMesh != null)
        {
            towerMesh.mesh = Towers.GetMesh(tower);
        }
        MeshRenderer towerRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
        if (towerRenderer != null)
        {
            towerRenderer.material = Towers.GetMaterial(tower);
        }
        if (building == Building.no)
        {
            modelObject.transform.Translate(new Vector3(0, buildingOffset, 0), Space.World);
            building = Building.yes;
        }
            

        UpdateHexes();
    }

    public void LoadTowerStats(Dictionary<string, float> values)
    {
        foreach (KeyValuePair<string, float> val in values)
        {
            switch (val.Key)
            {
                case "attack_speed":
                    attackSpeed = val.Value; break;
                case "attack_range":
                    attackRange = val.Value; break;
                case "attack_damage":
                    attackDamage = val.Value; break;

                case "chain_count":
                    projectileChain = Mathf.RoundToInt(val.Value); break;
                case "chain_min_range":
                    projectileChainDistanceMin = val.Value * TerrainGen.hexSize; break;
                case "chain_max_range":
                    projectileChainDistanceMax = val.Value * TerrainGen.hexSize; break;

                case "attack_projectiles":
                    attackProjectiles = (int)val.Value; break;

                case "projectile_speed":
                    projectileSpeed = val.Value; break;
                case "projectile_aoe":
                    projectileAoE = val.Value; break;
                case "projectile_aoe_falloff":
                    projectileAoEFalloff = false; break;
                case "projectile_yoff":
                    projectileYOffSet = val.Value; break;
                case "projectile_homing":
                    projectileHoming = false; break;
                case "projectile_instant":
                    projectileInstant = true; break;
                case "projectile_particles":
                    projectileParticles = (int)val.Value; break;
                case "projectile_explosion":
                    projectileParticleExplosion = (int)val.Value; break;
                case "projectile_arc":
                    projectileDoesArc = true; break;
                default: break;
            }
        }
    }



    public override void AddAugment(string tower)
    {
        base.AddAugment(tower);
        Dictionary<string, float> augment = Towers.instance.GetAugment(tower);
        if (augment != null)
        {
            foreach (KeyValuePair<string, float> val in augment)
            {
                string property = val.Key.ToLower();
                switch (property)
                {

                    case "attack_speed_mult":
                        attackSpeed *= val.Value; break;
                    case "attack_range_mult":
                        attackRange *= val.Value; break;
                    case "attack_range_add":
                        attackRange += val.Value; break;
                    case "attack_damage_mult":
                        attackDamage *= val.Value; break;
                    case "attack_projectiles":
                        attackProjectiles += (int)val.Value; break;

                    case "chain_count":
                        projectileChain += Mathf.RoundToInt(val.Value); break;
                    case "chain_min_mult":
                        projectileChainDistanceMin *= val.Value; break;
                    case "chain_min_range":
                        projectileChainDistanceMin += val.Value; break;
                    case "chain_max_range_mult":
                        projectileChainDistanceMax *= val.Value; break;
                    case "chain_max_range":
                        projectileChainDistanceMax += val.Value; break;
                    case "chain_loss":
                        projectileChainLoss = val.Value; break;
                    case "fork_count":
                        projectileForkCount +=(int) val.Value; break;
                    case "fork_projectiles":
                        projectileForkProjectiles +=(int) val.Value; break;
                    case "fork_loss":
                        projectileForkLoss = val.Value; break;
                    

                    case "projspeedmult":
                        projectileSpeed *= val.Value; break;
                    case "projaoe":
                        projectileAoE += val.Value; break;
                    case "projaoemult":
                        projectileAoE *= val.Value; break;

                    case "projaoefalloff":
                        projectileAoEFalloff = false; break;
                    case "projhoming":
                        projectileHoming = false; break;
                    case "projinstant":
                        projectileInstant = true; break;

                    default: break;
                }
            }
        }
        UpdateHexes();

    }



    public override Dictionary<string, float> GetDetails()
    {
        Dictionary<string, float> details = new Dictionary<string, float>();

        details.Add(GetTowerTypeString(), -1);
        details.Add("augments", linksNum);
        details.Add("augmentsMax", linksMax);

        if (attackSpeed != 0)
            details.Add("attack_speed", attackSpeed);
        if (attackRange != 0)
            details.Add("attack_range", attackRange);
        if (attackDamage != 0)
            details.Add("attack_damage", attackDamage);
        if (projectileSpeed != 0)
            details.Add("projectile_speed", projectileSpeed);
        if (projectileAoE != 0)
            details.Add("projectile_aoe", projectileAoE);
        if (projectileAoEFalloff == true)
            details.Add("projectile_aoe_falloff", -1);
        if (projectileInstant)
            details.Add("projectile_instant", -1);
        
        return details;
    }
    #endregion
    #region Linking

    #endregion Linking
    #region Enum Handling
    //Tower Type String to Enum
    public static string GetTowerTypeString(TowerTypes types)
    {
        return GetTowerTypeStringP(types);
    }
    public string GetTowerTypeString()
    {
        return GetTowerTypeStringP(towerType);
    }

    private static string GetTowerTypeStringP(TowerTypes type)
    {
        switch (type)
        {
            case TowerTypes.Basic:
                return "basic";
            case TowerTypes.Fire:
                return "fire";
            case TowerTypes.Air:
                return "air";
            case TowerTypes.Earth:
                return "earth";
            case TowerTypes.Water:
                return "water";

            default: return null; ;
        }
    }

    public static TowerTypes GetTowerType(string type)
    {
        switch (type)
        {
            case "basic":
                return TowerTypes.Basic;
            case "fire":
                return TowerTypes.Fire;
            case "air":
                return TowerTypes.Air;
            case "earth":
                return TowerTypes.Earth;
            case "water":
                return TowerTypes.Water;

            default: return TowerTypes.none; ;
        }
    }
    #endregion
}


public enum TowerTypes
{
    Basic,
    Fire,
    Air,
    Earth,
    Water,
    none
}


