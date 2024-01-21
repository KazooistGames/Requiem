using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revanent : AIBehaviour
{
    //private _Flames footFlame;

    private float Aggression = 0.75f;

    //private float patternTimer = 0.0f;
    //private float patternPeriod = 0.0f;

    public enum Pattern
    {
        Dueling,
        Overpowering,
        Kiting,
    }
    public Pattern CurrentPattern = Pattern.Dueling;

    protected override void Awake()
    {
        base.Awake();

    }
    protected override void Start()
    {
        base.Start();
        //footFlame = Instantiate(Requiem.SpiritFlameTemplate).GetComponent<_Flames>();
        //footFlame.boundObject = gameObject;
        //footFlame.gameObject.SetActive(true);
        entity.flames.gameObject.SetActive(true);
        entity.flames.FlamePresentationStyle = _Flames.FlameStyles.Soulless;
        Intelligence = 1.0f;
        tangoPeriodScalar = 1f;
        tangoStrafePauseFreq = 0f;
        tangoStrafeEnabled = true;
        //martialReactiveAttack = true;
        meanderPauseFrequency = 0.75f;
        martialPreferredState = martialState.none;
        itemManagementSeekItems = true;
        itemManagementGreedy = true;
        sensorySightRangeScalar = 1.25f;
        entity.FinalDashEnabled = true;
        RestingState = AIState.seek;
        entity.JustDisarmed.AddListener(reactToDisarm);
        new GameObject().AddComponent<Greatsword>().PickupItem(entity);
        new GameObject().AddComponent<Shortsword>().PickupItem(entity);
        new GameObject().AddComponent<Shortsword>().PickupItem(entity);
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
            if (!_MartialController.Weapon_Actions.ContainsKey(mainWep))
            {
                queueNextRoundOfActions(mainWep);
            }
            switch (CurrentPattern)
            {
                case Pattern.Dueling:
                    itemManagementNoSingles = false;
                    itemManagementPreferredType = Entity.WieldMode.TwoHanders;
                    if (entity.Posture == Entity.PostureStrength.Strong)
                    {
                        CurrentPattern = Pattern.Overpowering;
                        _MartialController.Override_Action(mainWep, Weapon.ActionAnim.Idle);
                        _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Idle);
                    }
                    if ((mainWep.Action == Weapon.ActionAnim.Recovering || mainWep.Action == Weapon.ActionAnim.Recoiling) && dashingCooldownTimer >= 0.5f)
                    {
                        dashingChargePeriod = 0f;
                        dashingDesiredDirection = transform.position - entity.Foe.transform.position;
                    }
                    break;
                case Pattern.Overpowering:
                    itemManagementNoSingles = false;
                    itemManagementPreferredType = Entity.WieldMode.TwoHanders;
                    if (mainWep.Action == Weapon.ActionAnim.StrongCoil && dashingCooldownTimer >= 1f)
                    {
                        dashingChargePeriod = 0.25f;
                        dashingDesiredDirection = entity.Foe.transform.position - transform.position;
                    }
                    else if (mainWep.Action == Weapon.ActionAnim.Guarding && dashingCooldownTimer >= 3f)
                    {
                        dashingChargePeriod = 1.0f;
                        dashingDesiredDirection = entity.Foe.transform.position - transform.position;
                    }
                    break;
                case Pattern.Kiting:
                    itemManagementNoSingles = true;
                    itemManagementPreferredType = Entity.WieldMode.OneHanders;
                    if(!entity.leftStorage && !entity.rightStorage && entity.backStorage)
                    {
                        CurrentPattern = Pattern.Dueling;
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
        //if (footFlame)
        //{
        //    Destroy(footFlame.gameObject);
        //} 
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
                case Pattern.Kiting:
                    if(mainWep.equipType == Wieldable.EquipType.OneHanded)
                    {
                        _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Aiming, 0.5f, checkFoeNotDefending);
                        _MartialController.Queue_Action(mainWep, Weapon.ActionAnim.Throwing);
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
        if(entity.Posture != Entity.PostureStrength.Weak)
        {

        }
        else if (dashingCooldownTimer > 0.5f)
        {
            dashingChargePeriod = 0;
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            float randomLeftRightOffset = Mathf.Sign(Random.value - 0.5f) * 135;
            dashingDesiredDirection = angleToVector(getAngle(disposition.normalized) + randomLeftRightOffset);
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
                    dashingDesiredDirection = angleToVector(getAngle(disposition.normalized) + randomLeftRightOffset);
                }
                else
                {
                    _MartialController.Override_Action(mainWep, Weapon.ActionAnim.Guarding, getPausePeriod());
                }
                break;
            case Pattern.Overpowering:
                CurrentPattern = Pattern.Dueling;
                _MartialController.Override_Action(mainWep, Weapon.ActionAnim.Idle);
                _MartialController.Override_Queue(mainWep, Weapon.ActionAnim.Idle);
                break;
            case Pattern.Kiting:
                if (Random.value <= Aggression && dashingCooldownTimer > 0.5f && mainWep.Action != Weapon.ActionAnim.Guarding)
                {
                    dashingChargePeriod = 0;
                    Vector3 disposition = entity.Foe.transform.position - transform.position;
                    float randomLeftRightOffset = Mathf.Sign(Random.value - 0.5f) * 90;
                    dashingDesiredDirection = angleToVector(getAngle(disposition.normalized) + randomLeftRightOffset);
                }
                break;
        }
        
    }


    protected override void reactToFoeThrowing()
    {
        if (checkMyWeaponInRange())
        {
            
        }
        else if(dashingCooldownTimer > 0.5f && mainWep.Action != Weapon.ActionAnim.Guarding)
        {
            dashingChargePeriod = 0;
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            float randomLeftRightOffset = Mathf.Sign(Random.value - 0.5f) * 90;
            dashingDesiredDirection = angleToVector(getAngle(disposition.normalized) + randomLeftRightOffset);
        }
        else
        {
            _MartialController.Override_Action(mainWep, Weapon.ActionAnim.Guarding, getPausePeriod(min: 1.5f));
        }
        
    }

    /***** PRIVATE *****/
    private void reactToDisarm()
    {
        CurrentPattern = Pattern.Kiting;
    }

    private bool checkFoeNotDefending()
    {
        return entity.Foe ? !entity.Foe.Defending : true;
    }

}

