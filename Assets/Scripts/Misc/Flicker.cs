using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;


public class Flicker : MonoBehaviour
{
    public bool Flicking = true;

    public float totalScalar = 1;

    public float maxIntensity = 10;
    public float minIntensity = 5f;
    public float unscaledIntensity;

    public float maxLightRange = 0.75f;
    public float minLightRange = 0.5f;
    public float unscaledRange;

    public float flickerRate = 0.025f;
    private Light light_source;


    public void Start()
    {
        light_source = GetComponent<Light>();
        light_source.enabled = true;
        if (!light_source)
        {
            Destroy(this);
        }
        else
        {
            StartCoroutine(flicker_routine());
        }
    }
    private IEnumerator flicker_routine()
    {
        while (true)
        {
            float incrementIntensity = (maxIntensity - minIntensity) / 5f;
            float incrementRange = (maxLightRange - minLightRange) / 10f;
            if (Flicking)
            {
                unscaledIntensity = Mathf.Clamp(Mathf.Lerp(minIntensity, maxIntensity, UnityEngine.Random.value), Mathf.Max(unscaledIntensity - incrementIntensity, minIntensity), Mathf.Min(unscaledIntensity + incrementIntensity, maxIntensity));
                unscaledRange = Mathf.Clamp(Mathf.Lerp(minLightRange, maxLightRange, UnityEngine.Random.value), Mathf.Max(unscaledRange - incrementRange, minLightRange), Mathf.Min(unscaledRange + incrementRange, maxLightRange));
            }
            else
            {
                unscaledIntensity = Mathf.MoveTowards(unscaledIntensity, 0, incrementIntensity);
                unscaledRange = Mathf.MoveTowards(unscaledRange, 0, incrementRange);
            }
            light_source.intensity = unscaledIntensity * totalScalar;
            light_source.range = unscaledRange * totalScalar;
            yield return new WaitForSecondsRealtime(flickerRate);
        }
    }
    

}

