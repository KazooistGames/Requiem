using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biter : AI
{
    public float excitement = 0f;

    protected override void Awake()
    {
        base.Awake();
        entity = GetComponent<Character>() ? GetComponent<Character>() : gameObject.AddComponent<Character>();

    }
    protected override void Start()
    {
        base.Start();
        Intelligence = 0.5f;
        ReflexRate = 0.20f;
        tangoStrafeEnabled = false;
        dashingCooldownPeriod = 1.0f;
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
                        dashingInitiate = (disposition.magnitude < sensoryBaseRange * sensorySightRangeScalar * 0.25f || entity.DashCharging) && !entity.Shoved;
                    }
                }
                break;

        }
    }

}

