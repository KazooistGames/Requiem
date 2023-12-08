using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biter : AIBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        entity = GetComponent<Entity>() ? GetComponent<Entity>() : gameObject.AddComponent<Entity>();

    }
    protected override void Start()
    {
        base.Start();
        Intelligence = 0.5f;
        tangoStrafeEnabled = true;
        dashingChargePeriod = 1.0f;
        grabDPS = 10f;
        sensorySightRangeScalar = 1.0f;
        meanderPauseFrequency = 0.75f;
        itemManagementSeekItems = false;
        pursueStoppingDistance = sensoryBaseRange * sensorySightRangeScalar * 0.5f;
        grabEnabled = false;
    }

    protected override void Update()
    {
        base.Update();
        if (entity.Foe)
        {
            Vector3 disposition = entity.Foe.transform.position - transform.position;
            bool charged = dashingCooldownTimer >= 3;
            pursueStoppingDistance = sensoryBaseRange * sensorySightRangeScalar * (charged ? 0.3f : 0.5f);
            bool inPosition = disposition.magnitude < pursueStoppingDistance || entity.DashCharging;
            dashingChargePeriod = 0.5f;
            if (charged && inPosition && !entity.Shoved)
            {
                dashingDesiredDirection = disposition;
            }

        }
    }

}

