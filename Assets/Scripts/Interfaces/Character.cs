using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public abstract class Character : MonoBehaviour
{
    public static UnityEvent<Character> EntityVanquished = new UnityEvent<Character> { };

    public Player requiemPlayer;

    public float Strength = 100f;
    public float Resolve = 1.0f;
    public float Haste = 1.0f;

    public float Xp = 0f;
    public int Lvl = 0;

    public float Vitality;
    public float Speed;
    public float Special;
   
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
    public UnityEvent<Character, float> EventDashHit = new UnityEvent<Character, float>();

    public Character Foe;

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
    public UnityEvent<Character> Pickup = new UnityEvent<Character> { };
    public UnityEvent<Character> Interact = new UnityEvent<Character> { };
 
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

    public bool Staggered = false;
    protected float staggerPeriod = 0;
    protected float staggerTimer = 0;

    /********** DASHING **********/
    public static float Max_Velocity_Of_Dash { get; private set; } = 3.0f;
    public static float Min_Velocity_Of_Dash { get; private set; } = 1.0f;
    public bool DashCharging = false;
    public bool Dashing = false;
    public float DashPower { get; private set; } = 0.0f;
    public bool FinalDash { get; private set; } = false;
    public Vector3 dashDirection = Vector3.zero;
    private static AudioClip SOUND_OF_DASH;
    private List<GameObject> dashAlreadyHit = new List<GameObject>();
    protected bool CrashEnvironmentONS = true;

    private static float FINALDASH_POWER_SCALAR = 1.5f;
    private static float DASH_CHARGE_TIME = 0.25f;
    private static float CRASH_DAMAGE = 25f;


    public Dictionary<string, (float, float)> BleedingWounds = new Dictionary<string, (float, float)>();
    
    //public bool Running = false;

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
        Strength = 100f;
        Resolve = 10;
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
        if (!indicatorPrefab)
        {
            indicatorPrefab = Resources.Load<GameObject>("Prefabs/UX/Indicator");
        }
        if (!statBarPrefab)
        {
            statBarPrefab = Resources.Load<GameObject>("Prefabs/UX/StatBar");
        }
        if (!SOUND_OF_DASH)
        {
            SOUND_OF_DASH = Game.getSound("Audio/weapons/deepSwing");
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
        Special = Strength;
        StartCoroutine(routineDashHandler());
    }

    protected virtual void Update()
    {
        equipmentManagement();
        indicatorManagement();
        updateStats();
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
        modSpeed["dash"] = (DashPower > 0) ? Mathf.Lerp(-0.5f, -0.9f, DashPower) : 0;
        modAcceleration["nonlinear"] = baseSpeed > 0 ? Mathf.Lerp(0.25f, -0.25f, body.velocity.magnitude / baseSpeed ) : 0;
        AccelerationActual = modAcceleration.Values.Aggregate(BaseAcceleration, (result, multiplier) => result *= 1 + multiplier);
        SpeedActual = Speed * SpeedScalarGlobal;
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
        float effectiveAccel = (Shoved && !Dashing) ? AccelerationActual * 0.75f : AccelerationActual;
        Vector3 proposedVelocity;
        bool stopping = (WalkDirection == Vector3.zero || Shoved || Staggered) && body.velocity.magnitude > 0;
        if (stopping)
        {
            Vector3 increment = body.velocity * Time.fixedDeltaTime * effectiveAccel;
            proposedVelocity = increment.magnitude >= body.velocity.magnitude ? Vector3.zero : body.velocity - increment;
            body.velocity = new Vector3(proposedVelocity.x, body.velocity.y, proposedVelocity.z);
        }
        else
        {
            proposedVelocity = body.velocity + (WalkDirection * Time.fixedDeltaTime * effectiveAccel);
            if (new Vector2(proposedVelocity.x, proposedVelocity.z).magnitude > SpeedActual && !Shoved && !Staggered)
            {
                Vector3 normalized = proposedVelocity.normalized * SpeedActual;
                body.velocity = new Vector3(normalized.x, body.velocity.y, normalized.z);
            }
            else
            {
                body.velocity = proposedVelocity;
            }
        }

        if ((Shoved && !Dashing) || Staggered)
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
            Character foe = collision.gameObject.GetComponent<Character>();
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
            Character foe = collision.gameObject.GetComponent<Character>();
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

    public static float Strength_Ratio(Character A, Character B)
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
            Character entity = bone.GetComponentInParent<Character>();
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

    public abstract void Damage(float magnitude);

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
        //updatePosture();
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
    protected abstract void updatePosture();


    protected virtual void updateStats()
    {
        body.mass = Strength * scaleActual;
        //Stunned = (MainHand ? MainHand.Rebuked : false) || (OffHand ? OffHand.Rebuked : false);
        Vitality = Mathf.Clamp(Vitality, 0, Strength);
        Speed = modSpeed.Values.Aggregate(Haste, (result, multiplier) => result *= 1 + multiplier);
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
        anim.SetFloat("dashCharge", Dashing ? 0 : DashPower);
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
        Character foe = other.GetComponent<Character>();
        if (foe ? foe.Allegiance != Allegiance : false)
        {
            Vector3 disposition = foe.transform.position - transform.position;
            float minMag = Mathf.Lerp(Haste * SpeedScalarGlobal, Min_Velocity_Of_Dash, 0.5f);
            float maxMag = FinalDash ? Max_Velocity_Of_Dash * FINALDASH_POWER_SCALAR : Max_Velocity_Of_Dash;
            bool crash = instant || collision.relativeVelocity.magnitude >= minMag;       
            float actualMag = instant ? Mathf.Lerp(minMag, maxMag, DashPower) : Mathf.Min(collision.relativeVelocity.magnitude, maxMag);
            if (crash && Vector3.Dot(disposition.normalized, -dashDirection) <= -0.25f)
            {
                float impactRatio = Strength_Ratio(this, foe) * actualMag / Max_Velocity_Of_Dash;
                Vector3 ShoveVector = disposition.normalized * impactRatio * Max_Velocity_Of_Dash;
                ShoveVector *= 1.0f;
                foe.Shove(ShoveVector);
                foe.EventCrashed.Invoke();
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
                        float damage = CRASH_DAMAGE * actualMag / maxMag;
                        foe.Damage(damage);
                        EventDashHit.Invoke(foe, damage);
                    }
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
        else if (collision.relativeVelocity.magnitude < Min_Velocity_Of_Dash)
        {
            return;
        }
        ContactPoint contact = collision.GetContact(0);
        float dot = Vector3.Dot(contact.normal.normalized, collision.relativeVelocity.normalized);
        bool cleanHit = dot > 0.5f;
        if(cleanHit)
        {
            Character otherEntity = collision.gameObject.GetComponent<Character>();
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
                    EventCrashed.Invoke();
                }
                CrashEnvironmentONS = false;
                Mullet.PlayAmbientSound("Audio/Weapons/punch", transform.position, Mathf.Max(1.5f - velocityRatio, 0.5f), 1.0f, Mullet.Instance.DefaultAudioRange / 2, onSoundSpawn: sound => sound.layer = Game.layerEntity);
            }
        }
    }


    private IEnumerator routineDashHandler()
    {
        string key = "dash";
        bool directionIsUndetermined;
        bool heldChargeUntilCutoff;
        float finalDashCutoffCharge = 0.9f;
        while (true)
        {
            FinalDash = false;
            yield return new WaitUntil(() => DashCharging && !Staggered);
            directionIsUndetermined = dashDirection == Vector3.zero;
            heldChargeUntilCutoff = true;
            while (DashPower < 1.0)
            {
                float increment = Time.deltaTime * Haste / DASH_CHARGE_TIME;
                DashPower = Mathf.Clamp(DashPower + increment, 0, 1);
                if (directionIsUndetermined)
                {
                    dashDirection = WalkDirection == Vector3.zero ? dashDirection : WalkDirection;
                }
                if(DashPower < finalDashCutoffCharge)
                {
                    heldChargeUntilCutoff = heldChargeUntilCutoff && DashCharging;
                }
                else if(heldChargeUntilCutoff)
                {
                    FinalDash = !DashCharging && dashDirection == Vector3.zero;
                }

                yield return null;
            }
            dashAlreadyHit = new List<GameObject>();
            float scaledVelocity = Max_Velocity_Of_Dash * DashPower;
            if (Staggered)
            {

            }
            else if (scaledVelocity <= Min_Velocity_Of_Dash)
            {

            }
            else
            {
                if (FinalDash)
                {
                    scaledVelocity *= FINALDASH_POWER_SCALAR;
                    dashDirection = LookDirection;
                }           
                if(FinalDash || dashDirection != Vector3.zero)
                Dashing = true;
                Shove(dashDirection * scaledVelocity, true);
                GameObject sound = Mullet.PlayAmbientSound(SOUND_OF_DASH, transform.position, 2.5f - DashPower / 1.5f, 0.25f + DashPower);
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

}

