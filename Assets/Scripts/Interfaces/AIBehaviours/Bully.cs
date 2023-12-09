using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bully : AIBehaviour
{
    private float CombatSpeed = 0.5f;
    private float Aggression = 0.5f;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        new GameObject().AddComponent<Greataxe>().PickupItem(entity);
        Intelligence = 0.5f;
        tangoStrafeEnabled = false;
        dashingChargePeriod = 1f;
        meanderPauseFrequency = 0;
        itemManagementSeekItems = true;
        itemManagementPreferredType = Entity.WieldMode.TwoHanders;
        martialFoeVulnerable.AddListener(reactToFoeVulnerable);
        martialFoeAttacking.AddListener(reactToIncomingAttack);
        sensoryFoeSpotted.AddListener(reactToFoeChange);
        sensoryFoeLost.AddListener(reactToFoeChange);
        _MartialController.INSTANCE.ClearedQueue.AddListener(queueNextRoundOfActions);
    }

    protected override void Update()
    {
        base.Update();
        float period = entity.Posture == Entity.PostureStrength.Weak ? 1 : 3;
        if (dashingCooldownTimer > period)
        {
            dashingDesiredDirection = entity.Foe.transform.position - transform.position;
        }
    }

    /***** PUBLIC *****/



    /***** PROTECTED  *****/
    protected override void queueNextRoundOfActions(Weapon weapon)
    {
        if (weapon != mainWep)
        {
            return;
        }
        else if (!entity.Foe || !_MartialController.Weapon_Queues.ContainsKey(mainWep))
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Idle);
        }
        else if(entity.Posture == Entity.PostureStrength.Weak)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickCoil, CombatSpeed, checkMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickAttack);
        }
        else if(checkMyWeaponInRange())
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickCoil, CombatSpeed);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickAttack);
        }
        else
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.StrongCoil, CombatSpeed, checkMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.StrongAttack);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.Idle, CombatSpeed);
        }
    }

    protected override void reactToFoeVulnerable()
    {
        if (checkMyWeaponInRange())
        {
            _MartialController.Override_Action(mainWep, Weapon.ActionAnimation.QuickCoil, CombatSpeed);
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.QuickAttack);
        }
    }

    protected override void reactToFoeChange()
    {
        if (entity.Foe)
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Idle, CombatSpeed);
        }
        else
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Idle);
        }
    }

    protected override void reactToIncomingAttack()
    {
        if (_MartialController.Get_Next_Action(mainWep) == Weapon.ActionAnimation.Guarding || entity.Posture != Entity.PostureStrength.Weak)
        {
            return;
        }
        else if (mainWep.Action == Weapon.ActionAnimation.QuickCoil && checkMyWeaponInRange())
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.Guarding, getPausePeriod());
        }
        else
        {
            _MartialController.Override_Action(mainWep, mainWep.Action, CombatSpeed);
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Guarding, getPausePeriod());
        }
    }


    /***** PRIVATE *****/



}

