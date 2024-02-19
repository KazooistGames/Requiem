using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Janitor : AIBehaviour
{
    public static Janitor INSTANCE;
    public Hextile NextTask;

    public bool ContinualCollectionEnabled = false;

    protected SphereCollider CleanZone;

    protected override void Start()
    {
        base.Start();
        INSTANCE = this;
        Destroy(entity.indicator);
        CleanZone = GetComponent<SphereCollider>() ? GetComponent<SphereCollider>() : gameObject.AddComponent<SphereCollider>();
        CleanZone.isTrigger = true;
        CleanZone.radius = Hextile.Radius / Entity.Scale;
        waypointCoordinates = transform.position;
        StartCoroutine(tipBlurpRoutine());
    }


    protected override void Update()
    {
        base.Update();
        behaviourParams[BehaviourType.wallCrawl] = (false, 0);
        behaviourParams[BehaviourType.sensory] = (false, 0f);
        behaviourParams[BehaviourType.waypoint] = (true, 1);
        behaviourParams[BehaviourType.meander] = (true, 0.5f);
        waypointCommanded = true;
        waypointDeadbandingScalar = 3.0f;
        meanderPauseFrequency = 0.0f;
        meanderPeriod = 0.5f;  
        if (waypointDeadbanded)
        {                       
            int currentTaskIndex = Hextile.Tiles.IndexOf(NextTask);
            NextTask = currentTaskIndex >= Hextile.Tiles.Count - 1 ? Hextile.Tiles[0] : Hextile.Tiles[currentTaskIndex + 1];
            waypointCoordinates = NextTask.transform.position;
        }
        else if(NextTask)
        {
            Debug.DrawLine(transform.position, NextTask.transform.position, Color.red);
        }     
    }


    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (!ContinualCollectionEnabled)
        {

        }
        else if (playerIsTooCloseToObj(other.gameObject))
        {

        }
        else if (other.GetComponent<Weapon>())
        {
            Weapon weapon = other.GetComponent<Weapon>();
            bool inUse = weapon.Wielder || weapon.MountTarget || weapon.ImpaledObject || weapon.Telecommuting || weapon.Thrown || weapon == Player.INSTANCE.HostWeapon;
            if (!inUse)
            {
                weapon.Telecommute(gameObject, 3.0f, (x) => Destroy(x.gameObject));
            }
        }
        else if (!other.GetComponent<Bone>())
        {

        }
        else if (other.GetComponent<Bone>().Body.velocity.magnitude == 0)
        {
            other.GetComponent<Bone>().Collect(gameObject, 3.0f, (x) => Destroy(x.gameObject));
        }
    }

    /***** PUBLIC *****/
    public void CollectEverything()
    {
        Weapon[] weapons = FindObjectsOfType<Weapon>();
        foreach(Weapon weapon in weapons)
        {
            if (!weapon.Wielder && !weapon.ImpaledObject && weapon != Player.INSTANCE.HostWeapon)
            {
                weapon.Telecommute(gameObject, determineTelecommuteCollectionSeconds(weapon.gameObject), (x) => Destroy(x.gameObject));
            }
        }
        Bone[] bones = FindObjectsOfType<Bone>();
        foreach(Bone bone in bones)
        {
            bone.Collect(gameObject, determineTelecommuteCollectionSeconds(bone.gameObject), (x) => Destroy(x.gameObject));     
        }
    }

    /***** PRIVATE *****/
    private float determineTelecommuteCollectionSeconds(GameObject obj)
    {
        float distance = (transform.position - obj.transform.position).magnitude;
        return Mathf.Pow(distance, 0.75f);
    }
    private bool playerIsTooCloseToObj(GameObject obj)
    {
        if (!Player.INSTANCE)
        {
            return false;
        }
        else
        {
            return (Player.INSTANCE.transform.position - obj.transform.position).magnitude <= Hextile.Radius / 2;
        }
    }

    private List<string> tips = new List<string>()
    {
        "You can heal between rituals\nat the bloodwell",
        "You can bleed your foes\nby dashing with your quick-attacks",
        "Time your blocks\nto parry foes",
        "Time your strong-attacks well\nand see them strike true",
    };

    private IEnumerator tipBlurpRoutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => Player.INSTANCE != null);
            yield return new WaitUntil(() => (transform.position - Player.INSTANCE.transform.position).magnitude <= Hextile.Radius/2);
            GameObject currentBlurb = produceTipBlurb();
            yield return new WaitWhile(() => currentBlurb != null);
            yield return new WaitForSeconds(5);
        }
    }

    private GameObject produceTipBlurb()
    {
        string tipString;
        if(Player.INSTANCE.HostEntity.Vitality < 50)
        {
            tipString = "Heal at the bloodwell!";
        }
        else
        {
            tipString = tips[Random.Range(0, tips.Count)];
        }
        float duration = Mathf.CeilToInt(1 + tipString.Length * 0.1f);
        return _BlurbService.createBlurb(gameObject, tipString, new Color(1.0f, 0.75f, 0, 0.5f), duration, 0.8f);    
    }

}
