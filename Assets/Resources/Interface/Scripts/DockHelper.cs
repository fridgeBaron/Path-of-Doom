using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DockHelper : MonoBehaviour
{

    List<DockableWindow> dockedWindows;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}




public enum DockLocation
{
    Left,
    Right,
    Top,
    Bottom,
    Undocked
}

public enum DockAnchor
{
    Left,
    TopLeft,
    Top,
    TopRight,
    Right,
    BottomRight,
    Bottom,
    Center,
    None

}



