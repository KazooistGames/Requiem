using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revanent : AI
{

    private _Flames mainHandFlame;
    private _Flames offHandFlame;
    private _Flames footFlame;

    protected override void Awake()
    {
        base.Awake();
        mainHandFlame = Instantiate(Game.SpiritFlameTemplate).GetComponent<_Flames>();
        offHandFlame = Instantiate(Game.SpiritFlameTemplate).GetComponent<_Flames>();
        footFlame = Instantiate(Game.SpiritFlameTemplate).GetComponent<_Flames>();
    }
    protected override void Start()
    {
        base.Start();
        Intelligence = 1.0f;
        tangoPeriodScalar = 1f;
        tangoStrafePauseFreq = 0f;
        martialReactiveAttack = true;
        martialPreferredState = martialState.none;
        itemManagementSeekItems = true;
        itemManagementGreedy = true;
        itemManagementPreferredType = Character.WieldMode.TwoHanders;
        sensorySightRangeScalar = 1.25f;
        new GameObject().AddComponent<Greatsword>().PickupItem(entity);
        //new GameObject().AddComponent<Weapon_HandAxe>().PickupItem(entity);
        //new GameObject().AddComponent<Weapon_HandAxe>().PickupItem(entity);
    }

    protected override void Update()
    {
        base.Update();
        switch (State)
        {
            case AIState.none:
                StateTransition(AIState.seek);
                break;
            case AIState.seek:
                if (entity.Foe)
                {
                    StateTransition(AIState.aggro);
                }
                else
                {
                    ReflexRate = 0.20f;           
                }
                break;
            case AIState.aggro:
                if (trackingTrailCold)
                {
                    StateTransition(AIState.seek);
                }
                else
                {
                    ReflexRate = 0.05f;
                    Weapon mainWep = entity.MainHand ? entity.MainHand.GetComponent<Weapon>() : null;
                    Weapon offWep = entity.OffHand ? entity.OffHand.GetComponent<Weapon>() : null;
                    bool inRange = entity.Foe && mainWep ? (mainWep.Range) >= (entity.Foe.transform.position - transform.position).magnitude : false;
                    bool foeInRange = entity.Foe ? entity.Foe.MainHand ? entity.Foe.MainHand.GetComponent<Weapon>() ? (entity.Foe.MainHand.GetComponent<Weapon>().Range) >= (entity.Foe.transform.position - transform.position).magnitude : false : false : false;
                    bool defensive = martialCurrentState == martialState.defending;
                    //bool offensive = (mainWep ? mainWep.WindingUp : false) || (offWep ? offWep.WindingUp : false);
                    //tangoStrafeEnabled = !offensive || defensive;
                    //dashingDodgeFoe = (entity.posture == Entity.Posture.Stiff) || (offensive && inRange) || entity.Rebuked;
                    //dashingInitiate = (offensive || defensive) && !dashingDodgeFoe;
                    dashingDodgeAttacks = !dashingInitiate && !dashingDodgeFoe;
                    dashingDodgeAim = !dashingInitiate;
                    dashingPower = dashingInitiate ? 1.0f : 0.0f;
                    dashingChargePeriod = dashingInitiate ? 0.5f : 0.0f;
                    dashingCooldownPeriod = dashingInitiate ? 1.0f : 0.5f;
                }
                break;
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

}

