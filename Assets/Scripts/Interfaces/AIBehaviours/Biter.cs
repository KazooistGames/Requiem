using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biter : AIBehaviour
{

    private bool charged = true;
    private float chargePeriod = 10;
    protected override void Awake()
    {
        base.Awake();
        entity = GetComponent<Entity>() ? GetComponent<Entity>() : gameObject.AddComponent<Entity>();

    }
    protected override void Start()
    {
        base.Start();
        Intelligence = 0.75f;
        RestingState = AIState.seek;
        tangoStrafeEnabled = true;
        tangoStrafePauseFreq = 0;
        grabDPS = 0f;
        sensorySightRangeScalar = 1.0f;
        meanderPauseFrequency = 0.5f;
        itemManagementSeekItems = false;
        pursueStoppingDistance = sensoryBaseRange * sensorySightRangeScalar * 0.20f;
        grabEnabled = true;
        dashingChargePeriod = 0.5f;
    }

    protected override void Update()
    {
        base.Update();
        if (entity.Foe)
        {
            charged = dashingCooldownTimer >= chargePeriod;
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            bool inPosition = disposition.magnitude > pursueStoppingDistance;
            if (charged && inPosition && !entity.Shoved && !entity.DashCharging)
            {
                dashingDesiredDirection = disposition;
            }
            if (entity.Dashing)
            {
                tangoDeadbanded = false;
            }
        }
    }

    protected override void SetTangoParameters()
    {
        if (charged)
        {
            tangoStrafeEnabled = true;
            tangoInnerRange = entity.personalBox.radius * entity.scaleActual * 2;
            tangoOuterRange = sensoryBaseRange * sensorySightRangeScalar * 0.5f;
        }
        else
        {
            tangoStrafeEnabled = false;
            tangoInnerRange = tangoOuterRange = 0;
        }
    }

}

