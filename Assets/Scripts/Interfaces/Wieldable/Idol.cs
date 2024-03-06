using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using static Entity;

public class Idol : Wieldable
{
    public static Idol INSTANCE;

    public _Flames flames;

    public float pitchScalar = 1.0f;
    public float NoiseImpulse = 0.015f;

    public Entity mobEntity;

    private GameObject prompt;
    private GameObject currentBlurb;

    public enum ActivityLevel
    {
        Inert,
        Approached,
        Active,
        Aggro,
    }
    public ActivityLevel activityLevel = ActivityLevel.Inert;

    private List<string> approachedMessages = new List<string>()
    {
        "Take me to the alter.",
    };
    private List<string> activeMessages = new List<string>()
    {   
        "And so the struggler returns...",
        "Greetings, struggler.",
        "Come back for more?",
        "Awoken again, have we?",
        "...",
        "Welcome back.",
        "Back in the saddle, I see",
        "What eon is it?",
        "Something wicked this way comes.",
        "Never sated for long",
        "It is no surprise to see you here again, struggler.",
        "I was getting lonely.",
        "I have been waiting for your return.",
        "How far will you make it this time?",     
        "I thought you might give up this time... imagine",          
        "You may yet escape.",
        "Return me to the alter",
        "Complete the ritual. Close the circle.",
        "Destroy the pillars of this realm.",
        "Things look a bit different around here.. but you look the same.",
        "You still look the same.",
        "All this has happened before, in some way or another.",
        "Remind us all of why you are here.",
        "Blood is the price, blood is prize.",
        "Get to work.",
        "They are coming.",
        "Enter the fray.",
        "Show them no mercy.",
    };
    private List<string> aggroMessages = new List<string>()
    {   
        "Go away.",
        "Remind us all of why you are here.",
        "Blood is the price, blood is prize.",
        "Death comes.",
        "Show me your worth!",
    };

    protected override void Awake()
    {
        base.Awake();
        PhysicsBoxes.Add(GetComponent<BoxCollider>() ? GetComponent<BoxCollider>() : gameObject.AddComponent<BoxCollider>());
        Renderer = GetComponent<MeshRenderer>() ? GetComponent<MeshRenderer>() : gameObject.AddComponent<MeshRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        gameObject.layer = Requiem.layerItem;
        Body.useGravity = true;
        Body.isKinematic = false;
        Body.mass = 0.25f;
        INSTANCE = this;
        equipType = EquipType.Burdensome;
        pitchScalar = 0.6f;
        flames = GetComponentInChildren<_Flames>();
        flames.SetFlameStyle(_Flames.FlameStyles.Soulless);
        StartCoroutine(banter());
        Body.mass = 3f;
        PhysicsBoxes.AddRange(GetComponents<Collider>().ToList());
        //dematerializeGhosts(this);
        //EventPickedUp.AddListener(materializeSoulPearls);
        //EventDropped.AddListener(dematerializeGhosts);
    }

    protected override void Update()
    {
        base.Update();
        if (mobEntity)
        {
            activityLevel = ActivityLevel.Aggro;
        }
        else if (Wielder ? Wielder.requiemPlayer : false)
        {
            activityLevel = ActivityLevel.Active;
        }
        else if((Player.INSTANCE.transform.position - transform.position).magnitude <= Hextile.Radius)
        {
            activityLevel = ActivityLevel.Approached;
        }
        else
        {
            activityLevel = ActivityLevel.Inert;
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        _SoundService.PlayAmbientSound(Requiem.boneSounds[Random.Range(0, Requiem.boneSounds.Length)], transform.position, (0.5f + 0.5f * Random.value) * pitchScalar, 0.05f, _SoundService.Instance.DefaultAudioRange / 2).layer = gameObject.layer;
    }


    /***** PUBLIC *****/

    public float SayShit(List<string> messages)
    {
        if (currentBlurb)
        {
            Destroy(currentBlurb);
        }
        string message = messages[Random.Range(0, messages.Count)];
        float duration = Mathf.CeilToInt(1 + message.Length * 0.1f);
        currentBlurb = _BlurbService.createBlurb(gameObject, message, Color.Lerp(Color.white, Color.red, 0.75f), duration, 1.0f);
        return Time.time;
    }

    public Entity BecomeMob()
    {
        if (Wielder)
        {
            Wielder.Interact.RemoveListener(PickupItem);
        }
        DropItem();
        togglePhysicsBox(false);
        Body.isKinematic = true;
        name = "Head";

        GameObject Mob = new GameObject("Nemesis");
        Mob.transform.position = transform.position;
        mobEntity = Mob.AddComponent<IdolSkully>();
        Mob.AddComponent<Nemesis>().flames = flames;

        GameObject model = mobEntity.model = new GameObject("Model");
        model.transform.SetParent(mobEntity.transform, true);
        model.transform.localPosition = Vector3.zero;
        model.transform.localEulerAngles = Vector3.zero;
        mobEntity.head = gameObject; 

        transform.SetParent(model.transform, true);
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
        transform.localScale /= Entity.Scale;

        mobEntity.JustVanquished.AddListener(BecomeItem);
        mobEntity.FinalDashEnabled = true;
        mobEntity.JustWounded.AddListener((x) => SpawnAdds(1));
        mobEntity.JustHit.AddListener(spawnAddsIfDamageMakesWeak);
        return mobEntity;
    }

    public void BecomeItem()
    {
        mobEntity.JustVanquished.RemoveListener(BecomeItem);
        togglePhysicsBox(true);
        Body.isKinematic = false;
        flames.FlamePresentationStyle = _Flames.FlameStyles.Soulless;
    }


    /***** PROTECTED *****/




    /***** PRIVATE *****/
    private IEnumerator banter()
    {
        while (true)
        {
            switch (activityLevel)
            {
                case ActivityLevel.Inert:
                    if (currentBlurb)
                    {
                        Destroy(currentBlurb);
                    }
                    flames.emissionModule.enabled = false;
                    yield return new WaitWhile(() => activityLevel == ActivityLevel.Inert);
                    break;
                case ActivityLevel.Approached:
                    if (currentBlurb)
                    {
                        Destroy(currentBlurb);
                    }
                    flames.emissionModule.enabled = true;
                    SayShit(approachedMessages);
                    yield return new WaitWhile(() => activityLevel == ActivityLevel.Approached);
                    break;
                case ActivityLevel.Active:
                    if (currentBlurb)
                    {
                        Destroy(currentBlurb);
                    }
                    flames.emissionModule.enabled = true;
                    SayShit(activeMessages);
                    yield return new WaitWhile(() => activityLevel == ActivityLevel.Active);
                    break;
                case ActivityLevel.Aggro:
                    if (currentBlurb)
                    {
                        Destroy(currentBlurb);
                    }
                    flames.emissionModule.enabled = true;
                    SayShit(aggroMessages);
                    yield return new WaitWhile(() => activityLevel == ActivityLevel.Aggro);
                    break;
            }
            yield return null;
        }
    }

    /***** PUBLIC *****/
    public List<GameObject> SpawnAdds(int count)
    {
        List<GameObject> skullies = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            Entity entity;
            entity = Requiem.SPAWN(typeof(Skully), typeof(Biter), transform.position).GetComponent<Entity>();
            entity.Poise = entity.Strength;
            Vector3 randomOffset = new Vector3(UnityEngine.Random.value - 0.5f, 0, UnityEngine.Random.value - 0.5f) * 0.1f;
            entity.transform.position += randomOffset;
            entity.Shoved = false;
            skullies.Add(entity.gameObject);
        }
        return skullies;
    }

    private void spawnAddsIfDamageMakesWeak(float totalDamage)
    {
        if (totalDamage > mobEntity.Poise && mobEntity.Posture != PostureStrength.Weak)
        {
            SpawnAdds(Requiem_Arena.INSTANCE.Ritual);
        }
    }

    /***** PRIVATE *****/
    //private void materializeSoulPearls(Wieldable idol)
    //{
    //    List<SoulPearl> pearls = FindObjectsOfType<SoulPearl>().ToList();
    //    foreach (SoulPearl soulPearl in pearls)
    //    {
    //        soulPearl.FlyToPhylactery();
    //    }
    //}
    //private void dematerializeGhosts(Wieldable idol)
    //{
    //    List<Ghosty> ghosts = FindObjectsOfType<Ghosty>().ToList();
    //    foreach (Ghosty ghosty in ghosts)
    //    {
    //        ghosty.Die();
    //    }
    //}

}
