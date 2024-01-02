using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class Idol : Wieldable
{
    public static Idol INSTANCE;

    public _Flames flames;

    public float pitchScalar = 1.0f;
    public float NoiseImpulse = 0.015f;

    private GameObject prompt;
    private GameObject currentBlurb;
    private List<string> greetings = new List<string>()
    {
        "And so the struggler returns...",
        "Greetings, struggler.",     
        "Come back for more?",
        "Awoken again, have we?",
        "...",
        "Welcome back.",       
        "Back in the saddle, I see",
        "Go away.",
        "What eon is it?",
        "Something wicked this way comes."
    };
    private List<string> conversation = new List<string>()
    {
        "Never sated for long, are you?",
        "It is no surprise to see you here again, struggler.",
        "I was getting lonely.",
        "I have been waiting for your return.",
        "How far will you make it this time?",     
        "I thought you might give up this time... imagine",     
        "What technique do you prefer these days?",        
        "You may yet escape.",
        "Take me to the alter.",
        "Return me to the alter",
        "Complete the ritual. Close the circle.",
        "End this torment.",
        "Destroy the pillars of this realm.",
        "Things look a bit different around here.. but you look the same.",
        "You still look the same.",
        "It is like you dont age..",
        "Will we find the singularity?",
        "All this has happened before, in some way or another.",
        "Remind us all of why you are here.",
        "Blood is the price, blood is prize.",
        "Death comes.",
        "Show me your worth!",
        "Get to work.",
        "This reminds me of our first battle...",
        "Relive your life, reinvent your death.",
        "They are coming.",
        "Enter the fray.",
        "Show them no mercy.",
    };

    private Entity mobEntity;

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
        flames.boundObject = gameObject;
        StartCoroutine(banter());
        Body.mass = 3f;
        PhysicsBoxes.AddRange(GetComponents<Collider>().ToList());
    }

    protected override void Update()
    {
        base.Update();
        if (mobEntity)
        {
            flames.SetFlameStyle(mobEntity.DashCharging ? _Flames.FlameStyles.Magic : _Flames.FlameStyles.Soulless);
        }
        else if (Wielder ? Wielder.requiemPlayer : false)
        {
            
        }
        else if (prompt)
        {
            Destroy(prompt);
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

    public void BecomeMob()
    {
        DropItem();
        togglePhysicsBox(false);
        Body.isKinematic = true;
        name = "Head";

        GameObject Mob = new GameObject("Nemesis");
        Mob.transform.position = transform.position;
        mobEntity = Mob.AddComponent<IdolSkully>();
        Mob.AddComponent<Nemesis>();

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
    }

    public void BecomeItem()
    {
        mobEntity.JustVanquished.RemoveListener(BecomeItem);
        togglePhysicsBox(true);
        Body.isKinematic = false;
        StartCoroutine(reviveMob(15));
        flames.FlamePresentationStyle = _Flames.FlameStyles.Soulless;
    }


    /***** PROTECTED *****/




    /***** PRIVATE *****/
    private IEnumerator reviveMob(float periodSeconds)
    {
        //float timer = 0;
        yield return new WaitForSeconds(periodSeconds);
        BecomeMob();
    }

    private IEnumerator banter()
    {
        yield return new WaitUntil(() => Player.INSTANCE);
        flames.emissionModule.enabled = false;
        float timeOfLastBlurb = 0;
        yield return new WaitUntil(() => (Player.INSTANCE.transform.position - transform.position).magnitude <= Hextile.Radius);
        flames.emissionModule.enabled = true;
        timeOfLastBlurb = SayShit(greetings);
        yield return new WaitUntil(() => (Time.time - timeOfLastBlurb) > 30 || Wielder == Player.INSTANCE.HostEntity);
        while (true)
        {
            yield return new WaitUntil(() => (Player.INSTANCE.transform.position - transform.position).magnitude <= Hextile.Radius);
            if (Wielder == Player.INSTANCE.HostEntity)
            {
                timeOfLastBlurb = SayShit(conversation);
                yield return new WaitWhile(() => Wielder == Player.INSTANCE.HostEntity || Time.time - timeOfLastBlurb >= 30);
            }       
            yield return null;
        }
    }




}
