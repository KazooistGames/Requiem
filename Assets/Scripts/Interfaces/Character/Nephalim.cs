using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nephalim : Character
{
    protected MeshFilter[] bones;
    protected GameObject ribs;
    protected GameObject pelvis;
    protected GameObject leg1;
    protected GameObject leg2;


    protected override void Awake()
    {
        base.Awake();
        Strength = 200f;
        Haste = 1.0f;
        hoverScalar = 1.125f;
        scaleScalar = 1.20f;
    }

    protected override void Start()
    {
        base.Start();
        createSkeleton();
        gameObject.name = "Nephy";  
        EventWounded.AddListener(CRUMBLE);

    }

    protected override void Update()
    {
        base.Update();
        Weapon mainWep = MainHand ? MainHand.GetComponent<Weapon>() : null;
        if (mainWep)
        {
            if (!Foe)
            {
                _Martial.Override_Queue(mainWep, Weapon.Action.Idle);
            }
            else if(_Martial.Action_Queues[mainWep].Count == 0)
            {
                float disposition = (Foe.transform.position - transform.position).magnitude;
                if(mainWep.Range >= disposition)
                {
                    _Martial.Queue_Action(mainWep, Weapon.Action.QuickAttack);
                    _Martial.Queue_Action(mainWep, Weapon.Action.Guarding, 2);
                    _Martial.Queue_Action(mainWep, Weapon.Action.StrongAttack);
                }
                else
                {
                    _Martial.Queue_Action(mainWep, Weapon.Action.QuickCoil);
                }
            }
        }

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }


    /***** PUBLIC *****/


    /***** PROTECTED *****/
    protected override void Die()
    {
        foreach (MeshFilter bone in bones)
        {
            if (bone && !bone.GetComponent<Rigidbody>())
            {
                Debone(bone.gameObject);
            }
        }
        head = null;
        ribs = null;
        pelvis = null;
        leg1 = null;
        leg2 = null;
        base.Die();
    }


    /***** PRIVATE *****/

    private void CRUMBLE(float damage)
    {
        Mullet.PlayAmbientSound(Game.boneSounds[UnityEngine.Random.Range(0, Game.boneSounds.Length)], transform.position, 0.4f + 0.5f * UnityEngine.Random.value * 0.7f, 0.5f, Mullet.Instance.DefaultAudioRange / 2);
    }

    private void createSkeleton()
    {
        model = model ? model : Instantiate(Resources.Load<GameObject>("Prefabs/Entities/nephyBody"));
        model.name = "Model";
        model.transform.SetParent(transform, false);
        model.transform.localPosition = Vector3.zero;
        bones = model.GetComponentsInChildren<MeshFilter>();
        head = bones[0].gameObject;
        ribs = bones[1].gameObject;
        pelvis = bones[2].gameObject;
        leg1 = bones[3].gameObject;
        leg2 = bones[4].gameObject;
        foreach (MeshFilter bone in bones)
        {
            bone.gameObject.layer = gameObject.layer;
            if (bone.gameObject.GetComponent<MeshRenderer>())
            {
                bone.gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

}
