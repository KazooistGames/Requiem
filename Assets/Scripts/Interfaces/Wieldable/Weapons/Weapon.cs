using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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

    public UnityEvent<ActionAnimation, ActionAnimation> ChangingActionAnimations = new UnityEvent<ActionAnimation, ActionAnimation>();

    public enum ActionAnimation
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
    public ActionAnimation ActionAnimated = ActionAnimation.error;
    private ActionAnimation actionPreviouslyAnimated = ActionAnimation.error;


    public bool Throwing = false;

    public float Range = 0f;
    public float Power = 0f;
    public float BasePower = 0f;

    public Dictionary<string, float> modPower = new Dictionary<string, float>();

    public bool TrueStrike = false;
    public float Tempo;
    private float attackCharge = 0;
    public float attackChargeRate = 1;
    //private bool attackChargeONS = true;
    public float TempoTargetCenter { get; private set; } = 0.90f;
    public float TempoTargetWidth { get; private set; } = 0.2f;


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
        flames = Instantiate(Game.SpiritFlameTemplate).GetComponent<_Flames>();
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
        if (ActionAnimated != ActionAnimation.QuickAttack && ActionAnimated != ActionAnimation.StrongAttack)
        {
            playClashSoundONS = true;
        }
        if (Wielder)
        {
            ActionAnimated = getActionFromCurrentAnimationState();
            if (actionPreviouslyAnimated != ActionAnimated)
            {
                ChangingActionAnimations.Invoke(ActionAnimated, actionPreviouslyAnimated);
            }
            actionPreviouslyAnimated = ActionAnimated;
            togglePhysicsBox(false);
            heftSlowModifier = -Heft / Wielder.Strength;
            if (transform.parent != Wielder.transform)
            {
                transform.SetParent(Wielder.transform, true);
            }
            if (Wielded)
            {
                chargeTempo();
                if (ActionAnimated == ActionAnimation.Recoiling)
                {
                    alreadyHit = new List<GameObject>();
                    attackONS = true;
                    HitBox.enabled = false;
                }
                else if (ActionAnimated == ActionAnimation.Idle)
                {
                    alreadyHit = new List<GameObject>();
                    attackONS = true;
                    HitBox.enabled = false;
                    TrueStrike = false;
                    modifyWielderSpeed(0);
                }
                else if (ActionAnimated == ActionAnimation.StrongWindup)
                {
                    attackONS = true;
                    modifyWielderSpeed(heftSlowModifier);
                }
                else if(ActionAnimated == ActionAnimation.StrongCoil)
                {
                    attackONS = true;
                    modifyWielderSpeed(heftSlowModifier / 2);
                }
                else if (ActionAnimated == ActionAnimation.QuickWindup)
                {
                    attackONS = true;
                    modifyWielderSpeed(heftSlowModifier / 2);
                }
                else if (ActionAnimated == ActionAnimation.QuickCoil)
                {
                    attackONS = true;
                    modifyWielderSpeed(0);
                }
                else if (ActionAnimated == ActionAnimation.StrongAttack || ActionAnimated == ActionAnimation.QuickAttack)
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
                        if(ActionAnimated == ActionAnimation.QuickAttack)
                        {
                            playLightSwing();
                        }
                        else if(ActionAnimated == ActionAnimation.StrongAttack)
                        {
                            playHeavySwing();
                        }

                    }
                    modifyWielderSpeed(heftSlowModifier, true);
                }
                else if (ActionAnimated == ActionAnimation.Recovering)
                {
                    modifyWielderSpeed(0);
                    alreadyHit = new List<GameObject>();
                    attackONS = true;
                }
                else if (ActionAnimated == ActionAnimation.Guarding)
                {
                    HitBox.enabled = !nextAnimation.IsTag("Rebuked");
                    HitBox.GetComponent<CapsuleCollider>().radius = defendRadius;
                    modifyWielderSpeed(heftSlowModifier / 2);
                }
                else if (ActionAnimated == ActionAnimation.Aiming)
                {
                    modifyWielderSpeed(heftSlowModifier);
                }
                else if (ActionAnimated == ActionAnimation.Throwing)
                {
                    alreadyHit = new List<GameObject>();
                    modifyWielderSpeed(0);
                }
                else if (ActionAnimated == ActionAnimation.Parrying)
                {
                    HitBox.isTrigger = true;
                    HitBox.enabled = true;
                    HitBox.GetComponent<CapsuleCollider>().radius = defendRadius;
                }
                bool availableToGuard = !((ActionAnimated == ActionAnimation.Recovering) || (ActionAnimated == ActionAnimation.QuickAttack) || (ActionAnimated == ActionAnimation.StrongAttack));
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
                bool obstacle = (other.gameObject.layer == Game.layerObstacle || other.gameObject.layer == Game.layerWall);
                if (foe || foeWeapon || obstacle)
                {
                    if (foeWeapon ? foeWeapon.Allegiance != Allegiance : false)
                    {
                        if (ActionAnimated == ActionAnimation.StrongAttack || ActionAnimated == ActionAnimation.QuickAttack)
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
                        if (ActionAnimated == ActionAnimation.StrongAttack || ActionAnimated == ActionAnimation.QuickAttack)
                        {
                            RESOLVE_HIT(this, foe);
                        }
                    }
                    else if (obstacle && (ActionAnimated == ActionAnimation.StrongAttack || ActionAnimated == ActionAnimation.QuickAttack))
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
        Blocker.playTink();
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
        GameObject obstruction = testObstructionBetweenEntities(Defender.Wielder, Attacker.MostRecentWielder);
        if (obstruction ? obstruction != Defender.gameObject : false)
        {
            return;
        }
        if (Attacker.ActionAnimated == ActionAnimation.StrongAttack)
        {
            Attacker.playClang(1.0f);
        }
        else
        {
            Attacker.playTink();
        }
        if (Defender.ActionAnimated == ActionAnimation.Parrying)
        {
            RESOLVE_PARRY(Attacker, Defender);
        }
        else if(Defender.ActionAnimated == ActionAnimation.Guarding)
        {
            RESOLVE_BLOCK(Attacker, Defender);
        }
        else
        {
            Attacker.itemCollisionONS(Defender);
        }
    }

    private static bool RESOLVE_HIT(Weapon weapon, Entity foe)
    {
        if(foe.Allegiance == weapon.Allegiance) {  return false; }
        GameObject obstruction = testObstructionBetweenEntities(foe, weapon.MostRecentWielder);
        if (!obstruction)
        {
            weapon.Hitting.Invoke(weapon, foe);
            APPLY_WEAPON_SHOVE_TO_FOE(weapon, foe);
            weapon.playSlap(foe.transform.position);
            weapon.FullCollisionONS(foe.gameObject);
            Weapon_Hit.Invoke(weapon);
            return true;
        }
        else if (!obstruction.GetComponent<Weapon>())
        {
            weapon.FullCollisionONS(foe.gameObject);
        }
        else
        {
            RESOLVE_CLASH(weapon, obstruction.GetComponent<Weapon>());
        }
        return false;      
    }


    private void resolveObstacleHit(GameObject obstacle)
    {
        if (ActionAnimated == ActionAnimation.StrongAttack)
        {
            playClang(1.0f);
        }
        else
        {
            playTink();
        }
        alreadyHit.Add(obstacle);
    }

    private IEnumerator ImpaleRoutine(Collision collision)
    {
        setHighlightColor(Color.black);
        Entity foe = collision.gameObject.GetComponent<Entity>();
        ContactPoint contact = collision.GetContact(0);
        float dot = Vector3.Dot(contact.normal.normalized, collision.relativeVelocity.normalized);
        bool cleanhit = contact.thisCollider == blade && (collision.relativeVelocity.magnitude > 0 ? Mathf.Abs(dot) > (foe ? 0.25f : 0.75f) : true);
        if (cleanhit && !ImpaledObject)
        {
            if (foe)
            {
                Hitting.Invoke(this, foe);
            }
            ImpaledObject = collision.gameObject;
            Thrown = false;
            //ImpalingSomething = true;
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
                foe.EventAttemptPickup.AddListener(PickupItem);
                string key = "impaled" + gameObject.GetHashCode().ToString();
                if (foe.Vitality > 0)
                {
                    togglePhysicsBox(false);
                    alreadyHit.Add(foe.gameObject);
                    foe.EventCrashed.AddListener(impale_doupleDipDamage);
                    foe.EventVanquished.AddListener(ImpaleRelease);
                    foe.modSpeed[key] = -(Heft/foe.Strength);
                    foe.BleedingWounds[key] = (BasePower / 5, float.PositiveInfinity);
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
                GameObject impaledObject = collision.gameObject;
                yield return new WaitUntil(() => Wielder || !impaledObject);
                //ImpalingSomething = false;
                togglePhysicsBox(!Wielder);
                yield break;
            }
        }
        else
        {
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
            foe.EventCrashed.RemoveListener(impale_doupleDipDamage);
            foe.EventVanquished.RemoveListener(ImpaleRelease);
            foe.modSpeed.Remove(key);
            foe.BleedingWounds.Remove(key);
        }
        transform.SetParent(Game.INSTANCE.transform.parent, true);
        togglePhysicsBox(true);
        Body.isKinematic = false;
        enabled = true;
        //ImpalingSomething = false;        
        ImpaledObject = null;
        DropItem(yeet: false);
    }
    
    private void impale_doupleDipDamage()
    {
        Entity foe = ImpaledObject.GetComponent<Entity>();
        if (foe)
        {
            foe.EventCrashed.RemoveListener(impale_doupleDipDamage);
            foe.Damage(Power);
            playSlap(transform.position);
        }
        ImpaleRelease();
        DropItem(yeet: true, magnitude: 1);
    }


    private static void APPLY_WEAPON_SHOVE_TO_FOE(Weapon weapon, Entity foe, float scalar = 1.0f)
    {
        if (!foe) { return; }
        Vector3 origin = (weapon.Wielder ? Vector3.Lerp(weapon.transform.position, weapon.Wielder.transform.position, 0.4f) : weapon.MostRecentWielder.transform.position);
        Vector3 disposition = foe.transform.position - origin;
        Vector3 velocityChange = disposition.normalized * (weapon.Heft / 40f) * Entity.Strength_Ratio(weapon.MostRecentWielder, foe) * scalar;
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

    private static GameObject testObstructionBetweenEntities(Entity target, Entity origin)
    {
        if(!target || !origin) { return null; }
        Vector3 disposition = target.transform.position - origin.transform.position;
        disposition.y = 0;
        Vector3 rayStart = origin.transform.position;
        rayStart.y = target.transform.position.y + 0.3f * target.scaleActual;
        RaycastHit hit;
        if (Physics.Raycast(rayStart, disposition.normalized, out hit, disposition.magnitude, (1 << Game.layerWall) + (1 << Game.layerObstacle) + (1 << Game.layerItem), QueryTriggerInteraction.Collide))
        {
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

    private ActionAnimation getActionFromCurrentAnimationState()
    {
        if (currentAnimation.IsTag("Recoil") || nextAnimation.IsTag("Recoil"))
        {
            return ActionAnimation.Recoiling;
        }
        else if (currentAnimation.IsTag("Sheath"))
        {
            return ActionAnimation.Sheathed;
        }
        else if (currentAnimation.IsTag("Idle"))
        {
            if (nextAnimation.IsTag("Guard"))
            {
                return ActionAnimation.Parrying;
            }
            else if (nextAnimation.IsTag("Windup"))
            {
                return ActionAnimation.StrongWindup;
            }
            else if (nextAnimation.IsTag("StrongCoil"))
            {
                return ActionAnimation.StrongWindup;
            }
            else if (nextAnimation.IsTag("QuickCoil"))
            {
                return ActionAnimation.QuickWindup;
            }
            else
            {
                return ActionAnimation.Idle;
            }
        }
        else if (currentAnimation.IsTag("Guard"))
        {
            if (nextAnimation.IsTag("Idle"))
            {
                return ActionAnimation.Recovering;
            }
            else
            {
                return ActionAnimation.Guarding;
            }
        }
        else if (currentAnimation.IsTag("Windup"))
        {
            return ActionAnimation.StrongWindup;
        }
        else if (currentAnimation.IsTag("StrongCoil"))
        {
            return ActionAnimation.StrongCoil;
        }
        else if (currentAnimation.IsTag("QuickCoil"))
        {
            return ActionAnimation.QuickCoil;
        }
        else if (currentAnimation.IsTag("QuickAttack"))
        {
            if (nextAnimation.IsTag("Guard"))
            {
                return ActionAnimation.Recovering;
            }
            else if (nextAnimation.IsTag("Idle"))
            {
                return ActionAnimation.Recovering;
            }
            else
            {
                return ActionAnimation.QuickAttack;
            }
        }
        else if (currentAnimation.IsTag("StrongAttack"))
        {
            if (nextAnimation.IsTag("Guard"))
            {
                return ActionAnimation.Recovering;
            }
            else if (nextAnimation.IsTag("Windup"))
            {
                return ActionAnimation.StrongWindup;
            }
            else if (nextAnimation.IsTag("StrongCoil"))
            {
                return ActionAnimation.StrongWindup;
            }
            else
            {
                return ActionAnimation.StrongAttack;
            }
        }
        else if (currentAnimation.IsTag("Recovering"))
        {
            if (nextAnimation.IsTag("QuickCoil") || nextAnimation.IsTag("QuickWindup"))
            {
                return ActionAnimation.QuickWindup;
            }
            else if (nextAnimation.IsTag("StrongCoil") || nextAnimation.IsTag("Windup"))
            {
                return ActionAnimation.StrongWindup;
            }
            else
            {
                return ActionAnimation.Recovering;
            }
        }
        else if (currentAnimation.IsTag("Aim"))
        {
            return ActionAnimation.Aiming;
        }
        else if (currentAnimation.IsTag("Throw"))
        {
            return ActionAnimation.Throwing;
        }
        else
        {
            return ActionAnimation.error;
        }
    }

    private void chargeTempo()
    {
        if(ActionAnimated == ActionAnimation.StrongCoil)
        {
            attackCharge += Time.deltaTime * attackChargeRate;
            TrueStrike = attackCharge >= 1;
        }
        else
        {
            attackCharge = 0;
        }
    }

    private void playClang(float pitchScalar = 2.0f)
    {
        if (playClashSoundONS)
        {
            playClashSoundONS = false;
            _SoundService.PlayAmbientSound(clangClip, transform.position, clangPitch * pitchScalar, clangVolume, onSoundSpawn: sound => sound.layer = Game.layerEntity);
        }
        
    }

    private void playShing(float pichScalar = 1.0f)
    {
        _SoundService.PlayAmbientSound("Audio/Weapons/shing", transform.position, pichScalar * 40 / Heft, 0.25f, _SoundService.Instance.DefaultAudioRange / 4, onSoundSpawn: sound => sound.layer = gameObject.layer);
    }

    private void playSlap(Vector3 position)
    {
        _SoundService.PlayAmbientSound("Audio/Weapons/slap", position, Mathf.Pow(10f / Power, 0.75f), 0.25f, onSoundSpawn: sound => sound.layer = Game.layerEntity);
    }

    private void playTink()
    {
        if (playClashSoundONS)
        {
            playClashSoundONS = false;
            _SoundService.PlayAmbientSound(tinkClip, transform.position, tinkPitch + (tinkPitch * 0.25f) * (UnityEngine.Random.value - 0.5f), 0.25f, onSoundSpawn: sound => sound.layer = gameObject.layer);
        }  
    }

    private void playLightSwing()
    {
        _SoundService.PlayAmbientSound(lightSwingClip, transform.position, swingPitch, 1.0f, onSoundSpawn: sound => sound.layer = gameObject.layer);
    }
    private void playHeavySwing()
    {
        _SoundService.PlayAmbientSound(heavySwingClip, transform.position, swingPitch, 1.0f, onSoundSpawn: sound => sound.layer = gameObject.layer);
    }
    
}


