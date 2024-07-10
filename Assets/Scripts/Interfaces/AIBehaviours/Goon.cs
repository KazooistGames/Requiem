using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Goon : AIBehaviour
{
    //public float excitement = 0f;
    private static float behaviour_mutation_phase = 0;
    private static float CombatSpeed = 0.75f;

    public static float Aggression = 1f;
    public static float Fear = 1f;

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
        mutate_behaviour();
        create_spawn_weapon();
        Intelligence = 1f;
        tangoStrafeEnabled = true;
        martialPreferredState = martialState.attacking;
        sensorySightRangeScalar = 0.75f;
        sensoryAudioRangeScalar = 0.75f;
        meanderPauseFrequency = 0.5f;
        tangoStrafePauseFreq = 0.75f;
        tangoStrafeEnabled = true;
        itemManagementSeekItems = true;
        itemManagementDelayPeriod = 3;
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
        else
        {
            float inhibition_rng = UnityEngine.Random.value;
            if(inhibition_rng > Aggression)
            {
                _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, CombatSpeed, checkMyWeaponInRange, 3);
            }
            attack_cycle();
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, CombatSpeed);
            defend_cycle();
            if (inhibition_rng < Fear)
            {
                defend_cycle();
            }
        } 
    }


    protected override void reactToFoeChange()
    {
        if (entity.Foe)
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Idle, CombatSpeed);
            if (Aggression >= Fear)
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
        float guard_period = Mathf.Sqrt(Fear) * 4;
        _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Guarding, guard_period);
        _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, CombatSpeed);

    }
    private void attack_cycle()
    {
        _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, 0, checkMyWeaponInRange, 3);
        int number_of_swings = Mathf.CeilToInt(Aggression * 4);
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

    private static void mutate_behaviour()
    {
        if(behaviour_mutation_phase == 0)
        {
            behaviour_mutation_phase = UnityEngine.Random.value * 2 * Mathf.PI;
        }
        else
        {
            float step_size = Mathf.PI / 60;
            behaviour_mutation_phase += step_size;
        }
        Aggression = 0.5f + Mathf.Sin(behaviour_mutation_phase)/2;
        Fear = 0.5f + Mathf.Cos(behaviour_mutation_phase*2)/2;
    }

}

