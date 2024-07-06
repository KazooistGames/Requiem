using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Champion : AIBehaviour
{

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        Intelligence = 1.0f;
        ReflexRate = 0.05f;
        tangoStrafeEnabled = false;
        tangoStrafePauseFreq = 0;
        //martialReactiveAttack = true;
        //martialReactiveDefendThrow = true;
        martialPreferredState = martialState.none;
        //dashingCooldownPeriod = 0.5f;
        dashingChargePeriod = 0.5f;
        itemManagementSeekItems = true;
        itemManagementGreedy = true;
        sensorySightRangeScalar = 2.0f;
        new GameObject().AddComponent<Spear>().PickupItem(entity);
    }

    protected override void Update()
    {
        base.Update();
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

}

