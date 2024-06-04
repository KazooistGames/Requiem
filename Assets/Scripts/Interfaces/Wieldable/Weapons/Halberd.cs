using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Halberd : Weapon
{

    protected override void Awake()
    {
        base.Awake();
    }


    protected override void Start()
    {
        gameObject.AddComponent<attack_cycler>();
        CapsuleCollider wHitBox = HitBox.GetComponent<CapsuleCollider>();
        gameObject.name = "Halberd";
        transform.localScale = new Vector3(1f, 1.0f, 1f);
        Anim.updateMode = AnimatorUpdateMode.Normal;
        Filter.mesh = Requiem.weaponMeshes["Halberd"];
        Renderer.sharedMaterial.SetTexture("_MainTex", Resources.Load<Texture>("Textures/Halberd"));
        Renderer.sharedMaterial.SetTexture("_EmissionMap", Resources.Load<Texture>("Textures/Halberd"));
        wHitBox.center = new Vector3(0, 0.5f, 0.15f);
        wHitBox.radius = 0.15f;
        wHitBox.height = 1.5f;
        hitRadius = 0.1f;
        defendRadius = 0.4f;
        Range = 0.52f;
        BasePower = 20f;
        Heft = 50;
        equipType = EquipType.TwoHanded;
        base.Start();
    }
}
