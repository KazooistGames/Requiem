using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Landmark_Barrier : Landmark
{
    public static List<GameObject> BarrierModelTemplates = new List<GameObject>();

    public int InnerBarrierAttempts;
    public int OuterBarrierAttempts;

    protected Dictionary<Hextile.HexPosition, GameObject> innerModels = new Dictionary<Hextile.HexPosition, GameObject>();
    protected Dictionary<Hextile.HexPosition, GameObject> outerModels = new Dictionary<Hextile.HexPosition, GameObject>();

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

    private void createNewBarrier(bool innerBarrier = true)
    {
        GameObject barrierModel = Instantiate(BarrierModelTemplates[(int)(Random.value * BarrierModelTemplates.Count)]);
        Hextile.HexPosition barrierPosition = (Hextile.HexPosition)Random.Range(1, 7);
        if (innerBarrier)
        {
            setInnerBarrierPosition(barrierModel, barrierPosition);
        }
        else
        {
            setOutterBarrierPosition(barrierModel, barrierPosition);
        }
    }

    private void setInnerBarrierPosition(GameObject barrier, Hextile.HexPosition position)
    {
        if ()
            innerModels.Add(position, barrier);
    }

    private void setOutterBarrierPosition(GameObject barrier, Hextile.HexPosition position)
    {
        outerModels.Add(position, barrier);
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


    private void setupBarrierModels()
    {

    }

}
