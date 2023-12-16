using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Requiem_Arena : Requiem
{
    public int RadiusOfCrypts;
    public int LengthOfCorridors;
    public int CountOfChambers;
    public int CountOfCoves;

    public Hextile StartingChamber;
    public Landmark_Gate StartingGate;
    public List<List<Hextile>> crypt1 = new List<List<Hextile>>();
    private List<List<Hextile>> crypt2 = new List<List<Hextile>>();
    //private List<List<Hextile>> crypt3 = new List<List<Hextile>>();
    public List<Hextile> corridor1 = new List<Hextile>();
    //private List<Hextile> corridor2 = new List<Hextile>();
    //private List<Hextile> corridor3 = new List<Hextile>();
    public List<Hextile> chambers = new List<Hextile>();
    public List<Hextile> coves = new List<Hextile>();

    private List<KeyValuePair<Hextile, Hextile.HexPosition>> chamberCandidates = new List<KeyValuePair<Hextile, Hextile.HexPosition>>();
    private List<KeyValuePair<Hextile, Hextile.HexPosition>> coveCandidates = new List<KeyValuePair<Hextile, Hextile.HexPosition>>();

    private Hextile centerTile;
    private Landmark_Alter alter;
    private Landmark_Well well;
    private Idol idol;

    

    private Spawner PatrolSpawner;
    private Spawner MobSpawner;
    private Spawner EliteSpawner;
    private Spawner BossSpawner;

    public int Wave = 0;

        protected override void Start()
    {
        base.Start();
        StartCoroutine(SetupGame());
    }

    protected override void Update()
    {
        base.Update();
        if (StateOfGame != GameState.Liminal)
        {
            GameClock += Time.deltaTime;
        }
    }

    protected IEnumerator SetupGame()
    {
        yield return null;

        centerTile = Hextile.GenerateRootTile();
        yield return null;

        Hextile.HexPosition startingDirection = (Hextile.HexPosition)1;
        Hextile.HexPosition offsetBy3 = Hextile.RotateHexPosition(startingDirection, 3);
        yield return Hextile.DrawCircle(RadiusOfCrypts, Hextile.LastGeneratedTile, Tiles: crypt1); //crypt1
        StartingGate = new GameObject().AddComponent<Landmark_Gate>();
        StartingGate.AssignToTile(crypt1[0][0].Edge(startingDirection));
        StartingGate.SetPositionOnTile(startingDirection);
        yield return Hextile.DrawCircle(1, crypt1[0][0].Edge(startingDirection), startingDirection, crypt2);
        StartingChamber = crypt2[0][0];
        well = new GameObject().AddComponent<Landmark_Well>();
        well.AssignToTile(StartingChamber);

        List<Hextile> cryptChamberKeys = new List<Hextile>();
        chamberCandidates.AddRange(crypt1[0][0].AdjacentTiles.Where(x => x.Value != startingDirection && x.Value != offsetBy3).ToList());


        AllTilesInPlay.AddRange(crypt1.Aggregate(new List<Hextile>(), (x, result) => result.Concat(x).ToList()));
        AllTilesInPlay.AddRange(chambers);

        yield return cryptLandmarks(crypt1);

        yield return new WaitForSeconds(0.5f);
        new GameObject().AddComponent<Player>();
        idol = Instantiate(Resources.Load<GameObject>("Prefabs/Wieldable/Idol")).GetComponent<Idol>();
        Hextile randomTile = crypt1[crypt1.Count - 1][UnityEngine.Random.Range(0, crypt1[crypt1.Count - 1].Count)];
        idol.transform.position = RAND_POS_IN_TILE(randomTile);
        randomTile = crypt1[crypt1.Count - 1][UnityEngine.Random.Range(0, crypt1[crypt1.Count - 1].Count)];
        SPAWN(typeof(Shade), typeof(Janitor), RAND_POS_IN_TILE(randomTile));
        configureSpawners();
        Commissioned = true;

        yield return gameLoop();
    }

    private IEnumerator corridorLandmarks(List<Hextile> tiles)
    {
        foreach (Hextile tile in tiles)
        {
            new GameObject().AddComponent<Landmark_Barrier>().AssignToTile(tile);
        }
        yield break;
    }

    private IEnumerator cryptLandmarks(List<List<Hextile>> rings)
    {
        foreach (List<Hextile> ring in rings)
        {
            int ringNum = rings.IndexOf(ring);
            switch (ringNum)
            {
                case 0:
                    alter = new GameObject().AddComponent<Landmark_Alter>();
                    alter.AssignToTile(ring[0]);
                    break;
                case 1:
                    foreach (Hextile tile in ring)
                    {
                        Hextile.HexPosition position = rings[0][0].AdjacentTiles.First(x => x.Key == tile).Value;
                        if ((int)position % 2 == 0)
                        {
                            new GameObject().AddComponent<Landmark_Pillar>().AssignToTile(tile);
                            new GameObject().AddComponent<Landmark_Barrier>().AssignToTile(tile.Extend(position));
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
                        if (tile.Landmarks.Count == 0)
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

    protected IEnumerator gameLoop()
    {
        yield return new WaitUntil(() => Commissioned);
        Torch.Toggle(true);
        Player.INSTANCE.HostEntity.transform.position = RAND_POS_IN_TILE(StartingChamber);
        Player.INSTANCE.HostEntity.Vitality = 1;
        List<GameObject> spawnedMobs = new List<GameObject>();
        List<GameObject> spawnedElites = new List<GameObject>();
        MobSpawner.FinishedPeriodicSpawning.AddListener((mobs) => spawnedMobs = mobs);
        EliteSpawner.FinishedPeriodicSpawning.AddListener((elites) => spawnedElites = elites);
        StateOfGame = GameState.Lobby;
        yield return new WaitUntil(() => well.Used);
        while (true)
        {
            StateOfGame = GameState.Liminal;
            alter.DesiredOffering = Player.INSTANCE.HostEntity.gameObject;
            Wave++;
            StartingGate.OpenDoor();
            yield return null;
            yield return new WaitUntil(() => alter.Used && alter.Energized);
            StateOfGame = GameState.Wave;
            alter.DesiredOffering = alter.TopStep;
            StartingGate.CloseDoor();
            spawnedMobs = spawnMobs();
            yield return new WaitUntil(() => spawnedMobs.Count(x => x != null) == 0);
            well.Volume = 100;
        }
    }


    private void configureSpawners()
    {
        PatrolSpawner = gameObject.AddComponent<Spawner>();
        PatrolSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Skully));
        PatrolSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Biter));
        PatrolSpawner.transform.position = RandomPositionInRandomTileInPlay();
        PatrolSpawner.JustSpawned.AddListener(x => PatrolSpawner.transform.position = RandomPositionInRandomTileInPlay());

        MobSpawner = gameObject.AddComponent<Spawner>();
        MobSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Skelly));
        MobSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Goon));
        MobSpawner.JustSpawned.AddListener(x => PatrolSpawner.transform.position = RandomPositionInRandomTileInPlay());

        EliteSpawner = gameObject.AddComponent<Spawner>();
        EliteSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Nephalim));
        EliteSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Bully));
        EliteSpawner.JustSpawned.AddListener(x => PatrolSpawner.transform.position = RandomPositionInRandomTileInPlay());

        BossSpawner = gameObject.AddComponent<Spawner>();
        BossSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Wraith));
        BossSpawner.ListOfComponentsAddedToSpawnedObjects.Add(typeof(Revanent));
        BossSpawner.transform.position = RandomPositionInRandomTileInPlay();
    }


    public Dictionary<Type, int> EntityStrengths = new Dictionary<Type, int>()
    {
        { typeof(Skelly), 100 },
        { typeof(Nephalim), 200 },
        { typeof(Skully), 25 },
    };
    public Dictionary<Type, int> AIDifficulties = new Dictionary<Type, int>()
    {
        { typeof(Goon), 1 },
        { typeof(Biter), 2 },
        { typeof(Bully), 4 },
    };
    public Dictionary<Type, Type> AIEntityPairings = new Dictionary<Type, Type>()
    {
        {typeof(Goon),typeof(Skelly) },
        {typeof(Biter),typeof(Skully) },
        {typeof(Bully),typeof(Nephalim) }
    };
    public Dictionary<int, int> WaveStrengths = new Dictionary<int, int>()
    {
        {1, 500 },
        {2, 800 },
        {3, 1200 },
    };

    public List<Type> UnlockedEntities = new List<Type>();
    public List<Type> UnlockedAIs = new List<Type>();

    public int TotalStrengthOfWave;
    public List<Type> chosenAIsForWave = new List<Type>();
    public List<GameObject> WaveMobs = new List<GameObject>();
    private List<GameObject> spawnMobs()
    {      
        switch(Wave % 3)
        {
            case 1:
                TotalStrengthOfWave = 500;
                break;
            case 2:
                TotalStrengthOfWave = 800;
                break;
            case 0:
                TotalStrengthOfWave = 1200;
                break;
            default:
                TotalStrengthOfWave = 0;
                break;  
        }

        List<Type> viableAIs = new List<Type>();
        viableAIs = AIDifficulties.Where(x => x.Value <= Wave).Select(x => x.Key).ToList();

        chosenAIsForWave = new List<Type>();
        while(chosenAIsForWave.Count < Mathf.Min(viableAIs.Count, Mathf.FloorToInt(Mathf.Sqrt(Wave))))
        {
            int randomIndexFromViableAIs = UnityEngine.Random.Range(0, viableAIs.Count);
            chosenAIsForWave.Add(viableAIs[randomIndexFromViableAIs]);
            viableAIs.RemoveAt(randomIndexFromViableAIs);
        }

        //sort by max HP
        chosenAIsForWave = chosenAIsForWave.OrderByDescending(x => EntityStrengths[AIEntityPairings[x]]).ToList();

        //spawn 1 of highest
        //spawn other types in order of highest to lowest HP to match highest HP enemy
        //loop back through until at population
        int spawnIndex = 0;
        //int strengthSpawnedSoFar = 0;

        WaveMobs = new List<GameObject>();
        while (TotalStrengthOfWave > 0)
        {
            int strengthOfSpawnChunk = Mathf.Min(TotalStrengthOfWave, EntityStrengths[AIEntityPairings[chosenAIsForWave[0]]]);
            while (strengthOfSpawnChunk > 0)
            {
                Type spawnedAI = chosenAIsForWave[spawnIndex];
                Type spawnedEntity = AIEntityPairings[spawnedAI];
                TotalStrengthOfWave -= EntityStrengths[spawnedEntity];
                strengthOfSpawnChunk -= EntityStrengths[spawnedEntity];
                WaveMobs.Add(SPAWN(spawnedEntity, spawnedAI, alter.transform.position));
            }      
            spawnIndex++;
            if(spawnIndex >= chosenAIsForWave.Count)
            {
                spawnIndex = 0;
            }
        }

        return WaveMobs;
    }


}
