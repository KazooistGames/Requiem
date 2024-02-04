using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assassin : AIBehaviour
{
    //public float excitement = 0f;
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
        dashingChargePeriod = 0;
        queueNextRoundOfActions(mainWep);
    }

    protected override void Update()
    {
        base.Update();
    }


    /***** PUBLIC *****/



    /***** PROTECTED *****/
    protected override void queueNextRoundOfActions(Weapon weapon)
    {
        if (weapon != mainWep || mainWep == null)
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
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, 0, timeoutCheckMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
            _MartialController.Queue_Action(offWep, Weapon.ActionAnim.Idle, 0, timeoutCheckMyWeaponInRange);
            _MartialController.Queue_Action(offWep, Weapon.ActionAnim.QuickCoil);
            _MartialController.Queue_Action(offWep, Weapon.ActionAnim.QuickAttack);
        }
        else if(martialFoeEnteredRangeLatch || Random.value > 0.25f)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Guarding, getPausePeriod(min: 1.0f));
            _MartialController.Queue_Action(offWep, Weapon.ActionAnim.Guarding, getPausePeriod(min: 1.0f));
        }
        else
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Aiming, 0.5f);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Throwing);
        }
    }

    protected override void reactToFoeVulnerable()
    {
        if (checkMyWeaponInRange())
        {
            _MartialController.Override_Action(mainWep, Weapon.ActionAnim.QuickCoil); 
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.QuickAttack);            
            _MartialController.Override_Action(offWep, Weapon.ActionAnim.QuickCoil); 
            _MartialController.Override_Queue(offWep, Weapon.ActionAnim.QuickAttack);
        }
        else if (Random.value < Aggression)
        {
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            dashingDesiredDirection = angleToVector(getAngle(disposition.normalized));
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, 0, timeoutCheckMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
            _MartialController.Queue_Action(offWep, Weapon.ActionAnim.QuickCoil, 0, timeoutCheckMyWeaponInRange);
            _MartialController.Queue_Action(offWep, Weapon.ActionAnim.QuickAttack);
        }
    }

    protected override void reactToFoeChange()
    {
        if (entity.Foe)
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Idle);
        }
        else
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Idle);
        }
    }

    protected override void reactToIncomingAttack()
    {
        if(dashingCooldownTimer < 0.5f)
        {

        }
        else
        {
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            float randomOffset = Mathf.Sign(Random.value - 0.5f) * 30;
            dashingDesiredDirection = -angleToVector(getAngle(disposition.normalized) + randomOffset);      
        }
    }

    protected override void reactToFoeThrowing()
    {
        if (_MartialController.Weapon_Actions.ContainsKey(mainWep) ? _MartialController.Weapon_Actions[mainWep].Action == Weapon.ActionAnim.Guarding : true)
        {
            return;
        }
        else if (dashingCooldownTimer < 0.5f)
        {
            dashingChargePeriod = 0;
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            float randomLeftRightOffset = Mathf.Sign(Random.value - 0.5f) * 90;
            dashingDesiredDirection = angleToVector(getAngle(disposition.normalized) + randomLeftRightOffset);
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
    protected override void SetTangoParameters()
    {
        if (mainWep || offWep)
        {
            Weapon wep = mainWep ? mainWep : offWep;
            switch (martialCurrentState)
            {
                case martialState.none:
                    tangoInnerRange = wep.Range * 1.0f;
                    tangoOuterRange = sensoryBaseRange * sensorySightRangeScalar * 0.5f;
                    break;
                case martialState.attacking:
                    tangoInnerRange = wep.Range * 0.75f;
                    tangoOuterRange = wep.Range * 1f;
                    break;
                case martialState.defending:
                    tangoInnerRange = wep.Range * 1.5f;
                    tangoOuterRange = wep.Range * 2f;
                    break;
                case martialState.throwing:
                    tangoInnerRange = wep.Range * 2.0f;
                    tangoOuterRange = sensoryBaseRange * sensorySightRangeScalar * 0.5f;
                    break;
            }
            pursueStoppingDistance = tangoOuterRange;
        }
        else
        {
            tangoInnerRange = entity.personalBox.radius * entity.scaleActual;
            tangoOuterRange = sensorySightRangeScalar * sensoryBaseRange;
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
            return _MartialController.Debounce_Timers[mainWep] > 2;
        }
        else
        {
            return false;
        }
    }
}

