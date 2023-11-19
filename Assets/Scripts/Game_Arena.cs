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
    List<Hextile> AllTilesInPlay = new List<Hextile>();
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
    public GameState StateOfGame = GameState.Liminal;

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
        if(StateOfGame != GameState.Liminal)
        {
            GameClock += Time.deltaTime;
        }
    }

    protected IEnumerator PlayGame()
    {
        yield return null;
        centerTile = Hextile.GenerateRootTile();
        yield return null;
        yield return Hextile.DrawCircle(RadiusOfTiles, Hextile.LastGeneratedTile, Tiles: TileRings);
        yield return new WaitForSecondsRealtime(0.5f);
        AllTilesInPlay = TileRings.Aggregate(new List<Hextile>(), (x, result) => result.Concat(x).ToList());
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
                    Hextile wellTile = ring[UnityEngine.Random.Range(0, ring.Count)];
                    well.AssignToTile(wellTile);
                    new GameObject().AddComponent<Landmark_Barrier>().AssignToTile(ring.Where(x=>x != wellTile).ToList()[UnityEngine.Random.Range(0, 5)]);
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
        List<Torch> allTorches = new List<Torch>();

        allTorches = FindObjectsOfType<Torch>().ToList();        
        foreach (Torch torch in allTorches)
        {
            torch.Lit = false;
        }

        yield return new WaitUntil(() => Commissioned);
        Player.INSTANCE.HostEntity.transform.position = RAND_POS_IN_TILE(AllTilesInPlay[0]);
        List<GameObject> spawnedMobs = new List<GameObject>();
        List<GameObject> spawnedElites = new List<GameObject>();
        MobSpawner.FinishedPeriodicSpawning.AddListener((mobs) => spawnedMobs = mobs );
        EliteSpawner.FinishedPeriodicSpawning.AddListener((elites) => spawnedElites = elites );
        StateOfGame = GameState.Liminal;
        yield return new WaitUntil(() => alter.Used);
        idol.BecomeMob();
        alter.DesiredOffering = alter.TopStep;
        StateOfGame = GameState.Wave;
        while (true)
        {
            //start periodically spawning enemies and every minute re-evaluate
            Difficulty = (int)(GameClock % 60);
            MobSpawner.PeriodicallySpawn(Mathf.Max(0, 10 - Difficulty), 2, 1 + Difficulty, 1 + Difficulty * 2);
            EliteSpawner.PeriodicallySpawn(Mathf.Max(0, 30 - Difficulty * 2), 1, 0, Mathf.CeilToInt(Mathf.Sqrt(1 + Difficulty)));
            PatrolSpawner.PeriodicallySpawn(Mathf.Max(0, 60 - Difficulty * 3), 3, 0, 1 + Difficulty * 3);
            yield return new WaitForSeconds(60);
        }
    }
  

    private void configureSpawners()
    {
        PatrolSpawner = gameObject.AddComponent<Spawner>();
        PatrolSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Skully));
        PatrolSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Biter));
        PatrolSpawner.transform.position = getRandomPositionInRandomTileInPlay();
        PatrolSpawner.JustSpawned.AddListener(x => PatrolSpawner.transform.position = getRandomPositionInRandomTileInPlay());

        MobSpawner = gameObject.AddComponent<Spawner>();
        MobSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Skelly));
        MobSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Goon));
        MobSpawner.JustSpawned.AddListener(x => PatrolSpawner.transform.position = getRandomPositionInRandomTileInPlay());

        EliteSpawner = gameObject.AddComponent<Spawner>();
        EliteSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Nephalim));
        EliteSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Bully));
        EliteSpawner.JustSpawned.AddListener(x => PatrolSpawner.transform.position = getRandomPositionInRandomTileInPlay());

        BossSpawner = gameObject.AddComponent<Spawner>();
        BossSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Wraith));
        BossSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Revanent));
        BossSpawner.transform.position = getRandomPositionInRandomTileInPlay();
    }

    private Vector3 getRandomPositionInRandomTileInPlay()
    {
        Hextile randomTile = AllTilesInPlay[UnityEngine.Random.Range(0, AllTilesInPlay.Count)];
        return RAND_POS_IN_TILE(randomTile);
    }
}
