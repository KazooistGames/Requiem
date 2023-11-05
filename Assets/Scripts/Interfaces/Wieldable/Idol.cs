using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Idol : Bone
{
    public static Idol INSTANCE;

    public _Flames spiritFlame;

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

    protected override void Start()
    {
        base.Start();
        INSTANCE = this;
        equipType = EquipType.Burdensome;
        pitchScalar = 0.6f;
        spiritFlame = GetComponentInChildren<_Flames>();
        spiritFlame.SetFlamePresentation(_Flames.FlameStyles.Soulless);
        spiritFlame.boundObject = gameObject;
        StartCoroutine(banter());
        Body.mass = 3f;
        PhysicsBoxes.AddRange(GetComponents<Collider>().ToList());
    }

    protected override void Update()
    {
        base.Update();

        if (MountTarget && (Player.INSTANCE ? Player.INSTANCE.HostEntity : false))
        {
            transform.LookAt(Player.INSTANCE.HostEntity.head.transform);
        }
        if (Wielder ? Wielder.requiemPlayer : false)
        {
            
        }
        else if (prompt)
        {
            Destroy(prompt);
        }
    }


    public float sayShit(List<string> messages)
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

    private IEnumerator banter()
    {
        yield return new WaitUntil(() => Player.INSTANCE);
        spiritFlame.emissionModule.enabled = false;
        float timeOfLastBlurb = 0;
        yield return new WaitUntil(() => (Player.INSTANCE.transform.position - transform.position).magnitude <= Hextile.Radius);
        spiritFlame.emissionModule.enabled = true;
        timeOfLastBlurb = sayShit(greetings);
        yield return new WaitUntil(() => (Time.time - timeOfLastBlurb) > 30 || Wielder == Player.INSTANCE.HostEntity);
        while (true)
        {
            yield return new WaitUntil(() => (Player.INSTANCE.transform.position - transform.position).magnitude <= Hextile.Radius);
            if (Wielder == Player.INSTANCE.HostEntity)
            {
                timeOfLastBlurb = sayShit(conversation);
                yield return new WaitWhile(() => Wielder == Player.INSTANCE.HostEntity || Time.time - timeOfLastBlurb >= 30);
            }       
            yield return null;
        }
    }


}
