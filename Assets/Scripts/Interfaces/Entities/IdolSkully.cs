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
        Strength = 250f;
        Haste = 1f;
        Resolve = 20;
        BaseAcceleration = 8f;
        scaleScalar = 1.0f;
        berthScalar = 1.1f;

    }

    protected override void Start()
    {
        base.Start();
        gameObject.name = "IdolSkully";
        flames.gameObject.SetActive(true);
        flames.FlamePresentationStyle = _Flames.FlameStyles.Inferno;
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

    /***** PUBLIC *****/
    public override void Damage(float damage, bool silent = false)
    {
        base.Damage(damage);
        if (!silent)
        {
            _SoundService.PlayAmbientSound(Requiem.boneSounds[UnityEngine.Random.Range(0, Requiem.boneSounds.Length)], transform.position, 0.5f + 0.5f * UnityEngine.Random.value, 0.25f, _SoundService.Instance.DefaultAudioRange / 2);
        }
    }


    public override void Die()
    {
        _SoundService.PlayAmbientSound(Requiem.deathSounds[UnityEngine.Random.Range(0, Requiem.deathSounds.Length)], transform.position, 0.5f, 0.5f).layer = gameObject.layer;
        base.Die();
        //Destroy(head);
    }

    /***** PROTECTED *****/

    /***** PRIVATE *****/


}




