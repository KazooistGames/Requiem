using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Goon : AIBehaviour
{
 
    private static float CombatSpeed = 0.5f;

    public static Type Standard_Weapon = typeof(Handaxe);
    public static Type Alternative_Weapon = null;
    public static float Alternative_Weapon_Frequency = 0.20f;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        create_spawn_weapon();
        Intelligence = 1f;
        tangoStrafeEnabled = true;
        martialPreferredState = martialState.attacking;
        sensorySightRangeScalar = 1.5f;
        sensoryAudioRangeScalar = 1.5f;
        meanderPauseFrequency = 0.5f;
        tangoStrafePauseFreq = 0.75f;
        tangoStrafeEnabled = true;
        itemManagementSeekItems = true;
        itemManagementDelayPeriod = 3;
        itemManagementPreferredType = Entity.WieldMode.OneHanders;
        State = AIState.seek;
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
        else
        {
            float inhibition_rng = UnityEngine.Random.value;
            attack_cycle();
            defend_cycle();
        } 
    }


    protected override void reactToFoeChange()
    {
        if (entity.Foe)
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Idle, CombatSpeed);
            if (0.5 >= UnityEngine.Random.value)
            {
                attack_cycle();
            }
            else
            {
                defend_cycle();
            }
        }
        else
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Idle);
        }
    }



    /***** PRIVATE *****/


    private void defend_cycle()
    {
        _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Guarding, 2);
        //_MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, CombatSpeed);

    }
    private void attack_cycle()
    {
        //_MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, 0, checkMyWeaponInRange, 3);
        int number_of_swings = 4;
        for (int i = 0; i < number_of_swings; i++)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, CombatSpeed);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
        }
    }
    private void create_spawn_weapon()
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

