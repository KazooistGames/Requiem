using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Wieldable : MonoBehaviour
{    
    public static RuntimeAnimatorController defaultAnimController;
    public static RuntimeAnimatorController defaultAnimController2H;

    public UnityEvent<Wieldable> EventDropped = new UnityEvent<Wieldable>();
    public UnityEvent<Wieldable> EventPickedUp = new UnityEvent<Wieldable>();
    public UnityEvent<Wieldable> EventThrown = new UnityEvent<Wieldable>();

    public bool ImpalingSomething = false;

    public float Heft = 25;

    public bool Idling = true;
    public bool Recoiling = false;
    //protected float rebukeTimer = 0f;
    //protected float rebukePeriod = 0.0f;

    public Entity Wielder;
    public Entity.Loyalty Allegiance = Entity.Loyalty.neutral;
    public bool Wielded = false;
    public GameObject MountTarget;

    public bool PrimaryTrigger = false;
    public bool SecondaryTrigger = false;
    public bool TertiaryTrigger = false;
    public bool ThrowTrigger = false;

    public Animator Anim;
    public Rigidbody Body;
    public Collider HitBox;
    public List<Collider> PhysicsBoxes = new List<Collider>();
    public MeshFilter Filter;
    public MeshRenderer Renderer;

    public Entity MostRecentWielder;
    protected GameObject telecommuteTarget;
    public bool Telecommuting = false;
    public bool Thrown = false;

    public AnimatorStateInfo currentAnimation;
    public AnimatorStateInfo nextAnimation;

    public enum EquipType
    {
        OneHanded = 1,
        TwoHanded = 2,  
        Burdensome = 3,
    }
    public EquipType equipType;

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
        Recoiling = Wielder ? Wielder.Staggered : false;
        if (Wielder)
        {
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
                Idling = currentAnimation.IsTag("Idle") && !Recoiling;
                Anim.SetBool("throwTrigger", ThrowTrigger && !Recoiling && equipType != EquipType.Burdensome);
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
                    Wielder.wieldMode = Entity.WieldMode.Burdened;
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
            Entity entity = other.gameObject.GetComponent<Entity>();
            if (entity && !Wielder && !Thrown && !MountTarget)
            {
                if(equipType == EquipType.Burdensome)
                {
                    entity.EventAttemptInteraction.AddListener(PickupItem);
                }
                else
                {
                    entity.EventAttemptPickup.AddListener(PickupItem);
                }
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        Entity entity = other.gameObject.GetComponent<Entity>();
        if (entity && !Wielder && !Thrown)
        {
            if (equipType == EquipType.Burdensome)
            {
                entity.EventAttemptInteraction.RemoveListener(PickupItem);
            }
            else
            {
                entity.EventAttemptPickup.RemoveListener(PickupItem);
            }

            //setHighlightColor(Color.black);
        }
    }

    protected virtual void OnDestroy()
    {
        StopAllCoroutines();
    }

    /***** PUBLIC *****/
    public void Mount(GameObject obj, Vector3 localPosition)
    {
        StartCoroutine(mountHandler(obj, localPosition));
    }

    public void PickupItem(Entity newOwner)
    {
        StartCoroutine(pickupHandler(newOwner));
    }

    protected void setHighlightColor(Color highlight)
    {
        if (Renderer ? Renderer.sharedMaterial : false)
        {
            Color colorActual = Color.Lerp(Color.black, highlight, 0.1f);
            if (colorActual != Renderer.sharedMaterial.GetColor("_EmissionColor"))
            {
                Renderer.sharedMaterial.SetColor("_EmissionColor", colorActual);
            }
        }
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
                Wielder.wieldMode = (Wielder.leftStorage || Wielder.rightStorage || Wielder.backStorage) ? Entity.WieldMode.OneHanders : Entity.WieldMode.EmptyHanded;
            }
            Wielded = false;
            Wielder = null;
            EventDropped.Invoke(this);
            togglePhysicsBox(true);
            Body.useGravity = true;
            Body.isKinematic = false;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Body.angularDrag = 0f;
            PrimaryTrigger = false;
            SecondaryTrigger = false;
            TertiaryTrigger = false;
            Wielded = false;
            setHighlightColor(Color.black);
        }
        if (yeet)
        {
            direction = direction.magnitude == 0 ? new Vector3(UnityEngine.Random.value - 0.5f, UnityEngine.Random.value, UnityEngine.Random.value - 0.5f).normalized : direction.normalized;
            Body.AddForce(direction * magnitude, ForceMode.VelocityChange);
        }
    }

    public void Telecommute(GameObject target, float telecommuteScalar, Action<Wieldable> callback, bool enablePhysicsWhileInFlight = false, bool useScalarAsSpeed = false)
    {
        StartCoroutine(telecommuteRoutine(target, telecommuteScalar, callback, enablePhysicsWhileInFlight, useScalarAsSpeed));
    }

    /***** PROTECTED *****/
    protected virtual IEnumerator pickupHandler(Entity newOwner)
    {
        Thrown = false;
        yield return null;

        if (Wielder || !newOwner)
        {
            yield break;
        }
        Wielder = newOwner;
        Wielder.EventPickedUpWieldable.Invoke(this);
        if (!Wielder.leftStorage && !Wielder.rightStorage && !Wielder.backStorage)
        {
            Wielder.wieldMode = Entity.WieldMode.OneHanders;
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
            Wielder.wieldMode = Entity.WieldMode.Burdened;
            Wielder.MainHand = this;
            togglePhysicsBox(false);
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
        if (Wielder.leftStorage ? Wielder.leftStorage == Wielder.rightStorage : false)
        {
            Wielder.rightStorage = null;
        }
        if (equipType == EquipType.Burdensome)
        {
            if (newOwner.wieldMode != Entity.WieldMode.Burdened)
            {
                newOwner.EventAttemptInteraction.RemoveListener(PickupItem);
            }
        }
        else
        {
            newOwner.EventAttemptPickup.RemoveListener(PickupItem);
        }
        EventPickedUp.Invoke(this);
        yield break;
    }

    protected IEnumerator throwHandler()
    {
        while (true)
        {
            yield return new WaitUntil(() => Thrown && Wielder && equipType != EquipType.Burdensome);
            EventThrown.Invoke(this);
            setHighlightColor(Color.gray);
            float magnitude = 150/Heft;
            Vector3 direction = Wielder.LookDirection;
            direction.y = 0;
            DropItem(true, direction, magnitude);
            Body.AddForce(direction * magnitude, ForceMode.VelocityChange);
            yield return new WaitWhile(() =>!Thrown);
            yield return new WaitUntil(() => Wielder);
        }
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

    /***** PRIVATE *****/
    private IEnumerator mountHandler(GameObject mountedTo, Vector3 localPosition)
    {
        yield return null;
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

    private IEnumerator telecommuteRoutine(GameObject target, float teleScalar, Action<Wieldable> callback, bool enablePhysics, bool useScalarAsSpeed)
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


}
