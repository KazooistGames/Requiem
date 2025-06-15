using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public abstract class Waver : MonoBehaviour
{

    public List<GameObject> Mobs = new List<GameObject>();

    public Type entity_type = typeof(Skully);
    public Type behaviour_type = typeof(Biter);

    public Vector2 population_range = new Vector2(1, 5);
    public Vector2 group_size_range = new Vector2(1, 3);
    public Vector2 spawn_period_range = new Vector2(8, 12);

    public bool paused = true;

    private float spawn_period = 2;
    private float spawn_timer = 0f;


    public virtual void Start()
    {
        StartCoroutine(spawn_cycler());
    }

    public virtual void Update()
    {
        spawn_timer += Time.deltaTime;
    }

    public void OnDestroy()
    {
        StopAllCoroutines();
    }

    /***** PUBLIC *****/



    /***** PRIVATE *****/
    private IEnumerator spawn_cycler()
    {
        paused = true;
        while (true)
        {
            yield return null;
            Mobs = Mobs.Where(x => x != null).ToList();

            if (paused)
            {

            }
            else if (reason_to_spawn())
            {
                yield return spawn_mobs();
                spawn_timer -= spawn_period;
                spawn_period = UnityEngine.Random.Range(spawn_period_range.x, spawn_period_range.y);
            }
        }


    }


    private bool reason_to_spawn()
    {
        int total_living_mobs = get_active_mob_count();

        if(total_living_mobs >= population_range.y)
        {
            return false;
        }
        else if (total_living_mobs < population_range.x)
        {
            return true;
        }
        else if(spawn_timer >= spawn_period)
        {
            return true;
        }

        return false;
    }

    private IEnumerator spawn_mobs()
    {
        yield return null;
        int biter_group_size = Mathf.RoundToInt(UnityEngine.Random.Range(group_size_range.x, group_size_range.y));
        Hextile biter_chamber = random_chamber();

        for (int i = 0; i < biter_group_size; i++)
        {
            Vector3 random_spot_in_chamber = Requiem.RAND_POS_IN_TILE(biter_chamber);
            Mobs.Add(Requiem.SPAWN(entity_type, behaviour_type, random_spot_in_chamber));
            yield return null;
        }

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

    private static Hextile random_chamber()
    {
        int random_index = Mathf.RoundToInt(UnityEngine.Random.Range(0, Map.Chambers.Count()));
        return Map.Chambers[random_index];
    }


    //private static void collect_everything(GameObject collectionTarget)
    //{
    //    Weapon[] weapons = FindObjectsOfType<Weapon>();
    //    float telly_time;
    //    foreach (Weapon weapon in weapons)
    //    {
    //        if (!weapon.Wielder && !weapon.ImpaledObject && weapon != Player.INSTANCE.HostWeapon)
    //        {
    //            telly_time = Mathf.Pow((collectionTarget.transform.position - weapon.gameObject.transform.position).magnitude, 0.75f);
    //            weapon.Telecommute(collectionTarget, telly_time, (x) => Destroy(x.gameObject));
    //        }
    //    }
    //    Bone[] bones = FindObjectsOfType<Bone>();
    //    foreach (Bone bone in bones)
    //    {
    //        telly_time = Mathf.Pow((collectionTarget.transform.position - bone.gameObject.transform.position).magnitude, 0.75f);
    //        bone.Collect(collectionTarget, telly_time, (x) => Destroy(x.gameObject));
    //    }
    //}



}
