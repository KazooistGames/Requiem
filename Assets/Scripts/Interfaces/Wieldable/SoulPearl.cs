
using System.Linq;
using UnityEngine;


public class SoulPearl : Wieldable
{
    public Wieldable Phylactery;
    private _Flames spiritFlame;
    private SphereCollider physicsSphere;

    private float timeWanderingSeconds = 0;

    public static float Awareness_Radius = 1.5f;
    public static float Transition_Debounce = 3;

    protected override void Start()
    {
        gameObject.name = "SoulPearl";
        //awarenessRadius = Hextile.Radius * 1.5f;
        physicsSphere = gameObject.AddComponent<SphereCollider>();
        physicsSphere.center = Vector3.zero;
        physicsSphere.radius = 0.025f;
        PhysicsBoxes.Add(physicsSphere);
        spiritFlame = Instantiate(Game.SpiritFlameTemplate).GetComponent<_Flames>();
        spiritFlame.transform.SetParent(transform);
        spiritFlame.transform.localPosition = Vector3.zero;
        spiritFlame.shapeModule.shapeType = ParticleSystemShapeType.Circle;
        spiritFlame.shapeModule.radius = 0.001f;
        spiritFlame.shapeModule.scale = Vector3.one * 0.001f;
        spiritFlame.FlameStyle(_Flames.FlameStyles.Soulless);
        spiritFlame.particleLight.range /= 2;
        spiritFlame.particleLight.intensity /= 2;
        spiritFlame.boundObject = gameObject;
    }

    protected override void Update()
    {
        timeWanderingSeconds += Time.deltaTime;
        if(timeWanderingSeconds > Transition_Debounce && !Telecommuting)
        {
            if (!Player.INSTANCE)
            {

            }
            else if (Phylactery)
            {
                Telecommute(Phylactery.gameObject, 0.25f, (x) => x.GetComponent<SoulPearl>().Rematerialize(), useScalarAsSpeed: true);
            }
            else if ((Player.INSTANCE.transform.position - transform.position).magnitude <= Awareness_Radius)
            {
                Weapon randomWeapon = FindObjectsOfType<Weapon>().FirstOrDefault(x => !x.Wielder && (x.transform.position - transform.position).magnitude <= Awareness_Radius);
                if (randomWeapon)
                {
                    Phylactery = randomWeapon;
                    Telecommute(randomWeapon.gameObject, 0.5f, (x) => x.GetComponent<SoulPearl>().Rematerialize(), useScalarAsSpeed: true);;
                }
            }
        }

    }
    private void Rematerialize()
    {
        if (!Phylactery)
        {

        }
        else if (Phylactery.Wielder)
        {
            Phylactery = null;
        }
        else
        {
            Entity newForm = new GameObject().AddComponent<Ghosty>();
            newForm.gameObject.AddComponent<Goon>();
            newForm.transform.position = transform.position;
            if (Phylactery.Wielder)
            {
                Phylactery.DropItem();
            }
            Phylactery.PickupItem(newForm);    
            Destroy(gameObject);
        }

    }
}
