using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Champion : AIBehaviour
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
        ReflexRate = 0.05f;
        tangoStrafeEnabled = false;
        tangoStrafePauseFreq = 0;
        martialReactiveAttack = true;
        martialReactiveDefendThrow = true;
        martialPreferredState = martialState.none;
        dashingCooldownPeriod = 0.5f;
        dashingChargePeriod = 0.5f;
        itemManagementSeekItems = true;
        itemManagementGreedy = true;
        sensorySightRangeScalar = 2.0f;
        new GameObject().AddComponent<Spear>().PickupItem(entity);
    }

    protected override void Update()
    {
        base.Update();
        updateFlames(_Flames.FlameStyles.Inferno);
        switch (State)
        {
            case AIState.none:
                StateTransition(AIState.passive);
                break;
            case AIState.passive:
                if (entity.Foe)
                {
                    StateTransition(AIState.aggro);
                }
                break;
            case AIState.aggro:
                if (!entity.Foe)
                {
                    StateTransition(AIState.passive);
                }
                else
                {
                    ReflexRate = 0.05f;
                    Weapon wep = entity.MainHand ? entity.MainHand.GetComponent<Weapon>() : null;
                    bool inRange = entity.Foe && wep ? (wep.Range) >= (entity.Foe.transform.position - transform.position).magnitude : false;
                    //bool defensive = (wep ? wep.Defending : true);
                    //bool offensive = (wep ? wep.WindingUp || wep.WindingUp : false);
                    //dashingInitiate = defensive && inRange;
                    dashingDodgeAttacks = entity.Posture != Entity.PostureStrength.Strong;
                    martialReactiveDefend = !dashingDodgeAttacks;
                    dashingLunge = true;
                    dashingPower = dashingLunge ? 0.25f : 1.0f;
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
                //mainHandFlame.setFlamePreset(mainWep.Attacking || mainWep.WindingUp || mainWep.WindingUp ? flameLevel : 0);
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
                //offHandFlame.setFlamePreset(offWep.Attacking || offWep.WindingUp || offWep.WindingUp ? flameLevel : 0);
            }
        }
        else
        {
            offHandFlame.transform.SetParent(transform);
            offHandFlame.SetFlamePresentation(0);
        }
    }

}

