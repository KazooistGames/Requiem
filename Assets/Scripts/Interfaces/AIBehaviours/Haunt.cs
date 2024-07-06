using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Haunt : AIBehaviour
{
    public static Haunt INSTANCE;
    public Hextile TargetTile;


    protected override void Start()
    {
        base.Start();
        INSTANCE = this;
        Destroy(entity.indicator);
        State = AIState.custom;
        waypointCommanded = true;
        behaviourParams[BehaviourType.waypoint] = (true, 1.0f);
        behaviourParams[BehaviourType.wallCrawl] = (false, 0);
    }


    protected override void Update()
    {
        base.Update();

    }

   


}
