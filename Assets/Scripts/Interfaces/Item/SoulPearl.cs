
using System.Linq;
using UnityEngine;


public class SoulPearl : Item
{
    public Item Phylactery;
    private SpiritFlame spiritFlame;
    private SphereCollider physicsSphere;

    private float timeWanderingSeconds = 0;
    private float awarenessRadius;

    protected override void Start()
    {
        gameObject.name = "SoulPearl";
        equipType = EquipType.Consummable;
        awarenessRadius = Hextile.Radius * 1.5f;
        physicsSphere = gameObject.AddComponent<SphereCollider>();
        physicsSphere.center = Vector3.zero;
        physicsSphere.radius = 0.025f;
        PhysicsBoxes.Add(physicsSphere);
        spiritFlame = Instantiate(Game.SpiritFlameTemplate).GetComponent<SpiritFlame>();
        spiritFlame.transform.SetParent(transform);
        spiritFlame.transform.localPosition = Vector3.zero;
        spiritFlame.shapeModule.shapeType = ParticleSystemShapeType.Circle;
        spiritFlame.shapeModule.radius = 0.001f;
        spiritFlame.shapeModule.scale = Vector3.one * 0.001f;
        spiritFlame.setFlamePreset(SpiritFlame.Preset.Soulless);
        spiritFlame.particleLight.range /= 2;
        spiritFlame.particleLight.intensity /= 2;
    }

    protected override void Update()
    {
        timeWanderingSeconds += Time.deltaTime;
        if(timeWanderingSeconds > 1 && !Telecommuting)
        {
            if (Phylactery)
            {
                Telecommute(Phylactery.gameObject, 0.25f, (x) => x.GetComponent<SoulPearl>().Rematerialize(), useScalarAsSpeed: true);
            }
            else if ((Player.INSTANCE.transform.position - transform.position).magnitude <= awarenessRadius)
            {
                Weapon randomWeapon = FindObjectsOfType<Weapon>().FirstOrDefault(x => !x.Wielder && (x.transform.position - transform.position).magnitude <= awarenessRadius);
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


        if (Phylactery)
        {
            if (Phylactery.Wielder)
            {
                Phylactery.Wielder.alterPoise(Phylactery.Wielder.Strength);
            }
            else
            {
                Character newForm = new GameObject().AddComponent<Ghosty>();
                newForm.transform.position = transform.position;
                if (Phylactery.Wielder)
                {
                    Phylactery.DropItem();
                }
                Phylactery.PickupItem(newForm);
            }
            Destroy(gameObject);
        }

    }
}
