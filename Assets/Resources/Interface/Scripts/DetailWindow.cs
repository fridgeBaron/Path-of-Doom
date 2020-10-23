using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class DetailWindow : MonoBehaviour
{



    private Dictionary<string, float> values;
    [SerializeField]
    private List<Text> lines;
    [SerializeField]
    private List<Text> oldLines;

    RectTransform rectTransform;

    private GameObject textFab;

    float menuSize;

    public bool docked;
    NumberFormatInfo nfi;
    MenuPositioner dragHandle;
    

    // Start is called before the first frame update
    void Awake()
    {
        values = new Dictionary<string, float>();
        lines = new List<Text>();
        oldLines = new List<Text>();
        textFab = GetComponentInChildren<Text>().gameObject;
        dragHandle = GetComponentInChildren<MenuPositioner>(true);
        dragHandle.DragWindow = this.gameObject;
        textFab.SetActive(false);
        rectTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false);
        nfi = new NumberFormatInfo();
        nfi.NumberDecimalDigits = 0;
        this.transform.parent = this.transform.parent.parent;
    }

    public void SetPosition(Vector3 pos)
    {
        this.transform.position = dragHandle.OffSetPosition(pos);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void RefreshDetails(float newMenuSize)
    {
        //menuSize = newMenuSize;
        //transform.localScale = new Vector3(menuSize, menuSize, menuSize);
        SetText();
        //lines.Sort();
        UpdateSize();
    }

    private void UpdateSize()
    {
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, lines.Count * 16);
        //transform.localPosition = new Vector2(transform.position.x + , transform.position.y);
        Vector3 pos = transform.position;
        if (docked)
        {
            Vector3 menuPos;
            menuPos = rectTransform.localPosition;
            menuPos.y = -374 - (lines.Count * 8); ;
            menuPos += rectTransform.parent.position;
            rectTransform.position = menuPos;

        }
        int yOffSet = 0;

        yOffSet =(lines.Count * 8) - 16;
        int i = 0;
        foreach (Text line in lines)
        {
            pos.x = 0;
            pos.y = -16 * i + yOffSet;
            i++;
            line.transform.localPosition = pos;
        }
    }

    private void SetText()
    {
        Text line;
        foreach (KeyValuePair<string, float> value in values)
        {
            line = GetLine();
            if (value.Value != -1)
            {
                line.text = value.Key + string.Format("{0:0.#}", value.Value);
            }
                
            else
                line.text = value.Key;
        }
    }

    public bool AddDetail(string detail, float value, bool overWrite = false)
    {
        if (!values.ContainsKey(detail))
        {
            values.Add(detail, value);
            return false;
        }
        else if (overWrite)
        {
            values[detail] = value; ;
        }
        return true;
    }

    public bool RemoveDetail(string detail, float value)
    {
        if (values.ContainsKey(detail))
        {
            values.Remove(detail);
            return true;
        }
        return false;
    }

    public bool UpdateDetail(string detail, float value)
    {
        if (values.ContainsKey(detail))
        {
            values[detail] = value;
            return true;
        }
        return false;
    }

    private Text GetLine()
    {
        if (oldLines.Count > 0)
        {
            Text retText = oldLines[0];
            retText.gameObject.SetActive(true);
            oldLines.Remove(retText);
            lines.Add(retText);
            return retText;
        }
        else
        {
            Text retText = Instantiate(textFab).GetComponent<Text>();
            retText.gameObject.transform.SetParent(this.transform);
            lines.Add(retText);
            return retText;
        }
    }

    private void ClearLines()
    {
        foreach (Text line in lines)
        {
            oldLines.Add(line);
            line.gameObject.SetActive(false);
        }
        lines.Clear();
    }

    public void ClearDetails()
    {
        ClearLines();
        values.Clear();
    }


    public void LoadDetails(Dictionary<string, float> details)
    {
        int links = -1;
        int linksMax = -1;

        float damage = 0;
        float attackSpeed = 0;

        foreach (KeyValuePair<string, float> detail in details)
        {
            float val = detail.Value;
            string key = detail.Key;
            if (key.Contains("mult"))
                val *= 100;
            switch (detail.Key)
            {
                case "projectile_yoff":
                case "building_offset":
                case "projectile_particles":
                case "projectile_explosion":
                    break;


                default:
                    AddDetail(Translator.Get(detail.Key), val);
                    break;
            }
            //
        }
        if (damage != 0 && attackSpeed != 0)
        {
            AddDetail("DPS: ", attackSpeed * damage);
        }

        if (links != -1)
        {
            if (linksMax - links > 0)
        {
            AddDetail("Augments avaliable: ", linksMax - links);
        }
        else if (linksMax == links)
        {
            AddDetail("Augments Maxed!", -1);
        }
        }
        
    }

    public float GetDetail(string detail)
    {
        if (values.ContainsKey(detail))
        {
            return values[detail];
        }

        return -1;
    }
}
