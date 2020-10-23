using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleMaster : MonoBehaviour
{


    private static List<ParticleSystem> deadParticleSystems;
    private static List<ParticleSystem> particleSystems;

    [SerializeField]
    GameObject particleSystemFabLoad;

    [SerializeField]
    private ParticleSystem[] projectileFabsLoad;
    private static ParticleSystem[] projectileFabs;
    [SerializeField]
    private ParticleSystem[] deathFabsLoad;
    private static ParticleSystem[] deathFabs;

    [SerializeField]
    private ParticleSystem buildFabLoad;
    public static ParticleSystem buildFab;


    private static GameObject particleSystemFab;

    public static ParticleMaster instance;


    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            deadParticleSystems = new List<ParticleSystem>();
            particleSystems = new List<ParticleSystem>();
            projectileFabs = projectileFabsLoad;
            deathFabs = deathFabsLoad;
            particleSystemFab = Resources.Load<GameObject>("Assets/Resources/Effects/Particles/ParticleSystemFab.prefab");
            buildFab = buildFabLoad;
            if (particleSystemFab == null)
                particleSystemFab = particleSystemFabLoad;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    /*
    public static ParticleSystem GetSystem()
    {
        ParticleSystem retSystem;

        if (deadParticleSystems.Count > 0)
        {
            retSystem = deadParticleSystems[0];
            deadParticleSystems.RemoveAt(0);
            particleSystems.Add(retSystem);
            return retSystem;
        }
        GameObject newSystem = Instantiate(particleSystemFab);
        retSystem = newSystem.GetComponent<ParticleSystem>();
        particleSystems.Add(retSystem);
        return retSystem;
    }
    */

    public static ParticleSystem MakeSystem(ProjectileParticles projectileType)
    {
        ParticleSystem newSystem;
        //Debug.Log("Making a " + (int)projectileType);
        newSystem = Instantiate(projectileFabs[(int)projectileType]);
        return newSystem;
    }

    public static ParticleSystem MakeSystem(DeathParticles deathType)
    {
        ParticleSystem newSystem;
        //Debug.Log("Making a " + (int)projectileType);
        newSystem = Instantiate(deathFabs[(int)deathType]);
        return newSystem;
    }


    public static void SetSystem(ParticleSystem particleSystem, ProjectileParticles projectileType)
    {
        particleSystem =  projectileFabs[(int)projectileType];
    }

    public static void SetSystem(ParticleSystem particleSystem, DeathParticles deathType)
    {

    }


}



public enum ParticleTypes
{
    projectile,
    death,
    none
}

public enum ProjectileParticles
{
    none = 0,
    fire,
    fireCollide,
    water,
    waterCollide,
    earth,
    earthCollide,
    airCollide,
    basic,
    basicCollide

}

public enum DeathParticles
{
    normal = 0,
    bones,
    slime,
    none
}
