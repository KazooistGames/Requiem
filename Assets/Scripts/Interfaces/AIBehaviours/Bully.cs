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
        _MartialController.INSTANCE.ClearedQueue.AddListener(queueNextRoundOfActions);
    }

    protected override void Update()
    {
        base.Update();
        float period = entity.Posture == Entity.PostureStrength.Weak ? 1 : 3;

        if (dashingCooldownTimer < period || !entity.Foe)
        {

        }
        else if (!mainWep)
        {
            dashingChargePeriod = 0.5f;
            dashingDesiredDirection = entity.Foe.transform.position - transform.position;
        }
        else if (entity.Posture == Entity.PostureStrength.Weak || mainWep.Action == Weapon.ActionAnim.StrongCoil || dashingDesiredDirection != Vector3.zero)
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
            if(Random.value <= Aggression)
            {
                _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, CombatSpeed, timeoutCheckMyWeaponInRange);
                _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
            }
            else
            {
                _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Guarding, getPausePeriod());
            }
        }
        else if(checkMyWeaponInRange())
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, CombatSpeed);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
        }
        else if(Random.value <= Aggression)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.StrongCoil, CombatSpeed, timeoutCheckMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.StrongAttack);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, CombatSpeed);
        }
        else
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil, CombatSpeed, timeoutCheckMyWeaponInRange);
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
            return _MartialController.Debounce_Timers[mainWep] > CombatSpeed * 3;
        }
        else
        {
            return false;
        }
    }


}

