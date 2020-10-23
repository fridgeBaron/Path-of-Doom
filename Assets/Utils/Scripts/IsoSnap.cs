using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[ExecuteInEditMode]
public class IsoSnap : MonoBehaviour {


    [SerializeField]
    Vector3 isoPos;

    [SerializeField]
    public float tileW;
    [SerializeField]
    public float tileH;

    bool isIso;

    Vector3 lastPos;


	// Use this for initialization
	void Start () {
        isIso = false;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = (isoToCart(isoPos));
	}




    Vector3 isoToCart(Vector3 pos)
    {
        Vector3 retVec;

        retVec.x = (pos.x - pos.y) * tileW;
        retVec.y = (pos.x + pos.y) * tileH / 2;
        retVec.z = retVec.y;



        return retVec;
    }


}
