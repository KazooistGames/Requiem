using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goon : AIBehaviour
{
    //public float excitement = 0f;
    private float CombatSpeed = 0.5f;
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
        tangoStrafePauseFreq = 0.5f;
        tangoStrafeEnabled = true;
        itemManagementSeekItems = true;
        itemManagementPreferredType = Entity.WieldMode.OneHanders;

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
        else if (entity.Posture == Entity.PostureStrength.Weak)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, CombatSpeed);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Guarding, getPausePeriod());
        }
        else if (Random.value <= Aggression)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, CombatSpeed);       
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, CombatSpeed, checkMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
        }
        else
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, CombatSpeed, checkMyWeaponInRange);
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
        else if (mainWep.Action == Weapon.ActionAnim.QuickCoil && checkMyWeaponInRange() && Random.value >= Aggression)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Guarding, getPausePeriod());            
        }
        else if(Random.value >= Aggression || mainWep.Action == Weapon.ActionAnim.Idle)
        {
            _MartialController.Override_Action(mainWep, mainWep.Action, CombatSpeed);
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Guarding, getPausePeriod());
        }
    }


    /***** PRIVATE *****/


}

