using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriMenu : MonoBehaviour
{

    string currentMenu;
    Dictionary<string,List<string>> buttonList;
    List<GameObject> buttons;
    List<GameObject> deadButtons;
    List<GameObject> links;
    
    Vector3[] buttonPositions;

    public GameObject triButton;
    public DetailWindow hexDetails;
    public DetailWindow dockDetails;
    public GameObject dockDetailsButton;
    MenuPositioner dragHandle;

    bool docked;

    [SerializeField]
    float menuSize;

    // Start is called before the first frame update
    void Awake()
    {
        buttonList = new Dictionary<string, List<string>>();


        buttons = new List<GameObject>();
        deadButtons = new List<GameObject>();
        links = new List<GameObject>();
        dragHandle = GetComponentInChildren<MenuPositioner>();
        dragHandle.DragWindow = this.gameObject;
        dragHandle.gameObject.SetActive(false);
        hexDetails.docked = false;
        dockDetails.docked = true;

        List<string> newButtons = new List<string>
        {
            "Select",
            "Regen"
        };
        buttonList.Add("startmenu", newButtons);

        newButtons = new List<string>
        {
            "build_air_tower_button",
            "build_basic_tower_button",
            "build_earth_tower_button",
            "build_fire_tower_button",
            "build_water_tower_button"
        };
        buttonList.Add("towers_build", newButtons);

        newButtons = new List<string>
        {
            "build_speed_augment_button",
            "build_damage_augment_button",
            "build_range_augment_button",
            "build_chain_augment_button",
            "build_fork_augment_button",
            "build_multi_augment_button",
            "build_ethereal_augment_button"
        };
        buttonList.Add("augments", newButtons);

        newButtons = new List<string>
        {
            "Wall",
            "Other thing"
        };
        buttonList.Add("buildings", newButtons);

        newButtons = new List<string>
        {
            "Towers",
            "Augments",
            "Buildings",
            "Path"
        };
        buttonList.Add("base", newButtons);
        newButtons = new List<string>
        {
            "Sell"
        };
        buttonList.Add("towerselect", newButtons);

        newButtons = new List<string>
        {
            "Sell"
        };
        buttonList.Add("augmentselect", newButtons);

        newButtons = new List<string>
        {
            "Sell"
        };
        buttonList.Add("buildingselect", newButtons);

        newButtons = new List<string>
        {
            "Points",
            "More",
            "Less",
            "Faster",
            "Slower",
        };
        buttonList.Add("spawner", newButtons);

        buttonPositions = new Vector3[]
        {
            new Vector3(4,32,-1),
            new Vector3(4,-32,-1),
            new Vector3(66,32,1),
            new Vector3(66,-32,1),
            new Vector3(68,0,-1),
            new Vector3(4,-64,1),
            new Vector3(0,68,1)
        };
        //
    }

    public void PlaceMenu(Vector3 pos)
    {
        this.transform.position = dragHandle.OffSetPosition(pos);
    }

    public string CurrentMenu()
    {
        return currentMenu;
    }

    public void CreateMenu(string menu)
    {
        menu = menu.ToLower();
        currentMenu = menu;
        int pos = 0;
        Vector3 newPos;
        if (buttonList.ContainsKey(menu))
        {
            foreach (string button in buttonList[menu])
            {
                GameObject newObject;
                newObject = getButton();
                newObject.GetComponent<TriButton>().MakeButton(button, button);
                newObject.transform.localScale = new Vector3(menuSize, menuSize, menuSize);
                newPos = buttonPositions[pos];
                
                if (newPos.z == -1)
                {
                    newObject.transform.localScale = new Vector3(-menuSize, menuSize, menuSize);
                    newObject.GetComponent<TriButton>().FlipButton();
                }
                newPos *= menuSize;
                newPos.z = 0;
                newObject.transform.localPosition = newPos;
                pos++;
            }
        }
    }

    private GameObject getButton()
    {
        dragHandle.gameObject.SetActive(true);
        if (deadButtons.Count > 0)
        {
            GameObject button;
            button = deadButtons[0];
            button.SetActive(true);
            buttons.Add(button);
            deadButtons.Remove(button);
            return button;
        }

        GameObject newObject = Instantiate(triButton, this.transform);
        buttons.Add(newObject);
        return newObject;
    }

    public void DestroyMenu()
    {
        currentMenu = null;
        dragHandle.gameObject.SetActive(false);
        DestroyLinks();
        DestroyButtons();
        DestroyDetails();
    }

    public void DockDetails()
    {
        docked = !docked;
        if (docked)
        {
            Vector3 newPos = dockDetailsButton.transform.position;
            newPos.x -= 250;
            dockDetailsButton.transform.position = newPos;
            dockDetailsButton.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            Vector3 newPos = dockDetailsButton.transform.position;
            newPos.x += 250;
            dockDetailsButton.transform.position = newPos;
            dockDetailsButton.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    public bool HaveLinks()
    {
        if (links.Count > 0)
            return true;
        return false;
    }

    public void AddLink(GameObject newLink)
    {
        links.Add(newLink);
    }

    public void ReDrawLinks()
    {
        foreach (GameObject link in links)
        {
            LinkToggle toggle = link.GetComponent<LinkToggle>();
            if (toggle != null)
            {
                GameObject linkingAugment = toggle.linkingAugment;
                if (linkingAugment != null)
                    link.transform.position = MouseHook.mousehook.GetCamera().WorldToScreenPoint(linkingAugment.transform.position);
            }
                
        }
    }

    public void DestroyLinks()
    {
        foreach (GameObject link in links)
        {
            Destroy(link);
        }
        links.Clear();
    }

    public void DestroyButtons()
    {
        foreach (GameObject button in buttons)
        {
            deadButtons.Add(button);
            button.transform.localScale = new Vector3(1, 1, 1);
            button.GetComponent<TriButton>().DefaultButton();
            button.SetActive(false);
        }
        buttons.Clear();
    }

    public void ResetButtons()
    {
        foreach (GameObject button in buttons)
        {
            button.GetComponent<TriButton>().SetState(0);
        }
    }


    public void CreateDetails(Vector2 pos = new Vector2())
    {
        if (!docked)
        {
            hexDetails.gameObject.SetActive(true);
            if (pos != new Vector2())
                hexDetails.SetPosition(pos);
            hexDetails.ClearDetails();
        }
        else
        {
            dockDetails.gameObject.SetActive(true);
            dockDetails.ClearDetails();
        }      
    }


    public void AddDetail(string detail, float value)
    {
        if (!docked)
            hexDetails.AddDetail(detail, value);
        else
            dockDetails.AddDetail(detail, value);
    }

    public void RefreshDetails()
    {
        if (!docked)
            hexDetails.RefreshDetails(menuSize);
        else
            dockDetails.RefreshDetails(menuSize);
    }

    public void DestroyDetails()
    {
        hexDetails.ClearDetails();
        hexDetails.gameObject.SetActive(false);

        dockDetails.ClearDetails();
        hexDetails.gameObject.SetActive(false);
    }

    public void LoadDetails(Dictionary<string, float> details)
    {
        if (!docked)
            hexDetails.LoadDetails(details);
        else
            dockDetails.LoadDetails(details);
    }

    public float GetDetail(string detail)
    {
        if (!docked)
            return hexDetails.GetDetail(detail);
        else
            return dockDetails.GetDetail(detail);
        //
    }
}


