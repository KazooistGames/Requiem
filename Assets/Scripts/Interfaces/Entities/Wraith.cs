using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wraith : Entity
{
    //StandardAI = typeof(AI_Revanent);

    protected MeshFilter[] bodyparts;
    //protected GameObject head;
    protected GameObject torso;
    protected GameObject waist;
    protected GameObject leg1;
    protected GameObject leg2;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<Animator>() == null ? gameObject.AddComponent<Animator>() : GetComponent<Animator>();
        Strength = 150f;
        Resolve = 20f;
        Haste = 1.25f;
        BaseAcceleration = 8;
    }

    protected override void Start()
    {
        base.Start();
        createSkeleton();
        gameObject.name = "Nemesis";
    }

    protected override void Update()
    {
        base.Update();
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
        _SoundService.PlayAmbientSound(Requiem.boneSounds[UnityEngine.Random.Range(0, Requiem.boneSounds.Length)], transform.position, 0.5f + 0.5f * UnityEngine.Random.value, 0.25f, _SoundService.Instance.DefaultAudioRange / 2).layer = gameObject.layer;
    }

    private void createSkeleton()
    {
        model = model ? model : Instantiate(Resources.Load<GameObject>("Prefabs/Entities/wraithBody"));
        model.gameObject.name = "Model";
        model.transform.SetParent(transform, false);
        model.transform.localPosition = Vector3.zero;
        bodyparts = model.GetComponentsInChildren<MeshFilter>();
        head = bodyparts[0].gameObject;
        torso = bodyparts[1].gameObject;
        waist = bodyparts[2].gameObject;
        leg1 = bodyparts[3].gameObject;
        leg2 = bodyparts[4].gameObject;
        foreach (MeshFilter bone in bodyparts)
        {
            bone.gameObject.layer = gameObject.layer;
            if (bone.gameObject.GetComponent<MeshRenderer>())
            {
                bone.gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    public override void Die()
    {
        foreach (MeshFilter bone in bodyparts)
        {
            if (bone)
            {
                Debone(bone.gameObject);
            }
        }
        base.Die();
    }
}
