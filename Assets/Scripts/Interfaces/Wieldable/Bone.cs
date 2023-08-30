using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bone : Wieldable
{
    public float pitchScalar = 1.0f;
    public float NoiseImpulse = 0.015f;

    protected override void Awake()
    {
        base.Awake();
        PhysicsBoxes.Add(GetComponent<BoxCollider>() ? GetComponent<BoxCollider>() : gameObject.AddComponent<BoxCollider>());
        Renderer = GetComponent<MeshRenderer>() ? GetComponent<MeshRenderer>() : gameObject.AddComponent<MeshRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        gameObject.layer = Game.layerItem;
        Body.useGravity = true;
        Body.isKinematic = false;
        Body.mass = 0.25f;
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        _SoundService.PlayAmbientSound(Game.boneSounds[Random.Range(0, Game.boneSounds.Length)], transform.position, (0.5f + 0.5f * Random.value) * pitchScalar, 0.1f, _SoundService.Instance.DefaultAudioRange/2).layer = gameObject.layer;
    }


}

