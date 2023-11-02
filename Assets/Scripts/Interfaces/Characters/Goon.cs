using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goon : Character
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
                    Weapon mainWep = entity.MainHand ? entity.MainHand.GetComponent<Weapon>() : null;
                    if (mainWep)
                    {
                        martialCurrentState = mainWep.ActionAnimated == Weapon.ActionAnimation.Guarding ? martialState.defending : martialState.attacking;
                        if (!entity.Foe || !_MartialController.Action_Queues.ContainsKey(mainWep))
                        {
                            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Idle);
                        }
                        else if (_MartialController.Action_Queues[mainWep].Count == 0)
                        {
                            float disposition = (entity.Foe.transform.position - transform.position).magnitude;
                            if (mainWep.Range >= disposition)
                            {
                                _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickCoil, 0.5f);
                                _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickAttack);
                                if (Random.value >= 0.5f)
                                {
                                    _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickCoil, 0.5f);
                                    _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickAttack);
                                }
                                else
                                {
                                    _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.Guarding, 2);
                                }
                            }
                            else
                            {
                                _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.Idle);
                            }
                        }
                    }
                    ReflexRate = 0.05f;
                    tangoStrafePauseFreq = 0.5f;
                    tangoStrafeEnabled = true;           
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

