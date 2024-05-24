using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Goon : AIBehaviour
{
    //public float excitement = 0f;
    private float CombatSpeed = 0.5f;
    private float Aggression = 0.5f;
    private float Fear = 0.5f;

    public static Type Standard_Weapon = typeof(Handaxe);
    public static Type Alternative_Weapon = null;
    public static float Alternative_Weapon_Frequency = 0.25f;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        createSpawnWeapon();
        Intelligence = 0.75f;
        tangoStrafeEnabled = true;
        tangoStrafePauseFreq = 0.75f;
        martialPreferredState = martialState.attacking;
        sensorySightRangeScalar = 0.75f;
        sensoryAudioRangeScalar = 0.75f;
        meanderPauseFrequency = 0.5f;
        tangoStrafePauseFreq = 0.5f;
        tangoStrafeEnabled = true;
        itemManagementSeekItems = true;
        itemManagementPreferredType = Entity.WieldMode.OneHanders;
        Aggression = UnityEngine.Random.value;
        Fear = UnityEngine.Random.value;
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
        else if (UnityEngine.Random.value < Aggression)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, CombatSpeed);       
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, CombatSpeed, timeoutCheckMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
        }
        else if (entity.Poise / entity.Strength < Fear || entity.Posture == Entity.PostureStrength.Weak)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, CombatSpeed);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Guarding, getPausePeriod(min: 1.5f));
        }
        else
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, CombatSpeed, timeoutCheckMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, CombatSpeed);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
        }      
    }

    protected override void reactToFoeVulnerable()
    {
        if (checkMyWeaponInRange())
        {
            _MartialController.Override_Action(mainWep, Weapon.ActionAnim.QuickCoil, CombatSpeed);
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.QuickAttack);
        }
        else if(UnityEngine.Random.value < Aggression)
        {
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
        if(_MartialController.Get_Next_Action(mainWep) == Weapon.ActionAnim.Guarding) 
        {
            return;
        }
        else if(mainWep.Action != Weapon.ActionAnim.Idle && mainWep.Action != Weapon.ActionAnim.Recoiling)
        {

        }
        else if(entity.Posture == Entity.PostureStrength.Weak || UnityEngine.Random.value > Aggression)
        {
            _MartialController.Override_Action(mainWep, mainWep.Action, CombatSpeed);
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Guarding, getPausePeriod(min: 1.5f));
        }
    }

    protected override void reactToFoeThrowing()
    {
        if (!checkMyWeaponInRange() && UnityEngine.Random.value >= Aggression)
        {
            _MartialController.Override_Action(mainWep, mainWep.Action, CombatSpeed);
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Guarding, CombatSpeed, () => !martialFoeThrowingLatch);
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

    private void createSpawnWeapon()
    {
        if (UnityEngine.Random.value <= Alternative_Weapon_Frequency && Alternative_Weapon != null)
        {
            new GameObject().AddComponent(Alternative_Weapon).GetComponent<Weapon>().PickupItem(entity);
        }
        else
        {
            new GameObject().AddComponent(Standard_Weapon).GetComponent<Weapon>().PickupItem(entity);
        }
    }


}

