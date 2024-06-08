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
        none = -1,
        Inferno = 1,
        Soulless = 2,
        Magic = 3
    }
    public FlameStyles FlamePresentationStyle = FlameStyles.Magic;
    private FlameStyles cachedFlameStye = FlameStyles.none;

    public GameObject boundObject;


    private void Awake()
    {
        particleLight = GetComponent<Light>();
        particles = GetComponent<ParticleSystem>();
        emissionModule = particles.emission;
        shapeModule = particles.shape;
        colorModule = particles.colorOverLifetime;
    }

    const float max_emission_over_time = 3;
    const float max_emission_over_distance = 3;
    private void Update()
    {
        if(FlamePresentationStyle != cachedFlameStye) 
        {
            SetFlameStyle(FlamePresentationStyle);
        }
        if (boundObject)
        {
            Entity boundEntity = boundObject.gameObject.GetComponent<Entity>();
            Weapon boundWeapon = boundObject.gameObject.GetComponent<Weapon>();
            emissionModule.rateOverTime = max_emission_over_time * PowerLevel;
            emissionModule.rateOverDistanceMultiplier = max_emission_over_distance * PowerLevel;
            if (boundWeapon)
            {
                emissionModule.enabled = true;
                if (boundWeapon.Action == Weapon.ActionAnim.Parrying)
                {
                    SetFlameStyle(FlameStyles.Soulless);
                    PowerLevel = 50;
                    particleLight.range = 0.5f;
                    particleLight.intensity = 3;
                }
                else if (boundWeapon.Specials[Weapon.SpecialAttacks.Truestrike])
                {
                    SetFlameStyle(FlameStyles.Inferno);
                    PowerLevel = 50;
                    particleLight.range = 0.75f;
                    particleLight.intensity = 5;
                }
                else if(boundWeapon.Tempo > 0)
                {
                    SetFlameStyle(FlameStyles.Magic);
                    PowerLevel = 100 * boundWeapon.Tempo;
                    particleLight.range = 0.75f;
                    particleLight.intensity = 5;
                }
                else
                {
                    emissionModule.enabled = false;
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
    }

    /***** PUBLIC *****/
    public void SetFlameStyle(FlameStyles preset)
    {
        if(preset == cachedFlameStye)
        {
            return;
        }
        if(gradients.Count == 0)
        {
            buildGradients();
        }
        ParticleSystem.MinMaxGradient flameGradient = colorModule.color;
        switch (preset)
        {
            case FlameStyles.Inferno:
                flameGradient.mode = ParticleSystemGradientMode.Gradient;
                particleLight.color = colors[(int)preset - 1];
                flameGradient.gradient = gradients[(int)preset - 1];
                break;
            case FlameStyles.Soulless:
                flameGradient.mode = ParticleSystemGradientMode.Gradient;
                particleLight.color = colors[(int)preset - 1];
                flameGradient.gradient = gradients[(int)preset - 1];
                break;
            case FlameStyles.Magic:
                flameGradient.mode = ParticleSystemGradientMode.TwoGradients;
                flameGradient.gradientMin = gradients[0];
                flameGradient.gradientMax = gradients[1];
                particleLight.color = colors[0];
                break;
        }
        colorModule.color = flameGradient;
        FlamePresentationStyle = preset;
        cachedFlameStye = preset;
    }

    public void bindToObject(GameObject bindingObject)
    {
        if (!bindingObject) { return; }
        boundObject = bindingObject;
        buildGradients();
        SetFlameStyle(FlamePresentationStyle);
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
        SetFlameStyle(FlamePresentationStyle);
    }
}
