using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Royale : Game
{
    public GameObject Map;

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
        Hextile.GenerateHexMesh();
        Hextile.HEXBOARD = Instantiate(Map);
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
