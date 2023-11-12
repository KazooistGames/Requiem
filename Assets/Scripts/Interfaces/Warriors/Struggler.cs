using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Struggler : Warrior
{
    public Halberd MyHalberd;
    protected MeshFilter[] bodyparts;
    protected GameObject torso;
    protected GameObject leg1;
    protected GameObject leg2;
    protected override void Awake()
    {
        base.Awake();
        Strength = 100f;
        Haste = 1.0f;
    }

    protected override void Start()
    {
        base.Start();
        createProfile();
        gameObject.name = "Struggler";
        EventWounded.AddListener(fleshWound);
        MyHalberd = new GameObject().AddComponent<Halberd>();
        MyHalberd.PickupItem(this);
    }


    /***** PUBLIC *****/

    /***** PROTECTED *****/
    protected override void Die()
    {
        if (model)
        {
            Vector3 newScale = model.transform.localScale * scaleActual;
            Vector3 newPosition = model.transform.position;
            model.transform.parent = transform.parent;
            anim.Rebind();
            model.transform.localScale = newScale;
            model.transform.position = newPosition;
            model.layer = Game.layerItem;
            model.AddComponent<Rigidbody>();
            model.AddComponent<BoxCollider>();
            model.GetComponent<BoxCollider>().size = new Vector3(0.25f, 1.25f, 0.25f);
            model.GetComponent<BoxCollider>().center = Vector3.zero;
            model.GetComponent<Rigidbody>().velocity = body.velocity;
            if (Shoved)
            {
                Vector3 direction = Foe ? Foe.transform.position - transform.position : new Vector3(UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f);
                model.GetComponent<Rigidbody>().AddForce(-direction.normalized * 1, ForceMode.VelocityChange);
            }
        }
        _SoundService.PlayAmbientSound(Game.deathSounds[UnityEngine.Random.Range(0, Game.deathSounds.Length)], transform.position, 0.75f, 0.5f).layer = gameObject.layer;
        base.Die();
    }


    /***** PRIVATE *****/
    private void fleshWound(float damage)
    {
        if (Vitality > 0)
        {
            _SoundService.PlayAmbientSound(Game.damageSounds[UnityEngine.Random.Range(0, Game.damageSounds.Length)], transform.position, 0.75f, 0.5f).layer = gameObject.layer;
        }
        if (!BLOOD_SPLATTER_PREFAB)
        {
            BLOOD_SPLATTER_PREFAB = Resources.Load<GameObject>("Prefabs/Misc/bloodSplatter");
        }
        GameObject splatter = Instantiate(BLOOD_SPLATTER_PREFAB);
        splatter.transform.position = transform.position;
        splatter.transform.eulerAngles = new Vector3(45, UnityEngine.Random.value * 360f, 45);
        splatter.GetComponent<Projector>().orthographicSize = Mathf.Lerp(0.05f, 0.70f, (damage / 100));
    }

    private void createProfile()
    {
        model = model ? model : Instantiate(Resources.Load<GameObject>("Prefabs/Entities/devilBody"));
        model.gameObject.name = "Model";
        model.transform.SetParent(transform, false);
        model.transform.localPosition = Vector3.zero;
        bodyparts = model.GetComponentsInChildren<MeshFilter>();
        head = bodyparts[0].gameObject;
        torso = bodyparts[1].gameObject;
        leg1 = bodyparts[2].gameObject;
        leg2 = bodyparts[3].gameObject;
        foreach (MeshFilter bone in bodyparts)
        {
            bone.gameObject.layer = gameObject.layer;
            if (bone.gameObject.GetComponent<MeshRenderer>())
            {
                bone.gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

}
