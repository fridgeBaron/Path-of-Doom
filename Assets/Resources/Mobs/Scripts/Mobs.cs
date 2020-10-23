using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mobs : MonoBehaviour
{


    public static Mobs instance;

    public static Dictionary<string, Dictionary<string, float>> mobList;
    public static Dictionary<string, Dictionary<string, float>> specialList;
    public static List<string> mobNames;

    public static Sprite[] mobSprites;

    private void Awake()
    {
        

        if (instance == null)
        {
            mobSprites = Resources.LoadAll<Sprite>("Mobs/Sprites/mobSprites");

            instance = this;
            mobList = new Dictionary<string, Dictionary<string, float>>();
            specialList = new Dictionary<string, Dictionary<string, float>>();
            mobNames = new List<string>();
            //
            #region Mobs

            // Basic Skele
            Dictionary<string, float> statList = new Dictionary<string, float>
            {
                { "randomAll", 0.5f },
                { "healthMax", 100 },
                { "speed", 6 },
                { "climbmult", 0.25f },
                { "walk", 0  },
                { "size", 1 },
                { "attack_speed", 1 },
                { "attack_damage", 3 },

                { "world_size", 0.001f },

                { "SPR_1_sprite", 288 },
                { "SPR_2_sprite", 289 },
                { "SPR_3_sprite", 290 },
                { "SPR_4_sprite", 292 },
                { "deathParticle", 1 },

                { "gold", 4 },
                { "spawnPoints", 10 }

            };
            mobList.Add("skeleton", statList);
            mobNames.Add("skeleton");
            // Swole SKele
            statList = new Dictionary<string, float>
            {
                { "randomAll", 0.25f },
                { "healthMax", 500 },
                { "speed", 3.5f },
                { "climbmult", 0.65f },
                { "walk", 0  },
                { "size", 2 },
                { "world_size", 0.02f },
                 { "attack_speed", 3 },
                { "attack_damage", 15 },

                { "SPR_1_sprite", 208 },
                { "SPR_2_sprite", 209 },
                { "SPR_3_sprite", 210 },
                { "SPR_4_sprite", 211 },
                { "SPR_5_sprite", 212 },
                { "deathParticle", 1 },

                { "gold", 10 },
                { "spawnPoints", 100 }

            };
            mobList.Add("bonegolem", statList);
            mobNames.Add("bonegolem");
            //
            // Slime
            statList = new Dictionary<string, float>
            {
                { "randomAll", 0.1f },
                { "healthMax", 15 },
                { "speed", 5f },
                { "climbmult", 1f },
                { "size", .75f },
                { "world_size", 0.01f },
                { "walk", 0  },
                { "attack_speed", 1 },
                { "attack_damage", 1 },

                { "SPR_1_sprite", 234 },
                { "SPR_2_sprite", 235 },

                { "gold", 1 },
                { "spawnPoints", 5 }

            };
            mobList.Add("slime", statList);
            mobNames.Add("slime");
            statList = new Dictionary<string, float>
            {
                { "randomAll", 0.25f },
                { "healthMax", 750 },
                { "speed", 2f },
                { "climbmult", 4f },
                { "size", 3f },
                { "world_size", 0.03f },
                { "yoff", 0.35f },
                { "walk", 0  },
                { "attack_speed", 3 },
                { "attack_damage", 100 },


                { "sprite", 402 },

                { "gold", 20 },
                { "spawnPoints", 300 }

            };
            mobList.Add("tallalex", statList);
            mobNames.Add("tallalex");
            statList = new Dictionary<string, float>
            {
                { "randomAll", 0.7f },
                { "healthMax", 10 },
                { "speed", 9f },
                { "climbmult", 0.1f },
                { "size", 0.5f },

                { "world_size", 0.05f },
                { "yoff", 0.2f },
                { "flight", 0.2f  },
                { "attack_speed", 0.25f },
                { "attack_damage", 1 },


                { "sprite", 403 },

                { "gold", 5 },
                { "spawnPoints", 50 }

            };
            mobList.Add("wonderskelly", statList);
            mobNames.Add("wonderskelly");

            #endregion
            /*   */

            #region Specials

            //Speed Augment
            statList = new Dictionary<string, float>
            {
                { "atkSpeedMult", 1.5f },

                { "projSpeedMult", 1.5f },
            };
            specialList.Add("Speed", statList);

            #endregion

        }
        //
        else if (instance != this)
            Destroy(gameObject.GetComponent(instance.GetType()));
    }






    public Dictionary<string, float> GetMob(string mob)
    {
        if (mobList.ContainsKey(mob))
        {
            return mobList[mob];
        }
        return null;
    }

    public Dictionary<string, float> GetSpecial(string special)
    {
        if (specialList.ContainsKey(special))
        {
            return specialList[special];
        }
        return null;
    }

    public string getRandomMob()
    {
        return mobNames[Random.Range(0, mobNames.Count)];
    }

}
