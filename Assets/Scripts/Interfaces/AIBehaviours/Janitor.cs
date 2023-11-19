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
    }


    protected override void Update()
    {
        base.Update();
        switch (State)
        {
            case AIState.none:
                waypointCoordinates = transform.position;
                if(TaskList.Count > 0)
                {
                    NextTask = Hextile.Tiles[0];
                    StateTransition(AIState.passive);
                }
                else
                {
                    TaskList = Hextile.Tiles;
                }
                break;
            case AIState.passive:
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
                break;
            default:
                StateTransition(AIState.none);
                break;
        }
    }


    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        Wieldable item = other.GetComponent<Wieldable>();
        if(item ? item.GetComponent<Weapon>() || (item.GetType() == typeof(Bone) && item != Idol.INSTANCE): false)
        {
            Vector3 playerDispo = item.transform.position - Player.INSTANCE.transform.position;
            bool inUse = item.Wielder || item.MountTarget || item.ImpalingSomething || item.Telecommuting || item.Thrown;
            if (playerDispo.magnitude >= Hextile.Radius && !inUse)
            {
                item.Telecommute(gameObject, 3.0f, (x) => Destroy(x.gameObject));
            }
        }
    }



}
