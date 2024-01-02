using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Landmark_Pillar : Landmark
{
    public static List<GameObject> PillarModels = new List<GameObject>();

    protected Rigidbody body;

    private GameObject brokeTop;
    private GameObject brokeBottom;


    private void OnTriggerEnter(Collider other)
    {
        Weapon weapon = other.GetComponent<Weapon>();
        if(weapon ? weapon.TrueStrike : false)
        {
            body.isKinematic = true;
            model.GetComponent<MeshRenderer>().enabled = false;
            foreach(Collider collider in model.GetComponents<Collider>())
            {
                collider.enabled = false;
            }
            brokeTop.SetActive(true);
            brokeBottom.SetActive(true);
            Impacted();
        }
    }

    /***** PUBLIC *****/
    public override void AssignToTile(Hextile Tile)
    {
        base.AssignToTile(Tile);
        transform.localPosition = Vector3.zero + Vector3.up * Hextile.Thickness / 2f;
        gameObject.name = "Pillar";
        gameObject.layer = Requiem.layerObstacle;
        if (PillarModels.Count == 0)
        {
            PillarModels = Resources.LoadAll<GameObject>("Prefabs/Pillars/").ToList();
        }
        model = Instantiate(PillarModels[0]);
        model.transform.SetParent(transform, false);
        model.transform.localEulerAngles = Vector3.up * Random.value * 360;
        body = GetComponent<Rigidbody>() ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();
        body.mass = model.tag == "Broken" ? 100 : 150;
        body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        Vector3 temp = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(0, temp.y, 0);
        brokeTop = model.transform.GetChild(1).gameObject;
        brokeBottom = model.transform.GetChild(2).gameObject;
    }


    /***** PROTECTED *****/

    
    /***** PRIVATE *****/
}

