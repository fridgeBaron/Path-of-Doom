using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Towers : MonoBehaviour
{
    public static Towers instance;

    public static Dictionary<string, Dictionary<string,float>> towerList;
    public static Dictionary<string, Dictionary<string, float>> augmentList;
    public static Dictionary<string, Dictionary<string, float>> buildingList;

    public static Mesh airTower;
    public static Mesh basicTower;
    public static Mesh earthTower;
    public static Mesh fireTower;
    public static Mesh waterTower;

    public static Mesh damageAugment;
    public static Mesh speedAugment;
    public static Mesh rangeAugment;

    public static Mesh wallBuilding;
    public static Mesh baseBuilding;
    public static Mesh baseColl;

    public static Material airTowerMat;
    public static Material basicTowerMat;
    public static Material earthTowerMat;
    public static Material fireTowerMat;
    public static Material waterTowerMat;
    public static Material etherealMat;

   

    [SerializeField]
    Mesh airTowerHolder;
    [SerializeField]
    Mesh basicTowerHolder;
    [SerializeField]
    Mesh earthTowerHolder;
    [SerializeField]
    Mesh fireTowerHolder;
    [SerializeField]
    Mesh waterTowerHolder;

    [SerializeField]
    Mesh damageAugmentHolder;
    [SerializeField]
    Mesh speedAugmentHolder;
    [SerializeField]
    Mesh rangeAugmentHolder;

    [SerializeField]
    Mesh wallBuildingHolder;
    [SerializeField]
    Mesh baseBuildingHolder;
    [SerializeField]
    Mesh baseCollHolder;

    [SerializeField]
    Material airTowerMatHolder;
    [SerializeField]
    Material basicTowerMatHolder;
    [SerializeField]
    Material earthTowerMatHolder;
    [SerializeField]
    Material fireTowerMatHolder;
    [SerializeField]
    Material waterTowerMatHolder;
    [SerializeField]
    Material etherealMatHolder;

    TowerBase towerBase;

    private void Awake()
    {
        

        if (instance == null)
        {
            instance = this;
            towerList = new Dictionary<string, Dictionary<string, float>>();
            augmentList = new Dictionary<string, Dictionary<string, float>>();
            buildingList = new Dictionary<string, Dictionary<string, float>>();
            #region Towers
            // Basic Tower
            Dictionary<string, float> statList = new Dictionary<string, float>
            {
                { "build_cost", 35 },

                { "attack_speed", 1 },
                { "attack_range", 3 },
                { "attack_damage", 25 },
                { "attack_projectiles", 2f },

                { "projectile_speed", 3 },
                { "projectile_yoff", 0.6f },
                { "projectile_particles", 7 },
                { "building_offset", -0.19f},
                { "building_speed", 6},
                { "property_tower", 0}

            };
            towerList.Add("basic", statList);


            statList = new Dictionary<string, float>
            {
                { "build_cost", 45 },

                { "attack_speed", 3f },
                { "attack_range", 3 },
                { "attack_damage", 20 },
                
                { "chain_min_range", 0f },
                { "chain_max_range", 1 },
                { "projectile_instant", -1 },
                { "projectile_yoff", 0.5f },
                { "building_offset", -0.185f},
                { "building_speed", 15},
                { "property_tower", 0}

            };
            towerList.Add("air", statList);

            statList = new Dictionary<string, float>
            {
                { "build_cost", 60 },

                { "attack_speed", 0.5f },
                { "attack_range", 4 },
                { "attack_damage", 50 },

                { "never_fork", -1 },
                { "never_chain", -1 },
                { "projectile_speed", 2 },
                { "projectile_aoe", 1 },
                { "projectile_aoe_falloff", 1 },
                { "projectile_particles", 5 },
                { "projectile_explosion", 0 },
                { "projectile_yoff", 0.5f },
                { "projectile_arc", -1 },
                { "building_offset", -0.165f},
                { "building_speed", 20},
                { "property_tower", 0}

            };
            towerList.Add("earth", statList);

            statList = new Dictionary<string, float>
            {
                { "build_cost", 50 },

                { "attack_speed", 4f },
                { "attack_range", 2 },
                { "attack_damage", 15 },

                { "projectile_speed", 4f },
                { "projectile_particles", 1 },
                { "projectile_explosion", 2 },
                { "projectile_yoff", 0.2f },
                { "building_offset", -0.065f},
                { "building_speed", 10},
                { "property_tower", 0}


            };
            towerList.Add("fire", statList);

            statList = new Dictionary<string, float>
            {
                { "build_cost", 45 },

                { "attack_speed", 1.5f },
                { "attack_range", 6 },
                { "attack_damage", 25 },

                { "projectile_speed", 3 },
                { "projectile_particles", 3 },
                { "projectile_explosion", 4 },
                { "projectile_yoff", 0.8f },
                { "building_offset", -0.11f},
                { "building_speed", 15},
                { "property_tower", 0}


            };
            towerList.Add("water", statList);

            #endregion
            /*   */

            #region Augments

            //Speed Augment
            statList = new Dictionary<string, float>
            {
                { "build_cost", 65 },

                { "attack_speed_mult", 1.4f },

                { "projectile_speed_mult", 1.1f },

                { "build_offset", -0.02f},
                { "building_speed", 15},
                { "property_augment", 0}

            };
            augmentList.Add("speed", statList);
            //Damage Augment
            statList = new Dictionary<string, float>
            {
                { "build_cost", 50 },

                { "attack_damage_mult", 1.25f },

                { "building_offset", -0.02f},
                { "building_speed", 10},
                { "property_augment", 0}


            };
            augmentList.Add("damage", statList);
            //Range Augment
            statList = new Dictionary<string, float>
            {
                { "build_cost", 40 },

                { "attack_range_add", 1f },

                { "building_offset", -0.02f},
                { "building_speed", 15},
                { "property_augment", 0}

            };
            augmentList.Add("range", statList);
            //Chain Augment
            statList = new Dictionary<string, float>
            {
                { "build_cost", 95 },

                { "chain_count_add", 1f },
                { "chain_max_range_mult", 1.25f },
                { "chain_damage_mult", 0.8f },
                { "attack_damage_mult", 0.7f },

                { "building_offset", -0.02f},
                { "building_speed", 15},
                { "property_augment", 0},
                { "exclude_property_building", 0 },
                { "exclude_property_aoe", 0 }

            };
            augmentList.Add("chain", statList);
            //Fork Augment
            statList = new Dictionary<string, float>
            {
                { "build_cost", 80 },

                { "attack_range", -1f },
                { "attack_damage_mult", 0.8f },
                { "fork_count_add", 1f },
                { "chain_max_range_mult", 1.25f },
                { "fork_projectiles_add", 1f },
                { "fork_damage_mult", 0.5f },

                { "building_offset", -0.02f},
                { "building_speed", 15},
                { "property_augment", 0},
                { "exclude_property_building", 0 },
                { "exclude_property_aoe", 0 }

            };
            augmentList.Add("fork", statList);
            //Multishot Augment
            statList = new Dictionary<string, float>
            {
                { "build_cost", 70 },
                
                { "attack_damage_mult", 0.7f },
                { "attack_projectiles_add", 1 },

                { "building_offset", -0.02f},
                { "building_speed", 15},
                { "property_augment", 0},
                { "exclude_property_building", 0 },
                { "exclude_property_aoe", 0 }

            };
            augmentList.Add("multi", statList);

            //Ethereals
            statList = new Dictionary<string, float>
            {
                { "build_cost", 70 },

                { "attack_damage_mult", 0.9f },
                { "health_max_mult", 0.75f },

                { "building_offset", -0.02f},
                { "building_speed", 15},
                { "property_augment", 0},
                { "add_property_ethereal", 0}

            };
            augmentList.Add("ethereal", statList);

            //
            #endregion

            #region Buildings

            statList = new Dictionary<string, float>
            {
                { "build_cost", 35 },

                { "health_max", 300 },
                { "building_offset", -0.15f},
                { "building_speed", 5},
                { "property_building", 0},
                { "property_blocking", 0 }

            };
            buildingList.Add("wall", statList);

            statList = new Dictionary<string, float>
            {

                { "health_max", 1000 },
                { "build_offset", -0.16f},
                { "building_speed", 1},
                { "property_building", 0},
                { "property_home", 0}

            };
            buildingList.Add("base", statList);

            #endregion

            airTower = airTowerHolder;
            basicTower = basicTowerHolder;
            earthTower = earthTowerHolder;
            fireTower = fireTowerHolder;
            waterTower = waterTowerHolder;

            damageAugment = damageAugmentHolder;
            speedAugment = speedAugmentHolder;
            rangeAugment = rangeAugmentHolder;

            airTowerMat = airTowerMatHolder;
            basicTowerMat = basicTowerMatHolder;
            earthTowerMat = earthTowerMatHolder;
            fireTowerMat = fireTowerMatHolder;
            waterTowerMat = waterTowerMatHolder;
            etherealMat = etherealMatHolder;

            wallBuilding = wallBuildingHolder;
            baseBuilding = baseBuildingHolder;
            baseColl = baseCollHolder;
            //
        }

        else if (instance != this)
            Destroy(gameObject.GetComponent(instance.GetType()));
    }

    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public Dictionary<string, float> GetTower(string tower)
    {
        tower = tower.ToLower();
        if (towerList.ContainsKey(tower))
        {
            return towerList[tower];
        }
        return null;
    }


    public Dictionary<string, float> GetAugment(string augment)
    {
        if (augmentList.ContainsKey(augment))
        {
            return augmentList[augment];
        }
        return null;
    }

    public Dictionary<string, float> GetBuilding(string building)
    {
        building = building.ToLower();
        if (buildingList.ContainsKey(building))
        {
            return buildingList[building];
        }
        return null;
    }

    public float? GetTowerValue(string tower, string value)
    {
        if (towerList.ContainsKey(tower))
        {
            if (towerList[tower].ContainsKey(value))
            {
                return towerList[tower][value];
            }
        }
        return null;
    }

    public int GetCost(string item)
    {
        item = item.ToLower();
        if (towerList.ContainsKey(item))
            return (int) towerList[item]["build_cost"];
        if (augmentList.ContainsKey(item))
            return (int)augmentList[item]["build_cost"];
        if (buildingList.ContainsKey(item))
            return (int)buildingList[item]["build_cost"];
        return 0;
    }


    public static Mesh GetMesh(TowerTypes tower)
    {
        switch (tower)
        {
            case TowerTypes.Air:
                return airTower;
            case TowerTypes.Basic:
                return basicTower;
            case TowerTypes.Earth:
                return earthTower;
            case TowerTypes.Fire:
                return fireTower;
            case TowerTypes.Water:
                return waterTower;
        }


        return null;
    }


    public static Mesh GetMesh(BuildingType building)
    {
        switch (building)
        {
            case BuildingType.Base:
                return baseBuilding;
            case BuildingType.Wall:
                return wallBuilding;
        }
        return null;
    }

    public static Material GetMaterial(TowerTypes tower)
    {
        switch (tower)
        {
            case TowerTypes.Air:
                return airTowerMat;
            case TowerTypes.Basic:
                return basicTowerMat;
            case TowerTypes.Earth:
                return earthTowerMat;
            case TowerTypes.Fire:
                return fireTowerMat;
            case TowerTypes.Water:
                return waterTowerMat;
        }


        return null;
    }

    public static Mesh GetMesh(AugmentTypes augment)
    {
        switch (augment)
        {
            case AugmentTypes.Chain:
            case AugmentTypes.Fork:
            case AugmentTypes.Multi:
            case AugmentTypes.Damage:
            case AugmentTypes.Ethereal:
                return damageAugment;
            case AugmentTypes.Range:
                return rangeAugment;
            case AugmentTypes.Speed:
                return speedAugment;
        }
        return null;
    }

    public static AugmentTypes GetAugmentType(string type)
    {
        type = type.ToLower();
        switch (type)
        {
            case "damage":
                return AugmentTypes.Damage;
            case "range":
                return AugmentTypes.Range;
            case "speed":
                return AugmentTypes.Speed;
            case "chain":
                return AugmentTypes.Chain;
            case "fork":
                return AugmentTypes.Fork;
            case "multi":
                return AugmentTypes.Multi;
            case "ethereal":
                return AugmentTypes.Ethereal;
        }
        return AugmentTypes.none;
    }
    

}
