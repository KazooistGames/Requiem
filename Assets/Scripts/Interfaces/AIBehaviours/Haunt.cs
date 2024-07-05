using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Haunt : AIBehaviour
{
    public static Haunt INSTANCE;
    public Hextile TargetTile;


    protected override void Start()
    {
        base.Start();
        INSTANCE = this;
        Destroy(entity.indicator);
        State = AIState.custom;
        waypointCommanded = true;
        behaviourParams[BehaviourType.waypoint] = (true, 1.0f);
    }


    protected override void Update()
    {
        base.Update();
        if (waypointDeadbanded)
        {
            waypointCoordinates = find_target_tile().transform.position;
            waypointDeadbanded = false;
        }
    }

   
    private Hextile find_target_tile()
    {
        Hextile playerTile = Player.INSTANCE.HostEntity.TileLocation;
        int randomIndex = Random.Range(0, playerTile.AdjacentTiles.Count);
        return playerTile.AdjacentTiles.ElementAt(randomIndex).Key;
    }

}
