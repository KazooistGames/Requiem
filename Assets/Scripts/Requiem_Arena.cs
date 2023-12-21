using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;

public class Requiem_Arena : Requiem
{
    public static new Requiem_Arena INSTANCE;
    public static int RadiusOfArena = 2;
    public static int RadiusOfCrypt = 1;
    public static int LengthOfCorridor = 1;
    //public int CountOfChambers;
    //public int CountOfCoves;

    public Hextile StartingTile;
    public Landmark_Gate StartingGate;
    public List<List<Hextile>> ArenaTiles = new List<List<Hextile>>();
    private List<List<Hextile>> CryptTiles = new List<List<Hextile>>();
    public List<Hextile> CorridorTiles = new List<Hextile>();
    public List<Hextile> chambers = new List<Hextile>();
    public List<Hextile> coves = new List<Hextile>();
    //private List<KeyValuePair<Hextile, Hextile.HexPosition>> chamberCandidates = new List<KeyValuePair<Hextile, Hextile.HexPosition>>();
    //private List<KeyValuePair<Hextile, Hextile.HexPosition>> coveCandidates = new List<KeyValuePair<Hextile, Hextile.HexPosition>>();

    public Hextile CenterTile;
    public Landmark_Alter Alter;
    public Landmark_Well BloodWell;

    private Spawner PatrolSpawner;
    private Spawner MobSpawner;
    private Spawner EliteSpawner;
    private Spawner BossSpawner;

    public float SpeedTimeGateTimeLeft = 0;

    public int Wave = 0;

    protected override void Start()
    {
        base.Start();
        INSTANCE = this;
        StartCoroutine(SetupGame());
    }

    protected override void Update()
    {
        base.Update();

    }

    protected IEnumerator SetupGame()
    {
        yield return null;

        CenterTile = Hextile.GenerateRootTile();
        yield return null;

        Hextile.HexPosition startingDirection = (Hextile.HexPosition)1;
        Hextile.HexPosition offsetBy3 = Hextile.RotateHexPosition(startingDirection, 3);
        yield return Hextile.DrawCircle(RadiusOfArena, Hextile.LastGeneratedTile, Tiles: ArenaTiles); //crypt1
        StartingGate = new GameObject().AddComponent<Landmark_Gate>();
        StartingGate.AssignToTile(ArenaTiles[0][0].Edge(startingDirection));
        StartingGate.SetPositionOnTile(startingDirection);
        yield return Hextile.DrawCircle(RadiusOfCrypt, ArenaTiles[0][0].Edge(startingDirection), startingDirection, CryptTiles);

        AllTilesInPlay.AddRange(ArenaTiles.Aggregate(new List<Hextile>(), (x, result) => result.Concat(x).ToList()));
        AllTilesInPlay.AddRange(CryptTiles.Aggregate(new List<Hextile>(), (x, result) => result.Concat(x).ToList()));
        AllTilesInPlay.AddRange(chambers);

        yield return buildArenaLandmarks(ArenaTiles);
        yield return buildCryptLandmarks(CryptTiles);

        yield return new WaitForSeconds(0.5f);

        new GameObject().AddComponent<Player>();
        Hextile randomTile = ArenaTiles[ArenaTiles.Count - 1][UnityEngine.Random.Range(0, ArenaTiles[ArenaTiles.Count - 1].Count)];
        randomTile = ArenaTiles[ArenaTiles.Count - 1][UnityEngine.Random.Range(0, ArenaTiles[ArenaTiles.Count - 1].Count)];
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

    private IEnumerator buildCryptLandmarks(List<List<Hextile>> cryptTileRings)
    {
        StartingTile = cryptTileRings[0][0];
        BloodWell = new GameObject().AddComponent<Landmark_Well>();
        BloodWell.AssignToTile(StartingTile);
        yield break;
    }

    private IEnumerator buildArenaLandmarks(List<List<Hextile>> arenaTileRings)
    {
        foreach (List<Hextile> ring in arenaTileRings)
        {
            int ringNum = arenaTileRings.IndexOf(ring);
            switch (ringNum)
            {
                case 0:
                    Alter = new GameObject().AddComponent<Landmark_Alter>();
                    Alter.AssignToTile(ring[0]);
                    break;
                case 1:
                    foreach (Hextile tile in ring)
                    {
                        Hextile.HexPosition position = arenaTileRings[0][0].AdjacentTiles.First(x => x.Key == tile).Value;
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

    private GameObject blurbIndicator;
    protected IEnumerator gameLoop()
    {
        yield return new WaitUntil(() => Commissioned);
        Torch.Toggle(true);
        Player.INSTANCE.HostEntity.transform.position = RAND_POS_IN_TILE(StartingTile);
        Player.INSTANCE.HostEntity.Vitality = 1;
        List<GameObject> spawnedMobs = new List<GameObject>();
        List<GameObject> spawnedElites = new List<GameObject>();
        blurbIndicator = _BlurbService.createBlurb(Requiem_Arena.INSTANCE.Alter.TopStep, "Test", Color.red, sizeScalar: 3);
        blurbIndicator.SetActive(false);
        blurbIndicator.GetComponent<Text>().text = "0:00";
        //MobSpawner.FinishedPeriodicSpawning.AddListener((mobs) => spawnedMobs = mobs);
        //EliteSpawner.FinishedPeriodicSpawning.AddListener((elites) => spawnedElites = elites);
        StateOfGame = GameState.Lobby;
        yield return new WaitUntil(() => BloodWell.Used);
        while (true)
        {
            StateOfGame = GameState.Liminal;
            Alter.DesiredOffering = Player.INSTANCE.HostEntity.gameObject;
            Wave++;
            StartingGate.OpenDoor();
            yield return null;
            yield return new WaitUntil(() => Alter.Used && Alter.Energized);
            StateOfGame = GameState.Wave;
            Alter.DesiredOffering = Alter.TopStep;
            StartingGate.CloseDoor();
            TotalStrengthOfWave = WaveStrengths[Wave % 3];
            SpeedTimeGateTimeLeft = TotalStrengthOfWave / 5;
            spawnedMobs = spawnMobs(TotalStrengthOfWave);
            blurbIndicator.SetActive(true);
            while (Requiem.INSTANCE.StateOfGame == GameState.Wave && SpeedTimeGateTimeLeft > 0)
            {
                int minutesLeft = (int)SpeedTimeGateTimeLeft / 60;
                int secondsLeft = (int)SpeedTimeGateTimeLeft % 60;
                string timeLeftText = minutesLeft.ToString() + ":" + secondsLeft.ToString();
                blurbIndicator.GetComponent<Text>().text = timeLeftText;
                yield return new WaitForSeconds(1f);
                spawnedMobs.RemoveAll(x => x == null);
                int minStrength = TotalStrengthOfWave / 2;
                if (spawnedMobs.Aggregate(0f, (result, x) => result += x.GetComponent<Entity>().Strength) < minStrength)
                {
                    int respawnStrength = TotalStrengthOfWave / 2;
                    spawnedMobs.AddRange(spawnMobs(respawnStrength));
                }
                SpeedTimeGateTimeLeft -= 1;
            }
            foreach(GameObject mob in spawnedMobs)
            {
                if (mob)
                {
                    mob.GetComponent<Entity>().Vitality = 0;
                }
            }
            yield return new WaitUntil(() => spawnedMobs.Count(x => x != null) == 0);
            BloodWell.Volume = 100;
            blurbIndicator.SetActive(false);
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
        { typeof(Wraith), 150 },
    };
    public Dictionary<Type, int> AIDifficulties = new Dictionary<Type, int>()
    {
        { typeof(Goon), 1 },
        { typeof(Biter), 2 },
        { typeof(Bully), 4 },
        { typeof(Revanent), 7 },
    };
    public Dictionary<Type, Type> AIEntityPairings = new Dictionary<Type, Type>()
    {
        {typeof(Goon),typeof(Skelly) },
        {typeof(Biter),typeof(Skully) },
        {typeof(Bully),typeof(Nephalim) },
        {typeof(Revanent),typeof(Wraith) },
    };
    public Dictionary<int, int> WaveStrengths = new Dictionary<int, int>()
    {
        {1, 500 },
        {2, 800 },
        {0, 1200 },
    };

    public List<Type> UnlockedEntities = new List<Type>();
    public List<Type> UnlockedAIs = new List<Type>();

    public int TotalStrengthOfWave;
    public List<Type> chosenAIsForWave = new List<Type>();
    public List<GameObject> WaveMobs = new List<GameObject>();
    private List<GameObject> spawnMobs(int strengthToSpawn)
    {

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

        WaveMobs = new List<GameObject>();
        int spawnIndex = 0;
        List<Hextile> viableSpawnTiles = ArenaTiles.Aggregate(new List<Hextile>(), (ring, result) => result.Concat(ring).ToList());
        while (strengthToSpawn > 0)
        {
            int strengthOfSpawnChunk = Mathf.Min(strengthToSpawn, EntityStrengths[AIEntityPairings[chosenAIsForWave[0]]]);
            while (strengthOfSpawnChunk > 0)
            {
                Type spawnedAI = chosenAIsForWave[spawnIndex];
                Type spawnedEntity = AIEntityPairings[spawnedAI];
                strengthToSpawn -= EntityStrengths[spawnedEntity];
                strengthOfSpawnChunk -= EntityStrengths[spawnedEntity];
                Hextile randomTile = viableSpawnTiles[UnityEngine.Random.Range(0, viableSpawnTiles.Count)];
                WaveMobs.Add(SPAWN(spawnedEntity, spawnedAI, RAND_POS_IN_TILE(randomTile)));
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
