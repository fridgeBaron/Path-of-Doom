using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBase : MonoBehaviour {

    mobBase target;
    float speed;
    float damage;
    float arc;
    float distance;

    float areaEffect;
    public int chainCount;
    float chainDistanceMin;
    float chainDistanceMax;
    float chainDamageLoss;
    int forkCount;
    int forkProjectiles;
    float forkDamageLoss;

    ParticleSystem projParticleSystem;
    public int projExplosion;
    public bool killParticles;
    
    [SerializeField]
    bool isHoming;
    bool isInstant;
    Vector3 targetPos;
    Vector3 faceDir;

    ProjectileParticles effect;
    ProjectileParticles explosion;

    TowerBase tower;

    private static List<Lightning> lightningList;
    private static List<Lightning> deadLightningList;
    private static GameObject lightningFab;
    private static int lightningNum;

    [SerializeField]
    public static GameObject projectileParent;


    List<mobBase> chainList;
    List<MobEffect> mobEffectList;

    #region Projectile Initializers

    public void setBaseTower(TowerBase tower)
    {
        this.tower = tower;
    }

    public ProjectileBase()
    {
        target = null;
        speed = 4;
        damage = 0;
        isHoming = true;
    }

    public ProjectileBase(mobBase target, float speed, float damage, bool isHoming)
    {
        this.target = target;
        this.speed = speed;
        this.damage = damage;
        this.isHoming = isHoming;
    }

    public void SetProjectile(mobBase target, float speed, float damage, float areaEffect, int chainCount, float chainDistanceMin, float chainDistanceMax, int forkCount, int forkProjectiles, float forkLoss,  bool isHoming, bool isInstant)
    {
        this.target = target;
        this.speed = speed;
        this.damage = damage;
        this.areaEffect = areaEffect;
        this.chainCount = chainCount;
        this.chainDistanceMin = chainDistanceMin;
        this.chainDistanceMax = chainDistanceMax;
        this.forkCount = forkCount;
        this.forkProjectiles = forkProjectiles;
        this.forkDamageLoss = forkLoss;
        this.isHoming = isHoming;
        this.isInstant = isInstant;
        if (chainCount > 0)
        {
            chainList = new List<mobBase>();
        }
    }

    public void SetParticleSystems(ProjectileParticles effect, ProjectileParticles explosion)
    {
        this.effect = effect;
        this.explosion = explosion;
        
    }
    public void SpawnParticles()
    {
        if (effect != 0) 
        {
            ParticleSystem particleSystem = ParticleMaster.MakeSystem(effect);
            particleSystem.transform.parent = this.transform;
            particleSystem.transform.localPosition = Vector3.zero;
            projParticleSystem = particleSystem;
        }
    }

    public ProjectileBase CopyProjectile(float DamageReduction = 1)
    {
        ProjectileBase newProj = TowerBase.GetNewProjectile();
        newProj.gameObject.SetActive(true);
        //
        newProj.setBaseTower(tower);
        newProj.SetProjectile(target, speed, damage, areaEffect, chainCount, chainDistanceMin, chainDistanceMax, forkCount, forkProjectiles, forkDamageLoss, true, false);
        newProj.SetParticleSystems(effect, explosion);
        newProj.SpawnParticles();
        return newProj;
    }
    
    #endregion

    private void Start()
    {
        if (lightningList == null)
        {
            lightningList = new List<Lightning>();
            deadLightningList = new List<Lightning>();
            lightningNum = 0;
        }
        if (lightningFab == null)
        {
            lightningFab = Resources.Load("Towers/Effects/lightningFab") as GameObject;
        }
    }

    // Use this for initialization
    void OnEnable () {
        mobEffectList = new List<MobEffect>();
        arc = 0.5f;
        distance = 0;
        killParticles = false;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Move();
	}





    public void addMobEffects(List<MobEffect> effects)
    {
        foreach(MobEffect effect in effects)
        {
            mobEffectList.Add(effect);
        }
    }
    public void addMobEffect(MobEffect effect)
    {
        mobEffectList.Add(effect);
    }

    #region Lighting

    public static int GetCount()
    {
        if (deadLightningList != null)
        {
            return deadLightningList.Count;
        }
        return 0;
    }

    public static void AddLightning(Lightning newLightning)
    {
        lightningList.Add(newLightning);
    }

    public static void RemoveLightning(Lightning oldLightning)
    {
        lightningList.Remove(oldLightning);
        deadLightningList.Add(oldLightning);
        deadLightningList.Add(oldLightning);
        oldLightning.gameObject.SetActive(false);
    }

    public static Lightning GetLightning()
    {
        Lightning retLightning;
        if (deadLightningList.Count > 0)
        {
            retLightning = deadLightningList[0];
            deadLightningList.RemoveAt(0);
            lightningList.Add(retLightning);
            retLightning.gameObject.SetActive(true);
            return retLightning;
        }
        GameObject newObject;
        newObject = Instantiate(lightningFab);
        retLightning = newObject.GetComponent<Lightning>();
        lightningList.Add(retLightning);
        return retLightning;
    }

    #endregion

    private void Move()
    {
        if (!isInstant)
        {
            if (speed == 0)
            {
                TowerBase.RemoveProjectile(this);
            }
            if (isHoming)
            {
                if (target.gameObject.activeInHierarchy)
                {
                    Vector3 pos = transform.position;

                    faceDir = MyMath.GetDirectionRatio(transform.position, target.transform.position);
                    pos.x -= speed * faceDir.x * Time.deltaTime;
                    pos.z -= speed * faceDir.z * Time.deltaTime;
                    pos.y -= speed * faceDir.y * Time.deltaTime;
                    transform.position = pos;
                    targetPos = target.transform.position;
                    if (MyMath.calcDistance(pos, target.transform.position, new Vector3(1,0,1)) < speed * Time.deltaTime / 50)
                    {
                        transform.position = target.transform.position;
                        HitTarget();
                    }

                }
                else
                {
                    isHoming = false;
                }
            }
            if (!isHoming)
            {
                Vector3 pos = transform.position;
                faceDir = MyMath.GetDirectionRatio(transform.position, targetPos);

                pos.x -= speed / 100 * faceDir.x;
                pos.y -= speed / 100 * faceDir.y;
                pos.z -= speed / 100 * faceDir.z;

                transform.position = pos;

                if (MyMath.calcDistance(pos, targetPos) < 0.1f)
                {
                    if (areaEffect > 0)
                    {
                        List<mobBase> mobList = MobLister.GetMobsInRange(pos, areaEffect * TerrainGen.hexSize);
                        if (mobList.Count > 0)
                        {
                            foreach (mobBase mob in mobList)
                            {
                                mob.DamageMob(damage);
                            }
                        }
                    }
                    else
                    {
                        List<mobBase> mobList = MobLister.GetMobsInRange(pos, 0.1f);
                        if (mobList.Count > 0)
                        {
                            mobList[0].DamageMob(damage);
                        }
                    }
                    TowerBase.RemoveProjectile(this);
                }
            }

        }
        if (isInstant)
        {
            List<mobBase> findMobs = new List<mobBase>();
            chainList = new List<mobBase>();
            chainList.Add(target);
            if (chainCount > 0)
            {
                for (int i = 0; i < chainCount; i ++)
                {
                    findMobs = MobLister.GetMobsInRange(transform.position, chainDistanceMax * TerrainGen.hexSize);
                    if (findMobs.Count > 0)
                    {
                        for (int o = 0; o < findMobs.Count; o ++)
                        {
                            if (MyMath.calcDistance(transform.position, findMobs[o].transform.position) > chainDistanceMin * TerrainGen.hexSize)
                            {
                                if (!chainList.Contains(findMobs[o]))
                                {
                                    chainList.Add(findMobs[o]);
                                    transform.position = findMobs[o].transform.position;
                                    break;
                                }
                            }
                        }    
                    }
                }
                float dmgMult = 1;
                float chainDamage = damage;

                Lightning lightning;
                lightning = GetLightning();
                lightning.SetTower(tower);
                lightning.SetNumber(lightningNum);
                lightningNum++;
                lightning.setTargets(chainList);

                foreach (mobBase mob in chainList)
                {
                    chainDamage *= dmgMult;
                    mob.DamageMob(chainDamage);
                    mob.AddMobEffects(mobEffectList);
                    dmgMult *= 0.8f;
                }
                


                TowerBase.RemoveProjectile(this);
            }
            
        }
    }

    private void HitTarget()
    {
        bool killedTarget = target.DamageMob(damage);

        if (Fork()) { }
        else if (Chain()) { }
        else if (AoE()) { }

        

        if (projExplosion != 0) 
        {
            ParticleSystem newSystem = ParticleMaster.MakeSystem((ProjectileParticles)projExplosion);
            newSystem.transform.position = transform.position;
        }
        TowerBase.RemoveProjectile(this);
    }

    private bool AoE()
    {
        if (areaEffect > 0)
        {
            List<Vector2Int> hexes;
            hexes = TerrainGen.GetHexInRange(TerrainGen.GetGridPosition2D(transform.position), Mathf.RoundToInt(1 + areaEffect));
            List<mobBase> mobs = new List<mobBase>();
            foreach (Vector2Int hex in hexes)
            {
                List<mobBase> mobHex = MobLister.GetMobList(hex);
                if (mobHex != null)
                {
                    foreach (mobBase mob in mobHex)
                    {
                        float distance = MyMath.calcDistance(transform.position, mob.transform.position);
                        if (distance < areaEffect * TerrainGen.hexSize && mob != target)
                        {
                            
                            mobs.Add(mob);
                        }
                           
                    }
                }
            }
            foreach (mobBase mob in mobs)
            {
                float distance = MyMath.calcDistance(transform.position, mob.transform.position);
                float dmgMult = distance / (TerrainGen.hexSize * areaEffect);
                mob.DamageMob(damage);
            }
            return true;
        }
        return false;
    }

    private bool Chain()
    {
        if (chainCount > 0)
        {
            List<Vector2Int> hexes;
            hexes = TerrainGen.GetHexInRange(TerrainGen.GetGridPosition2D(transform.position),(int) chainDistanceMax);
            List<mobBase> mobs = new List<mobBase>();
            foreach (Vector2Int hex in hexes)
            {
                List<mobBase> mobHex = MobLister.GetMobList(hex);
                if ( mobHex != null)
                {
                    foreach (mobBase mob in mobHex)
                    {
                        float distance = MyMath.calcDistance(transform.position, mob.transform.position);
                        if (distance > chainDistanceMin && distance < chainDistanceMax && mob != target)
                            mobs.Add(mob);
                    }
                }
            }
            chainCount -= 1;
            damage *= chainDamageLoss;
            if (mobs.Count > 0)
            {
                Debug.Log("Chaining");
                ProjectileBase newProj = CopyProjectile(chainDamageLoss);
                newProj.SetParticleSystems(effect, explosion);
                newProj.transform.position = this.transform.position;
                newProj.target = mobs[Random.Range(0, mobs.Count - 1)];
                Debug.DrawLine(newProj.transform.position, newProj.target.transform.position, Color.red,1);
            }
            return true;
            //
        }
        return false;
    }

    private bool Fork()
    {
        if (forkCount > 0 && forkProjectiles > 0)
        {
            List<Vector2Int> hexes;
            hexes = TerrainGen.GetHexInRange(TerrainGen.GetGridPosition2D(transform.position), (int)chainDistanceMax);
            List<mobBase> mobs = new List<mobBase>();
            foreach (Vector2Int hex in hexes)
            {
                List<mobBase> mobHex = MobLister.GetMobList(hex);
                if (mobHex != null)
                {
                    foreach (mobBase mob in MobLister.GetMobList(hex))
                    {
                        float distance = MyMath.calcDistance(transform.position, mob.transform.position);
                        if (distance > chainDistanceMin && distance < chainDistanceMax)
                            mobs.Add(mob);
                    }
                }
            }
            //
            forkCount -= 1;
            forkProjectiles =Mathf.RoundToInt(forkProjectiles * 0.5f);
            damage *= forkDamageLoss;

            if (mobs.Count > 0)
            {
                for (int i = 0; i < forkProjectiles; i++)
                {
                    ProjectileBase newProj = CopyProjectile(chainDamageLoss);
                    newProj.transform.position = this.transform.position;
                    int randomTarget = Random.Range(0, mobs.Count-1);
                    newProj.target = mobs[randomTarget];
                    mobs.RemoveAt(randomTarget);
                    if (mobs.Count == 0)
                        break;

                }
            }
            //
            return true;
        }
        return false;
    }
}
