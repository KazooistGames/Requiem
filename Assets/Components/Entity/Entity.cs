using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public abstract class Entity : MonoBehaviour
{
    public static UnityEvent<Entity> EntityVanquished = new UnityEvent<Entity> { };

    public static List<Type> StandardTypes = new List<Type>()
    {
        typeof(Skelly),
        typeof(Nephalim),
        typeof(Skully),
        typeof(Wraith),
    };

    public bool Mutate = false;
    public bool Mutated = false;

    public Player requiemPlayer;

    public float Strength = 100f;
    public float Resolve = 1.0f;
    public float Haste = 1.0f;

    public float Xp = 0f;
    public int Lvl = 0;

    public float Vitality;
    public float Poise;
    public float Agility;

    public float Tempo { get; private set; } = 0;
    public float TempoTarget = 0;

    private static float POISE_REGEN_PERIOD = 3f;
    private static float POISE_DEBOUNCE_PERIOD = 3f;
    private static float POISE_RESTING_PERCENTAGE = 1f;
    private float poiseDebounceTimer = 0.0f;

    private static float TEMPO_DRAIN_PERIOD = 2f;
    private static float TEMPO_DEBOUNCE_PERIOD = 2f;
    private static float TEMPO_RESTING_VALUE = 0f;
    private float tempoDebounceTimer = 0.0f;

    public bool FinalDash { get; private set; } = false;
    public static float dashMaxVelocity { get; private set; } = 3.0f;
    public static float dashMinVelocity { get; private set; } = 1.0f;
    private static float dashChargeTime = 0.25f;
   
    public static float SpeedScalarGlobal { get; private set; } = 0.5f;
    public static float Scale { get; private set; } = 0.2f;
    public static float Berth { get; private set; } = 0.25f;
    public static float Height { get; private set; } = 1.25f;
    public static float DefaultTurnSpeed { get; private set; } = 900f;
    public static float HoverHeight { get; private set; } = 0.725f;
    protected float HoverHeightLocal = HoverHeight;
    protected float floorHeight;

    public float scaleScalar { get; protected set; } = 1.0f;
    public float berthScalar { get; protected set; } = 1.0f;
    public float heightScalar { get; protected set; } = 0.8f;
    public float hoverScalar { get; protected set; } = 1.0f;

    public float scaleActual { get; private set; }
    public float berthActual { get; private set; }
    public float heightActual { get; private set; }
    public float hoverActual { get; private set; }

    public UnityEvent Vanquished = new UnityEvent();
    public UnityEvent<float> Wounded = new UnityEvent<float>();
    public UnityEvent JustCrashed = new UnityEvent();

    public Entity Foe;

    protected float BaseAcceleration = 8;
    public float SpeedActual { get; private set; } = 0f;
    public float AccelerationActual { get; private set; } = 0f;
    public Dictionary<string, float> modSpeed = new Dictionary<string, float>();
    public Dictionary<string, float> modAcceleration = new Dictionary<string, float>();

    public static RuntimeAnimatorController DefaultAnimController;
    public Vector3 WalkDirection = Vector3.zero;
    public Vector3 LookDirection = Vector3.zero;
    public float TurnSpeed;
    public Dictionary<string, float> modTurnSpeed = new Dictionary<string, float>();

    public Item MainHand;
    public Item OffHand;
    public Item leftStorage;
    public Item rightStorage;
    public Item backStorage;
    public UnityEvent<Entity> Pickup = new UnityEvent<Entity> { };
    public UnityEvent<Entity> Interact = new UnityEvent<Entity> { };
 
    public CapsuleCollider personalBox;
    protected GameObject model;
    protected CapsuleCollider hurtBox;
    public Rigidbody body;
    public Projector indicator;
    public GameObject statBar;
    protected Animator anim;

    private static GameObject indicatorPrefab;
    private static GameObject statBarPrefab;
    private static GameObject bloodSplatter;

    public bool Shoved = false;
    public float shoveRecoveryPeriod = 0;
    private float shoveRecoveryTimer = 0;

    public bool Rebuked = false;
    public bool Stunned = false;
    protected float stunPeriod = 0;
    protected float stunTimer = 0;

    public bool Defending = false;

    public bool DashCharging = false;
    public bool Dashing = false;
    public float DashPower = 0.0f;
    public Vector3 dashDirection = Vector3.zero;
    private static AudioClip dashSound;
    private List<GameObject> dashAlreadyHit = new List<GameObject>();

    public Dictionary<string, (float, float)> BleedingWounds = new Dictionary<string, (float, float)>();

    protected bool CrashEnvironmentONS = true;
    protected static float CRASH_DAMAGE = 25f;
    
    public bool Running = false;

    public GameObject Location;
    public GameObject head;


    public enum Posture
    {
        Stiff = -1,
        Warm = 0,
        Flow = 1,
    }
    public Posture posture = Posture.Warm;

    public enum WieldMode
    {
        none,
        EmptyHanded,
        OneHanders,
        TwoHanders,
        Burdened,
    }
    public WieldMode wieldMode = WieldMode.EmptyHanded;

    public enum Loyalty
    {
        hostile = -1,
        neutral = 0,
        one = 1,
        two = 2,
        three = 3,
        four = 4,
    }
    public Loyalty Allegiance = Loyalty.neutral;

    protected SpiritFlame flames;

    //******************Functions*************************
    protected virtual void Awake()
    {
        hurtBox = gameObject.AddComponent<CapsuleCollider>();
        personalBox = gameObject.AddComponent<CapsuleCollider>();
        body = GetComponent<Rigidbody>() == null ? gameObject.AddComponent<Rigidbody>() : GetComponent<Rigidbody>();
        anim = GetComponent<Animator>() == null ? gameObject.AddComponent<Animator>() : GetComponent<Animator>();
        anim.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        if (!DefaultAnimController)
        {
            DefaultAnimController = Resources.Load<RuntimeAnimatorController>("Animation/entities/EntityDefault/entityDefaultAnimation");
        }
        if (!indicatorPrefab)
        {
            indicatorPrefab = Resources.Load<GameObject>("Prefabs/UX/Indicator");
        }
        if (!statBarPrefab)
        {
            statBarPrefab = Resources.Load<GameObject>("Prefabs/UX/StatBar");
        }
        if (!dashSound)
        {
            dashSound = Game.getSound("Audio/weapons/deepSwing");
        }
    }

    protected virtual void Start()
    {
        Resolve = 1;
        scaleActual = Scale * scaleScalar;
        berthActual = Berth * berthScalar;
        heightActual = Height * heightScalar;
        hoverActual = HoverHeight * hoverScalar;
        personalBox.isTrigger = true;
        personalBox.radius = Mullet.BellCurve(0.7f, 0.1f, 0.5f, 1.0f);
        personalBox.height = heightActual * 1.5f;
        gameObject.layer = Game.layerEntity;
        transform.localScale = Vector3.one * scaleActual;
        transform.localEulerAngles = new Vector3(0, 0, 0);
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.useGravity = true;
        body.constraints = (RigidbodyConstraints)(64 + 32 + 16 + 4);
        hurtBox.center = Vector3.up * 0.5f * scaleActual;
        hurtBox.radius = berthActual;
        hurtBox.height = heightActual;
        Vitality = Strength;
        Poise = Strength;
        TurnSpeed = DefaultTurnSpeed;
        statBar = Instantiate(statBarPrefab, transform);
        indicator = Instantiate(indicatorPrefab, transform).GetComponent<Projector>();
        Material newMaterial = new Material(Shader.Find("Custom/Projection"));
        newMaterial.SetTexture("_ShadowTex", Resources.Load<Texture>("Textures/IndicatorSimple"));
        indicator.material = newMaterial;
        indicator.orthographic = true;
        indicator.orthographicSize = berthActual * scaleActual;
        indicator.transform.parent = transform;
        indicator.enabled = false;
        flames = Instantiate(Game.SpiritFlameTemplate).GetComponent<SpiritFlame>();
        flames.bindToObject(gameObject);
        flames.flamePreset = SpiritFlame.Preset.Magic;
        StartCoroutine(routineDash());
        StartCoroutine(routineTempoTarget());
    }

    protected virtual void Update()
    {
        equipmentManagement();
        indicatorManagement();
        updateStats();
        updatePosture();
        if (Vitality <= 0)
        {
            Physics.autoSimulation = false;
            Physics.Simulate(Time.deltaTime);
            Physics.autoSimulation = true;
            if (Mutate)
            {
                Mutation();
            }
            else
            {
                Die();
            }
        }
        else
        {
            List<string> keys = BleedingWounds.Keys.ToList();
            foreach(string key in keys)
            {
                if(BleedingWounds[key].Item2 <= 0)
                {
                    BleedingWounds.Remove(key);
                }
                else
                {
                    BleedingWounds[key] = (BleedingWounds[key].Item1, BleedingWounds[key].Item2 - Time.deltaTime);
                    Vitality -= BleedingWounds[key].Item1 * Time.deltaTime;
                }
            }
        }
        if (requiemPlayer)
        {
            if (!requiemPlayer.HostEntity)
            {
                requiemPlayer.HostEntity = this;
            }
            Allegiance = requiemPlayer.Faction;
            if (GetComponent<AI>())
            {
                GetComponent<AI>().Enthralled = true;
                GetComponent<AI>().enabled = !requiemPlayer;
                GetComponent<AI>().followVIP = requiemPlayer.HostEntity.gameObject;
            }
        }
        if (!anim.runtimeAnimatorController)
        {
            anim.runtimeAnimatorController = DefaultAnimController;
        }
        else
        {
            animationControls();
        }
    }

    protected virtual void FixedUpdate()
    {
        //float baseSpeed = Mathf.Pow(Haste, 0.5f) * SpeedScalarGlobal;
        float baseSpeed = Haste * SpeedScalarGlobal;
        //modSpeed["flow"] = posture == Posture.Flow ? Resolve / 100 : 0;
        modSpeed["dash"] = (DashPower > 0 && !Running) ? Mathf.Lerp(-0.5f, -0.9f, DashPower) : 0;
        modAcceleration["nonlinear"] = baseSpeed > 0 ? Mathf.Lerp(0.25f, -0.25f, body.velocity.magnitude / baseSpeed ) : 0;
        AccelerationActual = modAcceleration.Values.Aggregate(BaseAcceleration, (result, multiplier) => result *= 1 + multiplier);
        SpeedActual = Agility * SpeedScalarGlobal;
        hurtBox.radius = Shoved ? berthActual * 0.8f : berthActual;
        if (Shoved)
        {
            if (shoveRecoveryTimer >= shoveRecoveryPeriod || (body.velocity.magnitude <= baseSpeed * 0.1f && shoveRecoveryTimer >= 0.1f * shoveRecoveryPeriod))
            {
                CrashEnvironmentONS = true;
                Shoved = false;
                shoveRecoveryPeriod = 0;
                shoveRecoveryTimer = 0;
            }
            else
            {
                shoveRecoveryTimer += Time.fixedDeltaTime;
            }
        }
        else
        {
            shoveRecoveryPeriod = 0;
            shoveRecoveryTimer = 0;
        }
        if (Stunned)
        {
            if (stunTimer >= stunPeriod)
            {
                Stunned = false;
            }
            else
            {
                stunTimer += Time.fixedDeltaTime;
            }
        }
        RaycastHit hoverHit;
        Vector3 hoverRayInit = transform.position;
        hoverRayInit.y = Hextile.Thickness;
        if (Physics.Raycast(hoverRayInit, Vector3.down, out hoverHit, Hextile.Thickness, (1 << Game.layerTile), QueryTriggerInteraction.Ignore))
        {
            floorHeight = hoverHit.point.y + hoverActual * scaleActual;
            Location = hoverHit.collider.gameObject;
        }
        transform.position = new Vector3(transform.position.x, floorHeight, transform.position.z);
        float effectiveAccel = (Shoved && !Dashing) ? AccelerationActual * 0.75f : AccelerationActual;
        Vector3 proposedVelocity;
        bool stopping = (WalkDirection == Vector3.zero || Shoved || Stunned) && body.velocity.magnitude > 0;
        if (stopping)
        {
            Vector3 increment = body.velocity * Time.fixedDeltaTime * effectiveAccel;
            proposedVelocity = increment.magnitude >= body.velocity.magnitude ? Vector3.zero : body.velocity - increment;
            body.velocity = new Vector3(proposedVelocity.x, body.velocity.y, proposedVelocity.z);
        }
        else
        {
            proposedVelocity = body.velocity + (WalkDirection * Time.fixedDeltaTime * effectiveAccel);
            if (new Vector2(proposedVelocity.x, proposedVelocity.z).magnitude > SpeedActual && !Shoved && !Stunned)
            {
                Vector3 normalized = proposedVelocity.normalized * SpeedActual;
                body.velocity = new Vector3(normalized.x, body.velocity.y, normalized.z);
            }
            else
            {
                body.velocity = proposedVelocity;
            }
        }

        if ((Shoved && !Dashing) || Stunned)
        {
            body.freezeRotation = true;
        }
        else
        {
            TurnSpeed = modTurnSpeed.Values.Aggregate(DefaultTurnSpeed, (result, multiplier) => result *= (1 + multiplier));
            LookDirection.y = 0;
            float scaledY = transform.localEulerAngles.y;
            float scaledTarget = 90 - AI.getAngle(LookDirection);
            scaledTarget = scaledTarget > 180 ? scaledTarget - 360 : scaledTarget;
            float difference = (scaledTarget - scaledY);
            float degMax = TurnSpeed * Time.fixedDeltaTime;
            difference = Mathf.Clamp(Mathf.Abs(difference) >= 180 ? difference - (Mathf.Sign(difference) * 360) : difference, -1*degMax, degMax);
            transform.RotateAround(transform.position, Vector3.up, difference);
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!dashAlreadyHit.Contains(collision.gameObject))
        {
            Entity foe = collision.gameObject.GetComponent<Entity>();
            if (Dashing && foe)
            {
                resolveDashHit(collision);
            }
            else if (collision.collider.gameObject.layer == Game.layerWall || collision.collider.gameObject.layer == Game.layerObstacle || foe)
            {
                resolveCrash(collision);
            }
        }        
    }

    protected virtual void OnCollisionStay(Collision collision)
    {
        if (!dashAlreadyHit.Contains(collision.gameObject))
        {
            Entity foe = collision.gameObject.GetComponent<Entity>();
            if (Dashing && foe)
            {
                resolveDashHit(collision, instant: true);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        StopAllCoroutines();
        EntityVanquished.Invoke(this);
    }

    /********** PUBLIC **********/

    public static float Strength_Ratio(Entity A, Entity B)
    {
        if (A && B)
        {
            return Mathf.Sqrt(A.Strength / B.Strength);
        }
        else
        {
            return 1.0f;
        }
    }

    public static void Debone(GameObject bone)
    {
        if (bone)
        {
            Rigidbody body = bone.GetComponent<Rigidbody>() ? bone.GetComponent<Rigidbody>() : bone.AddComponent<Rigidbody>();
            body.useGravity = true;
            if (!bone.GetComponent<Bone>())
            {
                bone.AddComponent<Bone>();
            }
            //Entity entity = bone.transform.parent.parent.GetComponent<Entity>();
            Entity entity = bone.GetComponentInParent<Entity>();
            Animator anim = entity.GetComponent<Animator>();
            Vector3 newScale = bone.transform.localScale * entity.scaleActual;
            Vector3 newPosition = bone.transform.position;
            bone.transform.parent = entity.transform.parent;
            anim.Rebind();
            bone.transform.localScale = newScale;
            bone.transform.position = newPosition;
            bone.layer = Game.layerItem;

            bone.GetComponent<Rigidbody>().velocity = entity.body.velocity;
            if (entity.Shoved)
            {
                Vector3 direction = entity.Foe ? entity.Foe.transform.position - entity.transform.position : new Vector3(UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f);
                bone.GetComponent<Rigidbody>().AddForce(-direction.normalized * 0.75f, ForceMode.VelocityChange);
            }
        }
    }

    public virtual void Damage(float magnitude)
    {
        float poiseDamage = Mathf.Min(Poise, magnitude);
        alterPoise(-poiseDamage);
        float vitalityDamage = posture == Posture.Stiff ? magnitude : magnitude - poiseDamage;
        if (vitalityDamage > 0)
        {
            Vitality -= vitalityDamage;
            Wounded.Invoke(vitalityDamage);        
        }
    }

    public void alterTempo(float value)
    {
        float existingDelta = Tempo - TEMPO_RESTING_VALUE;
        Tempo += value;
        Tempo = Mathf.Clamp(Tempo, 0, 1);
        if (value * existingDelta >= 0)
        {
            tempoDebounceTimer = 0.0f;
        }
        else
        {
            tempoDebounceTimer = TEMPO_DEBOUNCE_PERIOD;
        }
    }


    public void alterPoise(float value)
    {
        bool reducingPoise = value <= 0;
        if (reducingPoise && posture == Posture.Stiff)
        {       
            Rebuke(0.25f + 1.5f * (-value / Strength));
            return;
        }
        float existingDelta = Poise - POISE_RESTING_PERCENTAGE * Strength;
        Poise += value;
        Poise = Mathf.Clamp(Poise, -1, Strength);
        if (value * existingDelta >= 0) 
        {
            poiseDebounceTimer = 0.0f;
        }
        else
        {
            poiseDebounceTimer = POISE_DEBOUNCE_PERIOD;
        }
        updatePosture();
        if (reducingPoise && posture == Posture.Stiff)
        {
            Rebuke(0.25f + 1.5f * (-value / Strength));
        }
    }

    public void Rebuke(float duration)
    {    
        Weapon mainWep = MainHand ? MainHand.GetComponent<Weapon>() : null;
        Weapon offWep = OffHand ? OffHand.GetComponent<Weapon>() : null;
        if (mainWep)
        {
            mainWep.Rebuke(duration, true);
        }
        if (offWep)
        {
            offWep.Rebuke(duration, true);
        }
        
    }

    public void Disarm(float yeetMagnitude = 2)
    {
        updatePosture();
        Weapon mainWep = MainHand ? MainHand.GetComponent<Weapon>() : null;
        Weapon offWep = OffHand ? OffHand.GetComponent<Weapon>() : null;
        if (mainWep)
        {
            mainWep.DropItem(yeet: true, magnitude: yeetMagnitude);
        }
        if (offWep)
        {
            offWep.DropItem(yeet: true, magnitude: yeetMagnitude);
        }      
    }

    public void Shove(Vector3 VelocityChange, bool Dash = false)
    {
        Shoved = true;
        body.AddForce(VelocityChange, ForceMode.VelocityChange);
        float forceDelta = VelocityChange.magnitude;
        shoveRecoveryPeriod += forceDelta / AccelerationActual;
    }
    
    /***** PROTECTED *****/

    protected virtual void Die()
    {
        if (!requiemPlayer)
        {
            Game.KillCount++;
        }
        if (leftStorage)
        {
            leftStorage.DropItem();
        }
        if (rightStorage)
        {
            rightStorage.DropItem();
        }
        if (backStorage)
        {
            backStorage.DropItem();
        }
        if (MainHand)
        {
            MainHand.DropItem();
        }
        if (OffHand)
        {
            OffHand.DropItem();
        }
        Vanquished.Invoke();
        Destroy(gameObject);
    }

    protected virtual void Mutation()
    {
        Mutated = true;
        Mutate = false;
        Stunned = false;
        Vitality = Strength;
        //Poise = Strength;
        if (MainHand)
        {
            MainHand.Primary = false;
            MainHand.Secondary = false;
            MainHand.ThrowTrigger = false;
        }
        if (OffHand)
        {
            OffHand.Primary = false;
            OffHand.Secondary = false;
            OffHand.ThrowTrigger = false;
        }
    }


    protected void BLEED(float damage)
    {
        if (Vitality > 0)
        {
            Mullet.PlayAmbientSound(Game.damageSounds[UnityEngine.Random.Range(0, Game.damageSounds.Length)], transform.position, 1.0f, 0.5f).layer = gameObject.layer;
        }
        if (!bloodSplatter)
        {
            bloodSplatter = Resources.Load<GameObject>("Prefabs/Misc/bloodSplatter");
        }
        GameObject splatter = Instantiate(bloodSplatter);
        splatter.transform.position = transform.position;
        splatter.transform.eulerAngles = new Vector3(45, UnityEngine.Random.value * 360f, 45);
        splatter.GetComponent<Projector>().orthographicSize = Mathf.Lerp(0.05f, 0.70f, (damage / 100));
    }


    /*** PRIVATE ***/

    private void animationControls()
    {
        anim.enabled = true;
        anim.SetFloat("dashCharge", (DashCharging && !Dashing) ? DashPower : 0);
        anim.SetBool("moving", body.velocity.magnitude > Haste * SpeedScalarGlobal / 20f ? true : false);
        anim.SetFloat("velocity", Mathf.Max(body.velocity.magnitude, Haste * SpeedScalarGlobal / 4f));
        Vector3 relativeDirection = body.velocity == Vector3.zero ? Vector3.zero : AI.angleToDirection(AI.getAngle(LookDirection) - AI.getAngle(body.velocity) + 90);
        anim.SetFloat("x", Shoved ? -relativeDirection.x : relativeDirection.x);
        anim.SetFloat("z", Shoved ? -relativeDirection.z : relativeDirection.z);
        
    }

    private void indicatorManagement()
    {
        if (indicator)
        {
            indicator.enabled = !requiemPlayer;
            if (Dashing || DashCharging)
            {
                indicator.material.color = Color.red;
            }
            else
            {
                indicator.material.color = Color.white;
            }
        }
    }

    private void equipmentManagement()
    {
        if (leftStorage)
        {
            leftStorage.enabled = true;
            leftStorage = leftStorage.Wielder ? leftStorage.Wielder == this ? leftStorage : null : null;
        }
        if (rightStorage)
        {
            rightStorage.enabled = true;
            rightStorage = rightStorage.Wielder ? rightStorage.Wielder == this ? rightStorage : null : null;
        }
        if (backStorage)
        {
            backStorage.enabled = true;
            backStorage = backStorage.Wielder ? backStorage.Wielder == this ? backStorage : null : null;
        }
        switch (wieldMode)
        {
            case WieldMode.EmptyHanded:
                MainHand = null;
                OffHand = null;
                break;
            case WieldMode.OneHanders:
                if (!MainHand)
                {
                    MainHand = rightStorage ? rightStorage : (leftStorage ? (leftStorage.Idling || !leftStorage.Wielded ? leftStorage : null) : null);
                }
                else if (MainHand.Wielder != this || MainHand.equipType == Item.EquipType.TwoHanded)
                {
                    MainHand = null;
                }
                if (!OffHand)
                {
                    OffHand = rightStorage && leftStorage ? leftStorage : null;
                }
                else if (OffHand.Wielder != this || MainHand == OffHand)
                {
                    OffHand = null;
                }
                if (!rightStorage && !leftStorage)
                {
                    if (backStorage)
                    {
                        wieldMode = WieldMode.TwoHanders;
                    }
                }
                break;
            case WieldMode.TwoHanders:
                MainHand = backStorage;
                OffHand = null;
                if (!backStorage)
                {
                    if (rightStorage || leftStorage)
                    {
                        wieldMode = WieldMode.OneHanders;
                    }
                }
                break;
            case WieldMode.Burdened:
                OffHand = null;
                break;
        }
    }

    private void updatePosture()
    {
        if (Stunned)
        {
            posture = Posture.Stiff;
        }
        else if (Poise >= Strength)
        {
            posture = Posture.Flow;
        }
        else if (Poise <= 0)
        {
            posture = Posture.Stiff;
        }
        else if (posture != Posture.Stiff)
        {
            posture = Posture.Warm;
        }
    }

    private void updateStats()
    {
        body.mass = Strength * scaleActual;
        Rebuked = (MainHand ? MainHand.Rebuked : false) || (OffHand ? OffHand.Rebuked : false);
        Weapon mainWep = MainHand ? MainHand.GetComponent<Weapon>() : null;
        //Defending = (mainWep ? mainWep.Defending : false) || (OffHand ? OffHand.GetComponent<Weapon>() ? OffHand.GetComponent<Weapon>().Defending : false : false);
        Vitality = Mathf.Clamp(Vitality, 0, Strength);
        Agility = modSpeed.Values.Aggregate(Haste, (result, multiplier) => result *= 1 + multiplier);
        if (Stunned)
        {
            Poise = 0;
        }
        else
        {
            if (!mainWep)
            {
                Tempo = TEMPO_RESTING_VALUE;
            }
            else if ((tempoDebounceTimer += Time.deltaTime) >= TEMPO_DEBOUNCE_PERIOD)
            {
                Tempo = Mathf.MoveTowards(Tempo, TEMPO_RESTING_VALUE, Time.deltaTime / TEMPO_DRAIN_PERIOD);

            }
            if ((poiseDebounceTimer += Time.deltaTime) >= (POISE_DEBOUNCE_PERIOD / Resolve))
            {
                float increment = Resolve * Time.deltaTime * Strength / POISE_REGEN_PERIOD;
                float restingValue = POISE_RESTING_PERCENTAGE * Strength;
                float delta = Poise - restingValue;
                if (Mathf.Abs(delta) <= increment)
                {
                    Poise = POISE_RESTING_PERCENTAGE * Strength;
                }
                else if (Poise > restingValue)
                {
                    Poise -= increment;
                }
                else if (Poise < restingValue)
                {
                    Poise += increment;
                }
            }
        }
        Poise = Mathf.Clamp(Poise, 0, Strength);
        Tempo = Mathf.Clamp(Tempo, 0, 1);
    }
    private void resolveDashHit(Collision collision, bool instant = false)
    {
        GameObject other = collision.gameObject;
        Entity foe = other.GetComponent<Entity>();
        if (foe ? foe.Allegiance != Allegiance : false)
        {
            Vector3 disposition = foe.transform.position - transform.position;
            float minMag = Mathf.Lerp(Haste * SpeedScalarGlobal, dashMinVelocity, 0.5f);
            float maxMag = FinalDash ? dashMaxVelocity * 2 : dashMaxVelocity;
            bool crash = instant || collision.relativeVelocity.magnitude >= minMag;       
            float actualMag = instant ? Mathf.Lerp(minMag, maxMag, DashPower) : Mathf.Min(collision.relativeVelocity.magnitude, maxMag);
            if (crash && Vector3.Dot(disposition.normalized, -dashDirection) <= -0.25f)
            {
                float impactRatio = actualMag / dashMaxVelocity * Strength_Ratio(this, foe);
                Vector3 ShoveVector = disposition.normalized * impactRatio * dashMaxVelocity;
                ShoveVector *= 1.0f;
                foe.Shove(ShoveVector);
                foe.JustCrashed.Invoke();
                if (foe.requiemPlayer ? false : !foe.Foe)
                {
                    foe.Vitality = 0;
                    //alterPoise(Strength);
                }
                else
                {                
                    if (FinalDash)
                    {
                        foe.Rebuke(actualMag / maxMag);
                    }
                    float damage = CRASH_DAMAGE * actualMag / maxMag;
                    foe.Damage(damage);
                    alterPoise(0);
                }
                Mullet.PlayAmbientSound("Audio/Weapons/punch", transform.position, Mathf.Max(1.25f - (impactRatio / 2), 0.5f), 1.0f, Mullet.Instance.DefaultAudioRange / 2, onSoundSpawn: sound => sound.layer = Game.layerEntity);           
            }
        }
        dashAlreadyHit.Add(other);
    }



    private void resolveCrash(Collision collision)
    {
        if (!Shoved)
        {
            return;
        }
        else if (collision.relativeVelocity.magnitude < dashMinVelocity)
        {
            return;
        }
        ContactPoint contact = collision.GetContact(0);
        float dot = Vector3.Dot(contact.normal.normalized, collision.relativeVelocity.normalized);
        bool cleanHit = dot > 0.5f;
        if(cleanHit)
        {
            Entity otherEntity = collision.gameObject.GetComponent<Entity>();
            float velocityRatio = (collision.relativeVelocity.magnitude - dashMinVelocity) / (dashMaxVelocity - dashMinVelocity);
            if ((otherEntity ? !otherEntity.Dashing : false) && !dashAlreadyHit.Contains(collision.gameObject))
            {
                float impactToFoe = Strength_Ratio(this, otherEntity) * velocityRatio / 2;
                otherEntity.Shove(-collision.relativeVelocity.normalized * impactToFoe);
                otherEntity.Damage(impactToFoe * CRASH_DAMAGE);
                float impactToSelf = Strength_Ratio(otherEntity, this) * velocityRatio / 2;
                Shove(collision.relativeVelocity.normalized * impactToSelf);
                Damage(impactToSelf * CRASH_DAMAGE);
                JustCrashed.Invoke();
                dashAlreadyHit.Add(otherEntity.gameObject);
                otherEntity.dashAlreadyHit.Add(gameObject);
                Mullet.PlayAmbientSound("Audio/Weapons/punch", transform.position, Mathf.Max(1.5f - velocityRatio, 0.5f), 1.0f, Mullet.Instance.DefaultAudioRange / 2, onSoundSpawn: sound => sound.layer = Game.layerEntity);
            }
            else if (CrashEnvironmentONS)
            {
                Landmark landmark = collision.gameObject.GetComponent<Landmark>();
                if (landmark)
                {
                    landmark.Impacted();
                }
                if (!Dashing)
                {
                    Damage(velocityRatio * CRASH_DAMAGE);
                    JustCrashed.Invoke();
                }
                CrashEnvironmentONS = false;
                Mullet.PlayAmbientSound("Audio/Weapons/punch", transform.position, Mathf.Max(1.5f - velocityRatio, 0.5f), 1.0f, Mullet.Instance.DefaultAudioRange / 2, onSoundSpawn: sound => sound.layer = Game.layerEntity);
            }
        }
    }




    /***** ROUTINES *****/
    private IEnumerator routineDash()
    {
        string key = "dash";
        while (true)
        {
            FinalDash = false;
            yield return new WaitUntil(() => DashCharging && !Stunned);
            bool underterminedDirection = dashDirection == Vector3.zero;
            while (DashCharging)
            {
                //DashPower = FinalDash ? 1.0f : Mathf.Clamp(DashPower + Time.deltaTime / (dashChargeTime * (1 + weaponHeft)), 0, 1);
                float increment = Time.deltaTime * Haste / dashChargeTime;
                DashPower = FinalDash ? 1.0f : Mathf.Clamp(DashPower + increment, 0, 1);
                if (underterminedDirection)
                {
                    dashDirection = WalkDirection == Vector3.zero ? dashDirection : WalkDirection;
                }
                yield return null;
            }
            dashAlreadyHit = new List<GameObject>();
            float scaledVelocity = dashMaxVelocity * DashPower;
            if (!Stunned && dashDirection != Vector3.zero && scaledVelocity > dashMinVelocity)
            {
                Dashing = true;
                if (FinalDash)
                {
                    scaledVelocity *= 2;
                    alterPoise(-Poise);
                }
                Shove(dashDirection * scaledVelocity, true);
                GameObject sound = Mullet.PlayAmbientSound(dashSound, transform.position, 2.5f - DashPower / 1.5f, 0.25f + DashPower);
                sound.layer = Game.layerItem;
                sound.transform.SetParent(transform);
                yield return new WaitWhile(() => Shoved);
            }
            modSpeed[key] = 0.0f;
            Dashing = false;
            DashPower = 0.0f;
            dashDirection = Vector3.zero;
        }
    }

    private IEnumerator routineTempoTarget()
    {
        Vector2 limits = new Vector2(0.5f, 0.8f);
        float speed = 0.1f;
        while (true)
        {
            TempoTarget = limits.x;
            yield return new WaitUntil(() => { TempoTarget += Time.deltaTime * speed; return TempoTarget >= limits.y; });
            TempoTarget = limits.y;
            yield return new WaitUntil(() => { TempoTarget -= Time.deltaTime * speed; return TempoTarget <= limits.x; });
        }
    }

}

