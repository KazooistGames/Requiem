using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Flames : MonoBehaviour
{
    public enum ControlMode
    {
        Entity,
        Item,
        Manual
    }
    protected ParticleSystem particles;
    public Light particleLight;

    public ParticleSystem.EmissionModule emissionModule;
    public ParticleSystem.ShapeModule shapeModule;
    public ParticleSystem.ColorOverLifetimeModule colorModule;

    public float PowerLevel = 100f;

    private static List<Gradient> gradients = new List<Gradient>();
    private static List<Color> colors = new List<Color>();
    public enum FlameStyles
    {
        Inferno = 1,
        Soulless = 2,
        Magic = 3
    }
    public FlameStyles FlamePresentationStyle = FlameStyles.Magic;

    public GameObject boundObject;

    //private float lightIntensityMin = 3f;
    //private float lightIntensityMax = 5.0f;
    //private float lightRangeMin = 0.5f;
    //private float lightRangeMax = 0.8f;
    //private float emissionOverTimeMin = 0;
    //private float emissionOverTimeMax = 250;
    //private float emissionOverDistanceMin = 0;
    //private float emissionOverDistanceMax = 1000;

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
        //emissionModule.rateOverTime = 200;
        //emissionModule.rateOverDistance = 200;
        //particleLight.intensity = Mathf.Lerp(lightIntensityMin, lightIntensityMax, PowerLevel);
        //particleLight.range = Mathf.Lerp(lightRangeMin, lightRangeMax, PowerLevel);

    }

    private void Update()
    {
        if (boundObject)
        {
            //setFlamePreset(FlameStyle);
            Entity boundEntity = boundObject.gameObject.GetComponent<Entity>();
            Weapon boundWeapon = boundObject.gameObject.GetComponent<Weapon>();
            if (boundWeapon)
            {
                emissionModule.enabled = boundWeapon.TrueStrike || boundWeapon.ActionAnimated == Weapon.ActionAnimation.Parrying;
                if (boundWeapon.TrueStrike)
                {
                    SetFlamePresentation(FlameStyles.Inferno);
                }
                else if (boundWeapon.ActionAnimated == Weapon.ActionAnimation.Parrying)
                {
                    SetFlamePresentation(FlameStyles.Inferno);
                }
            }
            else if (boundEntity)
            {
                emissionModule.enabled = boundEntity.FinalDash;
            }
            else
            {
                emissionModule.enabled = true;
            }
        }
        else
        {
            emissionModule.enabled = false;
        }        
    }

    /***** PUBLIC *****/
    public void SetFlamePresentation(FlameStyles preset)
    {
        if(preset == FlamePresentationStyle) { return; }
        ParticleSystem.MinMaxGradient flameGradient = colorModule.color;
        switch (preset)
        {
            case FlameStyles.Inferno:
                particleLight.intensity = 7.5f;
                flameGradient.mode = ParticleSystemGradientMode.Gradient;
                particleLight.color = colors[(int)preset - 1];
                flameGradient.gradient = gradients[(int)preset - 1];
                break;
            case FlameStyles.Soulless:
                particleLight.intensity = 5f;
                flameGradient.mode = ParticleSystemGradientMode.Gradient;
                particleLight.color = colors[(int)preset - 1];
                flameGradient.gradient = gradients[(int)preset - 1];
                break;
            case FlameStyles.Magic:
                particleLight.intensity = 7.5f;
                flameGradient.mode = ParticleSystemGradientMode.TwoGradients;
                flameGradient.gradientMin = gradients[0];
                flameGradient.gradientMax = gradients[1];
                break;
        }
        colorModule.color = flameGradient;
        FlamePresentationStyle = preset;    
    }

    public void bindToObject(GameObject bindingObject)
    {
        if (!bindingObject) { return; }
        boundObject = bindingObject;
        buildGradients();
        SetFlamePresentation(FlamePresentationStyle);
        transform.SetParent(boundObject.transform, false);
        Entity boundEntity = boundObject.gameObject.GetComponent<Entity>();
        Wieldable boundWeapon = boundObject.gameObject.GetComponent<Weapon>();
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

    /***** PRIVATE *****/
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
        SetFlamePresentation(FlamePresentationStyle);
    }
}
