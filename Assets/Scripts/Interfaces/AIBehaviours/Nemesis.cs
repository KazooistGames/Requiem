using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nemesis : AIBehaviour
{
    public float excitement = 0f;

    protected override void Awake()
    {
        base.Awake();
        //entity = GetComponent<Entity>() ? GetComponent<Entity>() : gameObject.AddComponent<Entity>();

    }
    protected override void Start()
    {
        base.Start();
        Intelligence = 1f;
        ReflexRate = 0.05f;
        tangoStrafeEnabled = false;
        dashingCooldownPeriod = 3.0f;
        dashingChargePeriod = 1.0f;
        dashingPower = 0.5f;
        grabDPS = 10f;
        sensorySightRangeScalar = 1.0f;
        itemManagementSeekItems = false;
        grabEnabled = false;
    }

    protected override void Update()
    {
        base.Update();
        if (Enthralled)
        {
            StateTransition(AIState.enthralled);
        }
        switch (State)
        {
            case AIState.none:
                StateTransition(AIState.seek);
                break;
            case AIState.seek:
                if (entity.Foe)
                {
                    StateTransition(AIState.aggro);
                }
                else
                {

                }
                break;
            case AIState.aggro:
                if (trackingTrailCold)
                {
                    StateTransition(AIState.seek);
                }
                else
                {
                    pursueStoppingDistance = sensoryBaseRange * sensorySightRangeScalar * 0.5f;
                    if (entity.Foe)
                    {
                        Vector3 disposition = entity.Foe.transform.position - transform.position;
                        dashingInitiate = (disposition.magnitude < pursueStoppingDistance || entity.DashCharging) && !entity.Shoved;
                        if (entity.DashCharging)
                        {
                            dashingCooldownPeriod = 1;
                        }
                        else if (entity.FinalDash)
                        {
                            dashingCooldownPeriod = 5;
                        }
                    }
                }
                break;

        }
    }

}

