using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Janitor : AIBehaviour
{
    public static Janitor INSTANCE;
    public List<Hextile> TaskList = new List<Hextile>();
    public Hextile NextTask;

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
    }


    protected override void Update()
    {
        base.Update();
        if(TaskList.Count > 0)
        {
            NextTask = Hextile.Tiles[0];
            StateTransition(AIState.passive);
        }
        else
        {
            TaskList = Hextile.Tiles;
        }
        behaviourParams[BehaviourType.wallCrawl] = (false, 0);
        behaviourParams[BehaviourType.sensory] = (false, 0f);
        waypointCommanded = true;
        waypointDeadbandingScalar = 3.0f;
        meanderPauseFrequency = 0.0f;
        meanderPeriod = 0.5f;
        behaviourParams[BehaviourType.waypoint] = (true, 1);
        behaviourParams[BehaviourType.meander] = (true, 0.5f);           
        if (waypointDeadbanded)
        {                       
            int currentTaskIndex = TaskList.IndexOf(NextTask);
            NextTask = currentTaskIndex >= TaskList.Count - 1 ? TaskList[0] : TaskList[currentTaskIndex + 1];
            waypointCoordinates = NextTask.transform.position;
        }
        entity.modSpeed["mosey"] = -0.5f;      
    }


    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (playerIsTooCloseToObj(other.gameObject))
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
        else if (other.GetComponent<Bone>())
        {
            other.GetComponent<Bone>().Collect(gameObject, 3.0f, (x) => Destroy(x.gameObject));
        }
    }

    /***** PRIVATE *****/
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

}
