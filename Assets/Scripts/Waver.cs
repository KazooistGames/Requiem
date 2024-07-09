using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Waver : MonoBehaviour
{
    public static List<GameObject> Mobs = new List<GameObject>();
    public static GameObject BossMob;

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
    public static Haunt HauntSpawn;
    public static float waveTimer = 0;

    private static int totalSize = 10;
    private static int minPopulation = 3;
    private static int maxPopulation = 5;

    private static Vector2 spawnPeriodRange = new Vector2(3, 6);
    private static float spawnPeriod = 3;
    private static float spawnTimer = 0;

    private static float cooldownPeriod = 3f;
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
            HauntSpawn = new_spawnpoint.AddComponent<Haunt>();
            Light spawnLight = HauntSpawn.gameObject.AddComponent<Light>();
            spawnLight.intensity = 5;
            spawnLight.range = 0.5f;
            spawnLight.color = new Color(0.75f, 0.5f, 1f);
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
            if (BossMob)
            {
                HauntSpawn.transform.position = BossMob.transform.position;
            }
            else
            {
                EndBoss();
            }
        }
        else if (check_wave_dead())
        {
            EndWave();
        }
        else
        {
            attempt_spawn();
        }

    }

    /***** PUBLIC *****/

    public static void StartBoss()
    {
        WaveCount++;
        HauntSpawn.GetComponent<Entity>().model.SetActive(false);
        BossMob = new GameObject();
        BossMob.transform.position = HauntSpawn.transform.position;
        BossMob.AddComponent<Wraith>();
        BossMob.AddComponent<Revanent>();
        Status = WaveStatus.Boss;
    }

    public static void EndBoss()
    {
        HauntSpawn.GetComponent<Entity>().model.SetActive(true);
        Status = WaveStatus.Finished;
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
        collect_everything(HauntSpawn.gameObject);
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
            int boss_wave = 5;
            if(WaveCount%boss_wave == 0)
            {
                StartBoss();
            }
            else
            {
                int random_reinforcements = UnityEngine.Random.Range(0, WaveCount % boss_wave);
                int total = 10 + random_reinforcements * 2;
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
        waveTimer += Time.deltaTime;
        if (get_remaining_mob_count() <= 0)
        {

        }
        else if(get_active_mob_count() >= maxPopulation)
        {

        }
        else if((spawnTimer += Time.deltaTime) >= spawnPeriod)
        {            
            spawn_mobs(1);
            spawnTimer = 0;
            if (get_active_mob_count() < minPopulation)
            {
                spawnPeriod = 1;
            }
            else
            {
                spawnPeriod = UnityEngine.Random.Range(spawnPeriodRange.x, spawnPeriodRange.y);
            }
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
        new_mob.transform.position = HauntSpawn.transform.position;
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
            HauntSpawn.waypointCoordinates = Requiem.RAND_POS_IN_TILE(find_target_tile());
            HauntSpawn.waypointDeadbanded = false;
            yield return new WaitUntil(() => HauntSpawn.waypointDeadbanded);
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
            return Player.INSTANCE.HostEntity.TileLocation;
        }
    }

    private static void collect_everything(GameObject collectionTarget)
    {
        Weapon[] weapons = FindObjectsOfType<Weapon>();
        float telly_time;
        foreach (Weapon weapon in weapons)
        {
            if (!weapon.Wielder && !weapon.ImpaledObject && weapon != Player.INSTANCE.HostWeapon)
            {
                telly_time = Mathf.Pow((collectionTarget.transform.position - weapon.gameObject.transform.position).magnitude, 0.75f);
                weapon.Telecommute(collectionTarget, telly_time, (x) => Destroy(x.gameObject));
            }
        }
        Bone[] bones = FindObjectsOfType<Bone>();
        foreach (Bone bone in bones)
        {
            telly_time = Mathf.Pow((collectionTarget.transform.position - bone.gameObject.transform.position).magnitude, 0.75f);
            bone.Collect(collectionTarget, telly_time, (x) => Destroy(x.gameObject));
        }
    }

}
