using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bully : AIBehaviour
{
    private float CombatSpeed = 0.5f;
    private float Aggression = 0.75f;
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
        meanderPauseFrequency = 0;
        itemManagementSeekItems = true;
        itemManagementPreferredType = Entity.WieldMode.TwoHanders;
        itemManagementNoSingles = true;
        martialFoeVulnerable.AddListener(reactToFoeVulnerable);
        martialFoeAttacking.AddListener(reactToIncomingAttack);
        sensoryFoeSpotted.AddListener(reactToFoeChange);
        sensoryFoeLost.AddListener(reactToFoeChange);
        entity.JustMadeWeak.AddListener(goDefensive);
        _MartialController.INSTANCE.ClearedQueue.AddListener(queueNextRoundOfActions);
    }

    protected override void Update()
    {
        base.Update();
        float period = entity.Posture == Entity.PostureStrength.Weak ? 1 : 2;

        if (dashingCooldownTimer < period || !entity.Foe)
        {

        }
        else if (!mainWep)
        {

        }
        else if (entity.Posture == Entity.PostureStrength.Weak || mainWep.Action == Weapon.ActionAnim.StrongCoil || mainWep.Action == Weapon.ActionAnim.Guarding || dashingDesiredDirection != Vector3.zero)
        {
            dashingChargePeriod = 1f;
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
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Idle);
        }
        else if(entity.Posture == Entity.PostureStrength.Weak)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack, requisite: () => entity.Posture != Entity.PostureStrength.Weak);
        }
        else if(checkMyWeaponInRange())
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, 0);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle);
        }
        else if(Random.value <= Aggression)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.StrongCoil, CombatSpeed, timeoutCheckMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.StrongAttack);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, CombatSpeed);
        }
        else
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, 0, timeoutCheckMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle);
        }
    }

    protected override void reactToFoeVulnerable()
    {
        if (checkMyWeaponInRange())
        {
            _MartialController.Override_Action(mainWep, Weapon.ActionAnim.QuickCoil, 0);
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.QuickAttack);
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
    private void goDefensive()
    {
        _MartialController.Override_Action(mainWep, Weapon.ActionAnim.Guarding, getPausePeriod(min: 0.5f, randScalar: 0.5f));      
    }

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

    protected override void SetTangoParameters()
    {
        if (mainWep || offWep)
        {
            Weapon wep = mainWep ? mainWep : offWep;
            switch (martialCurrentState)
            {
                case martialState.none:
                    tangoInnerRange = entity.personalBox.radius * entity.scaleActual;
                    tangoOuterRange = sensoryBaseRange * sensorySightRangeScalar * 0.5f;
                    break;
                case martialState.attacking:
                    tangoInnerRange = wep.Range * 0.5f;
                    tangoOuterRange = wep.Range * 0.75f;
                    break;
                case martialState.defending:
                    tangoInnerRange = wep.Range * 1.0f;
                    tangoOuterRange = wep.Range * 1.5f;
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

