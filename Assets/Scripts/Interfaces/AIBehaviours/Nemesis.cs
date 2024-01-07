using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nemesis : AIBehaviour
{
    public Hellfire hellfire;
    public _Flames flames;

    private float timeToRecoverFromDash = 0;
    private float finalDashRecoveryTime = 4;
    private float quickDashRecoveryTime = 1;

    private float beamDelayPeriod = 2;
    private float beamDuration = 4;

    private float beamDurationTimer = 0;
    private float beamDelayTimer = 0;

    public enum Cycle
    {
        DashCycle,
        BeamCycle,
    }
    public Cycle BattleCycle = Cycle.DashCycle;

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
        sensoryBaseRange = 1.5f;
        meanderPauseFrequency = 0f;
        itemManagementSeekItems = false;
        pursueStoppingDistance = sensoryBaseRange * sensorySightRangeScalar * Mathf.Lerp(0.3f, 0.5f, Random.value);
        grabEnabled = false;
        dashingChargePeriod = 1f;
        hellfire = Instantiate(Resources.Load<GameObject>("Prefabs/Hellfire")).GetComponent<Hellfire>();
        hellfire.transform.SetParent(transform, true);
        hellfire.transform.localPosition = Vector3.forward;
        hellfire.transform.localEulerAngles = Vector3.up * 90;
        hellfire.transform.localScale = Vector3.one;
        hellfire.Wielder = entity;
    }

    protected override void Update()
    {
        base.Update();
        if (entity.Foe)
        {
            BattleCycle = entity.Posture == Entity.PostureStrength.Weak ? Cycle.BeamCycle : Cycle.DashCycle;
            switch (BattleCycle)
            {
                case Cycle.DashCycle:
                    dashCycleUpdates();
                    break;
                case Cycle.BeamCycle:
                    beamCycleUpdates();
                    break;
            }
            string key = "Beam!";
            if (hellfire.form == Hellfire.Form.Beam)
            {
                entity.modSpeed[key] = -0.9f;
                entity.modTurnSpeed[key] = -0.95f;
            }
            else
            {
                entity.modSpeed[key] = 0f;
                entity.modTurnSpeed[key] = 0f;
            }
        }
    }



    /***** PUBLIC *****/


    /***** PROTECTED *****/
    protected override void SetTangoParameters()
    {
        if (BattleCycle == Cycle.BeamCycle)
        {
            tangoStrafeEnabled = true;
            tangoInnerRange = sensoryBaseRange * sensorySightRangeScalar * 0.25f;
            tangoOuterRange = sensoryBaseRange * sensorySightRangeScalar * 0.75f;
        }
        else if (dashingCooldownTimer >= timeToRecoverFromDash)
        {
            tangoStrafeEnabled = false;
            tangoInnerRange = tangoOuterRange = 0;
        }
        else
        {
            tangoStrafeEnabled = true;
            tangoInnerRange = entity.personalBox.radius * entity.scaleActual * 2;
            tangoOuterRange = sensoryBaseRange * sensorySightRangeScalar * 0.5f;
        }
    }

    /***** PRIVATE *****/
    private void dashCycleUpdates()
    {
        beamDurationTimer = 0;
        beamDelayTimer = 0;
        hellfire.form = Hellfire.Form.Off;
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
        if(beamDelayTimer < beamDelayPeriod)
        {
            beamDelayTimer += Time.deltaTime;
            hellfire.form = Hellfire.Form.Preheat;
        }
        else if(beamDurationTimer < beamDuration)
        {
            beamDurationTimer += Time.deltaTime;
            hellfire.form = Hellfire.Form.Beam;
        }
        else
        {
            beamDelayTimer = 0;
            beamDurationTimer = 0;
        }
    }

}

