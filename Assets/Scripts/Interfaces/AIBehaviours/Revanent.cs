using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revanent : AIBehaviour
{

    private _Flames mainHandFlame;
    private _Flames offHandFlame;
    private _Flames footFlame;

    private float Aggression = 0.75f;

    protected override void Awake()
    {
        base.Awake();

    }
    protected override void Start()
    {
        base.Start();
        mainHandFlame = Instantiate(Game.SpiritFlameTemplate).GetComponent<_Flames>();
        offHandFlame = Instantiate(Game.SpiritFlameTemplate).GetComponent<_Flames>();
        footFlame = Instantiate(Game.SpiritFlameTemplate).GetComponent<_Flames>();
        Intelligence = 1.0f;
        tangoPeriodScalar = 1f;
        tangoStrafePauseFreq = 0f;
        //martialReactiveAttack = true;
        martialPreferredState = martialState.none;
        itemManagementSeekItems = true;
        itemManagementGreedy = true;
        itemManagementPreferredType = Entity.WieldMode.TwoHanders;
        sensorySightRangeScalar = 1.25f;
        entity.FinalDashEnabled = true;
        new GameObject().AddComponent<Greatsword>().PickupItem(entity);
        //new GameObject().AddComponent<Weapon_HandAxe>().PickupItem(entity);
        //new GameObject().AddComponent<Weapon_HandAxe>().PickupItem(entity);
    }

    protected override void Update()
    {
        base.Update();
        float period = entity.Posture == Entity.PostureStrength.Weak ? 1 : 3;

        if (dashingCooldownTimer > period && entity.Foe)
        {
            if (!mainWep)
            {

            }
            else if (entity.Posture == Entity.PostureStrength.Weak || mainWep.Action == Weapon.ActionAnimation.StrongCoil || dashingDesiredDirection != Vector3.zero)
            {
                //dashingDesiredDirection = entity.Foe.transform.position - transform.position;
            }
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (footFlame)
        {
            Destroy(footFlame.gameObject);
        } 
        if (mainHandFlame)
        {
            Destroy(mainHandFlame.gameObject);
        } 
        if (offHandFlame)
        {
            Destroy(offHandFlame.gameObject);
        }
    }


    protected override void queueNextRoundOfActions(Weapon weapon)
    {
        if (weapon != mainWep)
        {
            return;
        }
        else if (!entity.Foe || !_MartialController.Weapon_Queues.ContainsKey(mainWep))
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Idle);
        }
        else if (entity.Posture == Entity.PostureStrength.Weak)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickCoil, requisite: checkMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickAttack);
        }
        else if (checkMyWeaponInRange())
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickCoil);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickAttack);
        }
        else if (Random.value <= Aggression)
        {
            if(dashingCooldownTimer > 3)
            {
                dashingChargeTimer = 1;
                dashingDesiredDirection = entity.Foe.transform.position - transform.position;
            }
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.StrongCoil, requisite: checkMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.StrongAttack);
        }
        else
        {

            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickCoil);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickAttack);
        }
    }

    protected override void reactToFoeVulnerable()
    {
        if (checkMyWeaponInRange())
        {
            _MartialController.Override_Action(mainWep, Weapon.ActionAnimation.QuickCoil);
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.QuickAttack);
        }
    }

    protected override void reactToFoeChange()
    {
        if (entity.Foe)
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Idle);
        }
        else
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Idle);
        }
    }

    protected override void reactToIncomingAttack()
    {
        if (_MartialController.Get_Next_Action(mainWep) == Weapon.ActionAnimation.Guarding)
        {
            return;
        }
        else if (checkMyWeaponInRange())
        {
            if(dashingCooldownTimer > 1)
            {
                dashingChargePeriod = 0;
                Vector3 disposition = entity.Foe.transform.position - transform.position;
                float randomLeftRightOffset = Mathf.Sign(Random.value - 0.5f) * 120;
                dashingDesiredDirection = angleToDirection(getAngle(disposition.normalized) + randomLeftRightOffset);
            }
        }
        else
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Guarding, getPausePeriod());
        }
    }

}

