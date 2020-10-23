using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthTracker : MonoBehaviour
{

    Texture2D healthTex;
    [SerializeField]
    MeshRenderer healthRenderer;

    // Start is called before the first frame update
    void OnEnable()
    {
        healthTex = new Texture2D(1, 1);
        //healthRenderer = GetComponentInChildren<MeshRenderer>(true);
        healthRenderer.material.mainTexture = healthTex;
        healthRenderer.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetHealth(float val)
    {
        Color newColor = new Color();
        newColor.r = Mathf.Clamp(2 - (val * 2), 0, 1);
        newColor.g = Mathf.Clamp(-1 + (val * 2), 0, 1);
        healthTex.SetPixel(1, 1, newColor);
        healthTex.Apply();
        healthRenderer.gameObject.SetActive(true);
    }


}
