using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revanent : AIBehaviour
{

    private _Flames mainHandFlame;
    private _Flames offHandFlame;
    private _Flames footFlame;

    private float Aggression = 0.5f;

    private float OutOfRangeTimer = 0;
    private float InRangeTimer = 0;

    public enum Pattern
    {
        Seething,
        Overpowering,
        Dueling
    }
    public Pattern CurrentPattern = Pattern.Seething;

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
        if (checkMyWeaponInRange())
        {
            InRangeTimer += Time.deltaTime;
            OutOfRangeTimer = 0;
        }
        else
        {
            OutOfRangeTimer += Time.deltaTime;
            InRangeTimer = 0;
        }
        if (!mainWep)
        {

        }
        else if (!entity.Foe)
        {
            _MartialController.Cancel_Actions(mainWep);
        }
        else
        {
            switch (CurrentPattern)
            {
                case Pattern.Seething:
                    if (mainWep.Action == Weapon.ActionAnim.Recovering && dashingCooldownTimer >= 1)
                    {
                        dashingChargePeriod = 0.5f;
                        dashingDesiredDirection = transform.position - entity.Foe.transform.position;
                    }
                    break;
                case Pattern.Overpowering:
                    if (mainWep.Action == Weapon.ActionAnim.StrongCoil && dashingCooldownTimer >= 3)
                    {
                        dashingChargePeriod = 1f;
                        dashingDesiredDirection = entity.Foe.transform.position - transform.position;
                    }
                    else if (mainWep.Action == Weapon.ActionAnim.Guarding && dashingCooldownTimer >= 0.5f)
                    {
                        dashingChargePeriod = 0.25f;
                        dashingDesiredDirection = entity.Foe.transform.position - transform.position;
                    }
                    break;
                case Pattern.Dueling:
                
                    break;
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

    /***** PUBLIC *****/
    /***** PROTECTED *****/

    protected override void queueNextRoundOfActions(Weapon weapon)
    {
        if (weapon == mainWep)
        {
            switch (CurrentPattern)
            {
                case Pattern.Seething:
                    _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, requisite: checkMyWeaponInRange);
                    _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
                    break;
                case Pattern.Overpowering:
                    if (Random.value <= Aggression)
                    {
                        _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.StrongCoil, requisite: checkMyWeaponInRange);
                        _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.StrongAttack);
                    }
                    else
                    {
                        _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Guarding, 1 + Random.value);               
                    }

                    break;
                case Pattern.Dueling:

                    break;
            }
        }
    }

    protected override void reactToFoeVulnerable()
    {
        if (checkMyWeaponInRange())
        {
            dashingChargePeriod = 0;
            dashingDesiredDirection = entity.Foe.transform.position - transform.position;
            _MartialController.Override_Action(mainWep, Weapon.ActionAnim.QuickCoil, requisite: checkMyWeaponInRange);
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.QuickAttack);
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
        switch (CurrentPattern)
        {
            case Pattern.Seething:
                if (_MartialController.Weapon_Actions[mainWep].Action == Weapon.ActionAnim.Guarding)
                {
                    return;
                }
                else if (Random.value <= Aggression && dashingCooldownTimer > 0.5f)
                {
                    dashingChargePeriod = 0;
                    Vector3 disposition = entity.Foe.transform.position - transform.position;
                    float randomLeftRightOffset = Mathf.Sign(Random.value - 0.5f) * 90;
                    dashingDesiredDirection = angleToDirection(getAngle(disposition.normalized) + randomLeftRightOffset);
                }
                else
                {
                    _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Guarding, getPausePeriod());
                }
                break;
            case Pattern.Overpowering:
                break;
        }
        
    }

    /***** PRIVATE *****/



}

