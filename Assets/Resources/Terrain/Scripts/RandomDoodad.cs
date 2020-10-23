using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomDoodad : MonoBehaviour
{

    [SerializeField]
    GameObject[] doodads;




    // Start is called before the first frame update
    void Start()
    {
        if (doodads != null)
        {
            Instantiate(doodads[Random.Range(0, doodads.Length)], this.transform);
        }
        Destroy(this);
    }

    //
}
