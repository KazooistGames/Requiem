using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;
using static UnityEngine.EventSystems.EventTrigger;

public class Landmark_Well : Landmark
{
    public static UnityEvent<Entity, float> JustGulped = new UnityEvent<Entity, float>();

    public float Volume = 100;
    private float fullDrinkPeriod = 3.0f;
    protected static GameObject WellModelTemplate;
    protected GameObject Model;
    protected MeshRenderer bloodPoolRenderer;
    private Vector2 bumpMapOffset = Vector2.zero;
    private float bloodMinHeight = 0.05f;
    private float bloodMaxHeight = 0.15f;
    private float bloodWaveXspeed = 0.3f;
    private float bloodWaveYspeed = 0.1f;

    private GameObject gulpSound;
    private float gulpSize = 20;

    private List<GameObject> bloodSplatters = new List<GameObject>();
    protected static GameObject BLOOD_SPLATTER_PREFAB;

    private GameObject blurbInteractPrompt;

    protected void Start()
    {
        splatter(Vector3.zero, 0.25f);
        blurbInteractPrompt = _BlurbService.createBlurb(gameObject, "F", Color.white, sizeScalar: 2);
        blurbInteractPrompt.SetActive(false);
    }
    protected void Update()
    {
        Volume = Mathf.Clamp(Volume, 0, 100);
        bloodPoolRenderer.gameObject.transform.localPosition = Vector3.up * Mathf.Lerp(bloodMinHeight, bloodMaxHeight, Volume / 100f);
        bumpMapOffset = new Vector2(Mathf.Sin(Time.time * bloodWaveXspeed), Mathf.Cos(Time.time * bloodWaveYspeed));
        bloodPoolRenderer.material.SetTextureOffset("_MainTex", bumpMapOffset);
        bloodPoolRenderer.gameObject.transform.Rotate(Vector3.up, Time.deltaTime * 5);
    }

    protected void OnTriggerEnter(Collider other)
    {
        Entity entity = other.gameObject.GetComponent<Entity>();
        if (entity ? entity == Player.INSTANCE.HostEntity : false)
        {
            entity.Interact.AddListener(gulp);
            blurbInteractPrompt.SetActive(true);
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        Entity entity = other.gameObject.GetComponent<Entity>();
        if (entity ? entity == Player.INSTANCE.HostEntity : false)
        {
            entity.Interact.RemoveListener(gulp);
            blurbInteractPrompt.SetActive(false);
            Energized = false;
        }

    }

    protected void OnTriggerStay(Collider other)
    {
        if (!Energized)
        {
            Energized = other.gameObject.GetComponent<Entity>();
        }
    }

    /***** PUBLIC *****/

    public override void AssignToTile(Hextile tile)
    {
        base.AssignToTile(tile);

        gameObject.name = "Well";
        gameObject.layer = Requiem.layerObstacle;
        gameObject.AddComponent<Rigidbody>().isKinematic = true;
        transform.localPosition = Vector3.up * Hextile.Thickness / 2.25f;
        if (!WellModelTemplate)
        {
            WellModelTemplate = Resources.Load<GameObject>("Prefabs/Structures/Well");
        }
        Model = Instantiate(WellModelTemplate);
        Model.transform.SetParent(transform, false);
        Model.transform.localEulerAngles = Vector3.up * UnityEngine.Random.value * 360;
        Model.layer = Requiem.layerObstacle;
        bloodPoolRenderer = Model.GetComponentsInChildren<MeshRenderer>()[1];
        bloodPoolRenderer.sharedMaterial = Instantiate(Resources.Load<Material>("Materials/FX/bloodPool"));
    }

    public void Refill(float addedVolume = 100)
    {
        Volume = Mathf.Clamp(Volume + addedVolume, 0, 100);
        Energized = false;
    }


    /***** PROTECTED *****/


    /***** PRIVATE *****/
    private void gulp(Entity benefactor)
    {
        StartCoroutine(gulpRoutine(benefactor, gulpSize));
    }

    private IEnumerator gulpRoutine(Entity benefactor, float volumeToGulp)
    {
        if (Used) { yield break; }
        JustGulped.Invoke(benefactor, volumeToGulp);
        while(volumeToGulp > 0 && Volume > 0)
        {
            Used = true;
            if (!Requiem.INSTANCE.Paused)
            {
                if (!gulpSound)
                {
                    playGulpSound(benefactor);
                    Vector3 disposition = benefactor.transform.position - transform.position;
                    Vector3 tinyOffset = new Vector3(UnityEngine.Random.value- 0.5f, 0, UnityEngine.Random.value - 0.5f) * 0.1f;
                    float scale = 0.05f + UnityEngine.Random.value * 0.15f;
                    splatter(disposition + tinyOffset, scale);
                }
                Energized = true;
                float increment = Time.deltaTime / fullDrinkPeriod;
                volumeToGulp -= 100 * increment;
                Volume -= 100 * increment;
                benefactor.Vitality += benefactor.Strength * increment;
            }
            yield return null;
        }
        Used = false;
        yield break;
    }

    private GameObject splatter(Vector3 position, float size)
    {
        if (!BLOOD_SPLATTER_PREFAB)
        {
            BLOOD_SPLATTER_PREFAB = Resources.Load<GameObject>("Prefabs/Misc/bloodSplatter");
        }
        GameObject newSplatter = Instantiate(BLOOD_SPLATTER_PREFAB);
        bloodSplatters.Add(newSplatter);
        newSplatter.transform.SetParent(transform);
        newSplatter.transform.localPosition = position;
        newSplatter.transform.localEulerAngles = Vector3.up * UnityEngine.Random.value * 180 + Vector3.right * 90;
        newSplatter.transform.localScale = Vector3.one;
        newSplatter.GetComponent<Projector>().orthographicSize = size;
        return newSplatter;
    }


    private void playGulpSound(Entity benefactor)
    {
        gulpSound = _SoundService.PlayAmbientSound("Audio/well/slurp", transform.position, 0.8f, 0.8f, _SoundService.Instance.DefaultAudioRange / 4);
        gulpSound.GetComponent<AudioSource>().time = 1f;
        gulpSound.transform.SetParent(benefactor.transform);
    }

    private void playSlurp()
    {
        _SoundService.PlayAmbientSound("Audio/well/slurp", transform.position, 1, 0.75f, _SoundService.Instance.DefaultAudioRange / 4);
    }
    private void playChant()
    {
        _SoundService.PlayAmbientSound("Audio/well/low", transform.position, 1, 0.75f, _SoundService.Instance.DefaultAudioRange / 4);
        //Mullet.PlayAmbientSound("Audio/well/high", transform.position, 1, 0.25f, Mullet.Instance.DefaultAudioRange / 4);
    }

}
