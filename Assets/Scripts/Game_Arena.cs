using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Game_Arena : Game
{
    private int RadiusOfTiles = 1;
    private List<List<Hextile>> TileRings = new List<List<Hextile>>();
    List<Hextile> AllTiles = new List<Hextile>();
    private Hextile centerTile;
    private Landmark_Well well;
    private Landmark_Alter alter;
    private Idol idol;

    public enum GameState
    {
        Liminal,
        Wave,
        Boss,
        Finale,
    }
    public GameState State = GameState.Liminal;

    private Spawner PatrolSpawner;
    private Spawner MobSpawner;
    private Spawner EliteSpawner;
    private Spawner BossSpawner;

    public int Difficulty = 0;


    protected override void Start()
    {
        base.Start();
        StartCoroutine(PlayGame());
    }

    protected override void Update()
    {
        base.Update();
        gameObject.name = "ARENA";
    }

    protected IEnumerator PlayGame()
    {
        yield return null;
        centerTile = Hextile.GenerateRootTile();
        yield return null;
        yield return Hextile.DrawCircle(RadiusOfTiles, Hextile.LastGeneratedTile, Tiles: TileRings);
        yield return new WaitForSecondsRealtime(0.5f);
        AllTiles = TileRings.Aggregate(new List<Hextile>(), (x, result) => result.Concat(x).ToList());
        yield return gameInitLandmarks();
        //yield return gameInitCrypts();
        new GameObject().AddComponent<Player>();
        idol = Instantiate(Resources.Load<GameObject>("Prefabs/Wieldable/Idol")).GetComponent<Idol>();
        Hextile randomTile = TileRings[TileRings.Count - 1][UnityEngine.Random.Range(0, TileRings[TileRings.Count - 1].Count)];
        idol.transform.position = RAND_POS_IN_TILE(randomTile);
        alter.DesiredOffering = idol.gameObject;
        randomTile = TileRings[TileRings.Count - 1][UnityEngine.Random.Range(0, TileRings[TileRings.Count - 1].Count)];
        SPAWN(typeof(Shade), typeof(Janitor), RAND_POS_IN_TILE(randomTile));
        configureSpawners();
        Commissioned = true;
        yield return gameLoop();
    }

    private IEnumerator gameInitLandmarks()
    {
        foreach (List<Hextile> ring in TileRings)
        {
            int ringNum = TileRings.IndexOf(ring);
            switch (ringNum)
            {
                case 0:

                    alter = new GameObject().AddComponent<Landmark_Alter>();
                    alter.AssignToTile(centerTile);

                    break;
                case 1:
                    well = new GameObject().AddComponent<Landmark_Well>();
                    well.AssignToTile(ring[UnityEngine.Random.Range(0, ring.Count)]);
                    foreach (Hextile tile in ring)
                    {
                        if (tile ? (tile.Landmarks.Count == 0) : false)
                        {
                            //new GameObject().AddComponent<Landmark_Barrier>().AssignToTile(tile);
                            new GameObject().AddComponent<Landmark_Pillar>().AssignToTile(tile);
                        }
                        yield return new WaitForFixedUpdate();
                        yield return null;
                    }
                    break;
            }
        }
    }


    protected IEnumerator gameLoop()
    {
        
        State = GameState.Liminal;
        Difficulty = 1;
        yield return new WaitUntil(() => Commissioned);
        Player.INSTANCE.HostEntity.transform.position = RAND_POS_IN_TILE(AllTiles[0]);
        bool allMobsSpawned = false;
        MobSpawner.FinishedPeriodicSpawning.AddListener((mobs) => allMobsSpawned = true);
        bool allElitesSpawned = false;
        EliteSpawner.FinishedPeriodicSpawning.AddListener((elites) => allElitesSpawned = true);
        while (Difficulty < 6)
        {
            State = GameState.Liminal;
            allMobsSpawned = false;
            yield return new WaitUntil(() => alter.Used);
            State = GameState.Wave;
            int MobsThisWave = 6 * Difficulty;
            PatrolSpawner.PeriodicallySpawn(10, Difficulty, 2 * Difficulty, 3 * Difficulty);
            MobSpawner.PeriodicallySpawn(20, Difficulty, 2 * Difficulty, 3 * Difficulty);
            EliteSpawner.PeriodicallySpawn(30, 1, Difficulty, Difficulty);
            yield return new WaitUntil(() => allMobsSpawned && allElitesSpawned);
            State = GameState.Boss;
            allElitesSpawned = false;
            List<GameObject> wraiths = BossSpawner.InstantSpawn(Mathf.CeilToInt(Difficulty/2));
            yield return new WaitUntil(() => wraiths.Count(x=>x!= null) == 0);
            Difficulty++;
            well.Refill(50);             
        }
        State = GameState.Finale;
        SPAWN(typeof(Devil), typeof(Champion), centerTile.transform.position);
    }
  

    private void configureSpawners()
    {
        PatrolSpawner = gameObject.AddComponent<Spawner>();
        PatrolSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Skully));
        PatrolSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Biter));
        PatrolSpawner.transform.position = centerTile.transform.position + Vector3.up;

        MobSpawner = gameObject.AddComponent<Spawner>();
        MobSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Skelly));
        MobSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Goon));
        MobSpawner.transform.position = centerTile.transform.position + Vector3.up;

        EliteSpawner = gameObject.AddComponent<Spawner>();
        EliteSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Nephalim));
        EliteSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Bully));
        EliteSpawner.transform.position = centerTile.transform.position + Vector3.up;

        BossSpawner = gameObject.AddComponent<Spawner>();
        BossSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Wraith));
        BossSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Revanent));
        BossSpawner.transform.position = centerTile.transform.position + Vector3.up;
    }

}
