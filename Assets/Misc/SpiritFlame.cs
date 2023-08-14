using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritFlame : MonoBehaviour
{
    protected ParticleSystem particles;
    public Light particleLight;

    public ParticleSystem.EmissionModule emissionModule;
    public ParticleSystem.ShapeModule shapeModule;
    public ParticleSystem.ColorOverLifetimeModule colorModule;

    public float PowerLevel = 1.0f;

    private static List<Gradient> gradients = new List<Gradient>();
    private static List<Color> colors = new List<Color>();
    public enum Preset
    {
        Off = 0,
        Inferno = 1,
        Soulless = 2,
        Magic = 3
    }
    public Preset flamePreset = 0;

    public enum Mode
    {
        Off = 0,
        Manual = 1,
        Auto = 2
    }
    public Mode flameMode = Mode.Manual;
    public GameObject boundObject;

    private float lightIntensityMin = 3f;
    private float lightIntensityMax = 5.0f;
    private float lightRangeMin = 0.5f;
    private float lightRangeMax = 0.8f;
    private float emissionOverTimeMin = 0;
    private float emissionOverTimeMax = 250;
    private float emissionOverDistanceMin = 0;
    private float emissionOverDistanceMax = 1000;

    private void Awake()
    {
        particleLight = GetComponent<Light>();
        particles = GetComponent<ParticleSystem>();
        emissionModule = particles.emission;
        shapeModule = particles.shape;
        colorModule = particles.colorOverLifetime;
    }

    private void Start()
    {
        buildGradients(); 
    }

    private void Update()
    {
        if(flameMode == Mode.Auto)
        {
            if (boundObject)
            {
                setFlamePreset(flamePreset);
                Character boundEntity = boundObject.gameObject.GetComponent<Character>();
                Weapon boundWeapon = boundObject.gameObject.GetComponent<Weapon>();
                if (boundWeapon)
                {
                    boundEntity = boundWeapon.MostRecentWielder;
                }
                if (boundEntity)
                {
                    emissionModule.enabled = boundWeapon ? boundWeapon.TrueStrike : boundEntity.FinalDash;
                    PowerLevel = boundEntity.Resolve * 10;
                    emissionModule.rateOverTime = Mathf.Lerp(emissionOverTimeMin, emissionOverTimeMax, PowerLevel);
                    emissionModule.rateOverDistance = Mathf.Lerp(emissionOverDistanceMax, emissionOverDistanceMin, PowerLevel);
                    particleLight.intensity = Mathf.Lerp(lightIntensityMin, lightIntensityMax, PowerLevel);
                    particleLight.range = Mathf.Lerp(lightRangeMin, lightRangeMax, PowerLevel);
                }
            }
            else
            {
                emissionModule.enabled = false;
            }
        }
    }
    /********** custom functions ************/
    public void setFlamePreset(Preset preset)
    {
        ParticleSystem.MinMaxGradient flameGradient = colorModule.color;
        //particleLight.intensity = 7.5f;
        //particleLight.range = 0.5f;
        if (flameMode == Mode.Manual)
        {
            emissionModule.enabled = true;
        }
        switch (preset)
        {
            case Preset.Off:
                if(flameMode == Mode.Manual)
                {
                    emissionModule.enabled = false;
                }
                break;
            case Preset.Inferno:
                particleLight.color = colors[(int)preset - 1];
                flameGradient.gradient = gradients[(int)preset - 1];
                break;
            case Preset.Soulless:
                particleLight.color = colors[(int)preset - 1];
                flameGradient.gradient = gradients[(int)preset - 1];
                break;
            case Preset.Magic:
                flameGradient.mode = ParticleSystemGradientMode.TwoGradients;
                flameGradient.gradientMin = gradients[0];
                flameGradient.gradientMax = gradients[1];
                break;
        }
        colorModule.color = flameGradient;
        flamePreset = preset;    
    }

    public void bindToObject(GameObject bindingObject)
    {
        if (!bindingObject) { return; }
        boundObject = bindingObject;
        buildGradients();
        flameMode = Mode.Auto;
        setFlamePreset(flamePreset);
        transform.SetParent(boundObject.transform, false);
        Character boundEntity = boundObject.gameObject.GetComponent<Character>();
        Weapon boundWeapon = boundObject.gameObject.GetComponent<Weapon>();
        if (boundEntity)
        {
            shapeModule.shapeType = ParticleSystemShapeType.Donut;
            transform.localPosition = Vector3.zero;
            shapeModule.position = Vector3.down * 0.6f;

        }
        if (boundWeapon)
        {
            transform.localPosition = Vector3.up * 0.25f;
            transform.localEulerAngles = Vector3.zero;
            transform.localScale = Vector3.one * 0.5f;
            shapeModule.shapeType = ParticleSystemShapeType.Mesh;
            shapeModule.mesh = boundWeapon.GetComponent<MeshFilter>().sharedMesh;
        }
    }

    private void buildGradients()
    {
        if (colors.Count == 0)
        {
            colors.Add(new Color(1.0f, 0.5f, 0.0f));
            colors.Add(new Color(1.0f, 1.0f, 1.0f));
        }
        if (gradients.Count == 0)
        {
            Gradient gradientOne = new Gradient();
            Gradient gradientTwo = new Gradient();
            Gradient gradientThree = new Gradient();
            gradientOne.SetKeys
            (
                new GradientColorKey[]
                {
                new GradientColorKey(new Color(1.0f, 0.7f, 0.0f), 0.0f),
                new GradientColorKey(new Color(1.0f, 0.5f, 0.5f), 0.8f),
                new GradientColorKey(new Color(1.0f, 0.7f, 0.0f), 1.0f),
                },
                new GradientAlphaKey[]
                {
                new GradientAlphaKey(0.25f, 0.0f),
                new GradientAlphaKey(0.20f, 0.8f),
                new GradientAlphaKey(0.10f, 1.0f)
                }
            );
            gradients.Add(gradientOne);
            gradientTwo.SetKeys
            (
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(1.0f, 1.0f, 1.0f), 0.0f),
                    new GradientColorKey(new Color(0.5f, 0.7f, 1.0f), 0.7f),
                    new GradientColorKey(new Color(0.55f, 0f, 1f), 1.0f),
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.4f, 0.0f),
                    new GradientAlphaKey(0.20f, 0.7f),
                    new GradientAlphaKey(0.60f, 1.0f)
                }
            );
            gradients.Add(gradientTwo);
            gradientThree.SetKeys
            (
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(1.0f, 1.0f, 1.0f), 0.0f),
                    new GradientColorKey(new Color(0.5f, 0.7f, 1.0f), 0.7f),
                    new GradientColorKey(new Color(0.55f, 0f, 1f), 1.0f),
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.4f, 0.0f),
                    new GradientAlphaKey(0.20f, 0.7f),
                    new GradientAlphaKey(0.60f, 1.0f)
                }
            );
            gradients.Add(gradientThree);
        }
        setFlamePreset(flamePreset);
    }
}
