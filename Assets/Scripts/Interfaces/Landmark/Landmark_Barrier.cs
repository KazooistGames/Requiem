using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Landmark_Barrier : Landmark
{
    public static List<GameObject> BarrierModels = new List<GameObject>();
    public int modelCount;
    protected List<GameObject> models = new List<GameObject>();
    protected List<(Hextile.HexPosition, float)> positions = new List<(Hextile.HexPosition, float)>();


    /********Barrier functions ***********/
    public override void AssignToTile(Hextile Tile)
    {
        base.AssignToTile(Tile);

        gameObject.name = "Barrier";
        gameObject.layer = Requiem.layerObstacle;
        if (BarrierModels.Count == 0)
        {
            BarrierModels = Resources.LoadAll<GameObject>("Prefabs/Barriers/").ToList();
        }
        configureBarrierModels();    

    }

    private void configureBarrierModels()
    {
        modelCount = Random.Range(4, 6);
        //modelCount = 6;
        for (int i = 0; i < modelCount; i++)
        {
            GameObject model = Instantiate(BarrierModels[(int)(Random.value * BarrierModels.Count)]);
            model.transform.SetParent(transform, false);
            float angle = 0;
            List<Hexwall> wallCandidates = Tile.Walls.Where(x => x.gameObject.activeSelf).ToList();
            if (wallCandidates.Count == 0)
            {
                return;
            }
            Hextile.HexPosition position = wallCandidates[Random.Range(0, wallCandidates.Count)].Position;
            switch (position)
            {
                case Hextile.HexPosition.UpRight:
                    angle = 60f;
                    break;
                case Hextile.HexPosition.Up:
                    angle = 0f;
                    break;
                case Hextile.HexPosition.UpLeft:
                    angle = -60f;
                    break;
                case Hextile.HexPosition.DownLeft:
                    angle = -120f;
                    break;
                case Hextile.HexPosition.Down:
                    angle = 180f;
                    break;
                case Hextile.HexPosition.DownRight:
                    angle = 120f;
                    break;
            }
            model.transform.localEulerAngles = Vector3.up * angle;
            float offsetAngle = 0;
            float positionScalar = Hextile.Radius / 3;
            if (positions.Count(x => x.Item1 == position) > 0)
            {
                offsetAngle = Random.Range(0, 6) * 60;
                do
                {
                    offsetAngle = Random.Range(1, 6) * 60;
                    offsetAngle += Mathf.Sign(Random.value - 0.5f) * 60;
                } while (positions.Count(x => x.Item1 == position && x.Item2 == offsetAngle) > 0);
                model.transform.localEulerAngles += Vector3.up * offsetAngle;
            }
            else if (Random.value > 0.5f)
            {
                offsetAngle = 180;
                positionScalar = Hextile.Radius - positionScalar;
            }

            positions.Add((position, offsetAngle));            
            model.transform.localPosition = model.transform.localPosition = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle), 1, Mathf.Cos(Mathf.Deg2Rad * angle)) * positionScalar;
            Rigidbody body = model.GetComponent<Rigidbody>() ? model.GetComponent<Rigidbody>() : model.AddComponent<Rigidbody>();
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            body.freezeRotation = true;
            body.isKinematic = true;
            models.Add(model);
        }
        StartCoroutine(unbindModels());
    }

    private IEnumerator unbindModels()
    {
        foreach(GameObject model in models)
        {
            Rigidbody body = model.GetComponent<Rigidbody>();
            MeshCollider meshCol = model.GetComponent<MeshCollider>();
            if (meshCol)
            {
                meshCol.convex = true;
            }
            body.isKinematic = false;
            yield return new WaitForFixedUpdate();
            yield return null;
            body.isKinematic = true;
            Vector3 temp = model.transform.localPosition;
            model.transform.localPosition = new Vector3(temp.x, Mathf.Lerp(0.32f, 0.42f, Random.value));
            if (meshCol)
            {
                meshCol.convex = false;
            }
        }
        yield break;
    }

}
