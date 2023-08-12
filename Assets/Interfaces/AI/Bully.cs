using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bully : AI
{
    public float excitement = 0f;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        new GameObject().AddComponent<Halberd>().PickupItem(entity);
        Intelligence = 0.5f;

        tangoStrafeEnabled = false;
        dashingChargePeriod = 0.5f;
        dashingPower = 1.0f;
        itemManagementSeekItems = true;
        itemManagementPreferredType = Entity.WieldMode.TwoHanders;
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
                    dashingCooldownTimer = 0.0f;
                    StateTransition(AIState.aggro);
                }
                else
                {
                    ReflexRate = 0.20f;
                    meanderPauseFrequency = 0.0f;
                }
                break;
            case AIState.aggro:
                if (trackingTrailCold)
                {
                    StateTransition(AIState.passive);
                }
                else
                {
                    ReflexRate = 0.05f;
                    Weapon mainWep = entity.MainHand ? entity.MainHand.GetComponent<Weapon>() : null;
                    bool inRange = entity.Foe && mainWep ? (mainWep.Range) >= (entity.Foe.transform.position - transform.position).magnitude : false;
                    //bool defending = mainWep ? mainWep.Defending : false;
                    //dashingInitiate = (defending && inRange) || entity.Mutated || entity.DashCharging;
                    dashingCooldownPeriod = entity.Mutated ? 1.0f : 2.0f;
                    martialPreferredState = entity.Mutated ? martialState.attacking : martialState.none;
                }
                break;

        }
    }

}

