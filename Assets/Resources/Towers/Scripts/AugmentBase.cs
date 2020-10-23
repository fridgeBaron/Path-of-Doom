using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentBase : BuildingBase
{

    string type;

    float attackSpeedMult;
    float attackRangeMult;
    float attackDamageMult;

    float attackSpeed;
    float attackRange;
    float attackDamage;

    float projectileSpeedMult;
    float projectileSpeed;

    float projectileAoEMult;
    float projectileAoE;

    float projectileChain;
    float projectileChainDistanceMinMult;
    float projectileChainDistanceMin;
    float projectileChainDistanceMaxMult;
    float projectileChainDistanceMax;

    List<BuildingBase> buildings;
    List<BuildingProperties> excludeProperties;




    // Start is called before the first frame update
    void Awake()
    {
        buildings = new List<BuildingBase>();
        healthBar = GetComponent<healthTracker>();
        //
    }

    private void OnEnable()
    {
        building = Building.no;
        modelObject = this.GetComponentInChildren<MeshRenderer>().gameObject;
        StartBuilding();
        keepLinks = new List<GameObject>();
        buildingProperties = new List<BuildingProperties>();
        excludeProperties = new List<BuildingProperties>();
        links = new Dictionary<GameObject, LinkType>();
    }


    // Update is called once per frame
    void Update()
    {
        if (building == Building.yes)
        {
            Build();
        }
    }
    

    protected override void DoneBuilding()
    {
           FindLinks();
    }

    public void SetAugmentType(string newType)
    {
        type = newType;
        MeshFilter mesh = this.gameObject.GetComponentInChildren<MeshFilter>();
        if (mesh != null)
        {
            mesh.mesh = Towers.GetMesh(Towers.GetAugmentType(newType));
        }

        buildingOffset = -0.02f;
        buildingSpeed = 10;

        Dictionary<string, float> values = Towers.instance.GetAugment(newType);
        LoadBuildingStats(values);
        LoadAugmentStats(values);

        buildingSpeed = Mathf.Abs(buildingOffset / buildingSpeed);
        if (building == Building.no)
        {
            modelObject.transform.Translate(new Vector3(0, buildingOffset, 0), Space.World);
            building = Building.yes;
        }
    }

    public void LoadAugmentStats(Dictionary<string, float> values)
    {
        health = 0;
        maxHealth = 100;

        foreach (KeyValuePair<string, float> val in values)
        {
            switch (val.Key)
            {
                case "exclude_property_building":
                    excludeProperties.Add(BuildingProperties.building); break;
                case "exclude_property_tower":
                    excludeProperties.Add(BuildingProperties.tower); break;
                case "exclude_property_augment":
                    excludeProperties.Add(BuildingProperties.augment); break;
            }
        }

        health = maxHealth;
    }

    public bool CanAugment(List<BuildingProperties> properties)
    {
        foreach (BuildingProperties prop in properties)
        {
            if (excludeProperties.Contains(prop))
            {
                return false;
            }
                
        }
        return true;
    }

    public string GetAugmentType()
    {
        return type;
    }



    public override int Sell()
    {
        Destroy(this.gameObject);
        foreach (BuildingBase building in buildings)
        {
            building.RemoveLink(this.gameObject);
        }
        return value;
    }



    public void AddBuilding(BuildingBase building)
    {
        buildings.Add(building);
    }


    private string GetAugmentTypeString(AugmentTypes type)
    {
        switch (type)
        {
            case AugmentTypes.Speed:
                return "Speed";
            case AugmentTypes.Damage:
                return "Damage";
            case AugmentTypes.Range:
                return "Range";
            case AugmentTypes.Chain:
                return "Chain";
            case AugmentTypes.Fork:
                return "Fork";
            case AugmentTypes.Multi:
                return "Multi";

            default: return null; ;
        }
    }

    // void check
}




public enum AugmentTypes
{
    Speed,
    Damage,
    Range,
    Chain,
    Fork,
    Multi,
    Ethereal,
    none
}