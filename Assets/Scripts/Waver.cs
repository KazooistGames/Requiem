using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waver : MonoBehaviour
{
    public enum Status
    {

        Disabled,
        Paused,
        Spawning,
        Waiting,
    }
    public Status status = Status.Disabled;

    public int TotalPopulation;
    public int MinPopulation;
    public int MaxPopulation;

    private float spawn_period = 3;

    List<GameObject> Mobs = new List<GameObject>();

    private void wave_spawn()
    {
        if(get_active_mob_count() >= MaxPopulation)
        {

        }
        else if (get_remaining_mob_count() <= 0)
        {

        }
        else if (get_active_mob_count() < MinPopulation)
        {
            int spawn_deficit = MinPopulation - get_active_mob_count();
            int spawn_size = Mathf.Min(spawn_deficit, get_remaining_mob_count());
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
        Mobs.AddRange(new_population);
        return new_population;
    }

    private GameObject create_mob()
    {
        GameObject new_mob = new GameObject();
        new_mob.AddComponent<Skelly>();
        new_mob.AddComponent<Goon>();
        return new_mob
    }

    private int get_active_mob_count()
    {
        int count = 0;
        foreach(GameObject mob in Mobs)
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
        return TotalPopulation - Mobs.Count;
    }
}
