using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using static System.Collections.Specialized.BitVector32;

public abstract class Weapon : Wieldable
{
    public static UnityEvent<Weapon> Weapon_Hit = new UnityEvent<Weapon> { };

    public UnityEvent<Weapon, Entity> Hitting = new UnityEvent<Weapon, Entity>();
    public UnityEvent<Weapon, Weapon> Clashing = new UnityEvent<Weapon, Weapon>();
    public UnityEvent<Weapon, Weapon> Blocking = new UnityEvent<Weapon, Weapon>();
    public UnityEvent<Weapon, Weapon> Parrying = new UnityEvent<Weapon, Weapon>();
    public UnityEvent<Weapon, Weapon> GettingParried = new UnityEvent<Weapon, Weapon>();
    public UnityEvent<Weapon> Swinging = new UnityEvent<Weapon>();
    public UnityEvent<Weapon> Rebuked = new UnityEvent<Weapon>();

    public UnityEvent<ActionAnim, ActionAnim> ChangingActionAnimations = new UnityEvent<ActionAnim, ActionAnim>();

    public enum ActionAnim
    {
        error,

        Recoiling,
        Sheathed,
        Idle,

        QuickWindup,
        QuickCoil,
        QuickAttack,

        StrongWindup,
        StrongCoil,
        StrongAttack,

        Recovering,

        Guarding,
        Parrying,

        Aiming,
        Throwing,
    }
    public ActionAnim Action = ActionAnim.error;
    private ActionAnim actionPreviouslyAnimated = ActionAnim.error;


    public bool Throwing = false;

    public float Range = 0f;
    public float Power = 0f;
    public float BasePower = 0f;

    public Dictionary<string, float> modPower = new Dictionary<string, float>();

    public bool TrueStrikeEnabled = false;
    public bool TrueStrike = false;
    public float Tempo;
    private float tempoCharge = 0;
    public float tempoChargePeriod = 0.5f;
    public float tempoChargeExponent = 3/4f;
    private bool tempoChargeONS = true;
    //private bool attackChargeONS = true;
    public float TempoTargetCenter { get; private set; } = 0.984f;
    public float TempoTargetWidth { get; private set; } = 0.03f;


    protected string lightSwingClip;
    protected string heavySwingClip;
    protected float swingPitch;
    protected float clangPitch;
    protected string tinkClip = "Audio/Weapons/tink";
    protected float tinkPitch;
    protected string clangClip;
    protected float clangVolume;
    private bool playClashSoundONS = true;

    protected List<GameObject> alreadyHit = new List<GameObject>();
    protected bool attackONS = true;

    protected float heftSlowModifier;
    protected string heftSlowKey;
    protected static float HEFT_SLOW_RAMP_SPEED = 3;
    protected static float HEFT_TURN_RAMP_SPEED = 6;

    protected float hitRadius = 0.1f;
    protected float defendRadius = 0.1f;
    protected Collider blade;

    protected _Flames flames;



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
        _MartialController.Queue_Action(this, ActionAnim.Idle);
    }

    protected override void Start()
    {
        base.Start();
        transform.localScale *= Entity.Scale;
        Anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animation/items/" + gameObject.name + "/" + gameObject.name +"Controller");
        Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        HitBox.isTrigger = true;
        foreach(Collider box in PhysicsBoxes)
        {
            box.isTrigger = false;
        }
        lightSwingClip = "Audio/Weapons/midSwing";
        heavySwingClip = "Audio/Weapons/deepSwing";
        swingPitch = 40 / Heft;
        clangClip = "Audio/Weapons/clang";
        clangPitch = 120 / Heft;
        clangVolume = 0.075f;
        tinkPitch = 50 / Heft;
        if(Heft == 0)
        {
            Heft = BasePower;
        }
        heftSlowKey = "heft" + gameObject.GetHashCode().ToString();
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
        flames = Instantiate(Requiem.SpiritFlameTemplate).GetComponent<_Flames>();
        flames.bindToObject(gameObject);
        flames.FlamePresentationStyle = _Flames.FlameStyles.Magic;
        flames.gameObject.SetActive(false);
        gameObject.AddComponent<WeaponRangeFinder>();
        EventPickedUp.AddListener(x => flames.gameObject.SetActive(x.Wielder.requiemPlayer));
    }

    protected override void Update()
    {
        base.Update();
        Power = Mathf.Max(modPower.Values.Aggregate(BasePower, (result, increment) => result += increment), 0);
        if (Action != ActionAnim.QuickAttack && Action != ActionAnim.StrongAttack)
        {
            playClashSoundONS = true;
        }
        if (Wielder)
        {
            Action = getActionFromCurrentAnimationState();
            if (actionPreviouslyAnimated != Action)
            {
                ChangingActionAnimations.Invoke(Action, actionPreviouslyAnimated);
            }
            actionPreviouslyAnimated = Action;
            togglePhysicsBox(false);
            heftSlowModifier = -Heft / Wielder.Strength;
            if (transform.parent != Wielder.transform)
            {
                transform.SetParent(Wielder.transform, true);
            }
            if (Wielded)
            {
                chargeTempo();
                if (Action == ActionAnim.Recoiling)
                {
                    alreadyHit = new List<GameObject>();
                    attackONS = true;
                    HitBox.enabled = false;
                }
                else if (Action == ActionAnim.Idle)
                {
                    alreadyHit = new List<GameObject>();
                    attackONS = true;
                    HitBox.enabled = false;
                    TrueStrike = false;
                    modifyWielderSpeed(0);
                }
                else if (Action == ActionAnim.StrongWindup)
                {
                    attackONS = true;
                    modifyWielderSpeed(heftSlowModifier);
                }
                else if(Action == ActionAnim.StrongCoil)
                {
                    attackONS = true;
                    modifyWielderSpeed(heftSlowModifier / 2);
                }
                else if (Action == ActionAnim.QuickWindup)
                {
                    attackONS = true;
                    modifyWielderSpeed(heftSlowModifier / 2);
                }
                else if (Action == ActionAnim.QuickCoil)
                {
                    attackONS = true;
                    modifyWielderSpeed(0);
                }
                else if (Action == ActionAnim.StrongAttack || Action == ActionAnim.QuickAttack)
                {
                    if (attackONS)
                    {
                        playClashSoundONS = true;
                        attackONS = false;
                        alreadyHit = new List<GameObject>();
                        HitBox.isTrigger = true;
                        HitBox.enabled = true;
                        HitBox.GetComponent<CapsuleCollider>().radius = hitRadius;
                        Swinging.Invoke(this);
                        if(Action == ActionAnim.QuickAttack)
                        {
                            playLightSwing();
                        }
                        else if(Action == ActionAnim.StrongAttack)
                        {
                            playHeavySwing();
                        }
                    }
                    modifyWielderSpeed(heftSlowModifier, true);
                }
                else if (Action == ActionAnim.Recovering)
                {
                    modifyWielderSpeed(0);
                    alreadyHit = new List<GameObject>();
                    attackONS = true;
                }
                else if (Action == ActionAnim.Guarding)
                {
                    HitBox.enabled = !nextAnimation.IsTag("Rebuked");
                    HitBox.GetComponent<CapsuleCollider>().radius = defendRadius;
                    modifyWielderSpeed(heftSlowModifier / 2);
                }
                else if (Action == ActionAnim.Aiming)
                {
                    modifyWielderSpeed(heftSlowModifier);
                }
                else if (Action == ActionAnim.Throwing)
                {
                    alreadyHit = new List<GameObject>();
                    modifyWielderSpeed(0);
                }
                else if (Action == ActionAnim.Parrying)
                {
                    HitBox.isTrigger = true;
                    HitBox.enabled = true;
                    HitBox.GetComponent<CapsuleCollider>().radius = defendRadius;
                }
                bool availableToGuard = !((Action == ActionAnim.Recovering) || (Action == ActionAnim.QuickAttack) || (Action == ActionAnim.StrongAttack));
                Anim.SetBool("primary", PrimaryTrigger && !Recoiling);
                Anim.SetBool("secondary", SecondaryTrigger && availableToGuard && !Recoiling);
                Anim.SetBool("tertiary", TertiaryTrigger && !Recoiling);
                Anim.SetBool("rebuked", Recoiling);
                Anim.Update(0);
            }
            else
            {
                setHighlightColor(Color.black);
                resetWeapon();
                HitBox.enabled = false;
            }
        }
        else
        {
            if (MostRecentWielder)
            {
                modifyWielderSpeed(0);
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
            Entity foe = collision.gameObject.GetComponent<Entity>();
            if (foe ? foe.Allegiance != MostRecentWielder.Allegiance : false)
            {
                if (RESOLVE_HIT(this, foe))
                {
                    checkImpale(collision);
                }
            }
            else if (collision.gameObject.layer == Requiem.layerObstacle || collision.gameObject.layer == Requiem.layerTile || collision.gameObject.layer == Requiem.layerWall || collision.gameObject.layer == Requiem.layerItem)
            {
                playTink();
                checkImpale(collision);
            }
        }
        else if (collision.collider.gameObject.layer == Requiem.layerTile && contact.thisCollider == blade)
        {
            playTink();
        }                    
        base.OnCollisionEnter(collision);

    }

    protected void OnTriggerStay(Collider other)
    {
        //base.OnTriggerEnter(other);
        Power = Mathf.Max(modPower.Values.Aggregate(BasePower, (result, increment) => result += increment), 0);
        if (other)
        {
            if (!alreadyHit.Contains(other.gameObject))
            {
                Entity foe = other.gameObject.GetComponent<Entity>();
                Weapon foeWeapon = other.gameObject.GetComponent<Weapon>();
                bool obstacle = other.gameObject.layer == Requiem.layerObstacle || other.gameObject.layer == Requiem.layerWall;
                if (foe || foeWeapon || obstacle)
                {
                    if (foeWeapon ? foeWeapon.Allegiance != Allegiance : false)
                    {
                        if (Action == ActionAnim.StrongAttack || Action == ActionAnim.QuickAttack)
                        {
                            RESOLVE_CLASH(this, foeWeapon);
                        }
                        else
                        {
                            if (foeWeapon.Wielder)
                            {
                                FullCollisionONS(foeWeapon.Wielder.gameObject);
                            }
                            else
                            {
                                alreadyHit.Add(foeWeapon.gameObject);
                            }
                        }
                    }
                    else if (foe && !other.isTrigger)
                    {
                        if (Action == ActionAnim.StrongAttack || Action == ActionAnim.QuickAttack)
                        {
                            RESOLVE_HIT(this, foe);
                        }
                    }
                    else if (obstacle && (Action == ActionAnim.StrongAttack || Action == ActionAnim.QuickAttack) && !other.isTrigger)
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
            modifyWielderSpeed(0);
        }
    }

    /**********PUBLIC**************/


    /**********PROTECTED**************/
    protected override IEnumerator pickupHandler(Entity newOwner)
    {
        yield return null;
        //playShing();
        if (ImpaledObject)
        {
            ImpaleRelease();
        }
        yield return base.pickupHandler(newOwner);
    }

    /**********PRIVATE**************/
    private static void RESOLVE_PARRY(Weapon Attacker, Weapon Parrier)
    {
        if (Attacker.Wielder)
        {

        }
        else if (Attacker.Thrown)
        {
            Attacker.Body.velocity = -Attacker.Body.velocity;
            Attacker.MostRecentWielder = Parrier.Wielder;
        }
        Parrier.playClang(2.0f);
        Attacker.GettingParried.Invoke(Attacker, Parrier);
        Parrier.Parrying.Invoke(Parrier, Attacker);
        Parrier.itemCollisionONS(Attacker);
        Attacker.FullCollisionONS(Parrier.Wielder.gameObject);
    }

    private static void RESOLVE_BLOCK(Weapon Attacker, Weapon Blocker)
    {
        Blocker.Blocking.Invoke(Blocker, Attacker);
        Attacker.Clashing.Invoke(Attacker, Blocker);
        APPLY_WEAPON_SHOVE_TO_FOE(Attacker, Blocker.Wielder, scalar: 0.5f);
        if (Attacker.Action == ActionAnim.StrongAttack)
        {
            float scalar = Attacker.TrueStrike ? 0.5f : 2 - Attacker.Tempo;
            Attacker.playClang(scalar);
        }
        else
        {
            Attacker.playTink();
        }
        if (Blocker.Wielder)
        {
            Attacker.FullCollisionONS(Blocker.Wielder.gameObject);
        }
    }

    private static void RESOLVE_CLASH(Weapon Attacker, Weapon Defender)
    {
        if (!Defender.Wielder)
        {
            Attacker.itemCollisionONS(Defender);
            return;
        }
        GameObject obstruction = getObstructionBetweenEntities(Defender.Wielder, Attacker.MostRecentWielder);
        if (obstruction ? obstruction != Defender.gameObject : false)
        {
            return;
        }
        if (Attacker.Action == ActionAnim.StrongAttack)
        {
            float scalar = Attacker.TrueStrike ? 0.5f : 2 - Attacker.Tempo;
            Attacker.playClang(scalar);
        }
        else
        {
            Attacker.playTink();
        }
        if (Defender.Action == ActionAnim.Parrying)
        {
            RESOLVE_PARRY(Attacker, Defender);
        }
        else if (Defender.Action == ActionAnim.Guarding)
        {
            RESOLVE_BLOCK(Attacker, Defender);
        }
        else
        {
            Attacker.itemCollisionONS(Defender);
            //RESOLVE_BLOCK(Attacker, Defender);
        }
    }

    private static bool RESOLVE_HIT(Weapon weapon, Entity foe)
    {
        if(foe.Allegiance == weapon.Allegiance) {  return false; }
        if (testBlockBetweenEntities(foe, weapon.MostRecentWielder))
        {
            RESOLVE_BLOCK(weapon, foe.MainHand.GetComponent<Weapon>());
        }
        else if (!getObstructionBetweenEntities(foe, weapon.MostRecentWielder))
        {
            weapon.Hitting.Invoke(weapon, foe);
            APPLY_WEAPON_SHOVE_TO_FOE(weapon, foe);
            weapon.playSlap(foe.transform.position);
            weapon.FullCollisionONS(foe.gameObject);
            Weapon_Hit.Invoke(weapon);
            return true;
        }
        else
        {
            weapon.FullCollisionONS(foe.gameObject);
        }
        return false;      
    }


    private void resolveObstacleHit(GameObject obstacle)
    {
        if (Action == ActionAnim.StrongAttack)
        {
            float scalar = TrueStrike ? 0.5f : 2 - Tempo;
            playClang(scalar);
        }
        else
        {
            playTink();
        }
        alreadyHit.Add(obstacle);
    }

    private void checkImpale(Collision collision)
    {
        Entity foe = collision.gameObject.GetComponent<Entity>();
        setHighlightColor(Color.black);
        ContactPoint contact = collision.GetContact(0);
        float dot = Vector3.Dot(contact.normal.normalized, collision.relativeVelocity.normalized);
        bool cleanhit = contact.thisCollider == blade && (collision.relativeVelocity.magnitude > 0 ? Mathf.Abs(dot) > (foe ? 0.25f : 0.5f) : true);
        if (cleanhit && !ImpaledObject)
        {
            StartCoroutine(ImpaleRoutine(collision.gameObject, contact.point));
        }
    }

    private IEnumerator ImpaleRoutine(GameObject collidedObject, Vector3 contactPoint)
    {
        Entity foe = collidedObject.GetComponent<Entity>();
        if (foe)
        {
            Hitting.Invoke(this, foe);
        }
        ImpaledObject = collidedObject;
        Thrown = false;
        //ImpalingSomething = true;
        Body.isKinematic = true;
        transform.SetParent(collidedObject.transform, true);
        togglePhysicsBox(false);
        Vector3 bladeLocation = transform.TransformPoint((blade as BoxCollider).center);
        Vector3 disposition = contactPoint - bladeLocation;
        Vector3 checkDisposition = contactPoint - MostRecentWielder.transform.position;
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
            foe.EventAttemptPickup.AddListener(PickupItem);
            string key = "impaled" + gameObject.GetHashCode().ToString();
            if (foe.Vitality > 0)
            {
                togglePhysicsBox(false);
                alreadyHit.Add(foe.gameObject);
                foe.JustCrashed.AddListener(impale_doupleDipDamage);
                foe.JustVanquished.AddListener(ImpaleRelease);
                foe.modSpeed[key] = -(Heft/foe.Strength);
                //foe.BleedingWounds[key] = (BasePower / 5, float.PositiveInfinity);
                yield return new WaitWhile(() => foe ? foe.Posture != Entity.PostureStrength.Strong : false);
                if (foe)
                {
                    ImpaleRelease();
                }
            }
            else
            {
                ImpaleRelease();
            }
            yield break;           
        }
        else
        {
            HitBox.enabled = true;
            GameObject impaledObject = collidedObject;
            yield return new WaitUntil(() => Wielder || !impaledObject);
            //ImpalingSomething = false;
            togglePhysicsBox(!Wielder);
            yield break;
        }
    
    }

    public void ImpaleRelease()
    {
        if (!ImpaledObject) { return; }
        Entity foe = ImpaledObject.GetComponent<Entity>();                                                                                                                                                                                   
        if (foe)
        {
            string key = "impaled" + gameObject.GetHashCode().ToString();
            foe.JustCrashed.RemoveListener(impale_doupleDipDamage);
            foe.JustVanquished.RemoveListener(ImpaleRelease);
            foe.modSpeed.Remove(key);
            //foe.BleedingWounds.Remove(key);
        }
        transform.SetParent(Requiem.INSTANCE.transform.parent, true);
        togglePhysicsBox(true);
        Body.isKinematic = false;
        enabled = true;
        //ImpalingSomething = false;        
        ImpaledObject = null;
        DropItem(yeet: false);
        playShing();
    }
    
    private void impale_doupleDipDamage()
    {
        Entity foe = ImpaledObject.GetComponent<Entity>();
        if (foe)
        {
            foe.JustCrashed.RemoveListener(impale_doupleDipDamage);
            foe.Damage(Power);
            playSlap(transform.position);
        }
        //ImpaleRelease();
        //DropItem(yeet: true, magnitude: 1);
    }


    private static void APPLY_WEAPON_SHOVE_TO_FOE(Weapon weapon, Entity foe, float scalar = 1.0f)
    {
        if (!foe || !weapon.MostRecentWielder) { return; }
        float impactPower = weapon.Heft * (1 + weapon.Tempo);
        Vector3 origin = (weapon.Wielder ? Vector3.Lerp(weapon.transform.position, weapon.Wielder.transform.position, 0.4f) : weapon.MostRecentWielder.transform.position);
        Vector3 disposition = foe.transform.position - origin;
        Vector3 velocityChange = disposition.normalized * (impactPower / 40f) * Entity.Strength_Ratio(weapon.MostRecentWielder, foe) * scalar;
        foe.Shove(velocityChange);
    }

    private void FullCollisionONS(GameObject obj)
    {
        if (obj)
        {
            alreadyHit.Add(obj);
            foreach(Transform transform in obj.transform)
            {
                alreadyHit.Add(transform.gameObject);
            }
        }
    }

    private void itemCollisionONS(Wieldable item)
    {
        alreadyHit.Add(item.gameObject);
    }

    private static bool testBlockBetweenEntities(Entity target, Entity origin)
    {
        if (target == null || origin == null) { return false; }
        float targetVsOriginAngle = Mathf.Abs(Vector3.Angle(target.LookDirection, origin.LookDirection));
        float marginFromHeadOn = 45;
        float differenceFromHeadOnAngle = 180 - targetVsOriginAngle;
        return target.Defending && differenceFromHeadOnAngle <= marginFromHeadOn;
    }

    private static GameObject getObstructionBetweenEntities(Entity target, Entity origin)
    {
        if(!target || !origin) { return null; }
        Vector3 disposition = target.transform.position - origin.transform.position;
        disposition.y = 0;
        Vector3 rayStart = origin.transform.position;
        rayStart.y = target.transform.position.y + 0.3f * target.scaleActual;
        RaycastHit hit;
        if (Physics.Raycast(rayStart, disposition.normalized, out hit, disposition.magnitude, (1 << Requiem.layerWall) + (1 << Requiem.layerObstacle), QueryTriggerInteraction.Collide))
        {
            bool obstacle = !hit.collider.isTrigger && hit.collider.gameObject;
            if (obstacle)
            {
                return hit.collider.gameObject;
            }
        }
        return null;
    }

    private void resetWeapon()
    {
        PrimaryTrigger = false;
        SecondaryTrigger = false;
        TertiaryTrigger = false;
        Anim.SetBool("primary", false);
        Anim.SetBool("secondary", false);
        Anim.SetBool("tertiary", false);
        Anim.SetBool("rebuked", false);
        if (MostRecentWielder)
        {
            modifyWielderSpeed(0);
        }
        if (!Thrown)
        {

        }
    }

    private void modifyWielderSpeed(float value, bool lockRotation = false)
    {
        if (MostRecentWielder)
        {
            if (!MostRecentWielder.modTurnSpeed.ContainsKey(heftSlowKey))
            {
                MostRecentWielder.modTurnSpeed[heftSlowKey] = 0;
            }
            if (!MostRecentWielder.modSpeed.ContainsKey(heftSlowKey))
            {
                MostRecentWielder.modSpeed[heftSlowKey] = 0;
            }
            MostRecentWielder.modTurnSpeed[heftSlowKey] =  Mathf.MoveTowards(MostRecentWielder.modTurnSpeed[heftSlowKey], lockRotation ? -0.95f : value, Time.deltaTime * HEFT_TURN_RAMP_SPEED);
            MostRecentWielder.modSpeed[heftSlowKey] = Mathf.MoveTowards(MostRecentWielder.modSpeed[heftSlowKey], value, Time.deltaTime * HEFT_SLOW_RAMP_SPEED);

        }
    }

    private ActionAnim getActionFromCurrentAnimationState()
    {
        if (currentAnimation.IsTag("Recoil") || nextAnimation.IsTag("Recoil"))
        {
            return ActionAnim.Recoiling;
        }
        else if (currentAnimation.IsTag("Sheath"))
        {
            return ActionAnim.Sheathed;
        }
        else if (currentAnimation.IsTag("Idle"))
        {
            if (nextAnimation.IsTag("Guard"))
            {
                return ActionAnim.Parrying;
            }
            else if (nextAnimation.IsTag("Windup"))
            {
                return ActionAnim.StrongWindup;
            }
            else if (nextAnimation.IsTag("StrongCoil"))
            {
                return ActionAnim.StrongWindup;
            }
            else if (nextAnimation.IsTag("QuickCoil"))
            {
                return ActionAnim.QuickWindup;
            }
            else
            {
                return ActionAnim.Idle;
            }
        }
        else if (currentAnimation.IsTag("Guard"))
        {
            if (nextAnimation.IsTag("Idle"))
            {
                return ActionAnim.Recovering;
            }
            else
            {
                return ActionAnim.Guarding;
            }
        }
        else if (currentAnimation.IsTag("Windup"))
        {
            return ActionAnim.StrongWindup;
        }
        else if (currentAnimation.IsTag("StrongCoil"))
        {
            if (nextAnimation.IsTag("StrongAttack"))
            {
                return ActionAnim.StrongAttack;
            }
            else
            {
                return ActionAnim.StrongCoil;
            }
        }
        else if (currentAnimation.IsTag("QuickCoil"))
        {
            return ActionAnim.QuickCoil;
        }
        else if (currentAnimation.IsTag("QuickAttack"))
        {
            if (nextAnimation.IsTag("Guard"))
            {
                return ActionAnim.Recovering;
            }
            else if (nextAnimation.IsTag("Idle"))
            {
                return ActionAnim.Recovering;
            }
            else
            {
                return ActionAnim.QuickAttack;
            }
        }
        else if (currentAnimation.IsTag("StrongAttack"))
        {
            if (nextAnimation.IsTag("Guard"))
            {
                return ActionAnim.Recovering;
            }
            else if (nextAnimation.IsTag("Idle"))
            {
                return ActionAnim.Recovering;
            }
            else if (nextAnimation.IsTag("Windup"))
            {
                return ActionAnim.StrongWindup;
            }
            else if (nextAnimation.IsTag("StrongCoil"))
            {
                return ActionAnim.StrongWindup;
            }
            else
            {
                return ActionAnim.StrongAttack;
            }
        }
        else if (currentAnimation.IsTag("Recovering"))
        {
            if (nextAnimation.IsTag("QuickCoil") || nextAnimation.IsTag("QuickWindup"))
            {
                return ActionAnim.QuickWindup;
            }
            else if (nextAnimation.IsTag("StrongCoil") || nextAnimation.IsTag("Windup"))
            {
                return ActionAnim.StrongWindup;
            }
            else
            {
                return ActionAnim.Recovering;
            }
        }
        else if (currentAnimation.IsTag("Aim"))
        {
            return ActionAnim.Aiming;
        }
        else if (currentAnimation.IsTag("Throw"))
        {
            return ActionAnim.Throwing;
        }
        else
        {
            return ActionAnim.error;
        }
    }

    private void chargeTempo()
    {
        Tempo = Mathf.Clamp(convertChargeToTempo(tempoCharge), 0, 1);
        bool strong = Wielder ? Wielder.Posture != Entity.PostureStrength.Weak : false;
        TrueStrike = TrueStrikeEnabled && strong && Mathf.Abs(TempoTargetCenter - Tempo) <= TempoTargetWidth / 2f;
        tempoChargePeriod = Heft / Wielder.Strength;
        if (Action == ActionAnim.StrongCoil)
        {
            if (tempoChargeONS)
            {
                tempoChargeONS = false;
            }
            else if(tempoCharge < 1)
            {
                float increment = (Time.deltaTime / tempoChargePeriod);
                tempoCharge += increment;
                Wielder.alterPoise(-increment * Heft / 2, impactful: false);
            }
            else
            {
                Wielder.alterPoise(0, impactful: false);
            }
        }
        else if(Action != ActionAnim.StrongAttack)
        {
            TrueStrike = false;
            tempoCharge = 0;
            tempoChargeONS = true;
        }
    }

    private float convertChargeToTempo(float charge)
    {
        float frequencyVariable = Mathf.PI * charge;
        float frequencyConstant = Mathf.PI / 2;
        float amplitudeScalar = 0.5f;
        float amplitudeConstant = 0.5f;
        float function = amplitudeScalar * Mathf.Sin(frequencyVariable + frequencyConstant) + amplitudeConstant;
        return 1 - function;
    }

    private void playClang(float pitchScalar = 2.0f)
    {
        if (playClashSoundONS)
        {
            playClashSoundONS = false;
            _SoundService.PlayAmbientSound(clangClip, transform.position, clangPitch * pitchScalar, clangVolume, soundSpawnCallback: sound => sound.layer = Requiem.layerEntity);
        }
        
    }

    public void playShing(float pichScalar = 1.0f)
    {
        GameObject sound = _SoundService.PlayAmbientSound("Audio/Weapons/shing", transform.position, pichScalar * 40 / Heft, 0.25f, soundSpawnCallback: sound => sound.layer = gameObject.layer);
        sound.GetComponent<AudioSource>().time = 0.15f;
    }

    private void playSlap(Vector3 position)
    {
        _SoundService.PlayAmbientSound("Audio/Weapons/slap", position, Mathf.Pow(10f / Power, 0.75f), 0.20f, soundSpawnCallback: sound => sound.layer = Requiem.layerEntity);
    }

    private void playTink()
    {
        if (playClashSoundONS)
        {
            playClashSoundONS = false;
            _SoundService.PlayAmbientSound(tinkClip, transform.position, tinkPitch + (tinkPitch * 0.25f) * (UnityEngine.Random.value - 0.5f), 0.25f, soundSpawnCallback: sound => sound.layer = gameObject.layer);
        }  
    }

    private void playLightSwing()
    {
        _SoundService.PlayAmbientSound(lightSwingClip, transform.position, swingPitch, 1.0f, soundSpawnCallback: sound => sound.layer = gameObject.layer);
    }
    private void playHeavySwing()
    {
        _SoundService.PlayAmbientSound(heavySwingClip, transform.position, swingPitch, 1.0f, soundSpawnCallback: sound => sound.layer = gameObject.layer);
    }
    
}


