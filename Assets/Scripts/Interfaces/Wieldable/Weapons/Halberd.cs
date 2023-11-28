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
        CapsuleCollider wHitBox = HitBox.GetComponent<CapsuleCollider>();
        gameObject.name = "Halberd";
        transform.localScale = new Vector3(1f, 1.0f, 1f);
        Anim.updateMode = AnimatorUpdateMode.Normal;
        Filter.mesh = Game.weaponMeshes["Halberd"];
        Renderer.sharedMaterial.SetTexture("_MainTex", Resources.Load<Texture>("Textures/Halberd"));
        Renderer.sharedMaterial.SetTexture("_EmissionMap", Resources.Load<Texture>("Textures/Halberd"));
        wHitBox.center = new Vector3(0, 0.2f, 0);
        wHitBox.height = 1.5f;
        hitRadius = 0.05f;
        defendRadius = 0.4f;
        Range = 0.52f;
        BasePower = 25f;
        Heft = 60;
        equipType = EquipType.TwoHanded;
        base.Start();
    }
}
