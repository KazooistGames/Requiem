using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assassin : AIBehaviour
{
    private static float DASH_COOLDOWN = 1;
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        new GameObject().AddComponent<Handaxe>().PickupItem(entity);        
        new GameObject().AddComponent<Handaxe>().PickupItem(entity);
        Intelligence = 1f;
        tangoStrafeEnabled = true;
        tangoStrafePauseFreq = 0.75f;
        martialPreferredState = martialState.attacking;
        sensorySightRangeScalar = 1f;
        sensoryAudioRangeScalar = 1f;
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
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Idle);
            _MartialController.Override_Queue(offWep, Weapon.ActionAnim.Idle);
        }
        else if(entity.Posture == Entity.PostureStrength.Weak)
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Guarding, 2);
            _MartialController.Override_Queue(offWep, Weapon.ActionAnim.Guarding, 2);
        }
        else if (checkMyWeaponInRange())
        {

            _MartialController.Override_Action(mainWep, Weapon.ActionAnim.QuickCoil);
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.QuickAttack);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, 0.5f);

            _MartialController.Override_Action(offWep, Weapon.ActionAnim.Idle, 0.5f);
            _MartialController.Override_Queue(offWep, Weapon.ActionAnim.QuickCoil);
            _MartialController.Queue_Action(offWep, Weapon.ActionAnim.QuickAttack);
        }
        else 
        {
            dashingDesiredDirection = entity.Foe.transform.position - transform.position;
            dashingEvaluator = () => { return entity.Foe ? entity.Foe.transform.position - transform.position : Vector3.zero; };
            dashingChargePeriod = 1f;
            _MartialController.Override_Action(mainWep, Weapon.ActionAnim.Idle, 0.5f);
            _MartialController.Override_Action(offWep, Weapon.ActionAnim.Idle, 0.5f);
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.QuickCoil, 0, checkMyWeaponInRange, 1);
            _MartialController.Override_Queue(offWep, Weapon.ActionAnim.QuickCoil, 0, checkMyWeaponInRange, 1);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
            _MartialController.Queue_Action(offWep, Weapon.ActionAnim.QuickAttack);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, 0.5f);
            _MartialController.Queue_Action(offWep, Weapon.ActionAnim.Idle, 0.5f);
        }
    }

    protected override void reactToFoeVulnerable()
    {
        if(_MartialController.Weapon_Actions.ContainsKey(mainWep) ? _MartialController.Weapon_Actions[mainWep].Action == Weapon.ActionAnim.Guarding : true)
        {
            _MartialController.Override_Action(mainWep, Weapon.ActionAnim.QuickCoil, 0, checkMyWeaponInRange, 2);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
            _MartialController.Override_Action(offWep, Weapon.ActionAnim.Idle, 0, checkMyWeaponInRange, 2);
            _MartialController.Queue_Action(offWep, Weapon.ActionAnim.QuickCoil);
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
        if(dashingCooldownTimer >= DASH_COOLDOWN && !entity.DashCharging)
        {
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            float randomOffset = Mathf.Sign(Random.value - 0.5f) * 30;
            dashingDesiredDirection = -angleToVector(getAngle(disposition.normalized) + randomOffset);
            dashingChargePeriod = 0;
        }
        else
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Guarding, 1);
            _MartialController.Override_Queue(offWep, Weapon.ActionAnim.Guarding, 1);
        }
    }

    protected override void reactToFoeThrowing()
    {
        if (_MartialController.Weapon_Actions.ContainsKey(mainWep) ? _MartialController.Weapon_Actions[mainWep].Action == Weapon.ActionAnim.Guarding : true)
        {
            return;
        }
        else if (dashingCooldownTimer >= DASH_COOLDOWN && !entity.DashCharging)
        {
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            float randomLeftRightOffset = Mathf.Sign(Random.value - 0.5f) * 90;
            dashingDesiredDirection = angleToVector(getAngle(disposition.normalized) + randomLeftRightOffset);
            dashingChargePeriod = 0;
        }
        else
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Guarding, 2);
            _MartialController.Override_Queue(offWep, Weapon.ActionAnim.Guarding, 2);
        }
    }

    //protected override void reactToIncomingDash()
    //{
    //    if (dashingCooldownTimer > 0.5f)
    //    {
    //        Vector3 disposition = entity.Foe.transform.position - transform.position;
    //        float randomLeftRightOffset = Mathf.Sign(Random.value - 0.5f) * 135;
    //        dashingDesiredDirection = angleToVector(getAngle(disposition.normalized) + randomLeftRightOffset);
    //        dashingChargePeriod = 0;
    //    }
    //}

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

}

