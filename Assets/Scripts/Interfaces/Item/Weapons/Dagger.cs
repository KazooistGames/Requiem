using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dagger : Weapon
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        CapsuleCollider wHitBox = HitBox.GetComponent<CapsuleCollider>();
        //BoxCollider wPhysicsBox = HitBox.GetComponent<BoxCollider>();
        gameObject.name = "Dagger";
        transform.localScale = new Vector3(1f, 0.70f, 0.6f);
        Anim.updateMode = AnimatorUpdateMode.Normal;
        Filter.mesh = Game.weaponMeshes["Sword"];
        Renderer.sharedMaterial.SetTexture("_MainTex", Resources.Load<Texture>("Textures/Sword"));
        Renderer.sharedMaterial.SetTexture("_EmissionMap", Resources.Load<Texture>("Textures/Sword"));
        wHitBox.center = new Vector3(0, 0.3f, 0);
        hitRadius = 0.1f;
        defendRadius = 0.1f;
        wHitBox.height = 0.6f;
        //wPhysicsBox.center = new Vector3(0, 0.3f, 0);
        //wPhysicsBox.size = new Vector3(0.1f, 0.6f, 0.1f);
        Range = 0.15f;
        BasePower = 15f;
        equipType = EquipType.OneHanded;
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

    /************* dagger specials *************/

}
