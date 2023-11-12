using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biter : Character
{
    public float excitement = 0f;

    protected override void Awake()
    {
        base.Awake();
        entity = GetComponent<Warrior>() ? GetComponent<Warrior>() : gameObject.AddComponent<Warrior>();

    }
    protected override void Start()
    {
        base.Start();
        Intelligence = 0.5f;
        ReflexRate = 0.20f;
        tangoStrafeEnabled = false;
        dashingCooldownPeriod = 2.0f;
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
                        dashingInitiate = (disposition.magnitude < pursueStoppingDistance || entity.DashCharging) && !entity.Shoved;

                    }
                }
                break;

        }
    }

}

