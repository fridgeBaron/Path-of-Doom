using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBase : MonoBehaviour
{
    private string buildingType;


    protected int value;
    [SerializeField]
    protected float health;
    protected int maxHealth;
    protected healthTracker healthBar;
    

    protected Building building;
    protected float buildingOffset;
    protected float buildingSpeed;
    protected ParticleSystem buildingParticles;
    //
    protected GameObject modelObject;
    protected MeshRenderer buildingMesh;


    [SerializeField]
    protected int linksMax;
    [SerializeField]
    protected int linksNum;
    [SerializeField]
    protected Dictionary<GameObject, LinkType> links;
    protected List<GameObject> loopedLinks;
    protected List<GameObject> keepLinks;
    protected bool updateLinks;
    protected int updateDelay;
    protected bool autoLink;
    protected List<BuildingProperties> buildingProperties;
    protected bool ethereal;

    protected int level;

    protected static LayerMask buildingMask = 1 << 8;

    public bool Ethereal
    {
        get
        {
            return ethereal;
        }
    }

    private void OnEnable()
    {
        building = Building.no;
        modelObject = this.GetComponentInChildren<MeshRenderer>().gameObject;
        StartBuilding();
        keepLinks = new List<GameObject>();
        links = new Dictionary<GameObject, LinkType>();
        autoLink = true;
        healthBar = GetComponent<healthTracker>();
        buildingProperties = new List<BuildingProperties>();
        linksMax = 1;
        updateLinks = true;
        updateDelay = 10;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (building == Building.yes)
        {
            Build();
        }
        if (updateLinks)
        {
            if (updateDelay <= 0)
            {
                UpdateSelf();
                updateLinks = false;
                updateDelay = 10;
            }
            updateDelay -= 1;
        }
    }

    public void SetBuildingType(string type)
    {
        //
    }

    public virtual int Sell()
    {
        Destroy(this.gameObject);
        return value;
    }

    public virtual bool TakeDamage(float damage)
    {
        health -= damage;
        healthBar.SetHealth(health / maxHealth);
        if (health < 0)
        {
            Destroy(this.gameObject);
            return true;
        }
        return false;
    }

    public void SetBuildingEffect(ParticleSystem system)
    {
        buildingParticles = system;
    }

    public void AddValue(int val)
    {
        value += val;
    }

    private void UpdateTower(GameObject tower)
    {
        if (tower.GetComponent<TowerBase>() != null)
        {
            tower.GetComponent<TowerBase>().updateLinks = true;
        }
    }

    public virtual void AddAugment(string tower)
    {
        Dictionary<string, float> augment = Towers.instance.GetAugment(tower);
        if (augment != null)
        {
            foreach (KeyValuePair<string, float> val in augment)
            {
                string property = val.Key.ToLower();
                switch (property)
                {
                    case "healthMult":
                        health = Mathf.RoundToInt(health * val.Value); break;
                    case "health_max":
                        health += (int)val.Value; break;
                    case "health_max_mult":
                        maxHealth =Mathf.RoundToInt(maxHealth * val.Value); break;
                    case "add_property_ethereal":
                        buildingProperties.Add(BuildingProperties.ethereal);
                        this.GetComponentInChildren<MeshRenderer>().material = Towers.etherealMat;
                        ethereal = true;
                        break;
                    default: break;
                }
            }
            if (health > maxHealth)
                health = maxHealth;
        }
    }


    /// <summary>
    /// Functions that have to deal with building -----------------------
    /// </summary>
    #region Building

    protected virtual void DoneBuilding()
    {
        UpdateSelf();
    }

    protected virtual void StartBuilding()
    {
        buildingParticles = Instantiate(ParticleMaster.buildFab, transform);
        buildingParticles.transform.position = transform.position;
    }

    protected void Build()
    {
        if (modelObject.transform.localPosition.y < 0)
        {
            modelObject.transform.Translate(new Vector3(0, buildingSpeed * Time.deltaTime, 0), Space.World);
        }
        else
        {
            building = Building.done;
            Vector3 builtPos = modelObject.transform.position;
            builtPos.y = this.transform.position.y;
            if (buildingParticles != null)
            {
                ParticleSystem.Burst doneBurst = new ParticleSystem.Burst();
                doneBurst.time = 0;
                doneBurst.count = value;
                buildingParticles.emission.SetBurst(0, doneBurst);
                ParticleSystem.MainModule main = buildingParticles.main;
                ParticleSystem.EmissionModule emmision = buildingParticles.emission;
                buildingParticles.time = 0;
                emmision.rateOverTime = 0;
                main.loop = false;
            }
            DoneBuilding();

            modelObject.transform.position = builtPos;
        }
    }

    public void SetBuilding(string buildingType)
    {
        buildingOffset = -0.02f;
        buildingSpeed = 10;
        this.buildingType = buildingType;

        this.gameObject.name = buildingType;

        LoadBuildingStats(Towers.instance.GetBuilding(buildingType));

        MeshFilter buildingMesh = this.gameObject.GetComponentInChildren<MeshFilter>();
        if (buildingMesh != null)
        {
            buildingMesh.mesh = Towers.GetMesh(GetBuildingType(buildingType));
        }
        buildingSpeed = Mathf.Abs(buildingOffset / buildingSpeed);
        //modelObject.transform.Translate(new Vector3(0, buildingOffset, 0), Space.World);
        if (building == Building.no)
        {
            modelObject.transform.Translate(new Vector3(0, buildingOffset, 0), Space.World);
            building = Building.yes;
        }
        if (buildingType == "base")
        {
            GameObject goal = new GameObject("Goal");
            goal.transform.position = this.transform.position;
            TerrainGen.goal = goal;
            GetComponentInChildren<MeshCollider>().sharedMesh = Towers.baseColl;
        }
        health = maxHealth;
    }

    public bool HasProperty(BuildingProperties prop)
    {
        if (buildingProperties.Contains(prop))
            return true;
        return false;
    }

    public void LoadBuildingStats(Dictionary<string, float> values)
    {
        health = 0;
        maxHealth = 100;
        ethereal = false;

        foreach (KeyValuePair<string, float> val in values)
        {
            switch (val.Key)
            {
                case "build_cost":
                    value = (int)val.Value; break;
                case "building_offset":
                    buildingOffset = val.Value; break;
                case "building_speed":
                    buildingSpeed = val.Value; break;
                case "health_max":
                    maxHealth = (int)val.Value; break;
                case "property_building":
                    buildingProperties.Add(BuildingProperties.building); break;
                case "property_tower":
                    buildingProperties.Add(BuildingProperties.tower); break;
                case "property_augment":
                    buildingProperties.Add(BuildingProperties.augment); break;
                case "property_blocking":
                    buildingProperties.Add(BuildingProperties.blocking); break;

            }
        }
        health = maxHealth;
    }
    
    public virtual Dictionary<string, float> GetDetails()
    {
        Dictionary<string, float> details = new Dictionary<string, float>();
        //details.Add(GetTowerTypeString(), -1);
        details.Add("augments", linksNum);
        details.Add("augmentsMax", linksMax);


        if (health != 0)
            details.Add("building_health", health);
        if (maxHealth != 0)
            details.Add("building_health_max", maxHealth);

        return details;
    }

    #endregion //Building

    /// <summary>
    /// Functions that have to deal with linking ----------------------------------
    /// </summary>
    #region linking

    public virtual void UpdateSelf()
    {
        if (this.GetType() ==  typeof(BuildingBase))
            SetBuilding(buildingType);
        if (autoLink)
        {
            foreach (KeyValuePair<GameObject, LinkType> link in links)
            {
                if (link.Value != LinkType.incompatible)
                {
                    if (linksMax - linksNum > 0)
                    {
                        ToggleLink(link.Key);
                        break;
                    }
                }
                
            }
        }


        ApplyLinks();
        MouseHook.mousehook.UpdateLinks();

    }

    public int GetLinksLeft()
    {
        return linksMax - linksNum;
    }

    public void LevelUp()
    {
        level += 1;
        linksMax += 1;
    }

    public virtual void UpdateLinks(Vector3 pos = new Vector3())
    {
        if (pos == Vector3.zero)
            pos = this.transform.position;
        if (links == null)
            links = new Dictionary<GameObject, LinkType>();
        linksNum = 0;

        Dictionary<GameObject, LinkType> linksCopy = new Dictionary<GameObject, LinkType>(links);
        foreach (KeyValuePair<GameObject, LinkType> link in links)
        {
            if (link.Value == LinkType.on)
                linksNum += 1;
        }

        foreach (KeyValuePair<GameObject, LinkType> link in linksCopy)
        {
            if (linksNum < linksMax)
            {
                if (links[link.Key] == LinkType.over)
                    links[link.Key] = LinkType.off;
            }
            else
            {
                if (links[link.Key] == LinkType.off)
                    links[link.Key] = LinkType.over;
            }

        }

        loopedLinks = new List<GameObject>();
        keepLinks = new List<GameObject>();
        List<GameObject> deadLinks = new List<GameObject>();
        FindLinks(pos);

        foreach (KeyValuePair<GameObject, LinkType> link in links)
        {
            if (!keepLinks.Contains(link.Key))
            {
                deadLinks.Add(link.Key);
            }
        }

        foreach (GameObject link in deadLinks)
        {
            links.Remove(link);
        }

        UpdateSelf();
    }

    public virtual void ToggleLink(GameObject toggle)
    {
        if (links.ContainsKey(toggle))
        {
            if (links[toggle] == LinkType.on)
            {
                links[toggle] = LinkType.off;
                linksNum -= 1;
                autoLink = false;
                Debug.Log("Turning off auigment");
            }
            else if (links[toggle] == LinkType.off)
            {
                if (linksNum < linksMax)
                {
                    links[toggle] = LinkType.on;
                    linksNum += 1;
                }
            }
        }

        updateLinks = true;
    }

    public virtual void FindLinks(Vector3 pos = new Vector3())
    {

        if (pos == Vector3.zero)
            pos = this.transform.position;
        /*
        Debug.DrawRay(pos, new Vector3(1, 0, 0), Color.cyan, 5);
        Debug.DrawRay(pos, new Vector3(0.5f, 0, 0.866f), Color.yellow, 5);
        Debug.DrawRay(pos, new Vector3(-0.5f, 0, 0.866f), Color.yellow, 5);
        Debug.DrawRay(pos, new Vector3(-1, 0, 0), Color.green, 5);
        Debug.DrawRay(pos, new Vector3(-0.5f, 0, -0.866f), Color.red, 5);
        Debug.DrawRay(pos, new Vector3(0.5f, 0, -0.866f), Color.red, 5);
        */

        RaycastHit hit;
        //Cast 3 - Right
        Physics.Raycast(pos, new Vector3(1, 0, 0), out hit, TerrainGen.hexSize, buildingMask);
        if (hit.collider != null)
            AddLinks(hit.transform.parent.gameObject);
        //Cast 5 - Bottom Right
        Physics.Raycast(pos, new Vector3(0.5f, 0, 0.866f), out hit, TerrainGen.hexSize, buildingMask);
        if (hit.collider != null)
            AddLinks(hit.transform.parent.gameObject);
        //Cast 7 Bottom Left
        Physics.Raycast(pos, new Vector3(-0.5f, 0, 0.866f), out hit, TerrainGen.hexSize, buildingMask);
        if (hit.collider != null)
            AddLinks(hit.transform.parent.gameObject);
        //Cast 9 Left
        Physics.Raycast(pos, new Vector3(-1, 0, 0), out hit, TerrainGen.hexSize, buildingMask);
        if (hit.collider != null)
            AddLinks(hit.transform.parent.gameObject);
        //Cast 11 Upper Left
        Physics.Raycast(pos, new Vector3(-0.5f, 0, -0.866f), out hit, TerrainGen.hexSize, buildingMask);
        if (hit.collider != null)
            AddLinks(hit.transform.parent.gameObject);
        //Cast 1 Upper Right
        Physics.Raycast(pos, new Vector3(0.5f, 0, -0.866f), out hit, TerrainGen.hexSize, buildingMask);
        if (hit.collider != null)
            AddLinks(hit.transform.parent.gameObject);

    }

    private void AddLinks(GameObject newLink)
    {
        if (newLink.GetComponent<AugmentBase>() != null)
        {
            keepLinks.Add(newLink);
            if (!links.ContainsKey(newLink))
            {
                AugmentBase augment = newLink.GetComponent<AugmentBase>();
                if (!augment.CanAugment(buildingProperties))
                    links.Add(newLink, LinkType.incompatible);
                else if (linksNum >= linksMax)
                    links.Add(newLink, LinkType.over);
                else
                    links.Add(newLink, LinkType.off);
                augment.AddBuilding(this);
                //FindLinks(newLink.transform.position);
            }
            else if (links[newLink] == LinkType.on)
            {
                if (!loopedLinks.Contains(newLink))
                {
                    loopedLinks.Add(newLink);
                    FindLinks(newLink.transform.position);
                }

            }
        }
    }
    
    public virtual void SetLink(GameObject toggle, LinkType state)
    {
        if (links.ContainsKey(toggle))
        {
            links[toggle] = state;
        }
        updateLinks = true;
    }

    public void RemoveLink(GameObject oldLink)
    {
        if (links.ContainsKey(oldLink))
        {
            links.Remove(oldLink);
        }
        updateLinks = true;
    }

    public void ForceUpdate()
    {
        UpdateLinks();
    }

    public Dictionary<GameObject, LinkType> GetLinks()
    {
        return links;
    }


    public void ApplyLinks()
    {
        if (links != null)
        {
            if (links.Count > 0)
            {
                foreach (KeyValuePair<GameObject, LinkType> val in links)
                {
                    if (val.Key != null)
                    {
                        if (val.Value == LinkType.on)
                        {
                            AddAugment(val.Key.GetComponent<AugmentBase>().GetAugmentType());
                        }
                    }
                    
                }
            }
        }
    }

    #endregion

    #region enumHandling

    public string GetBuildingType(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.Base:
                return "base";
            case BuildingType.Wall:
                return "wall";

        }
        return null;
    }

    public BuildingType GetBuildingType(string type)
    {
        type = type.ToLower();
        switch (type)
        {
            case "base":
                return BuildingType.Base;
            case "wall":
                return BuildingType.Wall;
        }
        return BuildingType.none;

    }

}

public enum LinkType
{
    off,
    on,
    over,
    incompatible
}

public enum BuildingType
{
    Base,
    Wall,
    none
}


public enum Building
{
    no,
    yes,
    done
}


public enum BuildingProperties
{
    building,
    tower,
    augment,
    home,
    aoe,
    arc,
    fork,
    chain,
    ethereal,
    blocking,
    instant
}

#endregion