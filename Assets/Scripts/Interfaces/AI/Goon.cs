using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goon : AI
{
    public float excitement = 0f;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        new GameObject().AddComponent<HandAxe>().PickupItem(entity);
        Intelligence = 0.75f;
        tangoStrafeEnabled = true;
        tangoStrafePauseFreq = 0.75f;
        martialReactiveAttack = false;
        martialReactiveDefend = true;
        martialReactiveDefendThrow = false;
        martialPreferredState = martialState.attacking;
        sensorySightRangeScalar = 1.0f;
        meanderPauseFrequency = 0.5f;
        itemManagementSeekItems = true;
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
                StateTransition(AIState.passive);
                break;
            case AIState.passive:
                if (entity.Foe)
                {
                    StateTransition(AIState.aggro);
                }
                break;
            case AIState.aggro:
                if (trackingTrailCold)
                {
                    StateTransition(AIState.seek);
                }
                else
                {
                    tangoStrafePauseFreq = 0.5f;
                    tangoStrafeEnabled = martialCurrentState == martialState.attacking;                 
                }
                break;
            case AIState.seek:
                if (entity.Foe)
                {
                    StateTransition(AIState.aggro);
                }
                else if (stateRunTimer > 5)
                {
                    StateTransition(AIState.passive);
                }
                break;
        }
    }

}

