using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinkToggle : MonoBehaviour
{


    public LinkType toggle;
    [SerializeField]
    Sprite toggleOff;
    [SerializeField]
    Sprite toggleOn;
    [SerializeField]
    Sprite toggleOver;
    [SerializeField]
    Sprite toggleIncompatible;



    public GameObject linkingTower;
    public GameObject linkingAugment;


    // Start is called before the first frame update
    void Start()
    {
           
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Toggle()
    {
        if (toggle == LinkType.on)
        {
            GetComponent<Image>().sprite = toggleOff;
            toggle = LinkType.off;
        }
        else if (toggle == LinkType.off)
        {
            GetComponent<Image>().sprite = toggleOff;
            toggle = LinkType.on;
        }
        BuildingBase building = linkingTower.GetComponent<BuildingBase>();
        if (linkingTower != null && toggle == LinkType.over)
        {
            if (building.GetLinksLeft() == 0 && Player.player.hasGold(100))
            {
                Player.player.TakeGold(100);
                building.LevelUp();
                toggle = LinkType.on;
                //building.AddLink(linkingAugment, LinkType.on);
                building.ToggleLink(linkingAugment);
                building.SetLink(linkingAugment, LinkType.on);
                building.ForceUpdate();
                MouseHook.mousehook.UpdateLinks();
                return;
            }
        }
        building.ToggleLink(linkingAugment);

    }

    public void SetToggle(LinkType state)
    {
        if (state == LinkType.on)
            GetComponent<Image>().sprite = toggleOn;
        else if (state == LinkType.off)
            GetComponent<Image>().sprite = toggleOff;
        else if (state == LinkType.over)
            GetComponent<Image>().sprite = toggleOver;
        else if (state == LinkType.incompatible)
            GetComponent<Image>().sprite = toggleIncompatible;
        toggle = state;
        if (linkingTower != null)
            linkingTower.GetComponent<TowerBase>().SetLink(linkingAugment, state);
    }

}


