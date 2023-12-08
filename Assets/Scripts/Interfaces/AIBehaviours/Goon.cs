using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goon : AIBehaviour
{
    //public float excitement = 0f;

    private Weapon mainWep;
    private float CombatSpeed = 0.75f;
    private float Aggression = 0.5f;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        new GameObject().AddComponent<Handaxe>().PickupItem(entity);
        Intelligence = 0.75f;
        tangoStrafeEnabled = true;
        tangoStrafePauseFreq = 0.75f;
        martialPreferredState = martialState.attacking;
        sensorySightRangeScalar = 1.0f;
        meanderPauseFrequency = 0.5f;
        itemManagementSeekItems = true;
        martialFoeVulnerable.AddListener(reactToFoeVulnerable);
        martialFoeAttacking.AddListener(reactToIncomingAttack);
        sensoryFoeSpotted.AddListener(reactToFoeChange);
        sensoryFoeLost.AddListener(reactToFoeChange);
        _MartialController.INSTANCE.ClearedQueue.AddListener(queueNextRoundOfActions);
    }

    protected override void Update()
    {
        base.Update();
        if (!mainWep)
        {
            mainWep = entity.MainHand ? entity.MainHand.GetComponent<Weapon>() : null;
        }

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
                    if (mainWep)
                    {
                        martialCurrentState = mainWep.CurrentAction == Weapon.ActionAnimation.Guarding ? martialState.defending : (mainWep.CurrentAction == Weapon.ActionAnimation.Aiming ? martialState.throwing : (mainWep.CurrentAction == Weapon.ActionAnimation.Idle ? martialState.none : martialState.attacking));
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

    /***** PUBLIC *****/



    /***** PROTECTED *****/
    protected void queueNextRoundOfActions(Weapon weapon)
    {
        if (weapon != mainWep)
        {
            return;
        }
        else if (!entity.Foe || !_MartialController.Weapon_Queues.ContainsKey(mainWep))
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Idle);
        }
        else if (entity.Posture == Entity.PostureStrength.Weak)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.Guarding, getPausePeriod());
        }
        else if (Random.value <= Aggression)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.Idle, CombatSpeed);       
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickCoil, CombatSpeed, checkMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickAttack);
        }
        else
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.Idle, CombatSpeed, checkMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickCoil, CombatSpeed);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickAttack);
        }      
    }

    protected void reactToFoeVulnerable()
    {
        if (checkMyWeaponInRange())
        {
            _MartialController.Override_Action(mainWep, Weapon.ActionAnimation.QuickCoil, CombatSpeed);
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.QuickAttack);
        }
    }

    protected void reactToFoeChange()
    {
        if (entity.Foe)
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Idle, getPausePeriod());
        }
        else
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Idle);
        }
    }

    protected void reactToIncomingAttack()
    {
        if(_MartialController.Weapon_Queues[mainWep].Peek().Action == Weapon.ActionAnimation.Guarding) 
        {
            return;
        }
        else if (mainWep.CurrentAction == Weapon.ActionAnimation.QuickCoil && checkMyWeaponInRange())
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.Guarding, getPausePeriod());            
        }
        else
        {
            _MartialController.Override_Action(mainWep, mainWep.CurrentAction, CombatSpeed);
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Guarding, getPausePeriod());
        }
    }


    /***** PRIVATE *****/
    private bool checkMyWeaponInRange()
    {
        if (mainWep && entity.Foe)
        {
            float disposition = (entity.Foe.transform.position - transform.position).magnitude;
            return disposition <= mainWep.Range;
        }
        else
        {
            return false;
        }
    }

    private float getPausePeriod()
    {
        return 0.5f + Random.value;
    }

}

