using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public abstract class Weapon : Wieldable
{
    public static UnityEvent<Weapon> On_Weapon_Hit = new UnityEvent<Weapon> { };

    public UnityEvent<Character> EventHitting = new UnityEvent<Character>();
    public UnityEvent<Weapon> EventClashedWeapon = new UnityEvent<Weapon>();
    public UnityEvent<Weapon> EventBlockedWeapon = new UnityEvent<Weapon>();
    public UnityEvent<Weapon> EventParriedWeapon = new UnityEvent<Weapon>();
    public UnityEvent EventSwinging = new UnityEvent();
    public UnityEvent EventRebuked = new UnityEvent();

    public enum Action
    {
        error,
        Recoiling,
        Sheathed,
        Idle,
        Coiling,
        Windup,
        Attacking,
        Recovering,
        Guarding,
        Parrying,
        Aiming,
        Throwing,
    }
    public Action ActionCurrentlyAnimated = Action.error;

    public enum DamageType
    {
        none,
        Stab,
        Slash,
        Smash
    }
    public DamageType damageType { get; private set; } = DamageType.none;

    public bool Throwing = false;

    public float Range = 0f;
    public float Power = 0f;
    public float BasePower = 0f;
    public Dictionary<string, float> modPower = new Dictionary<string, float>();

    public bool TrueStrike = false;
    public float Tempo { get; private set; }
    public float TempoTargetCenter { get; private set; } = 0.925f;
    public float TempoTargetWidth { get; private set; } = 0.15f;


    protected string swingClip;
    protected float swingPitch;
    protected float clashPitch;
    protected string tinkClip = "Audio/Weapons/tink";
    protected float tinkPitch;
    protected string clashClip;
    protected float clashVolume;
    protected List<GameObject> alreadyHit = new List<GameObject>();
    protected bool attackONS = true;

    protected float heftSlowModifier;
    protected string heftSwingKey;
    protected string heftGuardKey;
    protected static float HEFT_SLOW_RAMP_SPEED = 3;

    protected float hitRadius = 0.1f;
    protected float defendRadius = 0.1f;
    protected Collider blade;

    protected SpiritFlame flames;

    private GameObject impaledObject;

    protected override void Awake()
    {
        base.Awake();    
        HitBox = GetComponent<CapsuleCollider>() ? GetComponent<CapsuleCollider>() : gameObject.AddComponent<CapsuleCollider>();
        Filter = GetComponent<MeshFilter>() ? GetComponent<MeshFilter>() : gameObject.AddComponent<MeshFilter>();
        Renderer = GetComponent<MeshRenderer>() ? GetComponent<MeshRenderer>() : gameObject.AddComponent<MeshRenderer>();
        Body = GetComponent<Rigidbody>() ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();
        List<Material> weaponMaterials = new List<Material>();
        weaponMaterials.Add(Instantiate(Resources.Load<Material>("Materials/Weapons/Steel")));
        weaponMaterials.Add(Instantiate(Resources.Load<Material>("Materials/Weapons/Wood")));
        weaponMaterials.Add(Instantiate(Resources.Load<Material>("Materials/Weapons/Brass")));
        Renderer.materials = weaponMaterials.ToArray();
        Renderer.materials[0].EnableKeyword("_EMISSION");
        Renderer.materials[0].SetColor("_EmissionColor", Color.black); 
        Renderer.sharedMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
    }

    protected override void Start()
    {
        base.Start();
        transform.localScale *= Character.Scale;
        Anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animation/items/" + gameObject.name + "/" + gameObject.name +"Controller");
        Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        HitBox.isTrigger = true;
        foreach(Collider box in PhysicsBoxes)
        {
            box.isTrigger = false;
        }
        swingClip = (equipType == EquipType.TwoHanded) ? "Audio/Weapons/deepSwing" : "Audio/Weapons/midSwing";
        swingPitch = (equipType == EquipType.TwoHanded ? 50 : 20) / BasePower;
        clashClip = "Audio/Weapons/clang";
        clashPitch = 60 / BasePower;
        clashVolume = 0.075f;
        tinkPitch = 20 / BasePower;
        if(Heft == 0)
        {
            Heft = BasePower;
        }
        //heftTurnSlow = -Heft / 100f;
        heftSwingKey = "heftSwing" + gameObject.GetHashCode().ToString();
        heftGuardKey = "heftGuard" + gameObject.GetHashCode().ToString();
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        for (int i = 0; i < mesh.subMeshCount-1; i++)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.center = mesh.GetSubMesh(i).bounds.center;
            box.size = mesh.GetSubMesh(i).bounds.extents * 2;
            PhysicsBoxes.Add(box);
            if(i == 0)
            {
                box.size = box.size * 0.75f;
                blade = box;
            }
        }
        flames = Instantiate(Game.SpiritFlameTemplate).GetComponent<SpiritFlame>();
        flames.bindToObject(gameObject);
        flames.flamePreset = SpiritFlame.Preset.Magic;
        gameObject.AddComponent<WeaponRangeFinder>();
        StartCoroutine(routineTempo());
    }

    protected override void Update()
    {
        base.Update();
        Power = Mathf.Max(modPower.Values.Aggregate(BasePower, (result, increment) => result += increment), 0);
        if (Wielder)
        {
            ActionCurrentlyAnimated = getActionFromCurrentAnimationState();
            togglePhysicsBox(false);
            heftSlowModifier = -Heft / Wielder.Strength;
            modPower["TrueStrike"] = TrueStrike ? BasePower * Wielder.Strength/100 : 0;
            modPower["momentum"] = Vector3.Project(Wielder.body.velocity, Wielder.LookDirection).magnitude * BasePower / Character.Max_Velocity_Of_Dash;
            if (transform.parent != Wielder.transform)
            {
                transform.SetParent(Wielder.transform, true);
            }
            if (Wielded)
            {
                if (ActionCurrentlyAnimated != Action.Attacking && ActionCurrentlyAnimated != Action.Recovering && ActionCurrentlyAnimated != Action.Coiling)
                {
                    TrueStrike = false;
                }
                damageType = getDamageTypeFromCurrentAnimationState();
                if (ActionCurrentlyAnimated == Action.Recoiling)
                {
                    alreadyHit = new List<GameObject>();
                    attackONS = true;
                    HitBox.enabled = false;
                    modifyWielderSpeed(swingValue: Mathf.Lerp(0, -1, (rebukePeriod - rebukeTimer) * 3));
                    setHighlightColor(Color.gray);
                }
                else if (ActionCurrentlyAnimated == Action.Idle)
                {
                    alreadyHit = new List<GameObject>();
                    attackONS = true;
                    HitBox.enabled = false;
                    modifyWielderSpeed(0, 0);
                    setHighlightColor(Color.gray);
                }
                else if (ActionCurrentlyAnimated == Action.Coiling)
                {                
                    attackONS = true;
                    modifyWielderSpeed(swingValue: heftSlowModifier);
                    setHighlightColor(new Color(1, 0.1f, 0.1f));
                }
                else if(ActionCurrentlyAnimated == Action.Windup)
                {
                    modifyWielderSpeed(swingValue: 0);
                    setHighlightColor(new Color(1, 0.1f, 0.1f));
                }
                else if (ActionCurrentlyAnimated == Action.Attacking)
                {
                    if (attackONS)
                    {
                        if (MathF.Abs(TempoTargetCenter - Tempo) <= TempoTargetWidth/2)
                        {
                            TrueStrike = true;
                        }
                        //modPower["Tempo"] = Wielder.Tempo * BasePower;
                        EventSwinging.Invoke();
                        attackONS = false;
                        alreadyHit = new List<GameObject>();
                        HitBox.isTrigger = true;
                        HitBox.enabled = true;
                        HitBox.GetComponent<CapsuleCollider>().radius = hitRadius;
                        playSwing();
                    }
                    modifyWielderSpeed(swingValue: heftSlowModifier);
                    setHighlightColor(new Color(1, 0.1f, 0.1f));
                }
                else if (ActionCurrentlyAnimated == Action.Guarding)
                {
                    alreadyHit = new List<GameObject>();
                    attackONS = true;
                    HitBox.enabled = !nextAnimation.IsTag("Rebuked");
                    HitBox.GetComponent<CapsuleCollider>().radius = defendRadius;
                    modifyWielderSpeed(heftSlowModifier, 0);
                    setHighlightColor(new Color(0.1f, 0.1f, 1));
                }
                else if (ActionCurrentlyAnimated == Action.Aiming)
                {
                    modifyWielderSpeed(swingValue: heftSlowModifier);
                    setHighlightColor(new Color(1, 0.1f, 0.1f));
                }
                else if (ActionCurrentlyAnimated == Action.Throwing)
                {
                    alreadyHit = new List<GameObject>();
                    modifyWielderSpeed(0, 0);
                }
                else if (ActionCurrentlyAnimated == Action.Parrying)
                {
                    HitBox.isTrigger = true;
                    HitBox.enabled = true;
                    HitBox.GetComponent<CapsuleCollider>().radius = defendRadius;
                }

                Anim.SetBool("primary", PrimaryTrigger && !Rebuked);
                Anim.SetBool("secondary", Secondary && !Rebuked);
                Anim.SetBool("tertiary", Tertiary && !Rebuked);
                Anim.SetBool("rebuked", Rebuked);
                Anim.Update(0);
            }
            else
            {
                setHighlightColor(Color.black);
                resetWeapon();
                HitBox.enabled = false;
                TrueStrike = false;
            }
        }
        else
        {
            TrueStrike = Thrown ? TrueStrike : false;
            if (MostRecentWielder)
            {
                modifyWielderSpeed(0, 0);
            }
            if (!MountTarget)
            {
                resetWeapon();
            }
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);
        if (Thrown)
        {
            Wieldable item = collision.gameObject.GetComponent<Wieldable>();
            Character foe = collision.gameObject.GetComponent<Character>();
            if (foe ? foe.Allegiance != MostRecentWielder.Allegiance : false)
            {
                if (RESOLVE_STRIKE(this, foe))
                {
                    StartCoroutine(ImpaleRoutine(collision));
                }
            }
            else if (collision.gameObject.layer == Game.layerObstacle || collision.gameObject.layer == Game.layerTile || collision.collider.gameObject.layer == Game.layerWall || (item ? item.equipType == EquipType.Burdensome : false))
            {
                playTink();
                StartCoroutine(ImpaleRoutine(collision));
            }
        }
        else if (collision.collider.gameObject.layer == Game.layerTile && contact.thisCollider == blade)
        {
            playTink();
        }                    
        base.OnCollisionEnter(collision);

    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        Power = Mathf.Max(modPower.Values.Aggregate(BasePower, (result, increment) => result += increment), 0);
        if (other)
        {
            if (!alreadyHit.Contains(other.gameObject))
            {
                Character foe = other.gameObject.GetComponent<Character>();
                Weapon foeWeapon = other.gameObject.GetComponent<Weapon>();
                bool obstacle = (other.gameObject.layer == Game.layerObstacle || other.gameObject.layer == Game.layerWall);
                if (foe || foeWeapon || obstacle)
                {
                    if (foeWeapon ? foeWeapon.Allegiance != Allegiance : false)
                    {
                        if (ActionCurrentlyAnimated == Action.Attacking)
                        {
                            RESOLVE_CLASH(this, foeWeapon);
                        }
                        else
                        {
                            if (foeWeapon.Wielder)
                            {
                                entityCollisionONS(foeWeapon.Wielder);
                            }
                            else
                            {
                                alreadyHit.Add(foeWeapon.gameObject);
                            }
                        }
                    }
                    else if (foe && !other.isTrigger)
                    {
                        if (ActionCurrentlyAnimated == Action.Attacking)
                        {
                            RESOLVE_STRIKE(this, foe);
                        }
                        else
                        {
                            entityCollisionONS(foe);
                        }
                    }
                    else if (obstacle && ActionCurrentlyAnimated == Action.Attacking)
                    {
                        resolveObstacleHit(other.gameObject);
                    }
                }              
                else
                {
                    alreadyHit.Add(other.gameObject);
                }

            }
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (Wielder)
        {
            modifyWielderSpeed(0, 0);
        }
    }

    /**********PUBLIC**************/

    public void Rebuke(float recoveryTime, bool postureAgnostic = false)
    {
        if (recoveryTime >= rebukePeriod - rebukeTimer)
        {
            rebukeTimer = 0;
            rebukePeriod = recoveryTime;
            Rebuked = true;
            HitBox.enabled = false;
        }
    }

    /**********PROTECTED**************/



    /**********PRIVATE**************/


    private static void RESOLVE_PARRY(Weapon Parried, Weapon Parrier)
    {
        if (Parried.Wielder)
        {
            //float parryMagnitude = Mathf.Sqrt(Parried.Heft * Parrier.Heft);
            Parried.Wielder.Rebuke(Parried.Heft / Parried.Wielder.Strength);
            if (Parried.Wielder.posture == Character.Posture.Stiff)
            {
                //Parried.Wielder.alterPoise(-Parried.Power);
                Parried.Wielder.Disarm();
            }
            else
            {
                //Parried.Wielder.alterPoise(-Parried.Power);
            }
            Parried.playClang(1.0f);
        }
        else if (Parried.Thrown)
        {
            Parried.Body.velocity = -Parried.Body.velocity;
            Parried.MostRecentWielder = Parrier.Wielder;
        }
        Parrier.EventParriedWeapon.Invoke(Parried);
        Parrier.itemCollisionONS(Parried);
    }

    private static void RESOLVE_BLOCK(Weapon Attacker, Weapon Blocker)
    {
        Blocker.EventBlockedWeapon.Invoke(Attacker);
        Attacker.EventClashedWeapon.Invoke(Blocker);
        //Blocker.Wielder.alterPoise(-Attacker.Power * Attacker.Heft / Blocker.Wielder.Strength);
        APPLY_WEAPON_SHOVE_TO_FOE(Attacker, Blocker.Wielder, scalar: 0.5f);
        Attacker.playClang(2.0f);        
    }

    private static void RESOLVE_CLASH(Weapon Attacker, Weapon Defender)
    {
        if (!Defender.Wielder)
        {
            Attacker.itemCollisionONS(Defender);
            return;
        }
        GameObject obstruction = testObstructionBetweenEntities(Defender.Wielder, Attacker.MostRecentWielder);
        if (obstruction ? obstruction != Defender.gameObject : false)
        {
            return;
        }
        if (Attacker.TrueStrike)
        {
            Attacker.itemCollisionONS(Defender);
            Attacker.playClang(1.0f);
            Defender.Wielder.Disarm();
            return;
        }
        Attacker.entityCollisionONS(Defender.Wielder);
        if (Defender.ActionCurrentlyAnimated == Action.Parrying)
        {
            RESOLVE_PARRY(Attacker, Defender);
        }
        else
        {
            RESOLVE_BLOCK(Attacker, Defender);
        }   
    }

    private static bool RESOLVE_STRIKE(Weapon weapon, Character entity)
    {
        if (entity.Allegiance != weapon.Allegiance)
        {
            weapon.entityCollisionONS(entity);
            GameObject obstruction = testObstructionBetweenEntities(entity, weapon.MostRecentWielder);
            if (obstruction)
            {
                Weapon entityWeapon = obstruction.GetComponent<Weapon>();
                if(!entityWeapon)
                {
                    weapon.entityCollisionONS(entity);
                    return false;
                }
                else 
                {
                    if (!weapon.TrueStrike)
                    {
                        RESOLVE_CLASH(weapon, entityWeapon);
                        return false;
                    }
                }
            }
            weapon.EventHitting.Invoke(entity);
            entity.Damage(weapon.Power);
            APPLY_WEAPON_SHOVE_TO_FOE(weapon, entity);
            weapon.playSlap(entity.transform.position);
            weapon.entityCollisionONS(entity);
            On_Weapon_Hit.Invoke(weapon);
            return true;                        
        }
        return false;       
    }


    private void resolveObstacleHit(GameObject obstacle)
    {
        playTink();
        alreadyHit.Add(obstacle);
    }

    private IEnumerator ImpaleRoutine(Collision collision)
    {
        setHighlightColor(Color.black);
        Character foe = collision.gameObject.GetComponent<Character>();
        ContactPoint contact = collision.GetContact(0);
        float dot = Vector3.Dot(contact.normal.normalized, collision.relativeVelocity.normalized);
        bool cleanhit = contact.thisCollider == blade && (collision.relativeVelocity.magnitude > 0 ? Mathf.Abs(dot) > (foe ? 0.25f : 0.75f) : true);
        if (cleanhit && !ImpalingSomething)
        {
            impaledObject = collision.gameObject;
            Thrown = false;
            ImpalingSomething = true;
            Body.isKinematic = true;
            transform.SetParent(collision.gameObject.transform, true);
            togglePhysicsBox(false);
            Vector3 bladeLocation = transform.TransformPoint((blade as BoxCollider).center);
            Vector3 disposition = contact.point - bladeLocation;
            Vector3 checkDisposition = contact.point - MostRecentWielder.transform.position;
            if (Vector3.Dot(disposition, checkDisposition) > 0)
            {
                transform.position += disposition * 1.25f;
            }
            else
            {
                transform.position += disposition;
            }
            if (foe)
            {
                foe.Pickup.AddListener(PickupItem);
                string key = "impaled" + gameObject.GetHashCode().ToString();
                if (foe.Vitality > 0)
                {
                    togglePhysicsBox(false);
                    alreadyHit.Add(foe.gameObject);
                    foe.EventCrashed.AddListener(impale_doupleDipDamage);
                    foe.EventVanquished.AddListener(impale_release);
                    foe.modSpeed[key] = -(Power/foe.Strength);
                    foe.BleedingWounds[key] = (Power / 5, float.PositiveInfinity);
                    yield return new WaitWhile(() => foe ? foe.posture <= Character.Posture.Flow : false);
                    if (foe)
                    {
                        impale_release();
                    }
                }
                else
                {
                    impale_release();
                }
                yield break;           
            }
            else
            {
                HitBox.enabled = true;
                GameObject impaledObject = collision.gameObject;
                yield return new WaitUntil(() => Wielder || !impaledObject);
                ImpalingSomething = false;
                togglePhysicsBox(!Wielder);
                yield break;
            }
        }
        else
        {
            yield break;
        }         
    }

    private void impale_release()
    {
        Character foe = impaledObject.GetComponent<Character>();
        if (foe)
        {
            string key = "impaled" + gameObject.GetHashCode().ToString();
            foe.EventVanquished.RemoveListener(impale_release);
            foe.modSpeed.Remove(key);
            foe.BleedingWounds.Remove(key);
        }
        transform.SetParent(Game.INSTANCE.transform.parent, true);
        togglePhysicsBox(true);
        Body.isKinematic = false;
        enabled = true;
        ImpalingSomething = false;
        DropItem(yeet: false);
    }
    
    private void impale_doupleDipDamage()
    {
        Character foe = impaledObject.GetComponent<Character>();
        if (foe)
        {
            foe.EventCrashed.RemoveListener(impale_doupleDipDamage);
            foe.Damage(Power);
            playSlap(transform.position);
        }
        impale_release();
        DropItem(yeet: true, magnitude: 1);
    }

    protected override IEnumerator pickupHandler(Character newOwner)
    {
        yield return null;
        playShing();    
        yield return base.pickupHandler(newOwner);
    }

    private static void APPLY_WEAPON_SHOVE_TO_FOE(Weapon weapon, Character foe, float scalar = 1.0f)
    {
        Vector3 origin = (weapon.Wielder ? Vector3.Lerp(weapon.transform.position, weapon.Wielder.transform.position, 0.4f) : weapon.MostRecentWielder.transform.position);
        Vector3 disposition = foe.transform.position - origin;
        Vector3 velocityChange = disposition.normalized * (weapon.Heft / 25f) * Character.Strength_Ratio(weapon.MostRecentWielder, foe) * scalar;
        foe.Shove(velocityChange);
    }

    protected void entityCollisionONS(Character entity)
    {
        if (entity)
        {
            alreadyHit.Add(entity.gameObject);
            if (entity.leftStorage)
            {
                alreadyHit.Add(entity.leftStorage.gameObject);
            }
            if (entity.rightStorage)
            {
                alreadyHit.Add(entity.rightStorage.gameObject);
            }
            if (entity.backStorage)
            {
                alreadyHit.Add(entity.backStorage.gameObject);
            }
        }
    }

    private void itemCollisionONS(Wieldable item)
    {
        alreadyHit.Add(item.gameObject);
    }

    private static GameObject testObstructionBetweenEntities(Character target, Character origin)
    {
        Vector3 disposition = target.transform.position - origin.transform.position;
        disposition.y = 0;
        Vector3 rayStart = origin.transform.position;
        rayStart.y = target.transform.position.y + 0.3f * target.scaleActual;
        RaycastHit hit;
        if (Physics.Raycast(rayStart, disposition.normalized, out hit, disposition.magnitude, (1 << Game.layerWall) + (1 << Game.layerObstacle) + (1 << Game.layerItem), QueryTriggerInteraction.Collide))
        {
            //if(alreadyHit.Contains(hit.collider.gameObject)) { return null; }
            Wieldable item = hit.collider.GetComponent<Wieldable>();
            bool opposingItem = item ? item.Allegiance != origin.Allegiance : false;
            bool obstacle = !hit.collider.isTrigger && hit.collider.gameObject && hit.collider.gameObject.layer != Game.layerItem;
            if (opposingItem || obstacle)
            {
                return hit.collider.gameObject;
            }
        }
        return null;
    }

    private void resetWeapon()
    {
        PrimaryTrigger = false;
        Secondary = false;
        Tertiary = false;
        Anim.SetBool("primary", false);
        Anim.SetBool("secondary", false);
        Anim.SetBool("tertiary", false);
        Anim.SetBool("rebuked", false);
        Rebuked = false;
        if (Wielder)
        {
            modifyWielderSpeed(0, 0);
        }
        else
        {
            modPower["Resolve"] = 0;
        }
        if (!Thrown)
        {

        }
    }

    private void modifyWielderSpeed(float guardValue = float.MaxValue, float swingValue = float.MaxValue)
    {
        if (!MostRecentWielder)
        {
            return;
        }
        if(guardValue != float.MaxValue)
        {
            if (!MostRecentWielder.modTurnSpeed.ContainsKey(heftGuardKey))
            {
                MostRecentWielder.modTurnSpeed[heftGuardKey] = 0;
            }
            if (!MostRecentWielder.modSpeed.ContainsKey(heftGuardKey))
            {
                MostRecentWielder.modSpeed[heftGuardKey] = 0;
            }
            MostRecentWielder.modTurnSpeed[heftGuardKey] = Mathf.MoveTowards(MostRecentWielder.modTurnSpeed[heftGuardKey], guardValue, Time.deltaTime * HEFT_SLOW_RAMP_SPEED);
            MostRecentWielder.modSpeed[heftGuardKey] = Mathf.MoveTowards(MostRecentWielder.modTurnSpeed[heftGuardKey], guardValue, Time.deltaTime * HEFT_SLOW_RAMP_SPEED);
        }
        if (swingValue != float.MaxValue)
        {
            if (!MostRecentWielder.modTurnSpeed.ContainsKey(heftSwingKey))
            {
                MostRecentWielder.modTurnSpeed[heftSwingKey] = 0;
            }
            if (!MostRecentWielder.modSpeed.ContainsKey(heftSwingKey))
            {
                MostRecentWielder.modSpeed[heftSwingKey] = 0;
            }
            MostRecentWielder.modTurnSpeed[heftSwingKey] = Mathf.MoveTowards(MostRecentWielder.modTurnSpeed[heftSwingKey], swingValue, Time.deltaTime * HEFT_SLOW_RAMP_SPEED);
            MostRecentWielder.modSpeed[heftSwingKey] = Mathf.MoveTowards(MostRecentWielder.modTurnSpeed[heftSwingKey], swingValue, Time.deltaTime * HEFT_SLOW_RAMP_SPEED);
        }
    }

    private IEnumerator routineTempo()
    {
        //float scalar = 50 / Heft;
        //float exponent = 1f;
        while (true)
        {

            yield return new WaitUntil(() => Wielder && ActionCurrentlyAnimated == Action.Coiling);
            while (PrimaryTrigger && ActionCurrentlyAnimated == Action.Coiling)
            {
                Tempo = Mathf.Pow(Anim.GetAnimatorTransitionInfo(0).normalizedTime, 2);
                yield return null;
            }
            if (PrimaryTrigger && ActionCurrentlyAnimated != Action.Coiling)
            {
                Tempo = 0;
            }
            //yield return new WaitUntil(() => { if (!Wielder || ActionCurrentlyAnimated != Action.Windup) return true; Tempo += Mathf.Pow(currentAnimation.normalizedTime * currentAnimation.length, exponent) * Time.deltaTime * scalar; return Tempo >= 1; });
            //yield return new WaitUntil(() => { if (!Wielder || ActionCurrentlyAnimated != Action.Windup) return true; Tempo -= Mathf.Pow(currentAnimation.normalizedTime * currentAnimation.length, exponent) * Time.deltaTime * scalar; return Tempo <= 0; });
            yield return new WaitWhile(() => ActionCurrentlyAnimated == Action.Coiling);
        }
    }

    private DamageType getDamageTypeFromCurrentAnimationState()
    {
        if (currentAnimation.IsTag("Slash"))
        {
            return DamageType.Slash;
        }
        else if (currentAnimation.IsTag("Smash"))
        {
            return DamageType.Smash;
        }
        else if (currentAnimation.IsTag("Stab"))
        {
            return DamageType.Stab;
        }
        else
        {
            return DamageType.none;
        }
    }

    private Action getActionFromCurrentAnimationState()
    {
        if (currentAnimation.IsTag("Recoil") || nextAnimation.IsTag("Recoil"))
        {
            return Action.Recoiling;
        }
        else if (currentAnimation.IsTag("Sheath"))
        {
            return Action.Sheathed;
        }
        else if (currentAnimation.IsTag("Idle"))
        {
            if (nextAnimation.IsTag("Guard"))
            {
                return Action.Parrying;
            }
            else if (nextAnimation.IsTag("Windup"))
            {
                return Action.Coiling;
            }
            else
            {
                return Action.Idle;
            }
        }
        else if (currentAnimation.IsTag("Guard"))
        {
            if (nextAnimation.IsTag("Idle"))
            {
                return Action.Recovering;
            }
            else if (nextAnimation.IsTag("Windup"))
            {
                return Action.Coiling;
            }
            else
            {
                return Action.Guarding;
            }
        }
        else if (currentAnimation.IsTag("Coiling") || nextAnimation.IsTag("Windup"))
        {
            return Action.Coiling;
        }
        else if (currentAnimation.IsTag("Windup"))
        {
            return Action.Windup;
        }
        else if (getDamageTypeFromCurrentAnimationState() != DamageType.none)
        {
            if (nextAnimation.IsTag("Guard"))
            {
                return Action.Recovering;
            }
            else
            {
                return Action.Attacking;
            }
        }
        else if (currentAnimation.IsTag("Recovering"))
        {
            return Action.Recovering;
        }
        else if (currentAnimation.IsTag("Aim"))
        {
            return Action.Aiming;
        }
        else if (currentAnimation.IsTag("Throw"))
        {
            return Action.Throwing;
        }
        else
        {
            return Action.error;
        }
    }

    private void playClang(float pitchScalar = 2.0f)
    {
        Mullet.PlayAmbientSound(clashClip, transform.position, clashPitch * pitchScalar, clashVolume, onSoundSpawn: sound => sound.layer = Game.layerEntity);
    }

    private void playShing(float pichScalar = 1.0f)
    {
        Mullet.PlayAmbientSound("Audio/Weapons/shing", transform.position, pichScalar * 40 / BasePower, 0.25f, Mullet.Instance.DefaultAudioRange / 4, onSoundSpawn: sound => sound.layer = gameObject.layer);
    }

    private void playSlap(Vector3 position)
    {
        Mullet.PlayAmbientSound("Audio/Weapons/slap", position, Mathf.Clamp(20f / Power, 0.4f, 1.6f), 0.25f, onSoundSpawn: sound => sound.layer = Game.layerEntity);
    }

    private void playTink()
    {
        Mullet.PlayAmbientSound(tinkClip, transform.position, tinkPitch + (tinkPitch * 0.25f) * (UnityEngine.Random.value - 0.5f), 0.25f, onSoundSpawn: sound => sound.layer = gameObject.layer);
    }

    private void playSwing()
    {
        Mullet.PlayAmbientSound(swingClip, transform.position, swingPitch, 1.0f, onSoundSpawn: sound => sound.layer = gameObject.layer);
    }
    
}


