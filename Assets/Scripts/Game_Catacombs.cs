using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Catacombs : Game
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        gameObject.name = "CATACOMBS";
        StartCoroutine(InitializeCatacombs());
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    private IEnumerator InitializeCatacombs()
    {
        new GameObject().AddComponent<Lobby>();
        yield return null;
        yield return new WaitUntil(() => Lobby.INSTANCE.ReadyToPlay);
    }
}
