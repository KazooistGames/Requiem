using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Torch : Item
{
    public bool Lit = false;
    public static float maxFadeRange = 15;
    public static float minFadeRange = 3;
    public static float maxIntensity = 6f;
    public static float minIntensity = 4f;
    public static float maxLightRange = 1.25f;
    public static float minLightRange = 1.0f;
    public static float flickerRate = 0.1f;

    private float proximityToPlayerScalar;
    private Light flame;
    private ParticleSystem.EmissionModule particles;
    private ParticleSystem.SizeOverLifetimeModule particlesSize;
    private ParticleSystem.VelocityOverLifetimeModule particlesVelocity;

    protected override void Start()
    {
        base.Start();
        transform.localScale *= Entity.Scale;
        PhysicsBoxes.Add(GetComponent<CapsuleCollider>() ? GetComponent<CapsuleCollider>() : gameObject.AddComponent<CapsuleCollider>());
        PhysicsBoxes.Add(GetComponent<BoxCollider>() ? GetComponent<BoxCollider>() : gameObject.AddComponent<BoxCollider>());
        flame = GetComponentInChildren<Light>();
        particles = GetComponentInChildren<ParticleSystem>().emission;
        particlesSize = GetComponentInChildren<ParticleSystem>().sizeOverLifetime;
        particlesVelocity = GetComponentInChildren<ParticleSystem>().velocityOverLifetime;
        GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        equipType = EquipType.OneHanded;
        StartCoroutine(Flicker());
    }


    /**** torch functions ******/

    private IEnumerator Flicker()
    {
        yield return new WaitUntil(() => Player.INSTANCE);
        while (true)
        {
            if (flame && Lit)
            {
                float incrementIntensity = (maxIntensity - minIntensity) / 5f;
                float incrementRange = (maxLightRange - minLightRange) / 10f;
                float BeginToFadeRange = Hextile.Radius * minFadeRange;
                float CompletelyFadeRange = Hextile.Radius * maxFadeRange;
                float poximityToPlayer = (Player.INSTANCE.transform.position - transform.position).magnitude;
                proximityToPlayerScalar = Mathf.Lerp(1.0f, 0.0f, (poximityToPlayer - BeginToFadeRange) / (CompletelyFadeRange - BeginToFadeRange));
                particles.enabled = proximityToPlayerScalar > 0.25f;
                particlesSize.sizeMultiplier = Mathf.Lerp(0.0f, 1.0f, proximityToPlayerScalar);
                particlesVelocity.yMultiplier = Mathf.Lerp(0.0f, 0.5f, proximityToPlayerScalar);
                flame.enabled = proximityToPlayerScalar > 0.25f;
                flame.intensity = flame.enabled ? proximityToPlayerScalar * Mathf.Clamp(Mathf.Lerp(minIntensity, maxIntensity, Random.value), Mathf.Max(flame.intensity - incrementIntensity, minIntensity), Mathf.Min(flame.intensity + incrementIntensity, maxIntensity)) : 0;
                flame.range = flame.enabled ? proximityToPlayerScalar * Mathf.Clamp(Mathf.Lerp(minLightRange, maxLightRange, Random.value), Mathf.Max(flame.range - incrementRange, minLightRange), Mathf.Min(flame.range + incrementRange, maxLightRange)) : 0;
                yield return new WaitForSecondsRealtime(flickerRate);
            }
            else
            {
                particles.enabled = false;
                flame.enabled = false;
                flame.intensity = 0;
                flame.range = 0;
                yield return new WaitUntil(() => Lit);
            }

            
        }
    }
}
