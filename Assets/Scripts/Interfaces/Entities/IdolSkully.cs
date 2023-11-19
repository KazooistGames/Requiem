using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class IdolSkully : Skully
{
    protected override void Awake()
    {
        base.Awake();
        Strength = 400f;
        Haste = 0.75f;
        BaseAcceleration = 8f;
        scaleScalar = 1.0f;
    }

    protected override void Start()
    {
        base.Start();
        gameObject.name = "IdolSkully";
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
        _SoundService.PlayAmbientSound(Game.boneSounds[UnityEngine.Random.Range(0, Game.boneSounds.Length)], transform.position, 0.5f + 0.5f * UnityEngine.Random.value, 0.25f, _SoundService.Instance.DefaultAudioRange / 2);
    }


    protected void Mutate()
    {
        for(int i = 0; i < 2; i++)
        {
            Entity entity;
            entity = Game.SPAWN(typeof(Skully), typeof(Biter), transform.position).GetComponent<Entity>();
            entity.Poise = entity.Strength;
            entity.Shove(AIBehaviour.RandomDirection() * 1);
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




