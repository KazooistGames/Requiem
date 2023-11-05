using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sentinel : Character
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
        martialReactiveAttack = true;
        martialReactiveDefend = true;
        martialPreferredState = martialState.none;
        dashingCooldownPeriod = 1.0f;
        itemManagementSeekItems = true;
        dashingPower = 0.0f;
        itemManagementPreferredType = Warrior.WieldMode.OneHanders;
        new GameObject().AddComponent<Greataxe>().PickupItem(entity);
        //entity.modPosture["Sentinel" + GetHashCode().ToString()] = Entity.Posture.Strong;
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
                    //bool defensive = (wep ? wep.Defending : true);
                    //bool offensive = (wep ? wep.WindingUp: false);
                    //dashingDodgeFoe = defensive;
                    //dashingDodgeAim = offensive;
                    //dashingInitiate = !offensive && !defensive;
                    dashingLunge = true;
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

