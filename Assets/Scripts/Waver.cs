using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Waver : MonoBehaviour
{
    public static List<GameObject> Mobs = new List<GameObject>();

    public static int WaveCount = 0;
    public static int MobCount = 0;

    public static UnityEvent Finished = new UnityEvent();
    public static UnityEvent Started = new UnityEvent();

    public enum WaveStatus
    {
        Idle,
        Started,
        Finished,
        Boss,
    }
    public static WaveStatus Status = WaveStatus.Idle;

    public static Waver INSTANCE;
    public static Haunt Spawnpoint;
    public static float waveTimer = 0;

    private static int totalSize = 10;
    private static int minPopulation = 3;
    private static int maxPopulation = 5;

    private static Vector2 spawnPeriodRange = new Vector2(3, 6);
    private static float spawnPeriod = 3;
    private static float spawnTimer = 0;

    private static float cooldownPeriod = 10f;
    private static float cooldownTimer = 0f;

   
    public void Start()
    {

        if (INSTANCE)
        {
            Destroy(this);
        }
        else
        {
            INSTANCE = this;
            GameObject new_spawnpoint = new GameObject();
            new_spawnpoint.AddComponent<Shade>();
            Spawnpoint = new_spawnpoint.AddComponent<Haunt>();
            Spawnpoint.State = AIBehaviour.AIState.custom;
            Spawnpoint.waypointCommanded = true;
            Spawnpoint.behaviourParams[AIBehaviour.BehaviourType.waypoint] = (true, 1.0f);
            StartCoroutine(update_spawn_waypoint());
        }
    }

    public void Update()
    {
        if(Status == WaveStatus.Idle)
        {

        }
        else if (Status == WaveStatus.Finished)
        {
            attempt_start(Time.deltaTime);
        }
        else if(Status == WaveStatus.Boss)
        {

        }
        else if (check_wave_dead())
        {
            EndWave();
        }
        else
        {
            waveTimer += Time.deltaTime;
            if ((spawnTimer += Time.deltaTime) >= spawnPeriod)
            {
                spawnTimer -= spawnPeriod;
                spawnPeriod = UnityEngine.Random.Range(spawnPeriodRange.x, spawnPeriodRange.y);
                attempt_spawn();
            }
        }

    }

    /***** PUBLIC *****/

    public static void StartBoss()
    {
        Status = WaveStatus.Boss;
    }

    public static void StartWave(int total_size, int max_population, int min_population)
    {
        if (total_size <= 0 || max_population <= 0 || min_population < 0 || spawnPeriod < 0)
        {
            return;
        }
        totalSize = total_size;
        maxPopulation = max_population;
        minPopulation = min_population;
        ResetWave();
    }

    public static void ResetWave()
    {
        WaveCount++;
        Mobs = new List<GameObject>();
        waveTimer = 0;
        cooldownTimer = 0;
        Status = WaveStatus.Started;
        Started.Invoke();
    }

    public static void EndWave()
    {
        Status = WaveStatus.Finished;
        Finished.Invoke();
    }

    public static void KillWave()
    {
        foreach (GameObject mob in Mobs)
        {
            if (mob)
            {
                Destroy(mob);
            }
        }
        EndWave();
    }


    /***** PRIVATE *****/
    private static void attempt_start(float time_passed)
    {
        if((cooldownTimer += time_passed) >= cooldownPeriod)
        {
            cooldownTimer -= cooldownPeriod;
            if(WaveCount == 10)
            {
                StartBoss();
            }
            else
            {
                int random_reinforcements = UnityEngine.Random.Range(0, (WaveCount % 10) * 2);
                int total = 5 + random_reinforcements;
                int max = Mathf.CeilToInt(total / 2);
                int min = Mathf.FloorToInt(max / 2);
                StartWave(total, max, min);
            }
        }
    }

    private static bool check_wave_dead()
    {
        int dead_count = Mobs.Count(x => x == null);
        return dead_count >= totalSize;
    }

    private static void attempt_spawn()
    {
        if (get_remaining_mob_count() <= 0)
        {

        }
        else if(get_active_mob_count() >= maxPopulation)
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

    private static List<GameObject> spawn_mobs(int population_size)
    {
        List<GameObject> new_population = new List<GameObject> ();
        for (int i = 0; i < population_size; i++)
        {
            new_population.Add(create_mob());
        }
        Mobs.AddRange(new_population);
        return new_population;
    }

    private static GameObject create_mob(List<Type> components = null)
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
        new_mob.transform.position = Spawnpoint.transform.position;
        MobCount++;
        return new_mob;
    }


    private static int get_active_mob_count()
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

    private static int get_remaining_mob_count()
    {
        return totalSize - Mobs.Count;
    }

    private static IEnumerator update_spawn_waypoint()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(spawnPeriodRange.x, spawnPeriodRange.y));
        while (true)
        {
            Spawnpoint.waypointCoordinates = find_target_tile().transform.position;
            Spawnpoint.waypointDeadbanded = false;
            yield return new WaitUntil(() => Spawnpoint.waypointDeadbanded);
            yield return new WaitForSeconds(UnityEngine.Random.Range(spawnPeriodRange.x, spawnPeriodRange.y));
        }
    }
    private static Hextile find_target_tile()
    {
        if (!Player.INSTANCE)
        {
            return Map.CenterTile;
        }
        else if (!Player.INSTANCE.HostEntity)
        {
            return Map.CenterTile;
        }
        else if (!Player.INSTANCE.HostEntity.TileLocation)
        {
            return Map.CenterTile;
        }
        else
        {
            Hextile playerTile = Player.INSTANCE.HostEntity.TileLocation;
            int randomIndex = UnityEngine.Random.Range(0, playerTile.AdjacentTiles.Count);
            return playerTile.AdjacentTiles.ElementAt(randomIndex).Key;
        }
    }

}
