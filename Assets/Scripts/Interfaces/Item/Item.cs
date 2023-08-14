using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Item : MonoBehaviour
{    
    public static RuntimeAnimatorController defaultAnimController;
    public static RuntimeAnimatorController defaultAnimController2H;

    public UnityEvent EventDropped = new UnityEvent();
    public UnityEvent EventPickedUp = new UnityEvent();
    public UnityEvent EventThrown = new UnityEvent();

    public bool ImpalingSomething = false;

    public float Heft = 25;

    public bool Idling = true;
    public bool Rebuked = false;
    protected float rebukeTimer = 0f;
    protected float rebukePeriod = 0.0f;

    public Character Wielder;
    public Character.Loyalty Allegiance = Character.Loyalty.neutral;
    public bool Wielded = false;
    public GameObject MountTarget;

    public bool Primary = false;
    public bool Secondary = false;
    public bool Tertiary = false;
    public bool ThrowTrigger = false;

    public Animator Anim;
    public Rigidbody Body;
    public Collider HitBox;
    public List<Collider> PhysicsBoxes = new List<Collider>();
    public MeshFilter Filter;
    public MeshRenderer Renderer;

    public Character MostRecentWielder;
    protected GameObject telecommuteTarget;
    public bool Telecommuting = false;
    public bool Thrown = false;

    public AnimatorStateInfo currentAnimation;
    public AnimatorStateInfo nextAnimation;

    public enum EquipType
    {
        Unequippable = -1,
        Consummable = 0,
        OneHanded = 1,
        TwoHanded = 2,  
        Burdensome = 3,
    }
    public EquipType equipType = EquipType.Unequippable;

    protected virtual void Awake()
    {
        Anim = GetComponent<Animator>() ? GetComponent<Animator>() : gameObject.AddComponent<Animator>();
        Renderer = GetComponent<MeshRenderer>() ? GetComponent<MeshRenderer>() : gameObject.AddComponent<MeshRenderer>();
        Body = GetComponent<Rigidbody>() ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();
        Body.mass = equipType == EquipType.Burdensome ? 5 : 0.5f;
        gameObject.layer = Game.layerItem;
    }

    protected virtual void Start()
    {
        if (!defaultAnimController)
        {
            defaultAnimController = Resources.Load<RuntimeAnimatorController>("Animation/items/ItemDefault/ItemDefaultController");         
        }
        if (!defaultAnimController2H)
        {
            defaultAnimController2H = Resources.Load<RuntimeAnimatorController>("Animation/items/ItemDefault2H/ItemDefaultController2H");
        }
        StartCoroutine(throwHandler());
    }

    protected virtual void Update()
    {
        if (Wielder)
        {
            if (Rebuked)
            {
                if (rebukeTimer >= rebukePeriod)
                {
                    Rebuked = false;
                    rebukeTimer = 0;
                    rebukePeriod = 0;
                }
                else
                {
                    rebukeTimer += Time.deltaTime;
                }
            }
            MostRecentWielder = Wielder;
            if (!Anim.runtimeAnimatorController)
            {
                if (equipType == EquipType.OneHanded)
                {
                    Anim.runtimeAnimatorController = defaultAnimController;
                }
                else
                {
                    Anim.runtimeAnimatorController = defaultAnimController2H;
                }

            }
            Allegiance = Wielder.Allegiance;
            Wielded = Wielder.MainHand == this || Wielder.OffHand == this;
            currentAnimation = Anim.GetCurrentAnimatorStateInfo(0);
            nextAnimation = Anim.GetNextAnimatorStateInfo(0);
            if (Anim.enabled)
            {
                Idling = currentAnimation.IsTag("Idle") && !Rebuked;
                Anim.SetBool("throwTrigger", ThrowTrigger && !Rebuked && equipType != EquipType.Burdensome);
                Anim.SetBool("wielded", Wielded);
                if (equipType == EquipType.OneHanded)
                {
                    Anim.SetBool("duelWielding", Wielder.MainHand && Wielder.OffHand);
                    Anim.SetBool("offHand", Wielder.rightStorage == this);
                }
                if (currentAnimation.IsTag("Throw"))
                {
                    Thrown = true;
                }
            }
            else
            {
                Anim.enabled = true;
            }
            if (equipType == EquipType.Burdensome)
            {
                if (Wielded)
                {
                    Wielder.wieldMode = Character.WieldMode.Burdened;
                }
                else
                {
                    DropItem();
                }
            }
        }
        else
        {
            Anim.enabled = false;
            Wielded = false;
            Rebuked = false;
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (Thrown)
        {
            Thrown = false;
            setHighlightColor(Color.black);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other)
        {
            Character entity = other.gameObject.GetComponent<Character>();
            if (entity && !Wielder && !Thrown && !MountTarget && equipType != EquipType.Consummable)
            {
                if(equipType == EquipType.Burdensome)
                {
                    entity.Interact.AddListener(PickupItem);
                }
                else
                {
                    entity.Pickup.AddListener(PickupItem);
                }
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        Character entity = other.gameObject.GetComponent<Character>();
        if (entity && !Wielder && !Thrown && equipType != EquipType.Consummable)
        {
            if (equipType == EquipType.Burdensome)
            {
                entity.Interact.RemoveListener(PickupItem);
            }
            else
            {
                entity.Pickup.RemoveListener(PickupItem);
            }

            //setHighlightColor(Color.black);
        }
    }

    protected virtual void OnDestroy()
    {
        StopAllCoroutines();
    }

    /********** Item functions ************/
    protected void setHighlightColor(Color highlight)
    {
        if (Renderer ? Renderer.sharedMaterial : false)
        {
            Color colorActual = Color.Lerp(Color.black, highlight, 0.15f);
            if (colorActual != Renderer.sharedMaterial.GetColor("_EmissionColor"))
            {
                Renderer.sharedMaterial.SetColor("_EmissionColor", colorActual);
            }
        }
    }

    public void PickupItem(Character newOwner)
    {
        StartCoroutine(pickupHandler(newOwner));
    }

    protected virtual IEnumerator pickupHandler(Character newOwner)
    {
        Thrown = false;
        yield return null;

        if (Wielder || !newOwner)
        {
            yield break;
        }
        Wielder = newOwner;
        EventPickedUp.Invoke();
        if(equipType == EquipType.Burdensome)
        {
            if(newOwner.wieldMode == Character.WieldMode.Burdened)
            {
                yield break;
            }
            else
            {
                newOwner.Interact.RemoveListener(PickupItem);
            }
        }
        else
        {
            newOwner.Pickup.RemoveListener(PickupItem);

        }
        if (!Wielder.leftStorage && !Wielder.rightStorage && !Wielder.backStorage)
        {
            Wielder.wieldMode = Character.WieldMode.OneHanders;
        }
        if (equipType == EquipType.OneHanded)
        {
            if (Wielder.rightStorage && Wielder.leftStorage)
            {
                Wielder.rightStorage.DropItem();
            }
            if (Wielder.leftStorage)
            {
                Wielder.rightStorage = Wielder.leftStorage;
            }
            Wielder.leftStorage = this;  
        }
        else if (equipType == EquipType.TwoHanded)
        {
            if (Wielder.backStorage)
            {
                Wielder.backStorage.DropItem();
            }
            Wielder.backStorage = this;
        }
        else if (equipType == EquipType.Burdensome)
        {
            Wielder.wieldMode = Character.WieldMode.Burdened;
            Wielder.MainHand = this;
            //Wielder.modSpeed["burdensome" + GetHashCode().ToString()] = -0.25f;
        }
        else if(equipType == EquipType.Consummable)
        {
            consume(Wielder);
            yield break;
        }
        if (Body)
        {
            Body.isKinematic = true;
        }
        togglePhysicsBox(false);
        Allegiance = Wielder.Allegiance;
        transform.SetParent(Wielder.transform);
        Anim.enabled = true;
        if (gameObject.activeSelf)
        {
            Anim.Rebind();
            Anim.Update(0);
        }
        if(Wielder.leftStorage ? Wielder.leftStorage == Wielder.rightStorage : false)
        {
            Wielder.rightStorage = null;
        }
        yield break;
    }

    protected virtual void consume(Character consumer)
    {
    }

    public void DropItem(bool yeet = false, Vector3 direction = new Vector3(), float magnitude = 2)
    {
        if (Wielder)
        {
            Anim.enabled = false;
            if (HitBox)
            {
                HitBox.enabled = false;
            }
            transform.SetParent(Wielder.transform.parent);
            Wielder.modSpeed["heft" + gameObject.GetHashCode().ToString()] = 0;
            if(equipType == EquipType.Burdensome)
            {
                Wielder.modSpeed.Remove("burdensome" + GetHashCode().ToString());
                Wielder.wieldMode = (Wielder.leftStorage || Wielder.rightStorage || Wielder.backStorage) ? Character.WieldMode.OneHanders : Character.WieldMode.EmptyHanded;
            }
            Wielded = false;
            Wielder = null;
            EventDropped.Invoke();
            togglePhysicsBox(true);
            Body.useGravity = true;
            Body.isKinematic = false;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Body.angularDrag = 0f;
            Primary = false;
            Secondary = false;
            Tertiary = false;
            Wielded = false;
            setHighlightColor(Color.black);
        }
        if (yeet)
        {
            direction = direction.magnitude == 0 ? new Vector3(UnityEngine.Random.value - 0.5f, UnityEngine.Random.value, UnityEngine.Random.value - 0.5f).normalized : direction.normalized;
            Body.AddForce(direction * magnitude, ForceMode.VelocityChange);
        }
    }

    public void Telecommute(GameObject target, float telecommuteScalar, Action<Item> callback, bool enablePhysicsWhileInFlight = false, bool useScalarAsSpeed = false)
    {
        StartCoroutine(telecommuteRoutine(target, telecommuteScalar, callback, enablePhysicsWhileInFlight, useScalarAsSpeed));
    }

    private IEnumerator telecommuteRoutine(GameObject target, float teleScalar,  Action<Item> callback, bool enablePhysics, bool useScalarAsSpeed)
    {
        telecommuteTarget = target;
        Telecommuting = true;
        float timer = 0.0f;                                          
        Vector3 origin = transform.position;
        bool previousPhysicsBoxState = PhysicsBoxes.Count > 0 ? PhysicsBoxes[0].enabled : false;
        bool previousGravity = Body.useGravity;
        Body.useGravity = false;
        togglePhysicsBox(enablePhysics);
        void cancelCommute()
        {
            Telecommuting = false;
            Body.useGravity = previousGravity;
            togglePhysicsBox(previousPhysicsBoxState);
        }
        while (Telecommuting && !Wielder && target)
        {
            if (useScalarAsSpeed)
            {
                Vector3 totalDisposition = target.transform.position - transform.position;
                Vector3 increment = totalDisposition.normalized * teleScalar * Time.deltaTime;
                if (totalDisposition.magnitude <= increment.magnitude)
                {
                    callback(this);
                    cancelCommute();
                    yield break;
                }
                else
                {
                    transform.position += increment;
                }
            }
            else
            {
                timer += Time.deltaTime;
                float x = 2 * timer / teleScalar;
                x = Mathf.Clamp(x, 0f, 2f);
                float y = (Mathf.Pow(x, 2) - Mathf.Pow(x, 3) / 3);
                float scale = (y) / 1.33f;
                scale = Mathf.Clamp(scale, 0f, 1f);
                transform.position = Vector3.Lerp(origin, target.transform.position, scale);
                if (scale == 1)
                {
                    callback(this);
                    cancelCommute();
                    yield break;
                }
            }
            yield return null;
        }
        cancelCommute();
        yield break;
    }

    protected IEnumerator throwHandler()
    {
        while (true)
        {
            yield return new WaitUntil(() => Thrown && Wielder && equipType != EquipType.Burdensome);
            EventThrown.Invoke();
            setHighlightColor(Color.gray);
            float magnitude = 150/Heft;
            Vector3 direction = Wielder.LookDirection;
            direction.y = 0;
            DropItem(true, direction, magnitude);
            setHighlightColor(Color.red);
            Body.AddForce(direction * magnitude, ForceMode.VelocityChange);
            yield return new WaitWhile(() =>!Thrown);
            yield return new WaitUntil(() => Wielder);
        }
    }

    public void Mount(GameObject obj, Vector3 localPosition)
    {
        StartCoroutine(mountHandler(obj, localPosition));    
    }

    protected IEnumerator mountHandler(GameObject mountedTo, Vector3 localPosition)
    {
        MountTarget = mountedTo;
        if (Wielder)
        {
            DropItem();
            yield return null;
        }
        Body.isKinematic = true;
        togglePhysicsBox(false);
        transform.SetParent(mountedTo.transform, true);
        transform.localPosition = localPosition;
        yield return new WaitWhile(() => !Wielder && MountTarget);
        MountTarget = null;
        if (!Wielder && mountedTo)
        {
            transform.SetParent(mountedTo.transform.parent, true);
        }
        Body.isKinematic = false;
        togglePhysicsBox(true);
        yield break;
    }

    protected void togglePhysicsBox(bool newValue)
    {
        PhysicsBoxes.RemoveAll(x => !x);
        if (PhysicsBoxes.Count > 0)
        {
            foreach (Collider box in PhysicsBoxes.Where(x => x))
            {
                box.enabled = newValue;
            }
        }
    }



}
