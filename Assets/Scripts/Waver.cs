using System;
using System.Collections.Generic;
using UnityEngine;


public class Waver : MonoBehaviour
{
    public static Waver INSTANCE;

    private int totalPopulation = 10;
    private int minPopulation = 3;
    private int maxPopulation = 5;

    private float spawnPeriod = 3;
    private float spawnTimer = 0;

    private List<GameObject> mobs = new List<GameObject>();

    public delegate List<Type> SpawnAlgorithm();
    private SpawnAlgorithm spawnAlgorithm = null;


    public void Start()
    {
        if (INSTANCE)
        {
            Destroy(this);
        }
        else
        {
            INSTANCE = this;
        }
        create_mob();
    }
    public void Update()
    {
        if ((spawnTimer += Time.deltaTime) >= spawnPeriod)
        {
            spawnTimer -= spawnPeriod;
            wave_spawn();
        }
    }

     /***** PUBLIC *****/

    public void StartWave(int total_population, int max_population, int min_population, float spawn_period, SpawnAlgorithm spawn_algorithm = null)
    {
        if (total_population <= 0 || max_population <= 0 || min_population < 0 || spawnPeriod < 0)
        {
            return;
        }
        totalPopulation = total_population;
        maxPopulation = max_population;
        minPopulation = min_population;
        spawnPeriod = spawn_period;
        spawnAlgorithm = spawn_algorithm;
        mobs = new List<GameObject>();
    }

    public void CancelWave()
    {
        totalPopulation = 0;
        maxPopulation = 0;
        minPopulation = 0;
        spawnPeriod = 0;
    }

    public void KillWave()
    {
        foreach (GameObject mob in mobs)
        {
            if (mob)
            {
                Destroy(mob);
            }
        }
        CancelWave();
    }


    /***** PRIVATE *****/
    private void wave_spawn()
    {
        if(get_active_mob_count() >= maxPopulation)
        {

        }
        else if (get_remaining_mob_count() <= 0)
        {

        }
        else if (get_active_mob_count() < minPopulation)
        {
            int population_deficit = minPopulation - get_active_mob_count();
            int spawn_size = Mathf.Min(population_deficit, get_remaining_mob_count());
            spawn_mobs(spawn_size);
        }
        else
        {
            spawn_mobs(1);
        }
    }

    private List<GameObject> spawn_mobs(int population_size)
    {
        List<GameObject> new_population = new List<GameObject> ();
        for (int i = 0; i < population_size; i++)
        {
            new_population.Add(create_mob());
        }
        mobs.AddRange(new_population);
        return new_population;
    }

    private GameObject create_mob(List<Type> components = null)
    {
        GameObject new_mob = new GameObject();
        if(components == null)
        {
            new_mob.AddComponent<Skelly>();
            new_mob.AddComponent<Goon>();
        }
        else
        {
            foreach(Type component in components)
            {
                new_mob.AddComponent(component);
            }
        }
        new_mob.transform.position = transform.position;
        return new_mob;
    }

    private int get_active_mob_count()
    {
        int count = 0;
        foreach(GameObject mob in mobs)
        {
            if (mob != null)
            {
                count++;
            }
        }
        return count;
    }

    private int get_remaining_mob_count()
    {
        return totalPopulation - mobs.Count;
    }
}
