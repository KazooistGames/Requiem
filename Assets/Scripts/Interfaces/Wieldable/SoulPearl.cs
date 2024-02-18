
using System.Collections;
using System.Linq;
using UnityEngine;


public class SoulPearl : Wieldable
{
    public bool WillingToRematerialize = false;
    public Wieldable Phylactery;
    private _Flames spiritFlame;
    private SphereCollider physicsSphere;

    private float timeIdle = 0;

    public static float Awareness_Radius = 1.5f;
    public static float Materialization_Debounce = 3;
    public static float Wander_Debounce = 15;

    protected override void Start()
    {
        gameObject.name = "SoulPearl";
        //awarenessRadius = Hextile.Radius * 1.5f;
        physicsSphere = gameObject.AddComponent<SphereCollider>();
        physicsSphere.center = Vector3.zero;
        physicsSphere.radius = 0.025f;
        PhysicsBoxes.Add(physicsSphere);
        spiritFlame = Instantiate(Requiem.SpiritFlameTemplate).GetComponent<_Flames>();
        spiritFlame.transform.SetParent(transform);
        spiritFlame.transform.localPosition = Vector3.zero;
        spiritFlame.shapeModule.shapeType = ParticleSystemShapeType.Circle;
        spiritFlame.shapeModule.radius = 0.001f;
        spiritFlame.shapeModule.scale = Vector3.one * 0.001f;
        spiritFlame.SetFlameStyle(_Flames.FlameStyles.Soulless);
        spiritFlame.particleLight.range = 0.2f;
        spiritFlame.particleLight.intensity = 1.5f;
        spiritFlame.boundObject = gameObject;
        ParticleSystem.MainModule main = spiritFlame.GetComponent<ParticleSystem>().main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startSize = 0.05f;
        Wander_Debounce = Mathf.Lerp(10, 15, Random.value);
    }

    protected override void Update()
    {
        timeIdle += Time.deltaTime;
        if (Telecommuting)
        {

        }
        else if (timeIdle >= Wander_Debounce)
        {
            timeIdle = 0;
            Wander_Debounce = Mathf.Lerp(10, 15, Random.value);
            Vector3 target = Requiem.RAND_POS_IN_TILE(getRandomNeighborTile());
            Telecommute(target, 0.25f, callback: null, enablePhysicsWhileInFlight: false, useScalarAsSpeed: true);
        }
        if(Phylactery)
        {
            Weapon randomWeapon = FindObjectsOfType<Weapon>().FirstOrDefault(x => !x.Wielder && (x.transform.position - transform.position).magnitude <= Awareness_Radius);
            if (randomWeapon)
            {
                Phylactery = randomWeapon; 
            }
        }
        

    }

    /***** PUBLIC *****/
    public void FlyToPhylactery() 
    {
        if (!Phylactery)
        {
            Rematerialize();
        }
        else if (timeIdle >= Materialization_Debounce)
        {
            Telecommute(Phylactery.gameObject, 0.5f, (x) => x.GetComponent<SoulPearl>().Rematerialize(), useScalarAsSpeed: true);
        }
    }
    public void Rematerialize()
    {
        Entity newForm = new GameObject().AddComponent<Ghosty>();
        newForm.gameObject.AddComponent<Goon>();
        newForm.transform.position = transform.position;
        if (Phylactery)
        {
            if (Phylactery.Wielder)
            {
                Phylactery.DropItem();
            }
            Phylactery.PickupItem(newForm);
        }
        Destroy(gameObject);
    }

    /***** PRIVATE *****/
    private Hextile getClosestTile()
    {
        Hextile closest = Hextile.Tiles.OrderBy(x => (x.transform.position - transform.position).magnitude).First();
        return closest;
    }

    private Hextile getRandomNeighborTile()
    {
        Hextile closest = getClosestTile();
        Hextile randomNeighbor = closest.AdjacentTiles.Keys.ElementAt(Random.Range(0, closest.AdjacentTiles.Keys.Count));
        return randomNeighbor;
    }


}
