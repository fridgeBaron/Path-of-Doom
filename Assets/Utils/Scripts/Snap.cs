using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Snap : MonoBehaviour {


    [SerializeField]
    bool doSnap;
    [SerializeField]
    int snapSubdivisions;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (doSnap)
        {
            Vector3 position;
            position = transform.position;
            position.x = Mathf.RoundToInt(position.x*snapSubdivisions);
            position.y = Mathf.RoundToInt(position.y*snapSubdivisions);
            position.x /= snapSubdivisions;
            position.y /= snapSubdivisions;

            //Debug.Log(position.x + " / " + position.y);

            transform.position = position;
        }
	}
}
