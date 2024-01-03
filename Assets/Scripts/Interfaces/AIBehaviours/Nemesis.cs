using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nemesis : AIBehaviour
{
    private float timeToRecoverFromDash = 0;
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
        meanderPauseFrequency = 0.5f;
        itemManagementSeekItems = false;
        pursueStoppingDistance = sensoryBaseRange * sensorySightRangeScalar * Mathf.Lerp(0.3f, 0.5f, Random.value);
        grabEnabled = false;
        dashingChargePeriod = 0.75f;
    }

    protected override void Update()
    {
        base.Update();
        if (entity.Foe)
        {
            if (entity.Dashing)
            {
                tangoDeadbanded = false;
            }
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            bool inPosition = disposition.magnitude < pursueStoppingDistance || entity.DashCharging;
            bool shortDashTrigger = inPosition && dashingCooldownTimer > 2;
            bool finalDashTrigger = dashingCooldownTimer > 5;
            if (finalDashTrigger)
            {
                dashingChargePeriod = 2f;
                dashingDesiredDirection = disposition;
                timeToRecoverFromDash = 5;
            }
            else if(shortDashTrigger)
            {
                dashingChargePeriod = 0.5f;
                dashingDesiredDirection = disposition;
                timeToRecoverFromDash = 2;
            }
        }
    }

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



}

