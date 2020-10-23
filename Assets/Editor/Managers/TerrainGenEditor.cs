using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;


[CustomEditor(typeof(TerrainGen))]
public class TerrainGenEditor : Editor
{
    /*
    TerrainGen t;
    float offSet;
    // Start is called before the first frame update
    void OnEnable()
    {
        t = (TerrainGen)target;
    }

    public override void OnInspectorGUI()
    {
        TerrainGen.gridY = EditorGUILayout.IntField("Grid Y: ", TerrainGen.gridY);
        base.OnInspectorGUI();
        offSet = 20;
        float heightOffSet = (20) * 21;
        int newNoiseLength = Mathf.Clamp(EditorGUILayout.IntField("TerrainGenLength: ", t.noiseMapLength),0,15);
       
        if (t.noiseMaps != null)
        {
            if (newNoiseLength != t.noiseMapLength)
            {
                NoiseMap[] newMap = new NoiseMap[newNoiseLength];
                if (newNoiseLength > t.noiseMapLength)
                {
                    for (int i = 0; i < t.noiseMaps.Length; i++)
                    {
                        newMap[i] = t.noiseMaps[i];
                    }
                }
                else if (newNoiseLength < t.noiseMapLength)
                {
                    for (int i = 0; i < newNoiseLength; i++)
                    {
                        newMap[i] = t.noiseMaps[i];
                    }
                }
                for (int i = 0; i < t.noiseMaps.Length; i++)
                {
                    if (t.noiseMaps[i] == null)
                        t.noiseMaps[i] = new NoiseMap();
                }
            }
            for (int i = 0; i < t.noiseMaps.Length; i++)
            {
                if (t.noiseMaps[i] != null)
                {
                    t.noiseMaps[i].scale = EditorGUILayout.FloatField("Scale: ", t.noiseMaps[i].scale);
                    t.noiseMaps[i].weight = EditorGUILayout.FloatField("Weight: ", t.noiseMaps[i].weight);
                    t.noiseMaps[i].offSet = EditorGUILayout.Vector2IntField("Offset: ", t.noiseMaps[i].offSet);
                    t.CalcNoise(t.noiseMaps[i]);
                    
                    EditorGUI.DrawPreviewTexture(new Rect(90,heightOffSet + i * (offSet * 3), offSet * 3, offSet * 3), t.noiseMaps[i].noiseMap);
                }
                else
                {
                    t.noiseMaps[i] = new NoiseMap();
                }
            }
        }
        else
        {
            t.noiseMaps = new NoiseMap[t.noiseMapLength];
        }
        t.noiseMapLength = newNoiseLength;

        t.TryGenHexGrid();
    }
    */
}
