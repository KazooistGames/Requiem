using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bone : MonoBehaviour
{
    public int Value = 20;
    public float pitchScalar = 1.0f;
    public float NoiseImpulse = 0.015f;

    public Player collectTarget;
    public bool Telecommuting = false;

    public List<Collider> PhysicsBoxes = new List<Collider>();
    public Rigidbody Body;

    private bool allowCollect = false;
    private float allowCollectTimer = 0.0f;

    private static float FINAL_SCALE_AFTER_COLLECTION = 0.5f;

    protected void Awake()
    {
        PhysicsBoxes.Add(GetComponent<BoxCollider>() ? GetComponent<BoxCollider>() : gameObject.AddComponent<BoxCollider>());
        Body = GetComponent<Rigidbody>() ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();
    }

    protected void Start()
    {
        gameObject.layer = Game.layerItem;
        Body.useGravity = true;
        Body.isKinematic = false;
        Body.mass = 0.25f;
    }

    protected void Update()
    {
        allowCollectTimer += Time.deltaTime;
        allowCollect = allowCollectTimer >= 0.5f;
    }

    protected void OnDestroy()
    {
        StopAllCoroutines();
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (!collectTarget)
        {
            Rattle();
        }
    }

    protected void OnTriggerStay(Collider other)
    {
        if(other.gameObject == Player.INSTANCE.gameObject && allowCollect && !collectTarget)
        {
            Collect(Player.INSTANCE, 0.5f, Consume);
        }
    }



    /*** PUBLIC ***/
    public void Rattle()
    {
        _SoundService.PlayAmbientSound(Game.boneSounds[UnityEngine.Random.Range(0, Game.boneSounds.Length)], transform.position, (0.5f + 0.5f * UnityEngine.Random.value) * pitchScalar, 0.1f, _SoundService.Instance.DefaultAudioRange / 2).layer = gameObject.layer;
    }

    public static void Consume(Bone bone)
    {
        if (bone.collectTarget) 
        {
            bone.collectTarget.BonesCollected += bone.Value;
            bone.Rattle();
            Destroy(bone.gameObject);
        }

    }

    public void Collect(Player target, float telecommuteScalar, Action<Bone> callback, bool enablePhysicsWhileInFlight = false, bool useScalarAsSpeed = false)
    {
        StartCoroutine(collectRoutine(target, telecommuteScalar, callback, enablePhysicsWhileInFlight, useScalarAsSpeed));
    }


    /*** PROTECTED ***/


    /*** PRIVATE ***/
    private IEnumerator collectRoutine(Player target, float teleScalar, Action<Bone> callback, bool enablePhysics, bool useScalarAsSpeed)
    {
        collectTarget = target;
        Telecommuting = true;
        float timer = 0.0f;
        Vector3 origin = transform.position;
        Vector3 originalScale = transform.localScale;
        bool previousPhysicsBoxState = PhysicsBoxes.Count > 0 ? PhysicsBoxes[0].enabled : false;
        bool previousGravity = Body.useGravity;
        Body.useGravity = false;
        togglePhysicsBox(enablePhysics);
        void cancelCommute()
        {
            Telecommuting = false;
            Body.useGravity = previousGravity;
            togglePhysicsBox(previousPhysicsBoxState);
        }
        while (Telecommuting && target)
        {
            if (useScalarAsSpeed)
            {
                Vector3 totalDisposition = target.transform.position - transform.position;
                Vector3 increment = totalDisposition.normalized * teleScalar * Time.deltaTime;
                if (totalDisposition.magnitude <= increment.magnitude)
                {
                    callback(this);
                    cancelCommute();
                    yield break;
                }
                else
                {
                    transform.position += increment;
                }
            }
            else
            {
                timer += Time.deltaTime;
                float x = 2 * timer / teleScalar;
                x = Mathf.Clamp(x, 0f, 2f);
                float y = (Mathf.Pow(x, 2) - Mathf.Pow(x, 3) / 3);
                float scale = (y) / 1.33f;
                scale = Mathf.Clamp(scale, 0f, 1f);
                transform.position = Vector3.Lerp(origin, target.transform.position, scale);
                transform.localScale = Vector3.Lerp(originalScale, originalScale * FINAL_SCALE_AFTER_COLLECTION, scale);
                if (scale == 1)
                {
                    callback(this);
                    cancelCommute();
                    yield break;
                }
            }
            yield return null;
        }  
        cancelCommute();
        yield break;
    }

    private void togglePhysicsBox(bool newValue)
    {
        PhysicsBoxes.RemoveAll(x => !x);
        if (PhysicsBoxes.Count > 0)
        {
            foreach (Collider box in PhysicsBoxes.Where(x => x))
            {
                box.enabled = newValue;
            }
        }
    }

}

