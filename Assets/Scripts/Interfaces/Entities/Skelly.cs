using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skelly : Entity
{
    protected MeshFilter[] bodyParts;
    //protected GameObject head;
    protected GameObject torso;
    protected GameObject waist;
    protected GameObject leg1;
    protected GameObject leg2;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        createSkeleton();
        gameObject.name = "Skelly";
        EventWounded.AddListener(CRUMBLE);
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
    protected void CRUMBLE(float damage)
    {
        _SoundService.PlayAmbientSound(Game.boneSounds[UnityEngine.Random.Range(0, Game.boneSounds.Length)], transform.position, 0.5f + 0.5f * UnityEngine.Random.value, 0.5f, _SoundService.Instance.DefaultAudioRange / 2).layer = gameObject.layer;
    }

    private void createSkeleton()
    {
        model = model ? model : Instantiate(Resources.Load<GameObject>("Prefabs/Entities/skellyBody"));
        model.name = "Model";
        model.transform.SetParent(transform, false);
        model.transform.localPosition = Vector3.zero;
        bodyParts = model.GetComponentsInChildren<MeshFilter>();
        head = bodyParts[0].gameObject;
        torso = bodyParts[1].gameObject;
        waist = bodyParts[2].gameObject;
        leg1 = bodyParts[3].gameObject;
        leg2 = bodyParts[4].gameObject;
        foreach (MeshFilter bone in bodyParts)
        {
            bone.gameObject.layer = gameObject.layer;
            if (bone.gameObject.GetComponent<MeshRenderer>())
            {
                bone.gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    protected void Mutate()
    {
        Debone(head);
        Haste *= 1.25f;
        Strength *= 0.75f;
        if (GetComponent<AIBehaviour>())
        {
            Destroy(GetComponent<AIBehaviour>());
            gameObject.AddComponent<Assassin>();
        }
    }

    protected override void Die()
    {
        foreach (MeshFilter bone in bodyParts)
        {
            if (bone && !bone.GetComponent<Rigidbody>())
            {
                Debone(bone.gameObject);
            }
        }
        head = null;
        torso = null;
        waist = null;
        leg1 = null;
        leg2 = null;     
        base.Die();
    }
}
