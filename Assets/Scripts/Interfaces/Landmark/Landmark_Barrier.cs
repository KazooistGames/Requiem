using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Landmark_Barrier : Landmark
{
    public static List<GameObject> BarrierModelTemplates = new List<GameObject>();

    public bool OuterBarriersOnWallsOnly = false;

    public int InnerBarrierAttempts = 3;
    public int OuterBarrierAttempts = 3;

    protected Dictionary<Hextile.HexPosition, GameObject> innerBarrierModels = new Dictionary<Hextile.HexPosition, GameObject>();
    protected Dictionary<Hextile.HexPosition, GameObject> outerBarrierModels = new Dictionary<Hextile.HexPosition, GameObject>();

    protected List<(Hextile.HexPosition, float)> positions = new List<(Hextile.HexPosition, float)>();


    /***** PUBLIC *****/
    public override void AssignToTile(Hextile Tile)
    {
        base.AssignToTile(Tile);
        gameObject.name = "Barrier";
        gameObject.layer = Requiem.layerObstacle;
        if (BarrierModelTemplates.Count == 0)
        {
            BarrierModelTemplates = Resources.LoadAll<GameObject>("Prefabs/Barriers/").ToList();
        }
        setupBarrierModels();
    }


    /***** PROTECTED *****/


    /***** PRIVATE *****/

    private void setupBarrierModels()
    {
        for (int innerAttempt = 0; innerAttempt < InnerBarrierAttempts; innerAttempt++)
        {
            attemptToSpawnInnerBarrier();
        }
        for (int outerAttempt = 0; outerAttempt < OuterBarrierAttempts; outerAttempt++)
        {
            attemptToSpawnOuterBarrier();
        }
    }

    private void attemptToSpawnInnerBarrier()
    {
        Hextile.HexPosition barrierPosition = (Hextile.HexPosition)Random.Range(1, 7);
        if (!innerBarrierModels.ContainsKey(barrierPosition))
        {
            GameObject barrierModel = Instantiate(BarrierModelTemplates[(int)(Random.value * BarrierModelTemplates.Count)]);
            barrierModel.transform.SetParent(transform);
            float angle = hexPositionToAngle(barrierPosition) + 30;
            barrierModel.transform.localEulerAngles = Vector3.up * angle;
            Vector3 verticalOffset = Vector3.up * Hextile.Thickness / 2;
            Vector3 horizontalOffset = Vector3.RotateTowards(Vector3.forward, Vector3.back, -Mathf.Deg2Rad * angle, 0).normalized * Hextile.Radius * (1-Mathf.Cos(Mathf.Deg2Rad * 24.5f));
            barrierModel.transform.localPosition = verticalOffset + horizontalOffset;
            innerBarrierModels.Add(barrierPosition, barrierModel);
        }
    }

    private void attemptToSpawnOuterBarrier()
    {
        Hextile.HexPosition barrierPosition = (Hextile.HexPosition)Random.Range(1, 7);
        bool gateAlreadyOnWall = Tile.Landmarks.FirstOrDefault(x => x.GetComponent<Landmark_Gate>() && x.PositionOnTile == barrierPosition);
        if (!outerBarrierModels.ContainsKey(barrierPosition) && !gateAlreadyOnWall)
        {
            GameObject barrierModel = Instantiate(BarrierModelTemplates[(int)(Random.value * BarrierModelTemplates.Count)]);
            barrierModel.transform.SetParent(transform);
            float angle = hexPositionToAngle(barrierPosition);
            float angleOffset = 180 + Mathf.Sign(Random.value - 0.5f) * 30;
            barrierModel.transform.localEulerAngles = Vector3.up * (angle + angleOffset);
            Vector3 verticalOffset = Vector3.up * Hextile.Thickness / 2;
            Vector3 horizontalOffset = Vector3.RotateTowards(Vector3.forward, Vector3.back, -Mathf.Deg2Rad * angle, 0).normalized * Hextile.Radius * Mathf.Cos(Mathf.Deg2Rad * 35.5f);
            barrierModel.transform.localPosition = verticalOffset + horizontalOffset;
            outerBarrierModels.Add(barrierPosition, barrierModel);
        }
    }

    private float hexPositionToAngle(Hextile.HexPosition position)
    {
        switch (position)
        {
            case Hextile.HexPosition.UpRight:
                return 60f;
            case Hextile.HexPosition.Up:
                return 0f;
            case Hextile.HexPosition.UpLeft:
                return -60f;
            case Hextile.HexPosition.DownLeft:
                return -120f;
            case Hextile.HexPosition.Down:
                return 180f;
            case Hextile.HexPosition.DownRight:
                return 120f;
            default:
                return 0;
        }
    }




}
