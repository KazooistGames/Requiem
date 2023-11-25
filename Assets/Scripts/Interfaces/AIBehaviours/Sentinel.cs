using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sentinel : AIBehaviour
{
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        Intelligence = 1.0f;
        ReflexRate = 0.05f;
        tangoStrafeEnabled = false;
        tangoStrafePauseFreq = 0;
        martialPreferredState = martialState.none;
        itemManagementSeekItems = true;
        itemManagementPreferredType = Entity.WieldMode.OneHanders;
        new GameObject().AddComponent<Greataxe>().PickupItem(entity);
    }

    protected override void Update()
    {
        base.Update();
        switch (State)
        {
            case AIState.none:
                StateTransition(AIState.guard);
                break;
            case AIState.guard:
                if (entity.Foe ? (waypointCoordinates - entity.Foe.transform.position).magnitude < waypointOuterLimit : false)
                {
                    StateTransition(AIState.aggro);
                }
                else
                {
                    ReflexRate = 0.20f;
                    if (!entity.Foe)
                    {
                        waypointCoordinates = transform.position;
                    }
                }
                break;
            case AIState.aggro:
                if (!entity.Foe)
                {
                    StateTransition(AIState.guard);
                }
                else
                {
                    ReflexRate = 0.05f;
                    Weapon wep = entity.MainHand ? entity.MainHand.GetComponent<Weapon>() : null;
                    bool inRange = entity.Foe && wep ? (wep.Range) >= (entity.Foe.transform.position - transform.position).magnitude : false;
                }
                break;
        }
    }
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }


}

