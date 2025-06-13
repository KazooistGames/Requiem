using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static Map INSTANCE;
    public static int RadiusOfArena = 1;

    public static List<List<Hextile>> ArenaTiles = new List<List<Hextile>>();
    public static List<Landmark_Gate> Gates = new List<Landmark_Gate>();
    public static List<Hextile> Chambers = new List<Hextile>();

    public static Hextile CenterTile;
    public static Landmark_Alter Alter;
    public static Landmark_Bloodwell BloodWell;
    public static Landmark_Credits Credits;

    public static bool Commissioned = false;

    public static List<Hextile> Tiles = new List<Hextile>();

    void Start()
    {
        if (INSTANCE)
        {
            Destroy(this);
        }
        else
        {
            INSTANCE = this;
        }
        StartCoroutine(Build_Map());
    }


    private void OnDestroy()
    {
        Tiles.Clear();
        ArenaTiles.Clear();
        Gates.Clear();
        Chambers.Clear();
        CenterTile = null;
        Alter = null;
        BloodWell = null;
        Credits = null;
        StopAllCoroutines();
    }
    /***** PUBLIC *****/

    /***** PROTECTED *****/
    protected IEnumerator Build_Map()
    {
        yield return null;
        Goon.Alternative_Weapon = null;

        CenterTile = Hextile.GenerateRootTile();
        yield return null;
        yield return Hextile.DrawCircle(RadiusOfArena, Hextile.LastGeneratedTile, Tiles: ArenaTiles);

        Hextile.HexPosition firstGateDirection = (Hextile.HexPosition)1;
        Hextile edgeOne = ArenaTiles[0][0].Edge(firstGateDirection);
        Chambers.Add(edgeOne.Extend(firstGateDirection));
        yield return null;
        Gates.Add(new GameObject().AddComponent<Landmark_Gate>());
        Gates[0].AssignToTile(edgeOne);
        Gates[0].SetPositionOnTile(firstGateDirection);
        yield return null;

        Hextile.HexPosition secondGateDirection = Hextile.RotateHexPosition(firstGateDirection, -1);
        Hextile edgeTwo = ArenaTiles[0][0].Edge(secondGateDirection);
        Chambers.Add(edgeTwo.Extend(secondGateDirection));
        yield return null;
        Gates.Add(new GameObject().AddComponent<Landmark_Gate>());
        Gates[1].AssignToTile(edgeTwo);
        Gates[1].SetPositionOnTile(secondGateDirection);  
        yield return null;

        Hextile.HexPosition thirdGateDirection = Hextile.RotateHexPosition(secondGateDirection, -2);
        Hextile edgeThree = ArenaTiles[0][0].Edge(thirdGateDirection);
        Chambers.Add(edgeThree.Extend(thirdGateDirection));
        yield return null;
        Gates.Add(new GameObject().AddComponent<Landmark_Gate>());
        Gates[2].AssignToTile(edgeThree);
        Gates[2].SetPositionOnTile(thirdGateDirection);
        yield return null;

        Hextile.HexPosition fourthGateDirection = Hextile.RotateHexPosition(thirdGateDirection, -1);
        Hextile edgeFour = ArenaTiles[0][0].Edge(fourthGateDirection);
        Chambers.Add(edgeFour.Extend(fourthGateDirection));
        yield return null;
        Gates.Add(new GameObject().AddComponent<Landmark_Gate>());
        Gates[3].AssignToTile(edgeFour);
        Gates[3].SetPositionOnTile(fourthGateDirection);
        yield return null;


        //new GameObject().AddComponent<Landmark_Well>().AssignToTile(ArenaTiles[0][0].Edge((Hextile.HexPosition)5));

        /** BUILD CREDITS **/
        Credits = new GameObject().AddComponent<Landmark_Credits>();
        Credits.AssignToTile(Chambers[0]);
        yield return null;  
        Credits.SetPositionOnTile(firstGateDirection);

        Tiles.AddRange(ArenaTiles.Aggregate(new List<Hextile>(), (x, result) => result.Concat(x).ToList()));
        Tiles.AddRange(Chambers);

        yield return Build_Landmarks(ArenaTiles);

        yield return new WaitForSeconds(0.5f);

        new GameObject().AddComponent<Player>();
        Hextile randomTile = ArenaTiles[ArenaTiles.Count - 1][UnityEngine.Random.Range(0, ArenaTiles[ArenaTiles.Count - 1].Count)];
        randomTile = ArenaTiles[ArenaTiles.Count - 1][UnityEngine.Random.Range(0, ArenaTiles[ArenaTiles.Count - 1].Count)];

        Commissioned = true;
    }




    /***** PRIVATE *****/
    
    private IEnumerator Build_Landmarks(List<List<Hextile>> arenaTileRings)
    {
        foreach (List<Hextile> ring in arenaTileRings)
        {
            int ringNum = arenaTileRings.IndexOf(ring);
            switch (ringNum)
            {
                case 0:
                    Alter = new GameObject().AddComponent<Landmark_Alter>();
                    Alter.AssignToTile(ring[0]);
                    BloodWell = new GameObject().AddComponent<Landmark_Bloodwell>();
                    BloodWell.AssignToTile(ring[0].Edge((Hextile.HexPosition)5));
                    Landmark_Barrier barrier = new GameObject().AddComponent<Landmark_Barrier>();
                    barrier.InnerBarrierAttempts = 0;
                    barrier.AssignToTile(ring[0]);
                    break;
                case 1:
                    foreach (Hextile tile in ring)
                    {
                        Hextile.HexPosition position = arenaTileRings[0][0].AdjacentTiles.First(x => x.Key == tile).Value;
                        if (tile.Landmarks.Find(x => x.GetComponent<Landmark_Bloodwell>()))
                        {
                            Landmark_Barrier newBarrier = new GameObject().AddComponent<Landmark_Barrier>();
                            newBarrier.InnerBarrierAttempts = 0;
                            newBarrier.OuterBarrierAttempts = 4;
                            newBarrier.AssignToTile(tile);
                        }
                        else
                        {
                            new GameObject().AddComponent<Landmark_Barrier>().AssignToTile(tile);
                        }
                        yield return new WaitForFixedUpdate();
                        yield return null;
                    }
                    break;
                case 2:
                    foreach (Hextile tile in ring)
                    {
                        if (tile.Landmarks.Find(x=>x.GetComponent<Landmark_Bloodwell>()))
                        {
                            Landmark_Barrier newBarrier = new GameObject().AddComponent<Landmark_Barrier>();
                            newBarrier.InnerBarrierAttempts = 0;
                            newBarrier.OuterBarrierAttempts = 4;
                            newBarrier.AssignToTile(tile);
                        }
                        else
                        {
                            new GameObject().AddComponent<Landmark_Barrier>().AssignToTile(tile);
                        }
                        yield return new WaitForFixedUpdate();
                        yield return null;
                    }
                    break;
            }
        }
    }


}
