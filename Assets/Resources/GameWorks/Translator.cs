using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translator : MonoBehaviour
{


    public static Dictionary<Language, Dictionary<string, string>> codex;
    public static Language language;

    // Start is called before the first frame update
    void Start()
    {
        language = Language.english;
        codex = new Dictionary<Language, Dictionary<string, string>>();
        Dictionary<string, string> english;
        english = new Dictionary<string, string>
        {
            {"build_cost", "Cost: " },
            {"building_speed", "Build Time: " },
            {"augments", "Augments" },
            {"augments_left", "Augments Avaliable: " },
            {"augments_full", "Augments Maxes" },
            {"attack_speed", "APS: " },
            {"attack_range", "Range: " },
            {"attack_damage", "Damage: " },
            {"attack_projectiles", "Projectiles: " },

            {"chain_count", "Chains: " },
            {"chain_count_add", "Chains +" },
            {"chain_min_range", "Min Chain Range:" },
            {"chain_max_range", "Max Chain Range:" },
            {"chain_damage_mult", "Chain DMG %"  },
            {"chain_max_range_mult", "Chain max range %" },

            {"fork_count", "Forks: " },
            {"fork_count_add", "Forks +" },
            {"fork_projectiles", "Fork into: " },
            {"fork_projectiles_add", "Fork into +" },
            {"fork_damage_mult", "Fork DMG %" },

            {"projectile_speed", "Projectile Speed: " },
            {"projectile_speed_mult", "Projectile Speed %" },
            {"projectile_aoe", "AoE Range: " },
            {"projectile_aoe_falloff", "AoE Falloff: " },
            {"projectile_instant",  "Instant" },
            {"projectile_arc", "Arcing Attack" },
            {"never_chain", "Can Never Chain" },
            {"never_fork", "Can Never Fork" },

            {"attack_speed_mult", "Attack speed %" },
            {"attack_damage_mult", "Attack Damage %" },
            {"attack_range_mult", "Attack Range %" },

            {"attack_range_add", "Attack Range +" },
            {"damage_per_second", "DPS: " },


            {"build_air_tower_button", "Air" },
            {"build_basic_tower_button", "Basic" },
            {"build_earth_tower_button", "Earth" },
            {"build_fire_tower_button", "Fire" },
            {"build_water_tower_button", "Water" },


            {"build_damage_augment_button", "Damage" },
            {"build_range_augment_button", "Range"},
            {"build_speed_augment_button", "Speed" },
            {"build_multi_augment_button", "Multi" },
            {"build_chain_augment_button", "Chain" },
            {"build_fork_augment_button", "Fork" },
            {"build_ethereal_augment_button", "Ethereal" },


            {"spawn_time", "Spawn Interval: " },
            {"spawn_number", "Spawn Count: " },
            {"spawn_cooldown", "Orb interval: " },
            {"wave_points", "Orb Points: " }
        };

        codex.Add(Language.english, english);




    }


    public static string Get(string key, Language lang = Language.unknown)
    {
        if (lang == Language.unknown)
            lang = language;

        if (codex[lang].ContainsKey(key))
        {
            return codex[lang][key];
        }
        return key;
    }




}






public enum Language
{
    unknown,
    english
}
