using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghosty : Entity
{
    public Wieldable Phylactery;

    protected MeshFilter[] bodyparts;
    protected GameObject torso;
    protected GameObject leg1;
    protected GameObject leg2;

    private ParticleSystem profileParticles;
    private ParticleSystem.ShapeModule shapeModule;

    private float lifeTimer = 0;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<Animator>() == null ? gameObject.AddComponent<Animator>() : GetComponent<Animator>();
        Strength = 100f;
        Haste = 0.7f;
    }

    protected override void Start()
    {
        base.Start();
        createProfile();
        gameObject.name = "Ghosty";
        JustVanquished.AddListener(Dematerialize);
        JustDisarmed.AddListener(Die);
    }

    protected override void Update()
    {
        base.Update();
        lifeTimer += Time.deltaTime;
        if (Player.INSTANCE ? !Player.INSTANCE.HostEntity : true)
        {

        }
        else if ((Player.INSTANCE.HostEntity.transform.position - transform.position).magnitude > SoulPearl.Awareness_Radius && lifeTimer >= SoulPearl.Transition_Debounce)
        {
            Die();
        }
        Aggressive = Foe || Poise != Strength;
        Allegiance = Player.INSTANCE.HostEntity.Allegiance;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    //CUSTOM FUNCTIONS!!!!!!!!!!!!!

    public override void Damage(float damage)
    {
        base.Damage(damage);
    }
    private void createProfile()
    {
        model = model ? model : Instantiate(Resources.Load<GameObject>("Prefabs/Entities/ghostBody"));
        model.gameObject.name = "Model";
        model.transform.SetParent(transform, false);
        model.transform.localPosition = Vector3.zero;
        profileParticles = model.GetComponent<ParticleSystem>();
        shapeModule = profileParticles.shape;
        bodyparts = model.GetComponentsInChildren<MeshFilter>();
        head = bodyparts[0].gameObject;
        torso = bodyparts[1].gameObject;
        leg1 = bodyparts[2].gameObject;
        leg2 = bodyparts[3].gameObject;
        foreach (MeshFilter bone in bodyparts)
        {
            bone.gameObject.layer = gameObject.layer;
            if (bone.gameObject.GetComponent<MeshRenderer>())
            {
                bone.gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    public void Dematerialize()
    {
        SoulPearl pearl = new GameObject().AddComponent<SoulPearl>();
        pearl.transform.position = transform.position;
        if (Phylactery)
        {
            pearl.Phylactery = Phylactery;
        }
        if (leftStorage)
        {
            leftStorage.DropItem(yeet: true);
        }
        if (rightStorage)
        {
            rightStorage.DropItem(yeet: true);
        }
        if (backStorage)
        {
            backStorage.DropItem(yeet: true);
        }   
    }

}
