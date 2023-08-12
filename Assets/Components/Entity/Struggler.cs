using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Struggler : Entity
{
    protected MeshFilter[] bodyparts;
    //protected GameObject head;
    protected GameObject torso;
    protected GameObject leg1;
    protected GameObject leg2;

    private static GameObject bloodSplatter;


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        createProfile();
        gameObject.name = "Struggler";
        Wounded.AddListener(BLEED);
        if (!bloodSplatter)
        {
            bloodSplatter = Resources.Load<GameObject>("Prefabs/Misc/bloodSplatter");
        }
        flames.flamePreset = SpiritFlame.Preset.Magic;
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

    //CUSTOM FUNCTIONS!!!!!!!!!!!!!

    private void createProfile()
    {
        model = model ? model : Instantiate(Resources.Load<GameObject>("Prefabs/Entities/fleshBody"));
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
                Vector3 direction = Foe ? Foe.transform.position - transform.position : new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f);
                model.GetComponent<Rigidbody>().AddForce(-direction.normalized * 1, ForceMode.VelocityChange);
            }
        }
        Mullet.PlayAmbientSound(Game.deathSounds[Random.Range(0, Game.deathSounds.Length)], transform.position, 1, 0.5f).layer = gameObject.layer;
        base.Die();
    }

}
