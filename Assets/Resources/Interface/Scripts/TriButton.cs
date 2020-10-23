using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriButton : MonoBehaviour
{


    string buttonName;
    string buttonValue;

    bool inactive;

    [SerializeField]
    Sprite sprDefault;
    [SerializeField]
    Sprite sprHighlight;
    [SerializeField]
    Sprite sprNull;
    Image spr;

    // Start is called before the first frame update
    void Awake()
    {
        spr = GetComponent<Image>();
        spr.alphaHitTestMinimumThreshold = 0.5f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    public void SetState(int state)
    {
        if (inactive == true)
            return;
        switch (state)
        {
            case -1: spr.sprite = sprNull; break;
            case 0: spr.sprite = sprDefault; break;
            case 1: spr.sprite = sprHighlight; break;
            default: spr.sprite = sprNull; break;
        }
    }

    public void MakeButton(string newName, string newValue, int newState = 0)
    {
        buttonName = Translator.Get(newName);
        Text buttonText = GetComponentInChildren<Text>();
        buttonText.text = buttonName;
        buttonValue = newValue;
        SetState(newState);
    }

    public void MakeButton()
    {
        SetState(-1);
        inactive = false;
        //
    }

    public void FlipButton()
    {
        Text text = GetComponentInChildren<Text>();
        text.transform.localScale = new Vector3(-1, 1, 1);
        //text.alignment = TextAnchor.MiddleRight;
    }
    public void DefaultButton()
    {
        Text text = GetComponentInChildren<Text>();
        text.transform.localScale = new Vector3(1, 1, 1);
        //text.alignment = TextAnchor.MiddleRight;
    }
    public string getValue()
    {
        return buttonValue;
    }

}
