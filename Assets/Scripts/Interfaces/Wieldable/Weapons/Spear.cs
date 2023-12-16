using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : Weapon
{

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        CapsuleCollider wHitBox = HitBox.GetComponent<CapsuleCollider>();
        gameObject.name = "Spear";
        transform.localScale = new Vector3(1f, 1.25f, 1f);
        Anim.updateMode = AnimatorUpdateMode.Normal;
        Filter.mesh = Requiem.weaponMeshes["Spear"];
        Renderer.sharedMaterial.SetTexture("_MainTex", Resources.Load<Texture>("Textures/Spear"));
        Renderer.sharedMaterial.SetTexture("_EmissionMap", Resources.Load<Texture>("Textures/Spear"));
        wHitBox.center = new Vector3(0, 0.2f, 0);
        wHitBox.height = 1.5f;
        hitRadius = 0.05f;
        defendRadius = 0.4f;
        Range = 0.52f;
        BasePower = 20f;
        Heft = 40;
        equipType = EquipType.TwoHanded;
        base.Start();
    }
}
