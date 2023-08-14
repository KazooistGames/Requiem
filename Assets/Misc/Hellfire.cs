using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Hellfire : MonoBehaviour
{
    public bool debug = false;
    public enum Form
    {
        none = 0,
        Beam = 1,
        Blast = 2,
        Wield = 3,
        ForceField = 4,
    }

    public Form form = Form.none;
    public Player Wielder;
    public bool Wielded = false;
    public bool Depleted = false;
    public Vector3 CrownOffset = new Vector3(0, 1.2f, 0);
    public Vector3 CrownAngle = Vector3.zero;
    public bool PrimaryTrigger = false;
    public bool SecondaryTrigger = false;
    public bool TertiaryTrigger = false;
    public float Guzzle;
    public float Heat;
    public float Juice = 0;
    private float thrust;

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

    private float primaryTimer = 0f;
    private float primaryPeakVelocity = 12f;
    private List<GameObject> primaryHits = new List<GameObject>();
    public Gradient primaryColorGradient = new Gradient();
    public AnimationCurve primarySpeedCurve;
    public float primaryRange;
    private bool primaryLatch = false;

    private bool secondaryChargeLatch = false;
    private float secondaryTimer = 0f;
    private float secondaryPeriod = 1f;
    private bool secondaryCharged = false;
    private bool secondaryReleaseLatch = false;
    private List<GameObject> secondaryHits = new List<GameObject>();
    private Gradient secondaryColorGradient = new Gradient();
    public Color secondaryColorFirstKey;
    public Color secondaryColorSecondKey;
    public Color secondaryColorThirdKey;

    private bool tertiaryTriggerLatch = false;
    public bool tertiaryActiveLatch = false;

    private List<GameObject> tertiaryHits;
    private Weapon tertiaryItem;

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
        gameObject.name = "Halo";
        Guzzle = 5f;
        Heat = 100f;
        thrust = 5f;
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
        primaryColorGradient = new Gradient();
        primaryColorGradient.SetKeys
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
        secondaryColorFirstKey = new Color(1.0f, 1.0f, 1.0f);
        secondaryColorSecondKey = new Color(0.5f, 0.7f, 1.0f);
        secondaryColorThirdKey = new Color(0.55f, 0f, 1f);

    }

    void Update()
    {
        Juice = Mathf.Clamp(Juice, 0, 100);
        main.startSize = Mathf.Lerp(0.05f, 0.1f, Juice / 100);
        if (Wielder)
        {
            crownWielder();
            if (Juice <= 0)
            {
                Depleted = true;
                if (debug)
                {
                    Juice = 100f;
                }
            }
            else if (Juice == 100f)
            {
                Depleted = false;
            }
        }

    }

    void FixedUpdate()
    {
        if (Wielder)
        {
            switch (form)
            {
                case Form.none:
                    idleFunction();
                    break;
                case Form.Beam:
                    beam();
                    break;
                case Form.Blast:
                    blast();
                    break;
                case Form.Wield:
                    wield();
                    break;
                case Form.ForceField:
                    forceField();
                    break;
            }
        }
    }


    void OnParticleCollision(GameObject other)
    {
        Character entity = other.GetComponent<Character>();
        Rigidbody body = other.GetComponent<Rigidbody>();
        Vector3 disposition = (other.transform.position - transform.position);
        if (PrimaryTrigger)
        {
            List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
            ParticlePhysicsExtensions.GetCollisionEvents(particles, other, collisionEvents);
            float range = 0;
            foreach (ParticleCollisionEvent colEvent in collisionEvents)
            {
                range += (colEvent.intersection - Wielder.transform.position).magnitude;
            }
            range /= collisionEvents.Count;
            if(range > primaryRange)
            {
                primaryRange = range;
            }
        }
    }

    ////////// particle system manipulation ////////////

    private void idleFunction()
    {
        primaryTimer = 0f;
        primaryRange = 0f;
        main.startLifetime = 0.5f;
        main.startSpeed = 0f;
        main.maxParticles = 1500;
        main.startColor = Color.white;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        velocity.xMultiplier = 0;
        velocity.zMultiplier = 0;
        velocity.yMultiplier = Depleted ? 0 : 0.5f;
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
        shape.arcSpeed = Depleted ? 10 : idleArcSpeed;
        shape.arcMode = Depleted ? ParticleSystemShapeMultiModeValue.Random : ParticleSystemShapeMultiModeValue.Random;
        shape.radiusThickness = 0f;
        shape.radius = Mathf.Lerp(0.025f, 0.05f, Juice/100f);
        shape.scale = Vector3.one;
        shape.rotation = Vector3.right * 90f;
        lights.ratio = 0.1f;
        lights.maxLights = 250;
        emission.rateOverTime = 250;
        emission.enabled = true;
        size.sizeMultiplier = Wielded ? 0.75f : 0.5f;
        trails.widthOverTrail = 0.5f;
        collision.collidesWith = ~(1 << Wielder.gameObject.layer);
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
        //Requiem.Players.Find(x => x == Wielder).manualAimingTriggers["hellbeam"] = Wielded;    
        if (Wielded && ! Depleted)
        {
            CrownOffset = new Vector3(0, 0, 0.5f);
            CrownAngle = new Vector3(-90, 0, 0);
        }
        else
        {
            CrownOffset = new Vector3(0, 1.2f, 0);
            CrownAngle = new Vector3(0, 0, 0);
        }
        if (PrimaryTrigger && !Depleted)
        {
            CrownOffset = new Vector3(0, 0, 0.3f);
            CrownAngle = new Vector3(-90, 0, 0);
            primaryHits = new List<GameObject>();
            primaryLatch = true;
            Heat = 100f;
            Guzzle = 20f;
            Juice -= Guzzle * Time.fixedDeltaTime;
            primaryTimer += Time.fixedDeltaTime*3;
            main.startLifetime = Mathf.Lerp(0.4f, 0.5f, primaryTimer);
            main.startSpeed = Mathf.Lerp(0.0f, primaryPeakVelocity, primaryTimer);
            shape.angle = Mathf.Lerp(90, 0, primaryTimer*3);
            size.sizeMultiplier = Mathf.Lerp(0.75f, 0.25f, primaryTimer);
            ParticleSystem.MinMaxCurve velocityCurve = velocity.speedModifier;
            velocityCurve.mode = ParticleSystemCurveMode.Curve;
            velocityCurve.curve = primarySpeedCurve;
            velocity.speedModifier = velocityCurve;
            velocity.speedModifierMultiplier = 2.0f;
            ParticleSystem.MinMaxGradient gradient = color.color;
            gradient.mode = ParticleSystemGradientMode.Gradient;
            gradient.gradient = primaryColorGradient;
            color.color = gradient;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.arcSpeed = 5f;
            shape.arcMode = ParticleSystemShapeMultiModeValue.Random;
            shape.radiusThickness = 1f;
            shape.radius = Mathf.Lerp(0.00f, 0.05f, Juice/100f);
            lights.ratio = 0.1f;
            lights.maxLights = 250;
            emission.rateOverTime = 750;
            emission.enabled = true;
            trails.widthOverTrail = 2;
            trails.dieWithParticles = false;
            collision.dampen = 0.5f;
            collision.bounce = 0f;
            collision.sendCollisionMessages = true;
            collision.lifetimeLossMultiplier = 0.25f;
            RaycastHit castHit;
            Ray castRay = new Ray(Wielder.transform.position, Wielder.HostEntity.LookDirection);
            float castRadius = shape.radius + (main.startSize.constant * size.sizeMultiplier/2);
            Debug.DrawLine(castRay.origin, transform.position + castRay.direction * primaryRange, Color.blue);
            if (Physics.SphereCast(castRay, castRadius, out castHit, primaryRange, (1 << Game.layerEntity) + (1 << Game.layerObstacle) + (1 << Game.layerWall), QueryTriggerInteraction.Ignore))
            {
                GameObject other = castHit.collider.gameObject;
                Character entity = other.GetComponent<Character>();
                if (entity ? entity.Allegiance != Wielder.HostEntity.Allegiance : false)
                {
                    primaryHits.Add(other);
                    Vector3 disposition = other.transform.position - transform.position;
                    float damage = Heat * Time.fixedDeltaTime;
                    Vector3 forceVector = (disposition.normalized / disposition.magnitude) * (Juice / 100f);
                    entity.Damage(damage);
                    entity.Shove(forceVector * thrust * Time.fixedDeltaTime);
                }
            }
        }
        else if(primaryLatch)
        {
            if(particles.particleCount == 0)
            {
                emission.enabled = true;
                primaryLatch = false;
            }
            else
            {
                idleFunction();
                emission.enabled = false;
            }
        }
        else
        {
            idleFunction();
        }     
    }


    private void blast()
    {
        Heat = 100f;
        Guzzle = 10f;
        if ((SecondaryTrigger || secondaryChargeLatch) && !secondaryReleaseLatch && particles.particleCount >= 0.5f * main.startLifetime.constant * emission.rateOverTime.constant && !secondaryCharged)
        {
            secondaryChargeLatch = true;
            secondaryReleaseLatch = false;
            if (secondaryTimer >= secondaryPeriod)
            {
                secondaryCharged = true;
            }
            else
            {
                secondaryTimer += Time.fixedDeltaTime;
            }
            Juice -= Guzzle * Time.fixedDeltaTime;
            float ratio = secondaryTimer / secondaryPeriod;
            main.startLifetime = Mathf.Lerp(0.25f, 1f, ratio);
            shape.radius = Mathf.Lerp(0.04f, 0.04f + 0.12f * Juice, ratio);
            size.sizeMultiplier = Mathf.Lerp(1.0f, 2.0f, ratio);
            secondaryColorGradient.SetKeys
            (
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.Lerp(idleColorFirstKey, secondaryColorFirstKey, ratio), 0.0f),
                    new GradientColorKey(Color.Lerp(idleColorSecondKey,secondaryColorSecondKey, ratio), 0.7f),
                    new GradientColorKey(Color.Lerp(idleColorThirdKey, secondaryColorThirdKey, ratio), 1.0f),
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.5f, 0.0f),
                    new GradientAlphaKey(0.25f, 0.7f),
                    new GradientAlphaKey(0.75f, 1.0f)
                }
            );
            ParticleSystem.MinMaxGradient gradient = color.color;
            gradient.mode = ParticleSystemGradientMode.Gradient;
            gradient.gradient = secondaryColorGradient;
            color.color = gradient;
            main.startSpeed = 0f;
            trails.widthOverTrail = 0;
            shape.arcSpeed = 1f;
            shape.arcMode = ParticleSystemShapeMultiModeValue.Random;
            shape.radiusThickness = 1f;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            collision.dampen = 0.5f;
            collision.bounce = 0f;      
        }
        else if(secondaryCharged)
        {
            emission.enabled = false;
            secondaryChargeLatch = false;
            secondaryCharged = false;
            secondaryReleaseLatch = true;
            velocity.xMultiplier = Mathf.Sin(Mathf.Deg2Rad * Wielder.transform.eulerAngles.y) * 8f;
            velocity.zMultiplier = Mathf.Cos(Mathf.Deg2Rad * Wielder.transform.eulerAngles.y) * 8f;
        }
        else if(secondaryReleaseLatch && particles.particleCount == 0)
        {
            secondaryHits = new List<GameObject>();
            secondaryTimer = 0f;
            secondaryReleaseLatch = false;
            emission.enabled = true;
        }
        else if(emission.enabled)
        {
            idleFunction();
        }         
    }

    private void wield()
    {
        Heat = 50f;
        Guzzle = 2.5f;
        if (Depleted)
        {
            wieldExit();
        }
        else if (TertiaryTrigger)
        {
            tertiaryTriggerLatch = true;
        }
        else
        {
            if (tertiaryTriggerLatch)
            {
                tertiaryTriggerLatch = false;
                tertiaryHits = new List<GameObject>();
                if (!tertiaryActiveLatch)
                {
                    tertiaryActiveLatch = true;
                    main.maxParticles = 5000;
                    emission.rateOverTime = 5000;
                    if (Wielder.HostEntity.rightStorage)
                    {
                        //tertiaryItem = Wielder.HostEntity.RightHand;
                        main.startLifetime = 0.1f;
                        shape.shapeType = ParticleSystemShapeType.Mesh;
                        shape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
                        shape.scale = tertiaryItem.transform.localScale*Wielder.HostEntity.scaleActual;
                        shape.mesh = Wielder.HostEntity.rightStorage.GetComponent<MeshFilter>().mesh;
                        shape.rotation = Vector3.zero;
                        trails.mode = ParticleSystemTrailMode.Ribbon;
                    }
                    else
                    {
                        transform.localPosition = Vector3.zero;
                        main.startLifetime = 0.2f;
                        shape.shapeType = ParticleSystemShapeType.Sphere;
                        shape.radius = Wielder.HostEntity.berthActual;
                        shape.radiusThickness = 0f;
                    }
                }
                else
                {
                    wieldExit();
                }
            }
            else if(tertiaryActiveLatch)
            {
                if (Wielder.HostEntity.rightStorage)
                {
                    if(tertiaryItem != Wielder.HostEntity.rightStorage)
                    {
                        wieldExit();
                    }
                    //else if (Wielder.HostEntity.RightHand.Attacking)
                    //{
                    //}
                    else
                    {
                        tertiaryHits = new List<GameObject>();
                    }
                    transform.localPosition = Vector3.Lerp(transform.localPosition, tertiaryItem.transform.localPosition, 0.15f);
                    transform.localEulerAngles = tertiaryItem.transform.localEulerAngles;
                }
                else if(transform.parent == Wielder.transform)
                {
                    tertiaryHits = new List<GameObject>();
                }
                else
                {
                    wieldExit();
                }
            }
        }
    }
    private void wieldExit()
    {
        tertiaryActiveLatch = false;
        transform.parent = Wielder.transform;
        main.maxParticles = 1500;
        velocity.radial = 0f;
        trails.mode = ParticleSystemTrailMode.PerParticle;
    }

    private void forceField()
    {

    }

    public void crownWielder()
    {
        transform.SetParent(Wielder.transform);
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.Lerp(transform.localPosition, CrownOffset * Wielder.HostEntity.scaleActual, 0.15f);
        transform.localEulerAngles = CrownAngle;
    }

}



   
