using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(MobSpawner))]
public class MobSpawnerEditor : Editor {
    /*


    MobSpawner spawner;

    SerializedProperty gridSide;
    SerializedProperty spawnNumber;

    // Use this for initialization
    void OnEnable()
    {
        spawner = (MobSpawner)target;
        
    }

    public override void OnInspectorGUI()
    {
        Undo.RecordObject(spawner, "Editing shit");
        spawner.gridSide =(GridSide) EditorGUILayout.EnumPopup("Spawner Grid Side: ", spawner.gridSide);


        spawner.spawnNumber = EditorGUILayout.IntField("Spawn Count: ", spawner.spawnNumber);
        if (spawner.spawnNumber < 0)
            spawner.spawnNumber = 0;
        spawner.spawnTimer = EditorGUILayout.Slider("Spawn Time: ", spawner.spawnTimer, 1, 100);
        spawner.goal =(GameObject) EditorGUILayout.ObjectField("Goal:", spawner.goal, typeof(GameObject), true);
        spawner.spawnParticleFab = (GameObject)EditorGUILayout.ObjectField("Spawner Particle System preFab:", spawner.spawnParticleFab, typeof(GameObject), true);
        spawner.spawnOrbFabHolder = (GameObject)EditorGUILayout.ObjectField("SpawnerOrb preFab:", spawner.spawnOrbFabHolder, typeof(GameObject), true);

        spawner.coolDownTime = EditorGUILayout.Slider("Wave Cooldown: ", spawner.coolDownTime, 0, 100);
        spawner.wavePoints = EditorGUILayout.IntField("Wave Points: ", (int)spawner.wavePoints);
        EditorGUILayout.LabelField("Spawn Points: " + spawner.spawnPoints);
        EditorGUILayout.LabelField("Spawn Cooldown: " + spawner.spawnCoolDown);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField(spawner.spawnTime.ToString());
       // EditorGUILayout.LabelField(tower.lastFrame.ToString());
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        PrefabUtility.RecordPrefabInstancePropertyModifications(spawner);

        serializedObject.ApplyModifiedProperties();

    }
    */
}
