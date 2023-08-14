using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Stage : MonoBehaviour
{ 
    public static List<Stage> INSTANCES = new List<Stage>();

    public List<List<Hextile>> TileRings = new List<List<Hextile>>();
    public List<Hextile> AllTiles = new List<Hextile>();
    public Hextile CenterTile;
    public int Scale = 1;

    public bool Initialized = false;
    public bool Interactable = false;
    public bool PlayerInStage = false;

    public Landmark_Gate GateIn;
    public Landmark_Gate GateOut;

    //public Level Level;

    protected virtual void Start()
    {
        name = "STAGE";
        INSTANCES.Add(this);
        StartCoroutine(initializeRoutine());
    }

    private void Update()
    {
        if (!Initialized)
        {

        }
        else if (Player.INSTANCE ? !Player.INSTANCE.HostEntity : true)
        {

        }
        else
        {
            int tilesWithPlayerPresent = AllTiles.Count(x => x.ContainedObjects.Contains(Player.INSTANCE.HostEntity.gameObject));
            bool playerAtGate = GateIn ? GateIn.PlayerAtDoor : false;
            if(tilesWithPlayerPresent == 0)
            {
                PlayerInStage = false;
            }
            else if(Interactable && !playerAtGate)
            {
                PlayerInStage = true;
            }
        }
    }

    private void OnDestroy()
    {
        foreach(List<Hextile> tileList in TileRings)
        {
            foreach (Hextile tile in tileList)
            {
                if (tile)
                {
                    Destroy(tile);
                }
            }
        }

    }

    // CLASS FUNCTIONS //
    protected virtual IEnumerator initializeRoutine()
    {
        //generate circle
        yield return null;
        yield return Hextile.DrawCircle(Scale, GateIn.Tile, GateIn.PositionOnTile, TileRings);
        AllTiles = TileRings.Aggregate(new List<Hextile>(), (x, result) => result.Concat(x).ToList());
        CenterTile = TileRings[0][0];
        //create gate out and alter
        yield return null;
        if (!GateOut)
        {
            createGateOut();
        }
        //new GameObject().AddComponent<Landmark_Alter>().AssignToTile(CenterTile);
        //delete random tiles 
        yield return null;
        int totalTilesToDelete = Random.Range(0, 5);
        for (int i = 0; i < totalTilesToDelete; i++)
        {
            Hextile tile;
            int randomRing;
            int randomTile;
            do
            {
                randomRing = Random.Range(0, TileRings.Count);
                randomTile = Random.Range(0, TileRings[randomRing].Count);
                tile = TileRings[randomRing][randomTile];
            } while (!tile || tile == CenterTile || tile == GateOut.Tile || tile.AdjacentTiles.ContainsKey(GateIn.Tile));
            Destroy(tile);
            TileRings[randomRing].Remove(tile);
            AllTiles.Remove(tile);
        }
        //create other structures
        yield return null;
        foreach(Hextile tile in AllTiles)
        {
            if (!tile)
            {

            }
            else if(tile == CenterTile)
            {

            }
            else
            {
                new GameObject().AddComponent<Landmark_Barrier>().AssignToTile(tile);
            }
        }
        Initialized = true;
        yield break;
    }

    protected virtual void createGateOut(Hextile tileToPutGateOutOn = null, Hextile.HexPosition directionToPositionGate = Hextile.HexPosition.error)
    {
        Hextile.HexPosition newGatePosition = Hextile.Rotate(GateIn.PositionOnTile, Random.Range(-1, 2));
        Hextile temp = CenterTile;
        while (temp.AdjacentTiles.ContainsValue(newGatePosition))
        {
            temp = temp.Extend(newGatePosition);
        }
        GateOut = new GameObject().AddComponent<Landmark_Gate>();
        GateOut.AssignToTile(temp);
        GateOut.SetPositionOnTile(newGatePosition);
    }
  
}
