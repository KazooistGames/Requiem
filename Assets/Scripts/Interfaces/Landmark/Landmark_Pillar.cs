using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Landmark_Pillar : Landmark
{
    public static List<GameObject> PillarModels = new List<GameObject>();

    protected Rigidbody body;

    protected Torch torch;
    /********** pillar functions *************/
    public override void Impacted()
    {
        base.Impacted();
        if(torch ? torch.MountTarget: false)
        {
            torch.MountTarget = null;
            torch.DropItem(yeet: true);
            torch = null;
        }
    }

    public override void AssignToTile(Hextile Tile)
    {
        base.AssignToTile(Tile);
        transform.localPosition = Vector3.zero + Vector3.up * Hextile.Thickness / 2f;
        gameObject.name = "Pillar";
        gameObject.layer = Game.layerObstacle;
        if (PillarModels.Count == 0)
        {
            PillarModels = Resources.LoadAll<GameObject>("Prefabs/Pillars/").ToList();
        }
        model = Instantiate(PillarModels[Random.Range(0, PillarModels.Count)]);
        model.transform.SetParent(transform, false);
        body = GetComponent<Rigidbody>() ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();
        body.mass = model.tag == "Broken" ? 100 : 150;
        body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        //transform.LookAt(Game.Instance.Board.RootTile.transform.position, Vector3.up);
        //body.freezeRotation = true;
        Vector3 temp = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(0, temp.y, 0);
        torch = Instantiate(Resources.Load<GameObject>("Prefabs/Wieldable/torch")).GetComponent<Torch>();
        if (model.tag == "Broken")
        {
            torch.transform.position = transform.position + new Vector3(0, 0.35f, 0.07f);
            GameObject otherHalf = Instantiate(model);
            otherHalf.transform.position = model.transform.position + Vector3.up;
            otherHalf.transform.Rotate(new Vector3(Random.value * 180, Random.value * 180, Random.value * 180));
            Rigidbody body = otherHalf.AddComponent<Rigidbody>();
            body.mass = model.tag == "Broken" ? 100 : 150;
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            otherHalf.transform.SetParent(transform.parent);
        }
        else
        {
            torch.Mount(gameObject, new Vector3(0, 0.35f, 0.07f));
        }
        torch.transform.localEulerAngles = new Vector3(45, 0, 0);
        torch.Lit = false;
    }
}

