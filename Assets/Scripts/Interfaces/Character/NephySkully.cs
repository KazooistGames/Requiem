using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NephySkully : Character
{
    protected override void Awake()
    {
        base.Awake();
        Strength = 50f;
        berthScalar = 0.80f;
        Haste = 1.5f;
        BaseAcceleration = 4f;
        scaleScalar = 1.2f;
    }

    protected override void Start()
    {
        base.Start();
        createSkeleton();
        anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animation/entities/skully/skullyAnimation");
        gameObject.name = "NephySkully";
        hurtBox.height = 0;
        hurtBox.center = Vector3.up * 0.2f;
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
        Mullet.PlayAmbientSound(Game.boneSounds[UnityEngine.Random.Range(0, Game.boneSounds.Length)], transform.position, 0.5f + 0.5f * UnityEngine.Random.value, 0.25f, Mullet.Instance.DefaultAudioRange / 2);
    }

    private void createSkeleton()
    {
        model = model ? model : Instantiate(Resources.Load<GameObject>("Prefabs/Entities/nephySkullyBody"));
        model.gameObject.name = "Model";
        model.transform.SetParent(transform, false);
        head = head ? head : model.GetComponentInChildren<MeshFilter>().gameObject;
        head.name = "Head";
        head.transform.localPosition = Vector3.zero;
        head.GetComponent<MeshRenderer>().enabled = true;
        head.layer = gameObject.layer;
    }

    protected override void Mutation()
    {
        base.Mutation();
        for(int i = 0; i < 2; i++)
        {
            Character entity;
            entity = Game.SPAWN(typeof(Skully), typeof(Biter), transform.position).GetComponent<Character>();
            entity.Poise = entity.Strength;
            entity.Shove(AI.RandomDirection() * dashMinVelocity);
        }
        Destroy(head);
        Die();
    }
    protected override void Die()
    {
        Debone(head);
        head = null;
        base.Die();
    }


}




