using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biter : AIBehaviour
{
    public float excitement = 0f;

    protected override void Awake()
    {
        base.Awake();
        entity = GetComponent<Entity>() ? GetComponent<Entity>() : gameObject.AddComponent<Entity>();

    }
    protected override void Start()
    {
        base.Start();
        Intelligence = 0.5f;
        ReflexRate = 0.20f;
        tangoStrafeEnabled = false;
        dashingChargePeriod = 1.0f;
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
                    pursueStoppingDistance = sensoryBaseRange * sensorySightRangeScalar * 0.5f;
                    StateTransition(AIState.aggro);
                }
                else
                {
                    meanderPauseFrequency = 0.75f;
                }
                break;
            case AIState.aggro:
                if (trackingTrailCold)
                {
                    StateTransition(AIState.seek);
                }
                else
                {
                    if (entity.Foe)
                    {
                        Vector3 disposition = entity.Foe.transform.position - transform.position;
                        //dashingInitiate = (disposition.magnitude < pursueStoppingDistance || entity.DashCharging) && !entity.Shoved;

                    }
                }
                break;

        }
    }

}

