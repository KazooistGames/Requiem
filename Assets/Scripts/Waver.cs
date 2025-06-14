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

    public static Waver INSTANCE;


    public static bool paused = true;

    public static float waveTimer = 0;

    private static int minPopulation = 1;
    private static int maxPopulation = 20;

    private static float biter_period = 2;
    private static float biter_timer = 0f;


   
    public void Start()
    {

        if (INSTANCE)
        {
            Destroy(INSTANCE);
        }

        INSTANCE = this;

        StartCoroutine(spawn_cycler());

    }

    public void Update()
    {
        biter_timer += Time.deltaTime;
    }

    /***** PUBLIC *****/



    /***** PRIVATE *****/
    private IEnumerator spawn_cycler()
    {

        while (true)
        {
            yield return null;
            Mobs = Mobs.Where(x => x != null).ToList();

            if (paused)
            {

            }
            else if (reason_to_spawn())
            {
                yield return spawn_biters();
                biter_timer -= biter_period;
                biter_period = UnityEngine.Random.Range(5, 8);
            }
        }


    }


    private bool reason_to_spawn()
    {
        int total_living_mobs = get_active_mob_count();

        if(total_living_mobs >= maxPopulation)
        {
            return false;
        }
        else if (total_living_mobs < minPopulation)
        {
            return true;
        }
        else if(biter_timer >= biter_period)
        {
            return true;
        }

        return false;
    }

    private IEnumerator spawn_biters()
    {
        yield return null;
        int biter_group_size = Mathf.RoundToInt(UnityEngine.Random.Range(3, 5));
        Hextile biter_chamber = random_chamber();

        for (int i = 0; i < biter_group_size; i++)
        {
            Vector3 random_spot_in_chamber = Requiem.RAND_POS_IN_TILE(biter_chamber);
            Mobs.Add(Requiem.SPAWN(typeof(Skully), typeof(Biter), random_spot_in_chamber));
            yield return null;
        }

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

    private static Hextile random_chamber()
    {
        int random_index = Mathf.RoundToInt(UnityEngine.Random.Range(0, Map.Chambers.Count()));
        return Map.Chambers[random_index];
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
