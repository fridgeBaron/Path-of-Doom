using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DockableWindow : MonoBehaviour
{

    Rect window;
    DockableWindow parent;
    List<DockableWindow> children;
    bool isDocked;
    DockLocation dockLocation;
    DockAnchor dockAnchor;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

