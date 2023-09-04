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
        itemManagementPreferredType = Character.WieldMode.TwoHanders;
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
                    Weapon mainWep = entity.MainHand ? entity.MainHand.GetComponent<Weapon>() : null;
                    if (mainWep)
                    {
                        if (!entity.Foe || !_MartialController.Action_Queues.ContainsKey(mainWep))
                        {
                            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Idle);
                        }
                        else if (_MartialController.Action_Queues[mainWep].Count == 0)
                        {
                            float disposition = (entity.Foe.transform.position - transform.position).magnitude;
                            if (mainWep.Range >= disposition)
                            {
                                _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickAttack);
                                _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.Guarding, 2);
                                _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.StrongAttack);
                            }
                            else
                            {
                                _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickCoil);
                            }
                        }
                    }
                    ReflexRate = 0.05f;
                    //bool defending = mainWep ? mainWep.Defending : false;
                    //dashingInitiate = (defending && inRange) || entity.Mutated || entity.DashCharging;
                    //dashingCooldownPeriod = entity.Mutated ? 1.0f : 2.0f;
                    //martialPreferredState = entity.Mutated ? martialState.attacking : martialState.none;
                }
                break;

        }
    }

}

