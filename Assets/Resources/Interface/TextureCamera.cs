using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureCamera : MonoBehaviour
{

    RenderTexture rendTex;


    // Start is called before the first frame update
    void Start()
    {
        rendTex = GetComponent<Camera>().targetTexture;
    }

    // Update is called once per frame
    void OnGUI()
    {
        Graphics.DrawTexture(new Rect(-Screen.width/2, Screen.height/2, Screen.width, -Screen.height), rendTex);
        ClearOutTexture();
    }


    private void ClearOutTexture()
    {
        RenderTexture.active = rendTex;
        GL.Clear(true, true, Color.clear);
    }
}
