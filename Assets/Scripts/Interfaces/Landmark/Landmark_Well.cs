using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;

public class Landmark_Well : Landmark
{
    public float Volume = 100;
    private float drinkPeriod = 1.0f;
    protected static GameObject WellModelTemplate;
    protected GameObject Model;
    protected MeshRenderer bloodPoolRenderer;
    private Vector2 bumpMapOffset = Vector2.zero;
    private float bloodMinHeight = 0.05f;
    private float bloodMaxHeight = 0.15f;
    private float bloodWaveXspeed = 0.3f;
    private float bloodWaveYspeed = 0.1f;

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
        Warrior entity = other.gameObject.GetComponent<Warrior>();
        if(entity && !Used && !Energized)
        {
            entity.EventAttemptInteraction.AddListener(DRINK);
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        Warrior entity = other.gameObject.GetComponent<Warrior>();
        if (entity && !Used && !Energized)
        {
            entity.EventAttemptInteraction.RemoveListener(DRINK);
        }
    }

    public override void AssignToTile(Hextile tile)
    {
        base.AssignToTile(tile);

        gameObject.name = "Well";
        gameObject.layer = Game.layerObstacle;
        gameObject.AddComponent<Rigidbody>().isKinematic = true;
        transform.localPosition = Vector3.up * Hextile.Thickness / 2;
        if (!WellModelTemplate)
        {
            WellModelTemplate = Resources.Load<GameObject>("Prefabs/Structures/Well");
        }
        Model = Instantiate(WellModelTemplate);
        Model.transform.SetParent(transform, false);
        Model.transform.localEulerAngles = Vector3.up * UnityEngine.Random.value * 360;
        Model.layer = Game.layerObstacle;
        bloodPoolRenderer = Model.GetComponentsInChildren<MeshRenderer>()[1];
        bloodPoolRenderer.sharedMaterial = Instantiate(Resources.Load<Material>("Materials/FX/bloodPool"));
    }

    public void Refill(float addedVolume = 100)
    {
        Volume = Mathf.Clamp(Volume + addedVolume, 0, 100);
        Energized = false;
        Used = false;
    }

    public void DRINK(Warrior drinker)
    {
        StartCoroutine(drinkRoutine(drinker));
    }

    private IEnumerator drinkRoutine(Warrior drinker)
    {
        playSlurp();
        while (Volume > 0)
        {
            Energized = true;
            float increment = Time.deltaTime / drinkPeriod;
            Volume -= 100 * increment;
            drinker.Vitality += drinker.Strength * increment;
            yield return new WaitForEndOfFrame();
        }
        //playChant();
        Energized = false;
        Used = true;
        yield break;
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
