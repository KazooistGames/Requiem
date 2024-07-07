using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;


public class Flicker : MonoBehaviour
{
    public bool Flicking = true;
    public float maxIntensity = 10;
    public float minIntensity = 5f;
    public float maxLightRange = 0.75f;
    public float minLightRange = 0.5f;
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
                light_source.intensity = Mathf.Clamp(Mathf.Lerp(minIntensity, maxIntensity, UnityEngine.Random.value), Mathf.Max(light_source.intensity - incrementIntensity, minIntensity), Mathf.Min(light_source.intensity + incrementIntensity, maxIntensity));
                light_source.range = Mathf.Clamp(Mathf.Lerp(minLightRange, maxLightRange, UnityEngine.Random.value), Mathf.Max(light_source.range - incrementRange, minLightRange), Mathf.Min(light_source.range + incrementRange, maxLightRange));
            }
            else
            {
                light_source.intensity = Mathf.MoveTowards(light_source.intensity, 0, incrementIntensity);
                light_source.range = Mathf.MoveTowards(light_source.range, 0, incrementRange);
            }
            yield return new WaitForSecondsRealtime(flickerRate);
        }
    }
    

}

