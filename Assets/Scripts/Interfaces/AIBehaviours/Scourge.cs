using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AIBehaviour;

public class Scourge : AIBehaviour
{

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        Intelligence = 0.5f;
        tangoPeriodScalar = 1f;
        tangoStrafePauseFreq = 0f;
        martialPreferredState = martialState.throwing;
        itemManagementSeekItems = true;
        itemManagementGreedy = true;
        sensorySightRangeScalar = 1.5f;
        new GameObject().AddComponent<Handaxe>().PickupItem(entity);
        new GameObject().AddComponent<Handaxe>().PickupItem(entity);
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

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }


}
