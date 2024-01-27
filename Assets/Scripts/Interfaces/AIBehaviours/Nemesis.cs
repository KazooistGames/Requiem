using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nemesis : AIBehaviour
{
    public Hellfire hellfire;
    public _Flames flames;

    private float timeToRecoverFromDash = 0;
    private float finalDashRecoveryTime = 3;
    private float quickDashRecoveryTime = 1.5f;

    private float beamDelayPeriod = 2;
    private float beamDuration = 4;

    private float beamDurationTimer = 0;
    private float beamDelayTimer = 0;

    private bool beamONS = true;
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
        sensoryBaseRange = 2f;
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
        entity.Foe = Player.INSTANCE.HostEntity;
        playEvilLaugh(0.5f);
    }

    protected override void Update()
    {
        base.Update();
        if (entity.Foe)
        {
            //BattleCycle = entity.Posture == Entity.PostureStrength.Weak ? Cycle.BeamCycle : Cycle.DashCycle;
            switch (BattleCycle)
            {
                case Cycle.DashCycle:
                    dashCycleUpdates();
                    break;
                case Cycle.BeamCycle:
                    beamCycleUpdates();
                    break;
            }
            if (entity.Dashing)
            {
                tangoDeadbanded = false;
                flames.particleLight.intensity = 8;
            }
            else
            {
                flames.particleLight.intensity = 5;
            }
            string key = "Beam!";
            if (hellfire.form == Hellfire.Form.Beam)
            {
                entity.modSpeed[key] = -0.9f;
                entity.modTurnSpeed[key] = -0.9f;
            }
            else
            {
                entity.modSpeed[key] = 0f;
                entity.modTurnSpeed[key] = 0f;
            }
        }
        else
        {
            beamDurationTimer = 0;
            beamDelayTimer = 0;
            hellfire.form = Hellfire.Form.Off;
            flames.particleLight.intensity = 5;
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
        else if (entity.Posture == Entity.PostureStrength.Weak)
        {
            BattleCycle = Cycle.BeamCycle;
        }

    }

    private void beamCycleUpdates()
    {

        if(beamDelayTimer < beamDelayPeriod)
        {
            beamONS = true;
            beamDelayTimer += Time.deltaTime;
            hellfire.form = Hellfire.Form.Preheat;
            if (entity.Posture == Entity.PostureStrength.Strong)
            {
                BattleCycle = Cycle.DashCycle;
            }
        }
        else if(beamDurationTimer < beamDuration)
        {
            if (beamONS)
            {
                beamONS = false;
                playGroan(0.5f);
            }
            beamDurationTimer += Time.deltaTime;
            hellfire.form = Hellfire.Form.Beam;
        }
        else
        {
            beamDelayTimer = 0;
            beamDurationTimer = 0;
        }
    }

    private GameObject playEvilLaugh(float pitch)
    {
        GameObject sound = _SoundService.PlayAmbientSound("Audio/ambience/ambience2", transform.position, pitch, 1.5f, _SoundService.Instance.DefaultAudioRange, soundSpawnCallback: sound => sound.layer = Requiem.layerEntity);
        sound.transform.SetParent(Player.INSTANCE.transform);
        return sound;
    }

    private GameObject playGroan(float pitch)
    {
        GameObject sound = _SoundService.PlayAmbientSound("Audio/ambience/ambience1", transform.position, pitch, 1.0f, _SoundService.Instance.DefaultAudioRange, soundSpawnCallback: sound => sound.layer = Requiem.layerEntity);
        sound.transform.SetParent(transform);
        return sound;
    }

}

