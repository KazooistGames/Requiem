using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Newtonsoft.Json.Linq;
using static Weapon;
using UnityEngine.TextCore.Text;

public abstract class Warrior : MonoBehaviour
{
    public static UnityEvent<Warrior> EntityVanquished = new UnityEvent<Warrior> { };

    public Player requiemPlayer;

    public float Strength = 100f;
    public float Resolve = 10.0f;
    public float Haste = 1.0f;

    public float Xp = 0f;
    public int Lvl = 0;


    public float Vitality;
    public float Agility;
    public float Poise;

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

    public UnityEvent EventVanquished = new UnityEvent();
    public UnityEvent<float> EventWounded = new UnityEvent<float>();
    public UnityEvent EventCrashed = new UnityEvent();
    public UnityEvent<Warrior, float> EventLandedDashHit = new UnityEvent<Warrior, float>();
    public UnityEvent<Wieldable> EventPickedUpWieldable = new UnityEvent<Wieldable>();

    public Warrior Foe;

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

    public Wieldable MainHand;
    public Wieldable OffHand;
    public Wieldable leftStorage;
    public Wieldable rightStorage;
    public Wieldable backStorage;
    public UnityEvent<Warrior> EventAttemptPickup = new UnityEvent<Warrior> { };
    public UnityEvent<Warrior> EventAttemptInteraction = new UnityEvent<Warrior> { };

    public CapsuleCollider personalBox;
    protected GameObject model;
    protected CapsuleCollider hurtBox;
    public Rigidbody body;
    public Projector indicator;
    public GameObject statBar;
    protected Animator anim;

    private static GameObject INDICATOR_PREFAB;
    private static GameObject STATBAR_PREFAB;
    protected static GameObject BLOOD_SPLATTER_PREFAB;


    public bool Shoved = false;
    public float shoveRecoveryPeriod = 0;
    private float shoveRecoveryTimer = 0;
    private float shoveAccelScalar = 0.75f;

    public bool Staggered = false;
    protected float staggerPeriod = 0;
    protected float staggerTimer = 0;

    /********** DASHING **********/
    public static float Max_Velocity_Of_Dash { get; private set; } = 4.0f;
    public static float Min_Velocity_Of_Dash { get; private set; } = 2.0f;
    public bool DashCharging = false;
    public bool Dashing = false;
    public float DashPower { get; private set; } = 0.0f;


    public Vector3 dashDirection = Vector3.zero;
    private static AudioClip SOUND_OF_DASH;
    private List<GameObject> dashAlreadyHit = new List<GameObject>();
    protected bool CrashEnvironmentONS = true;

    private static float DASH_CHARGE_TIME = 0.25f;
    private static float CRASH_DAMAGE = 20f;
    private static float FINAL_DASH_RATIO = 2f;

    private static float POISE_REGEN_PERIOD = 4f;
    private static float POISE_DEBOUNCE_PERIOD = 4f;
    private static float POISE_RESTING_PERCENTAGE = 1f;
    private float poiseDebounceTimer = 0.0f;

    public Dictionary<string, (float, float)> BleedingWounds = new Dictionary<string, (float, float)>();

    public GameObject Location;
    public GameObject head;

    public bool FinalDash = false;
    public float Tempo { get; private set; }
    public float TempoTargetCenter { get; private set; } = 0.90f;
    public float TempoTargetWidth { get; private set; } = 0.2f;
    private bool tempoChargeONS = true;

    public enum PostureStrength
    {
        Weak = -1,
        Normal = 0,
        Strong = 1,
    }
    public PostureStrength Posture = PostureStrength.Strong;

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

    protected _Flames flames;

    //******************Functions*************************
    protected virtual void Awake()
    {
        Strength = 100f;
        Resolve = 20;
        Haste = 1.0f;
        BaseAcceleration = 8;
        hurtBox = gameObject.AddComponent<CapsuleCollider>();
        personalBox = gameObject.AddComponent<CapsuleCollider>();
        body = GetComponent<Rigidbody>() == null ? gameObject.AddComponent<Rigidbody>() : GetComponent<Rigidbody>();
        anim = GetComponent<Animator>() == null ? gameObject.AddComponent<Animator>() : GetComponent<Animator>();
        anim.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        if (!DefaultAnimController)
        {
            DefaultAnimController = Resources.Load<RuntimeAnimatorController>("Animation/entities/EntityDefault/entityDefaultAnimation");
        }
        if (!INDICATOR_PREFAB)
        {
            INDICATOR_PREFAB = Resources.Load<GameObject>("Prefabs/UX/Indicator");
        }
        if (!STATBAR_PREFAB)
        {
            STATBAR_PREFAB = Resources.Load<GameObject>("Prefabs/UX/StatBar");
        }
        if (!SOUND_OF_DASH)
        {
            SOUND_OF_DASH = Game.getSound("Audio/weapons/deepSwing");
        }
    }

    protected virtual void Start()
    {
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
        TurnSpeed = DefaultTurnSpeed;
        statBar = Instantiate(STATBAR_PREFAB, transform);
        indicator = Instantiate(INDICATOR_PREFAB, transform).GetComponent<Projector>();
        Material newMaterial = new Material(Shader.Find("Custom/Projection"));
        newMaterial.SetTexture("_ShadowTex", Resources.Load<Texture>("Textures/IndicatorSimple"));
        indicator.material = newMaterial;
        indicator.orthographic = true;
        indicator.orthographicSize = berthActual * scaleActual;
        indicator.transform.parent = transform;
        indicator.enabled = false;
        flames = Instantiate(Game.SpiritFlameTemplate).GetComponent<_Flames>();
        flames.bindToObject(gameObject);
        flames.FlamePresentationStyle = _Flames.FlameStyles.Magic;
        Poise = Strength;
        StartCoroutine(routineDashHandler());
        EventPickedUpWieldable.AddListener(handleWeaponPickedUp);
    }

    protected virtual void Update()
    {
        equipmentManagement();
        indicatorManagement();
        body.mass = Strength * scaleActual;
        if ((poiseDebounceTimer += Time.deltaTime) >= (POISE_DEBOUNCE_PERIOD))
        {
            float increment = Time.deltaTime * Strength / POISE_REGEN_PERIOD;
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
        Vitality = Mathf.Clamp(Vitality, 0, Strength);
        Poise = Mathf.Clamp(Poise, 0, Strength);
        Agility = modSpeed.Values.Aggregate(Haste, (result, multiplier) => result *= 1 + multiplier);
        updatePosture();
        if (Vitality <= 0)
        {
            Physics.autoSimulation = false;
            Physics.Simulate(Time.deltaTime);
            Physics.autoSimulation = true;
            Die();
        }
        else
        {
            List<string> keys = BleedingWounds.Keys.ToList();
            foreach (string key in keys)
            {
                if (BleedingWounds[key].Item2 <= 0)
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
            if (GetComponent<Character>())
            {
                GetComponent<Character>().Enthralled = true;
                GetComponent<Character>().enabled = !requiemPlayer;
                GetComponent<Character>().followVIP = requiemPlayer.HostEntity.gameObject;
            }
        }
        if (!anim.runtimeAnimatorController)
        {
            anim.runtimeAnimatorController = DefaultAnimController;
        }
        else
        {
            anim.enabled = true;
            anim.SetFloat("dashCharge", Dashing ? 0 : DashPower);
            anim.SetBool("moving", body.velocity.magnitude > Haste * SpeedScalarGlobal / 20f ? true : false);
            anim.SetFloat("velocity", Mathf.Max(body.velocity.magnitude, Haste * SpeedScalarGlobal / 4f));
            Vector3 relativeDirection = body.velocity == Vector3.zero ? Vector3.zero : Character.angleToDirection(Character.getAngle(LookDirection) - Character.getAngle(body.velocity) + 90);
            anim.SetFloat("x", Shoved ? -relativeDirection.x : relativeDirection.x);
            anim.SetFloat("z", Shoved ? -relativeDirection.z : relativeDirection.z);
        }
    }

    protected virtual void FixedUpdate()
    {
        //float baseSpeed = Mathf.Pow(Haste, 0.5f) * SpeedScalarGlobal;
        float baseSpeed = Haste * SpeedScalarGlobal;
        //modSpeed["flow"] = posture == Posture.Flow ? Resolve / 100 : 0;
        modSpeed["dash"] = (DashPower > 0) ? Mathf.Lerp(-0.5f, -0.9f, DashPower) : 0;
        modSpeed["staggered"] = Mathf.Lerp(0, -1, (staggerPeriod - staggerTimer) * 3);
        modAcceleration["nonlinear"] = baseSpeed > 0 ? Mathf.Lerp(0.25f, -0.25f, body.velocity.magnitude / baseSpeed) : 0;
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
        if (Staggered)
        {
            if (staggerTimer >= staggerPeriod)
            {
                Staggered = false;
            }
            else
            {
                staggerTimer += Time.fixedDeltaTime;
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
        float effectiveAccel = AccelerationActual;
        Vector3 proposedVelocity;
        bool stopping = (WalkDirection == Vector3.zero || Shoved) && body.velocity.magnitude > 0;
        if (stopping)
        {
            Vector3 increment = body.velocity * Time.fixedDeltaTime * effectiveAccel;
            proposedVelocity = increment.magnitude >= body.velocity.magnitude ? Vector3.zero : body.velocity - increment;
            body.velocity = new Vector3(proposedVelocity.x, body.velocity.y, proposedVelocity.z);
        }
        else
        {
            proposedVelocity = body.velocity + (WalkDirection * Time.fixedDeltaTime * effectiveAccel);
            if (new Vector2(proposedVelocity.x, proposedVelocity.z).magnitude > SpeedActual && !Shoved)
            {
                Vector3 normalized = proposedVelocity.normalized * SpeedActual;
                body.velocity = new Vector3(normalized.x, body.velocity.y, normalized.z);
            }
            else
            {
                body.velocity = proposedVelocity;
            }
        }

        if ((Shoved && !Dashing))
        {
            body.freezeRotation = true;
        }
        else
        {
            TurnSpeed = modTurnSpeed.Values.Aggregate(DefaultTurnSpeed, (result, multiplier) => result *= (1 + multiplier));
            LookDirection.y = 0;
            float scaledY = transform.localEulerAngles.y;
            float scaledTarget = 90 - Character.getAngle(LookDirection);
            scaledTarget = scaledTarget > 180 ? scaledTarget - 360 : scaledTarget;
            float difference = (scaledTarget - scaledY);
            float degMax = TurnSpeed * Time.fixedDeltaTime;
            difference = Mathf.Clamp(Mathf.Abs(difference) >= 180 ? difference - (Mathf.Sign(difference) * 360) : difference, -1 * degMax, degMax);
            transform.RotateAround(transform.position, Vector3.up, difference);
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!dashAlreadyHit.Contains(collision.gameObject))
        {
            Warrior foe = collision.gameObject.GetComponent<Warrior>();
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
            Warrior foe = collision.gameObject.GetComponent<Warrior>();
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

    public static float Strength_Ratio(Warrior A, Warrior B)
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
            Warrior entity = bone.GetComponentInParent<Warrior>();
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
        if (magnitude > 0)
        {

            Vitality -= magnitude;
            EventWounded.Invoke(magnitude);
        }
    }


    public void Stagger(float duration)
    {
        if (duration > staggerPeriod - staggerTimer)
        {
            staggerPeriod = duration;
            staggerTimer = 0;
            Staggered = true;
        }
    }

    public void Disarm(float yeetMagnitude = 2)
    {
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
        shoveRecoveryPeriod += forceDelta / (AccelerationActual * shoveAccelScalar);
    }

    /***** PROTECTED *****/
    protected virtual void updatePosture()
    {
        if (Poise <= 0)
        {
            Posture = PostureStrength.Weak;
        }
        else if(Poise >= Strength)
        {
            Posture = PostureStrength.Strong;
        }
        else
        {
            Posture = PostureStrength.Normal;

        }
    }

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
        EventVanquished.Invoke();
        Destroy(gameObject);
    }




    /*** PRIVATE ***/

    private void alterPoise(float value)
    {
        float existingDelta = Poise - POISE_RESTING_PERCENTAGE * Strength;
        Poise += value;
        Poise = Mathf.Clamp(Poise, -1, Strength);
        if (value * existingDelta >= 0)
        {
            poiseDebounceTimer = 0.0f;
        }
        updatePosture();
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
                else if (MainHand.Wielder != this || MainHand.equipType == Wieldable.EquipType.TwoHanded)
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

    private void resolveDashHit(Collision collision, bool instant = false)
    {
        GameObject other = collision.gameObject;
        Warrior foe = other.GetComponent<Warrior>();
        if (foe ? foe.Allegiance != Allegiance : false)
        {
            Vector3 disposition = foe.transform.position - transform.position;
            float minMag = Mathf.Lerp(Haste * SpeedScalarGlobal, Min_Velocity_Of_Dash, 0.5f);
            //float maxMag = FinalDash ? Max_Velocity_Of_Dash * FINALDASH_POWER_SCALAR : Max_Velocity_Of_Dash;
            bool crash = instant || collision.relativeVelocity.magnitude >= minMag;
            float actualMag = instant ? Mathf.Lerp(minMag, Max_Velocity_Of_Dash, DashPower) : collision.relativeVelocity.magnitude;
            if (crash && Vector3.Dot(disposition.normalized, -dashDirection) <= -0.25f)
            {
                float impactRatio = Strength_Ratio(this, foe) * actualMag / Max_Velocity_Of_Dash;
                Vector3 ShoveVector = disposition.normalized * impactRatio * Max_Velocity_Of_Dash;
                ShoveVector *= 0.75f;
                foe.Shove(ShoveVector);
                foe.EventCrashed.Invoke();
                float damage = Mathf.Sqrt(Strength) * impactRatio;
                if (foe.requiemPlayer ? false : !foe.Foe)
                {
                    foe.Vitality = 0;
                }
                else if(FinalDash)
                {
                    if(foe.Posture == PostureStrength.Weak)
                    {
                        foe.Damage(damage);
                    }
                    foe.alterPoise(-damage);   
                }
                EventLandedDashHit.Invoke(foe, damage);
                _SoundService.PlayAmbientSound("Audio/Weapons/punch", transform.position, Mathf.Max(1.25f - (impactRatio / 2), 0.5f), 1.0f, _SoundService.Instance.DefaultAudioRange / 2, onSoundSpawn: sound => sound.layer = Game.layerEntity);
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
        else if (collision.relativeVelocity.magnitude < Min_Velocity_Of_Dash)
        {
            return;
        }
        ContactPoint contact = collision.GetContact(0);
        float dot = Vector3.Dot(contact.normal.normalized, collision.relativeVelocity.normalized);
        bool cleanHit = dot > 0.5f;
        if (cleanHit)
        {
            Warrior otherEntity = collision.gameObject.GetComponent<Warrior>();
            float velocityRatio = (collision.relativeVelocity.magnitude - Min_Velocity_Of_Dash) / (Max_Velocity_Of_Dash - Min_Velocity_Of_Dash);
            if ((otherEntity ? !otherEntity.Dashing : false) && !dashAlreadyHit.Contains(collision.gameObject))
            {
                float impactToFoe = Strength_Ratio(this, otherEntity) * velocityRatio / 2;
                otherEntity.Shove(-collision.relativeVelocity.normalized * impactToFoe);
                otherEntity.Damage(impactToFoe * CRASH_DAMAGE);
                float impactToSelf = Strength_Ratio(otherEntity, this) * velocityRatio / 2;
                Shove(collision.relativeVelocity.normalized * impactToSelf);
                Damage(impactToSelf * CRASH_DAMAGE);
                EventCrashed.Invoke();
                dashAlreadyHit.Add(otherEntity.gameObject);
                otherEntity.dashAlreadyHit.Add(gameObject);
                _SoundService.PlayAmbientSound("Audio/Weapons/punch", transform.position, Mathf.Max(1.5f - velocityRatio, 0.5f), 1.0f, _SoundService.Instance.DefaultAudioRange / 2, onSoundSpawn: sound => sound.layer = Game.layerEntity);
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
                    alterPoise(-velocityRatio * CRASH_DAMAGE);
                    if(Posture == PostureStrength.Weak)
                    {
                        Damage(velocityRatio * CRASH_DAMAGE);
                    }
                    EventCrashed.Invoke();
                }
                CrashEnvironmentONS = false;
                _SoundService.PlayAmbientSound("Audio/Weapons/punch", transform.position, Mathf.Max(1.5f - velocityRatio, 0.5f), 1.0f, _SoundService.Instance.DefaultAudioRange / 2, onSoundSpawn: sound => sound.layer = Game.layerEntity);
            }
        }
    }

    private IEnumerator routineDashHandler()
    {
        string key = "dash";
        while (true)
        {
            float scaledVelocity = 0;
            float overChargeTimer = 0;
            yield return new WaitUntil(() => DashCharging);
            while (DashCharging || scaledVelocity <= Min_Velocity_Of_Dash)
            {
                float increment = Time.deltaTime * Haste / DASH_CHARGE_TIME;
                DashPower = Mathf.Clamp(DashPower + increment, 0, 1);
                if(DashPower >= 1 )
                {
                    overChargeTimer += increment / FINAL_DASH_RATIO;     
                    if(overChargeTimer > DASH_CHARGE_TIME * FINAL_DASH_RATIO)
                    {
                        FinalDash = true;
                    }
                }
                scaledVelocity = Max_Velocity_Of_Dash * DashPower;
                yield return null;
            }
            dashAlreadyHit = new List<GameObject>();
            if (dashDirection != Vector3.zero)
            {
                if (FinalDash)
                {
                    scaledVelocity *= FINAL_DASH_RATIO;
                }
                Dashing = true;
                Shove(dashDirection * scaledVelocity, true);
                GameObject sound = _SoundService.PlayAmbientSound(SOUND_OF_DASH, transform.position, 2.5f - DashPower / 1.5f, 0.25f + DashPower);
                sound.layer = Game.layerItem;
                sound.transform.SetParent(transform);
                yield return new WaitWhile(() => Shoved);
            }
            FinalDash = false;
            Tempo = 0;
            modSpeed[key] = 0.0f;
            Dashing = false;
            DashPower = 0.0f;
            dashDirection = Vector3.zero;
        }
    }

    private void handleWeaponPickedUp(Wieldable wieldable)
    {
        Weapon weapon = wieldable.GetComponent<Weapon>();
        if (weapon)
        {
            weapon.Swinging.AddListener(handleWeaponSwing);
            weapon.Clashing.AddListener(handleWeaponClash);
            weapon.Blocking.AddListener(handleWeaponBlock);
            weapon.Parrying.AddListener(handleWeaponParrying);
            weapon.GettingParried.AddListener(handleWeaponParried);
            weapon.Hitting.AddListener(handleWeaponHit);
            weapon.EventDropped.AddListener(handleWeaponDropped);
        }
    }

    private void handleWeaponSwing(Weapon myWeapon)
    {
        //if(myWeapon.TrueStrike)
        //{
            //alterPoise(-myWeapon.Heft / 4);
        //}

    }

    private void handleWeaponClash(Weapon myWeapon, Weapon theirWeapon)
    {
        if (myWeapon.TrueStrike) 
        {
            if (theirWeapon.Wielder)
            {
                theirWeapon.Wielder.Stagger(2 * myWeapon.Heft / theirWeapon.Wielder.Strength);
            }
        }
        else if (myWeapon.ActionAnimated == ActionAnimation.StrongAttack)
        {

        }
        else if (myWeapon.ActionAnimated == ActionAnimation.QuickAttack)
        {
            if (/*Posture == PostureStrength.Weak ||*/ theirWeapon.ActionAnimated == ActionAnimation.StrongAttack)
            {
                Stagger((theirWeapon.Heft / Strength));
            }
        }

    }

    private void handleWeaponBlock(Weapon myWeapon, Weapon theirWeapon)
    {
        if (theirWeapon.TrueStrike)
        {
            if (Posture == PostureStrength.Weak)
            {
                Disarm();
            }
            else
            {
                Stagger(2 * theirWeapon.Heft / Strength);
            }
            alterPoise(-theirWeapon.Heft);
        }
        else if (theirWeapon.ActionAnimated == ActionAnimation.StrongAttack)
        {
            Stagger(1.5f * theirWeapon.Heft / Strength);
            alterPoise(-theirWeapon.Heft);
        }
        else if (theirWeapon.ActionAnimated == ActionAnimation.QuickAttack)
        {
            //if (Posture == PostureStrength.Weak)
            //{
            //    Stagger(theirWeapon.Heft / Strength);
            //}
        }

    }

    private void handleWeaponParrying(Weapon myWeapon, Weapon theirWeapon)
    {
        if (theirWeapon.Wielder)
        {
            theirWeapon.Wielder.alterPoise(-Resolve);
        }
    }

    private void handleWeaponParried(Weapon myWeapon, Weapon theirWeapon)
    {
        if (myWeapon.TrueStrike)
        {

        }
        else 
        {
            if (Posture == PostureStrength.Weak)
            {
                Disarm();
            }
            else
            {
                Stagger(1.5f * (myWeapon.Heft / Strength));
            }
            alterPoise(-myWeapon.Heft);
        }
    }


    private void handleWeaponHit(Weapon myWeapon, Warrior foe)
    {
        float poiseDamage = Mathf.Min(foe.Poise, myWeapon.Power);
        float vitalityDamage = foe.Posture == PostureStrength.Weak ? myWeapon.Power : myWeapon.Power - foe.Resolve;
        foe.alterPoise(-poiseDamage);
        if (myWeapon.ActionAnimated == ActionAnimation.StrongAttack || foe.Posture == PostureStrength.Weak)
        {
            foe.Stagger(1.5f * poiseDamage / Strength);
        }
        if (vitalityDamage > 0) 
        {
            foe.Damage(vitalityDamage);
        }
    }


    private void handleWeaponDropped(Wieldable wieldable)
    {
        Weapon weapon = wieldable.GetComponent<Weapon>();
        if (weapon)
        {
            weapon.Swinging.RemoveListener(handleWeaponSwing);
            weapon.Clashing.RemoveListener(handleWeaponClash);
            weapon.Blocking.RemoveListener(handleWeaponBlock);
            weapon.Parrying.RemoveListener(handleWeaponParrying);
            weapon.Hitting.RemoveListener(handleWeaponHit);
            weapon.GettingParried.RemoveListener(handleWeaponParried);
        }
    }
    

}

