using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assassin : AIBehaviour
{
    //public float excitement = 0f;
    private float CombatSpeed = 0.0f;
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
        tangoStrafePauseFreq = 0.0f; 
        tangoStrafeEnabled = true;
        itemManagementSeekItems = true;
        itemManagementNoDoubles = true;
        itemManagementGreedy = true;
        itemManagementPreferredType = Entity.WieldMode.OneHanders;

    }

    protected override void Update()
    {
        base.Update();
    }


    /***** PUBLIC *****/



    /***** PROTECTED *****/
    protected override void queueNextRoundOfActions(Weapon weapon)
    {
        if (weapon != mainWep)
        {
            return;
        }
        else if (!entity.Foe || !_MartialController.Weapon_Queues.ContainsKey(mainWep))
        {
            _MartialController.Override_Action(mainWep, Weapon.ActionAnim.Idle);
            _MartialController.Override_Action(offWep, Weapon.ActionAnim.Idle);
        }
        else if (Random.value <= Aggression)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, CombatSpeed, timeoutCheckMyWeaponInRange);
            _MartialController.Queue_Action(offWep, Weapon.ActionAnim.QuickCoil, CombatSpeed, timeoutCheckMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
            _MartialController.Queue_Action(offWep, Weapon.ActionAnim.QuickAttack);
        }
        else
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Guarding, getPausePeriod(min: 1.5f));
            _MartialController.Queue_Action(offWep, Weapon.ActionAnim.Guarding, getPausePeriod(min: 1.5f));
        }
    }

    protected override void reactToFoeVulnerable()
    {
        if (checkMyWeaponInRange())
        {
            _MartialController.Override_Action(mainWep, Weapon.ActionAnim.QuickCoil, CombatSpeed); 
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.QuickAttack);
        }
        else if (Random.value < Aggression)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, CombatSpeed, timeoutCheckMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, CombatSpeed, timeoutCheckMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
        }
    }

    protected override void reactToFoeChange()
    {
        if (entity.Foe)
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Idle, CombatSpeed);
        }
        else
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Idle);
        }
    }

    protected override void reactToIncomingAttack()
    {
        if (_MartialController.Get_Next_Action(mainWep) == Weapon.ActionAnim.Guarding)
        {
            return;
        }
        else if (mainWep.Action == Weapon.ActionAnim.Idle || mainWep.Action == Weapon.ActionAnim.Recoiling)
        {
            _MartialController.Override_Action(mainWep, mainWep.Action, CombatSpeed);
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Guarding, getPausePeriod(min: 1.5f));
        }
    }

    protected override void reactToFoeThrowing()
    {
        if (!checkMyWeaponInRange() && Random.value >= Aggression)
        {
            _MartialController.Override_Action(mainWep, mainWep.Action, CombatSpeed);
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Guarding, CombatSpeed, () => !martialFoeThrowingLatch);
        }
    }

    protected override void reactToIncomingDash()
    {
        if (dashingCooldownTimer > 0.5f)
        {
            dashingChargePeriod = 0;
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            float randomLeftRightOffset = Mathf.Sign(Random.value - 0.5f) * 135;
            dashingDesiredDirection = angleToVector(getAngle(disposition.normalized) + randomLeftRightOffset);
        }
    }


    /***** PRIVATE *****/
    private bool timeoutCheckMyWeaponInRange()
    {
        if (!mainWep)
        {
            return false;
        }
        else if (checkMyWeaponInRange())
        {
            return true;
        }
        else if (_MartialController.Debounce_Timers.ContainsKey(mainWep))

        {
            return _MartialController.Debounce_Timers[mainWep] > CombatSpeed * 4;
        }
        else
        {
            return false;
        }
    }


}

