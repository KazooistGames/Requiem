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
        Strength = 300f;
        Haste = 0.75f;
        Resolve = 50;
        BaseAcceleration = 8f;
        scaleScalar = 1.0f;
        berthScalar = 1.1f;
    }

    protected override void Start()
    {
        base.Start();
        gameObject.name = "IdolSkully";
        JustWounded.AddListener((x) => SpawnAdds(1));
        JustHit.AddListener(spawnAddsIfDamageMakesWeak);
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

    public void SpawnAdds(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Entity entity;
            entity = Requiem.SPAWN(typeof(Skully), typeof(Biter), transform.position).GetComponent<Entity>();
            entity.Poise = entity.Strength;
            Vector3 randomOffset = new Vector3(UnityEngine.Random.value - 0.5f, 0, UnityEngine.Random.value - 0.5f) * 0.1f;
            entity.transform.position += randomOffset;
            entity.Shoved = false;
        }
    }


    public override void Die()
    {
        base.Die();
        SpawnAdds(Requiem_Arena.INSTANCE.Ritual);
        Destroy(head);
    }

    /***** PROTECTED *****/

    /***** PRIVATE *****/
    private void spawnAddsIfDamageMakesWeak(float totalDamage)
    {
        if(totalDamage > Poise && Posture != PostureStrength.Weak)
        {
            SpawnAdds(Requiem_Arena.INSTANCE.Ritual);
        }
    } 

}




