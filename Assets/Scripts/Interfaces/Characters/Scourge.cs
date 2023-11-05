using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Character;

public class Scourge : Character
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
        Intelligence = 0.5f;
        tangoPeriodScalar = 1f;
        tangoStrafePauseFreq = 0f;
        martialReactiveDefend = true;
        martialReactiveAttack = false;
        martialPreferredState = martialState.throwing;
        itemManagementSeekItems = true;
        itemManagementGreedy = true;
        dashingPower = 1.0f;
        dashingDodgeAttacks = false;
        sensorySightRangeScalar = 1.5f;
        new GameObject().AddComponent<Handaxe>().PickupItem(entity);
        new GameObject().AddComponent<Handaxe>().PickupItem(entity);
    }

    protected override void Update()
    {
        base.Update();
        updateFlames(_Flames.FlameStyles.Soulless);
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
                    waypointCoordinates = transform.position;
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
                    Weapon wep = entity.MainHand ? entity.MainHand.GetComponent<Weapon>() : null;
                    //bool inRange = entity.Foe && wep ? (wep.Range) >= (entity.Foe.transform.position - transform.position).magnitude : false;
                    //bool defensive = (wep ? wep.Defending : true);
                    //bool offensive = (wep ? wep.WindingUp : false);
                    tangoStrafeEnabled = false;
                    //dashingCooldownPeriod = 0.5f;
                    //entity.dashMaxVelocity = defensive ? 5.0f : 2.5f;
                    //dashingDelayPeriod = defensive ? 1.0f : 0.0f;
                    //dashingCooldownPeriod = defensive ? 2.0f : (offensive ? 0.5f : 1.0f);
                    //dashingDodgeFoe = !defensive;
                    //dashingDodgeAim = !defensive;
                    //dashingInitiate = defensive;
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
        Destroy(footFlame.gameObject);
        Destroy(mainHandFlame.gameObject);
        Destroy(offHandFlame.gameObject);
    }

    private void updateFlames(_Flames.FlameStyles flameLevel)
    {
        if (footFlame.transform.parent != entity.transform)
        {
            footFlame.transform.SetParent(entity.transform, false);
            footFlame.shapeModule.shapeType = ParticleSystemShapeType.Donut;
            footFlame.shapeModule.position = Vector3.down * 0.6f;
        }
        else
        {
            footFlame.SetFlamePresentation(flameLevel);
        }

        if (entity.MainHand ? entity.MainHand.GetComponent<Weapon>() : false)
        {
            if (mainHandFlame.transform.parent != entity.MainHand.transform)
            {
                mainHandFlame.transform.SetParent(entity.MainHand.transform, false);
                mainHandFlame.transform.localPosition = Vector3.up * 0.25f;
                mainHandFlame.transform.localEulerAngles = Vector3.zero;
                mainHandFlame.transform.localScale = Vector3.one * 0.5f;
                mainHandFlame.shapeModule.shapeType = ParticleSystemShapeType.Mesh;
                mainHandFlame.shapeModule.mesh = entity.MainHand.GetComponent<MeshFilter>().sharedMesh;
            }
            else
            {
                Weapon mainWep = entity.MainHand.GetComponent<Weapon>();
                //mainHandFlame.setFlamePreset(mainWep.Attacking || mainWep.Throwing ? flameLevel : 0);
            }
        }
        else
        {
            mainHandFlame.transform.SetParent(transform);
            mainHandFlame.SetFlamePresentation(0);
        }

        if (entity.OffHand ? entity.OffHand.GetComponent<Weapon>() : false)
        {

            if (offHandFlame.transform.parent != entity.OffHand.transform)
            {
                offHandFlame.transform.SetParent(entity.OffHand.transform, false);
                offHandFlame.transform.localPosition = Vector3.up * 0.25f;
                offHandFlame.transform.localEulerAngles = Vector3.zero;
                offHandFlame.transform.localScale = Vector3.one * 0.5f;
                offHandFlame.shapeModule.shapeType = ParticleSystemShapeType.Mesh;
                offHandFlame.shapeModule.mesh = entity.OffHand.GetComponent<MeshFilter>().sharedMesh;
            }
            else
            {
                Weapon offWep = entity.OffHand.GetComponent<Weapon>();
                //offHandFlame.setFlamePreset(offWep.Attacking || offWep.Throwing ? flameLevel : 0);
            }
        }
        else
        {
            offHandFlame.transform.SetParent(transform);
            offHandFlame.SetFlamePresentation(0);
        }
    }

}
