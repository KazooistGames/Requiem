using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortSword : Weapon
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        CapsuleCollider wHitBox = HitBox.GetComponent<CapsuleCollider>();
        gameObject.name = "ShortSword";
        transform.localScale = new Vector3(1, 1.5f, 1);
        Anim.updateMode = AnimatorUpdateMode.Normal;
        Filter.mesh = Game.weaponMeshes["Sword"];
        Renderer.sharedMaterial.SetTexture("_MainTex", Resources.Load<Texture>("Textures/Sword"));
        Renderer.sharedMaterial.SetTexture("_EmissionMap", Resources.Load<Texture>("Textures/Sword"));
        wHitBox.center = new Vector3(0, 0.3f, 0);
        hitRadius = 0.1f;
        defendRadius = 0.3f;
        wHitBox.height = 0.6f;
        equipType = EquipType.OneHanded;
        Range = 0.32f;
        BasePower = 15f;
        Heft = 20;
        //EventHitting.AddListener((x) => { if (Wielder) { Wielder.alterTempo(Power / 100); } });
        //EventParriedWeapon.AddListener((weapon) => { if (Wielder) { Wielder.alterTempo(weapon.Power / 100); } });
        base.Start();
    }

    protected override void Update()
    {
        //if (Wielder)
        //{
        //    Wielder.Tempo = Mathf.MoveTowards(Wielder.Tempo, 0, Time.deltaTime * 0.1f);
        //}
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

    /************* sword functions ***************/


}
