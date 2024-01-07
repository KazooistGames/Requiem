using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Skully : Entity
{
    protected override void Awake()
    {
        base.Awake();
        Strength = 25f;
        berthScalar = 0.80f;
        Haste = 1.25f;
        heightScalar = 0.4f;
    }

    protected override void Start()
    {
        base.Start();
        if (!model)
        {
            createSkeleton();
        }
        anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animation/entities/skully/skullyAnimation");
        gameObject.name = "Skully";
        hurtBox.height = 0.7f;
        hurtBox.center = Vector3.up * 0.2f;
    }

    protected override void Update()
    {
        base.Update();
        BaseAcceleration = Dashing ? 6 : 3;
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
    public override void Damage(float damage, bool silent = false)
    {
        base.Damage(damage);
        if (!silent)
        {
            _SoundService.PlayAmbientSound(Requiem.boneSounds[UnityEngine.Random.Range(0, Requiem.boneSounds.Length)], transform.position, 0.5f + 0.5f * UnityEngine.Random.value, 0.25f, _SoundService.Instance.DefaultAudioRange / 2);
        }
    }

    private void createSkeleton()
    {
        model = model ? model : Instantiate(Resources.Load<GameObject>("Prefabs/Entities/skullyBody"));
        model.gameObject.name = "Model";
        model.transform.SetParent(transform, false);
        head = head ? head : model.GetComponentInChildren<MeshFilter>().gameObject;
        head.name = "Head";
        head.transform.localPosition = Vector3.zero;
        head.GetComponent<MeshRenderer>().enabled = true;
        head.layer = gameObject.layer;
    }

    public override void Die()
    {
        Debone(head);
        //head = null;
        base.Die();
    }


}




