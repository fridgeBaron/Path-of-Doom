using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {


    [SerializeField]
    int gold;
    Text goldText;

    [SerializeField]
    int life;
    Text lifeText;

    [SerializeField]
    GameObject canvas;

    public static Player player;

	// Use this for initialization
	void Start () {
        Text[] texts = GetComponentsInChildren<Text>();
        for (int i = 0; i < texts.Length; i ++)
        {
            switch (texts[i].text)
            {
                case "[gold]":
                    goldText = texts[i];
                    break;
                case "[life]":
                    lifeText = texts[i];
                    break;
            }
        }
        player = this;
	}
	
    public bool hasGold(int checkGold)
    {
        if (gold >= checkGold)
            return true;
        return false;
    }


    public void TakeGold(int takeGold)
    {
        gold -= takeGold;
    }
    public void GiveGold(int giveGold)
    {
        gold += giveGold;
    }

    public void TakeLife(int takeLife)
    {
        life -= takeLife;
    }
    public void GiveLife(int giveLife)
    {
        life += giveLife;
    }

	// Update is called once per frame
	void FixedUpdate () {
        goldText.text = "" + gold;
        lifeText.text = "" + life;
	}



}
