using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revanent : AIBehaviour
{

    private _Flames mainHandFlame;
    private _Flames offHandFlame;
    private _Flames footFlame;

    private float Aggression = 0.5f;

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
        tangoStrafeEnabled = true;
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
            else if (mainWep.Action == Weapon.ActionAnimation.Guarding && dashingCooldownTimer >= 1)
            {
                dashingChargePeriod = mainWep.Action == Weapon.ActionAnimation.Guarding ? 0.5f : 1;
                dashingDesiredDirection = entity.Foe.transform.position - transform.position;
            }
            else if (mainWep.Action == Weapon.ActionAnimation.StrongCoil && dashingCooldownTimer >= 3)
            {

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
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.Guarding);
        }
        else if (Random.value <= Aggression)
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.StrongCoil, requisite: checkMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.StrongAttack);
        }
        else
        {
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickCoil, requisite: checkMyWeaponInRange);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickAttack);
            _MartialController.Queue_Action(mainWep, Weapon.ActionAnimation.QuickAttack);

        }
    }

    protected override void reactToFoeVulnerable()
    {
        if (checkMyWeaponInRange())
        {
            dashingChargePeriod = 0;
            dashingDesiredDirection = entity.Foe.transform.position - transform.position;
            _MartialController.Override_Action(mainWep, Weapon.ActionAnimation.QuickCoil, requisite: checkMyWeaponInRange);
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
        else if (dashingCooldownTimer > 0.5f)
        {
            dashingChargePeriod = 0;
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            float randomLeftRightOffset = Mathf.Sign(Random.value - 0.5f) * 90;
            dashingDesiredDirection = angleToDirection(getAngle(disposition.normalized) + randomLeftRightOffset);
        }
        else
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnimation.Guarding, getPausePeriod());
        }
    }

}

