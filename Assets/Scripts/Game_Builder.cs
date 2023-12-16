using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Builder: Requiem
{
    public int MapRadius = 8;
    List<List<Hextile>> MapRings = new List<List<Hextile>>();

    protected override void Start()
    {
        base.Start();
        StartCoroutine(gameInit());
    }

    protected override void Update()
    {
        base.Update();
    }

    /* ROUTINES */
    protected IEnumerator gameInit()
    {
        
        yield return Hextile.DrawCircle(MapRadius, Hextile.GenerateRootTile(), Tiles: MapRings);
        Commissioned = true;
        StartCoroutine(gameLoop());
        yield break;
    }

    protected IEnumerator gameLoop()
    {
        while (true)
        {
            yield return null;
        }
    }
}
