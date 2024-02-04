using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sentinel : AIBehaviour
{
    //public float excitement = 0f;
    private float CombatSpeed = 0.25f;
    private float Aggression = 0.5f;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        new GameObject().AddComponent<Spear>().PickupItem(entity);
        Intelligence = 0.75f;
        tangoStrafeEnabled = true;
        tangoStrafePauseFreq = 0.75f;
        martialPreferredState = martialState.attacking;
        sensorySightRangeScalar = 1.0f;
        meanderPauseFrequency = 0.5f;
        tangoStrafePauseFreq = 0.5f;
        tangoStrafeEnabled = true;
        itemManagementSeekItems = true;
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
        }
        else if (Random.value <= Aggression)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, 0, checkMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, CombatSpeed);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);           
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, 0);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);        
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, 0);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
        }
        else if (entity.Posture == Entity.PostureStrength.Weak)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Guarding, getPausePeriod(min: 1.5f));
        }
        else
        {
            dashingChargePeriod = 0.0f;
            dashingDesiredDirection = -(entity.Foe.transform.position - transform.position);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, CombatSpeed, checkMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
        }
    }

    protected override void reactToFoeVulnerable()
    {
        _MartialController.Override_Action(mainWep, Weapon.ActionAnim.QuickCoil, CombatSpeed, checkMyWeaponInRange);
        //_MartialController.Override_Queue(mainWep, Weapon.ActionAnim.QuickAttack);
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
            _MartialController.Override_Action(mainWep, Weapon.ActionAnim.Guarding, getPausePeriod(min: 1.5f));
        }
        else if(dashingCooldownTimer > 3)
        {
            dashingChargePeriod = 0;
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            float randomLeftRightOffset = Mathf.Sign(Random.value - 0.5f) * 90;
            dashingDesiredDirection = angleToVector(getAngle(disposition.normalized) + randomLeftRightOffset);
        }
    }

    protected override void reactToFoeThrowing()
    {
        if (!checkMyWeaponInRange())
        {
            _MartialController.Override_Action(mainWep, Weapon.ActionAnim.Guarding, CombatSpeed, () => !martialFoeThrowingLatch);
        }
        else
        {
            reactToIncomingAttack();
        }
    }


    /***** PRIVATE *****/

}

