using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(IsoSnap)), CanEditMultipleObjects]
public class isoSnapEditor : Editor {

    

    protected virtual void OnSceneGUI()
    {
        Tools.hidden = true;
        IsoSnap example = (IsoSnap)target;

        float size = 1f;

        EditorGUI.BeginChangeCheck();
        Vector3 newTargetPosition = Handles.PositionHandle(example.transform.position, Quaternion.identity);
        if (Event.current.type == EventType.Repaint)
        {
            Transform transform = ((IsoSnap)target).transform;
            Handles.color = Handles.xAxisColor;
            Handles.ArrowHandleCap(
                0,
                transform.position + new Vector3(0f, 0f, 0f),
                transform.rotation * Quaternion.LookRotation(new Vector3(1, 0.5f, 0)),
                size,
                EventType.Repaint
                );
            Handles.color = Handles.yAxisColor;
            Handles.ArrowHandleCap(
                0,
                transform.position + new Vector3(0f, 0f, 0f),
                transform.rotation * Quaternion.LookRotation(new Vector3(-1, 0.5f, 0)),
                size,
                EventType.Repaint
                );
            /*
            Handles.color = Handles.zAxisColor;
            Handles.ArrowHandleCap(
                0,
                transform.position + new Vector3(0f, 0f, 3f),
                transform.rotation * Quaternion.LookRotation(Vector3.forward),
                size,
                EventType.Repaint
                );
            */
        }


    }


    // Use this for initialization
    void Update () {
        
    }
	
	// Update is called once per frame
	void OnEnable () {
        
    }
}
