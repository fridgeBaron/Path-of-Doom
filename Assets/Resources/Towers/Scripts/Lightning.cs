using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour {

    LineRenderer lightningLine;
    List<mobBase> targets;
    List<Vector2> points;

    TowerBase tower;

    [SerializeField]
    public int number;

    private float timeKill;
    private float timeAlive;


	// Use this for initialization
	void OnEnable () {
        ProjectileBase.AddLightning(this);
        timeAlive = 0;
        lightningLine = GetComponent<LineRenderer>();
        timeKill = 1f;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        setLine(targets);
        timeAlive += Time.deltaTime;
        if (timeAlive >= timeKill)
        {
            //Debug.Log("Dead number Adding: " + number);
            ProjectileBase.RemoveLightning(this);
        }
        
	}

    public void SetTower(TowerBase tower)
    {
        this.tower = tower;
    }
    public void SetNumber(int number)
    {
        this.number = number;
    }

    Vector3[] ToPointArray(List<mobBase> targets)
    {
        Vector3[] pointArray;
        pointArray = new Vector3[targets.Count + 1];
        pointArray[0] = tower.transform.position;
        for (int i = 1; i < targets.Count; i ++)
        {
            pointArray[i] = targets[i].transform.position;
        }
        return pointArray;
    }

    void setLine(List<mobBase> targets)
    {
        lightningLine.positionCount = targets.Count;
        lightningLine.SetPositions(ToPointArray(this.targets));
    }

    public void setTargets(List<mobBase> targets)
    {
        this.targets = targets;
        
    }

    void setPoints(List<Vector2> points)
    {

    }
}
