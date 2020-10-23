using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(TowerBase))]
public class towerBaseEditor : Editor {

    TowerBase tower;
    /*
	// Use this for initialization
	void Awake () {
        tower = (TowerBase)target;
	}

    public override void OnInspectorGUI()
    {
        
        tower.AttackRange = EditorGUILayout.Slider("Range: ", (tower.AttackRange), 0, 10);
        tower.AttackSpeed = EditorGUILayout.Slider("Speed: ", tower.AttackSpeed, 10, 0.1f);

        tower.AttackDamage = EditorGUILayout.FloatField("Damage: ", tower.AttackDamage);
        tower.ProjectileSpeed = EditorGUILayout.Slider("Projectile Speed: ", tower.ProjectileSpeed, 1f, 20);
        tower.ProjectileAoE = EditorGUILayout.Slider("Aoe: ", tower.ProjectileAoE, 0, 1);
        tower.ProjectileChain = EditorGUILayout.IntSlider("Chain Targets: ", tower.ProjectileChain, 0, 10);
        tower.ProjectileChainDistanceMin = EditorGUILayout.Slider("Chain min: ", tower.ProjectileChainDistanceMin, 0, 10);
        tower.ProjectileChainDistanceMax = EditorGUILayout.Slider("Chain max: ", tower.ProjectileChainDistanceMax, 0, 10);

        

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (tower.statusEffect != null)
        {
            tower.statusEffect.moveSpeedMult = EditorGUILayout.Slider("Slow %: ", tower.statusEffect.moveSpeedMult, 0, 2);


        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        EditorGUILayout.LabelField(ProjectileBase.GetCount().ToString());

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (tower.targets != null)
        {
            if (tower.targets.Count > 0)
            {
                foreach (mobBase mob in tower.targets)
                {
                    //EditorGUILayout.LabelField(mob.Health.ToString());
                }
            }
        }
        
    }
*/
}
