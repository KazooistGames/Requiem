using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revanent : AIBehaviour
{

    private _Flames mainHandFlame;
    private _Flames offHandFlame;
    private _Flames footFlame;

    private float Aggression = 0.75f;

    //private float patternTimer = 0.0f;
    //private float patternPeriod = 0.0f;

    public enum Pattern
    {
        Dueling,
        Overpowering,
        //Dueling
    }
    public Pattern CurrentPattern = Pattern.Dueling;

    protected override void Awake()
    {
        base.Awake();

    }
    protected override void Start()
    {
        base.Start();
        mainHandFlame = Instantiate(Requiem.SpiritFlameTemplate).GetComponent<_Flames>();
        offHandFlame = Instantiate(Requiem.SpiritFlameTemplate).GetComponent<_Flames>();
        footFlame = Instantiate(Requiem.SpiritFlameTemplate).GetComponent<_Flames>();
        Intelligence = 1.0f;
        tangoPeriodScalar = 1f;
        tangoStrafePauseFreq = 0f;
        tangoStrafeEnabled = true;
        //martialReactiveAttack = true;
        meanderPauseFrequency = 0.75f;
        martialPreferredState = martialState.none;
        itemManagementSeekItems = true;
        itemManagementGreedy = true;
        itemManagementPreferredType = Entity.WieldMode.OneHanders;
        sensorySightRangeScalar = 1.25f;
        entity.FinalDashEnabled = true;
        RestingState = AIState.seek;
        new GameObject().AddComponent<Greatsword>().PickupItem(entity);
    }

    protected override void Update()
    {
        base.Update();
        if (!mainWep)
        {

        }
        else if (!entity.Foe)
        {
            _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Idle);
        }
        else
        {
            switch (CurrentPattern)
            {
                case Pattern.Dueling:
                    if (entity.Posture == Entity.PostureStrength.Strong)
                    {
                        CurrentPattern = Pattern.Overpowering;
                    }
                    if (mainWep.Action == Weapon.ActionAnim.Recovering && dashingCooldownTimer >= 1)
                    {
                        dashingChargePeriod = 0f;
                        dashingDesiredDirection = transform.position - entity.Foe.transform.position;
                    }
                    break;
                case Pattern.Overpowering:
                    if (mainWep.Action == Weapon.ActionAnim.StrongCoil && dashingCooldownTimer >= 1f)
                    {
                        dashingChargePeriod = 0.25f;
                        dashingDesiredDirection = entity.Foe.transform.position - transform.position;
                    }
                    else if (mainWep.Action == Weapon.ActionAnim.Guarding && dashingCooldownTimer >= 3f)
                    {
                        dashingChargePeriod = 1f;
                        dashingDesiredDirection = entity.Foe.transform.position - transform.position;
                    }
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
                case Pattern.Dueling:
                    _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Idle, requisite: checkMyWeaponInRange);
                    _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickAttack);
                    _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.QuickCoil);
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
         _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Idle);
    }

    protected override void reactToIncomingDash()
    {
        if(entity.Posture == Entity.PostureStrength.Weak)
        {

        }
        if (dashingCooldownTimer > 0.5f)
        {
            dashingChargePeriod = 0;
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            float randomLeftRightOffset = Mathf.Sign(Random.value - 0.5f) * 120;
            dashingDesiredDirection = angleToDirection(getAngle(disposition.normalized) + randomLeftRightOffset);
        }
    }

    protected override void reactToIncomingAttack()
    {
        switch (CurrentPattern)
        {
            case Pattern.Dueling:
                if (Random.value <= Aggression && dashingCooldownTimer > 0.5f && mainWep.Action != Weapon.ActionAnim.Guarding)
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
                CurrentPattern = Pattern.Dueling;
                break;
        }
        
    }

    /***** PRIVATE *****/



}

