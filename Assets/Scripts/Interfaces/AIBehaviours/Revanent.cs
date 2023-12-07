using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revanent : AIBehaviour
{

    private _Flames mainHandFlame;
    private _Flames offHandFlame;
    private _Flames footFlame;

    protected override void Awake()
    {
        base.Awake();

    }
    protected override void Start()
    {
        base.Start();
        mainHandFlame = Instantiate(Game.SpiritFlameTemplate).GetComponent<_Flames>();
        offHandFlame = Instantiate(Game.SpiritFlameTemplate).GetComponent<_Flames>();
        footFlame = Instantiate(Game.SpiritFlameTemplate).GetComponent<_Flames>();
        Intelligence = 1.0f;
        tangoPeriodScalar = 1f;
        tangoStrafePauseFreq = 0f;
        //martialReactiveAttack = true;
        martialPreferredState = martialState.none;
        itemManagementSeekItems = true;
        itemManagementGreedy = true;
        itemManagementPreferredType = Entity.WieldMode.TwoHanders;
        sensorySightRangeScalar = 1.25f;
        new GameObject().AddComponent<Greatsword>().PickupItem(entity);
        //new GameObject().AddComponent<Weapon_HandAxe>().PickupItem(entity);
        //new GameObject().AddComponent<Weapon_HandAxe>().PickupItem(entity);
    }

    protected override void Update()
    {
        base.Update();
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
                    ReflexRate = 0.20f;           
                }
                break;
            case AIState.aggro:
                if (trackingTrailCold)
                {
                    StateTransition(AIState.seek);
                }
                else if(entity.Foe)
                {
                    ReflexRate = 0.05f;
                    Weapon mainWep = entity.MainHand ? entity.MainHand.GetComponent<Weapon>() : null;
                    float disposition = (entity.Foe.transform.position - transform.position).magnitude;
                    bool inRange = mainWep ? mainWep.Range >= disposition : false;
                    if (mainWep)
                    {
                        if (!entity.Foe || !_MartialController.Weapon_Queues.ContainsKey(mainWep))
                        {
                            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Idle);
                        }
                        else if(entity.Posture == Entity.PostureStrength.Weak)
                        {
                            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Guarding);
                        }
                        else if (_MartialController.Weapon_Queues[mainWep].Count == 0)
                        {
                            if (inRange)
                            {
                                _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickAttack);
                                _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickAttack);
                                _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.StrongAttack);
                            }
                            else
                            {
                                _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.QuickCoil);
                            }
                        }

                        bool offensive = mainWep ? mainWep.CurrentActionAnimated == Weapon.ActionAnimation.QuickCoil : false;
                        tangoStrafeEnabled = !offensive;
                    }
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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (footFlame)
        {
            Destroy(footFlame.gameObject);
        } 
        if (mainHandFlame)
        {
            Destroy(mainHandFlame.gameObject);
        } 
        if (offHandFlame)
        {
            Destroy(offHandFlame.gameObject);
        }
    }

}

