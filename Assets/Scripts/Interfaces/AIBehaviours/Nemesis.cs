using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nemesis : AIBehaviour
{
    public Hellfire hellfire;

    private float timeToRecoverFromDash = 0;
    private float finalDashRecoveryTime = 5;
    private float quickDashRecoveryTime = 2;

    public enum Cycle
    {
        DashAttack,
        BeamAttack,
    }
    public Cycle BattleCycle = Cycle.DashAttack;

    protected override void Awake()
    {
        base.Awake();
        entity = GetComponent<Entity>() ? GetComponent<Entity>() : gameObject.AddComponent<Entity>();

    }
    protected override void Start()
    {
        base.Start();
        Intelligence = 0.5f;
        RestingState = AIState.seek;
        tangoStrafeEnabled = true;
        tangoStrafePauseFreq = 0;
        dashingChargePeriod = 1.0f;
        grabDPS = 10f;
        sensorySightRangeScalar = 1.0f;
        meanderPauseFrequency = 0f;
        itemManagementSeekItems = false;
        pursueStoppingDistance = sensoryBaseRange * sensorySightRangeScalar * Mathf.Lerp(0.3f, 0.5f, Random.value);
        grabEnabled = false;
        dashingChargePeriod = 1f;
        hellfire = Instantiate(Resources.Load<GameObject>("Prefabs/Hellfire")).GetComponent<Hellfire>();
    }

    protected override void Update()
    {
        base.Update();
        if (entity.Foe)
        {
            switch (BattleCycle)
            {
                case Cycle.DashAttack:
                    dashCycleUpdates();
                    break;
                case Cycle.BeamAttack:
                    beamCycleUpdates();
                    break;
            }
        }
    }



    /***** PUBLIC *****/


    /***** PROTECTED *****/
    protected override void SetTangoParameters()
    {
        if (dashingCooldownTimer >= timeToRecoverFromDash)
        {
            tangoStrafeEnabled = false;
            tangoInnerRange = tangoOuterRange = 0;
        }
        else
        {
            tangoStrafeEnabled = true;
        }
        tangoInnerRange = entity.personalBox.radius * entity.scaleActual * 2;
        tangoOuterRange = sensoryBaseRange * sensorySightRangeScalar * 0.5f;
    }

    /***** PRIVATE *****/
    private void dashCycleUpdates()
    {
        if (entity.Dashing)
        {
            tangoDeadbanded = false;
        }
        Vector3 disposition = entity.Foe.transform.position - transform.position;
        bool inPosition = disposition.magnitude < pursueStoppingDistance || entity.DashCharging;
        bool shortDashTrigger = inPosition && dashingCooldownTimer > quickDashRecoveryTime;
        bool finalDashTrigger = dashingCooldownTimer > finalDashRecoveryTime;
        if (finalDashTrigger)
        {
            dashingChargePeriod = 2f;
            dashingDesiredDirection = disposition;
            timeToRecoverFromDash = quickDashRecoveryTime;
        }
        else if (shortDashTrigger)
        {
            dashingChargePeriod = 0.5f;
            dashingDesiredDirection = disposition;
            timeToRecoverFromDash = finalDashRecoveryTime;
        }
    }

    private void beamCycleUpdates()
    {

    }

}

