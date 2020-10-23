using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPositioner : MonoBehaviour
{

    bool isDragging;
    public Vector2 offset;
    Vector3 dragStart;

    GameObject dragWindow;
    DockableWindow dockWindow;

    public GameObject DragWindow
    {
        set
        {
             dragWindow = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        isDragging = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    public Vector3 OffSetPosition(Vector3 position)
    {
        position.x += offset.x;
        position.y += offset.y;
        return position;
    }


    private void FixedUpdate()
    {
        if (isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            if (mousePos.x > 0 && mousePos.x < Screen.width && mousePos.y > 0 && mousePos.y < Screen.height)
            {
                Vector3 dragPos = dragWindow.transform.position;
                dragPos.x -= offset.x;
                dragPos.y -= offset.y;
                Vector2 oldOffset = offset;

                offset.x = (mousePos.x - dragStart.x);
                offset.y = (mousePos.y - dragStart.y);
                offset += oldOffset;
                dragPos.x += offset.x;
                dragPos.y += offset.y;
                
                dragWindow.transform.position = dragPos;
                
                if (offset.x < (-Screen.width/2 + 10) || offset.x > (Screen.width/2-10) || offset.y < (-Screen.height/2 + 10) || offset.y > (Screen.height/2-10))
                {
                    offset = Vector2Int.zero;
                    isDragging = false;
                }
                dragStart = Input.mousePosition;
            }
            else
            {
                isDragging = false;
            }
        }
    }

    public void StartDrag()
    {
        isDragging = true;
        dragStart = Input.mousePosition;
        if (dragWindow == null)
            dragWindow = this.transform.parent.gameObject;
    }
}
