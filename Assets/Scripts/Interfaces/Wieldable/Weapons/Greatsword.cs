using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Greatsword : Weapon
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        CapsuleCollider wHitBox = HitBox.GetComponent<CapsuleCollider>();
        gameObject.name = "GreatSword";
        transform.localScale = new Vector3(1, 2.5f, 2);
        Anim.updateMode = AnimatorUpdateMode.Normal;
        Filter.mesh = Requiem.weaponMeshes["Sword"];
        Renderer.sharedMaterial.SetTexture("_MainTex", Resources.Load<Texture>("Textures/Sword"));
        Renderer.sharedMaterial.SetTexture("_EmissionMap", Resources.Load<Texture>("Textures/Sword"));
        wHitBox.center = new Vector3(0, 0.3f, 0);
        hitRadius = 0.1f;
        defendRadius = 0.1f;
        wHitBox.height = 0.6f;
        Range = 0.42f;
        BasePower = 40f;
        Heft = 40;
        equipType = EquipType.TwoHanded;
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
