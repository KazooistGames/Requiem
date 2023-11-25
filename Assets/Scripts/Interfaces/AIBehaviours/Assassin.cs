using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assassin : AIBehaviour
{

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        Intelligence = 1.0f;
        grabEnabled = false;
        tangoStrafePauseFreq = 0;
        tangoPeriodScalar = 2;
        tangoStrafeEnabled = true;
        martialPreferredState = martialState.attacking;
        dashingChargePeriod = 0.1f;
        itemManagementGreedy = true;
        itemManagementNoDoubles = true;
    }

    protected override void Update()
    {
        base.Update();
        switch (State)
        {
            case AIState.none:
                waypointCoordinates = transform.position;
                StateTransition(AIState.seek);
                break;
            case AIState.seek:
                if (entity.Foe)
                {
                    StateTransition(AIState.aggro);
                }
                else
                {
                    meanderPauseFrequency = 0f;
                    entity.modSpeed["AIState"] = sensoryAlerted ? 0 : -0.75f;
                }
                break;
            case AIState.aggro:

                if (!entity.Foe)
                {
                    StateTransition(AIState.seek);
                }
                else
                {
                    ReflexRate = 0.05f;
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

