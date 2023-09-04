using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shade : Warrior
{
    static RuntimeAnimatorController shadeAnimations;

    private ParticleSystem particles;

    protected override void Awake()
    {
        base.Awake();
        Strength = 1f;
        Haste = 0.8f;
        BaseAcceleration = 4f;
    }

    protected override void Start()
    {
        base.Start();
        createVisage();
        gameObject.name = "Shade";
        hurtBox.enabled = false;
        if (!shadeAnimations)
        {
            shadeAnimations = Resources.Load<RuntimeAnimatorController>("Animation/Shade/shadeAnimation");
        }
        anim.runtimeAnimatorController = shadeAnimations;
        EventVanquished.AddListener(Dissipate);
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



    /**********SHADE FUNCTIONS************/
    private void Dissipate()
    {
        Destroy(gameObject);
    }

    private void createVisage()
    {
        model = Instantiate(Resources.Load<GameObject>("Prefabs/Entities/shadeVisage"));
        model.transform.SetParent(transform, false);
        model.transform.localScale = Vector3.one;
        model.transform.localPosition = Vector3.zero;
        particles = model.GetComponent<ParticleSystem>();
    }
}
