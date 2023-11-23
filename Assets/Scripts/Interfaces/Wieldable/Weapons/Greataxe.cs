using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Greataxe : Weapon
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        CapsuleCollider wHitBox = HitBox.GetComponent<CapsuleCollider>();
        gameObject.name = "Greataxe";
        transform.localScale = new Vector3(2.0f, 2.5f, 2.0f);
        Anim.updateMode = AnimatorUpdateMode.Normal;
        Filter.mesh = Game.weaponMeshes["Axe"];
        Renderer.sharedMaterial.SetTexture("_MainTex", Resources.Load<Texture>("Textures/Axe"));
        Renderer.sharedMaterial.SetTexture("_EmissionMap", Resources.Load<Texture>("Textures/Axe"));
        wHitBox.center = new Vector3(0.0f, 0.15f, 0.05f);
        hitRadius = 0.1f;
        defendRadius = 0.15f;
        wHitBox.height = 0.6f;
        equipType = EquipType.TwoHanded;
        Range = 0.45F;
        BasePower = 100f;
        Heft = 80;
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

}
