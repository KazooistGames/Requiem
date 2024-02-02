using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handaxe : Weapon
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        CapsuleCollider wHitBox = HitBox.GetComponent<CapsuleCollider>();
        gameObject.name = "HandAxe";
        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        Anim.updateMode = AnimatorUpdateMode.Normal;
        Filter.mesh = Requiem.weaponMeshes["Axe"];
        wHitBox.center = new Vector3(0.0f, 0.15f, 0.05f);
        hitRadius = 0.1f;
        defendRadius = 0.3f;
        wHitBox.height = 0.6f;
        equipType = EquipType.OneHanded;
        Range = 0.3f;
        BasePower = 25f;
        Heft = 40;
        //EventSwinging.AddListener(() => { if (Wielder) { Wielder.alterTempo(-Wielder.Tempo); } });
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


    /***********PUBLIC***********/

    /***********PROTECTED***********/

    /***********PRIVATE***********/



}
