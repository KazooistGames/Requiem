using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.IO.LowLevel.Unsafe;

public class Hellfire : MonoBehaviour
{
    public Entity Wielder;
    public float DPS = 50;
    public float Thrust = 20;

    public enum Form
    {
        Off,
        Preheat,
        Beam,
    }

    public Form form = Form.Off;
    public bool FireTrigger = false;
    public float Juice = 0;

    protected ParticleSystem particles;
    protected ParticleSystem.VelocityOverLifetimeModule velocity;
    protected ParticleSystem.MainModule main;
    protected ParticleSystem.ColorOverLifetimeModule color;
    protected ParticleSystem.ShapeModule shape;
    protected ParticleSystem.LightsModule lights;
    protected ParticleSystem.EmissionModule emission;
    protected ParticleSystem.TrailModule trails;
    protected ParticleSystem.SizeOverLifetimeModule size;
    protected ParticleSystem.CollisionModule collision;

    public Gradient idleColorGradient = new Gradient();
    public ParticleSystemShapeMultiModeValue idleArcMode = ParticleSystemShapeMultiModeValue.Random;
    public float idleArcSpeed = 1;
    public Color idleColorFirstKey;
    public Color idleColorSecondKey;
    public Color idleColorThirdKey;

    private float beamSpinupTimer = 0f;
    private float beamPeakVelocity = 12f;
    public Gradient beamColorGradient = new Gradient();
    public AnimationCurve beamSpeedCurve;
    public float beamRange;

    public float sputterAttemptPeriod = 0.5f;
    public float sputterAttemptTimer;
    public float sputterChance = 0.5f;
    public float sputterResetValue = 0.5f;

    private GameObject beamSound;

    void Awake()
    {
        particles = GetComponent<ParticleSystem>();
        velocity = particles.velocityOverLifetime;
        main = particles.main;
        color = particles.colorOverLifetime;
        shape = particles.shape;
        lights = particles.lights;
        emission = particles.emission;
        trails = particles.trails;
        size = particles.sizeOverLifetime;
        collision = particles.collision;
    }

    void Start()
    {
        gameObject.name = "Hellfire";
        idleColorFirstKey = new Color(1.0f, 0.7f, 0.0f);
        idleColorSecondKey = new Color(1.0f, 0.5f, 0.5f);
        idleColorThirdKey = new Color(1.0f, 0.7f, 0.0f);
        idleColorGradient = new Gradient();
        idleColorGradient.SetKeys
        (
            new GradientColorKey[]
            {
                new GradientColorKey(idleColorFirstKey, 0.0f),
                new GradientColorKey(idleColorSecondKey, 0.8f),
                new GradientColorKey(idleColorThirdKey, 1.0f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.25f, 0.0f),
                new GradientAlphaKey(0.2f, 0.8f),
                new GradientAlphaKey(0.1f, 1.0f)
            }
        );
        beamColorGradient = new Gradient();
        beamColorGradient.SetKeys
            (
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(1.0f, 0.75f, 0.5f), 0.0f),
                    new GradientColorKey(new Color(1.0f, 0.5f, 0.0f), 0.5f),
                    new GradientColorKey(new Color(1.0f, 0.5f, 0.5f), 1.0f),
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.75f, 0.0f),
                    new GradientAlphaKey(0.5f, 0.9f),
                    new GradientAlphaKey(0.75f, 1.0f)
                }
            );

    }

    void Update()
    {
        if((sputterAttemptTimer += Time.deltaTime) >= sputterAttemptPeriod)
        {
            sputterAttemptTimer = 0;
            if (Random.value <= sputterChance)
            {
                beamSpinupTimer = sputterResetValue;
            }
        }
        //form = Player.INSTANCE.CurrentKeyboard.rKey.isPressed ? Form.Beam : Form.Preheat;
    }

    void FixedUpdate()
    {
        switch (form)
        {
            case Form.Off:
                idleFunction();
                break;
            case Form.Preheat:
                preheat();
                break;
            case Form.Beam:
                beam();
                break;
        }
    }



    /***** PUBLIC *****/


    /***** PROTECTED *****/


    /***** PRIVATE *****/

    private void idleFunction()
    {
        emission.enabled = false;
        if (beamSound)
        {
            Destroy(beamSound);
        }
    }

    private void preheat()
    {
        if (beamSound)
        {
            Destroy(beamSound);
        }
        beamSpinupTimer = 0f;
        main.startLifetime = 0.5f;
        main.startSpeed = 0f;
        main.maxParticles = 1500;
        main.startColor = Color.white;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        velocity.xMultiplier = 0;
        velocity.zMultiplier = 0;
        velocity.yMultiplier = 0.5f;
        velocity.radial = 0f;
        ParticleSystem.MinMaxCurve velocityCurve = velocity.speedModifier;
        velocityCurve.mode = ParticleSystemCurveMode.Constant;
        velocity.speedModifier = velocityCurve;
        velocity.speedModifierMultiplier = 1;
        ParticleSystem.MinMaxGradient gradient = color.color;
        gradient.mode = ParticleSystemGradientMode.Gradient;
        gradient.gradient = idleColorGradient;
        color.color = gradient;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.arcSpeed = idleArcSpeed;
        shape.arcMode = ParticleSystemShapeMultiModeValue.Random;
        shape.radiusThickness = 0f;
        shape.radius = Mathf.Lerp(0.025f, 0.05f, Juice / 100f);
        shape.scale = Vector3.one;
        shape.rotation = Vector3.up * -90f;
        lights.ratio = 0.1f;
        lights.maxLights = 250;
        emission.rateOverTime = 250;
        emission.enabled = true;
        size.sizeMultiplier = 0.75f;
        trails.widthOverTrail = 0.5f;
        collision.collidesWith = int.MaxValue;
        collision.colliderForce = 0f;
        collision.multiplyColliderForceByCollisionAngle = false;
        collision.multiplyColliderForceByParticleSize = false;
        collision.multiplyColliderForceByParticleSpeed = false;
        collision.dampen = 0.5f;
        collision.bounce = 0f;
        collision.sendCollisionMessages = false;
    }

    private void beam()
    {
        if (!beamSound)
        {
            beamSound = playBeamSound(0.5f);
        }
        if(beamSpinupTimer < 1)
        {
            beamSpinupTimer += Time.fixedDeltaTime * 3;
            main.startLifetime = Mathf.Lerp(0.3f, 0.5f, beamSpinupTimer);
            main.startSpeed = Mathf.Lerp(0.0f, beamPeakVelocity, beamSpinupTimer);
            shape.angle = Mathf.Lerp(90, 0, beamSpinupTimer * 3);
            size.sizeMultiplier = Mathf.Lerp(0.75f, 0.25f, beamSpinupTimer);
            ParticleSystem.MinMaxCurve velocityCurve = velocity.speedModifier;
            velocityCurve.mode = ParticleSystemCurveMode.Curve;
            velocityCurve.curve = beamSpeedCurve;
            velocity.speedModifier = velocityCurve;
            velocity.speedModifierMultiplier = 2.0f;
            ParticleSystem.MinMaxGradient gradient = color.color;
            gradient.mode = ParticleSystemGradientMode.Gradient;
            gradient.gradient = beamColorGradient;
            color.color = gradient;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.arcSpeed = 5f;
            shape.arcMode = ParticleSystemShapeMultiModeValue.Random;
            shape.radiusThickness = 1f;
            shape.radius = Mathf.Lerp(0.00f, 0.05f, Juice / 100f);
            shape.rotation = Vector3.up * -90f;
            lights.ratio = 0.1f;
            lights.maxLights = 250;
            emission.rateOverTime = 750;
            emission.enabled = true;
            trails.widthOverTrail = 2;
            trails.dieWithParticles = false;
            collision.dampen = 0.75f;
            collision.bounce = 0f;
            collision.sendCollisionMessages = false;
            collision.lifetimeLossMultiplier = 0.5f;
        }
        else
        {
            RaycastHit hit;
            Ray ray = new Ray();
            ray.origin = transform.position;
            ray.direction = Wielder.LookDirection;
            Debug.DrawLine(ray.origin, ray.origin + ray.direction.normalized * 10, Color.white, 0.5f);
            int hitMask = (1 << Requiem.layerEntity) + (1 << Requiem.layerObstacle) + (1 << Requiem.layerWall);
            if (Physics.Raycast(ray, out hit, 10, hitMask, queryTriggerInteraction: QueryTriggerInteraction.Ignore))
            {
                Entity foe = hit.collider.gameObject.GetComponent<Entity>();
                if (foe ? foe.Allegiance != Wielder.Allegiance : false)
                {
                    foe.applyDamageToPoiseThenVitality(DPS * Time.fixedDeltaTime, silent: true);
                    foe.body.AddForce(ray.direction.normalized * Thrust * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
            }
        }
    }

    private GameObject playBeamSound(float pitch)
    {
        GameObject sound = _SoundService.PlayAmbientSound("Audio/wretch", transform.position, pitch, 1.5f, _SoundService.Instance.DefaultAudioRange, soundSpawnCallback: sound => sound.layer = Requiem.layerEntity);
        sound.GetComponent<AudioSource>().time = 0.5f;
        sound.transform.SetParent(transform);
        return sound;
    }


}



   
