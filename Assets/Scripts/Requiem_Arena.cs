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

    public List<List<Hextile>> ArenaTiles = new List<List<Hextile>>();
    public List<Landmark_Gate> Gates;
    public List<Hextile> Chambers = new List<Hextile>();

    public Hextile CenterTile;
    public Landmark_Alter Alter;
    public Landmark_Well BloodWell;
    public Shade QuestGiver;

    public float TimeGateTimeLeft = 0;

    public int Ritual = 0;

    private float WaveKillMultiplierBonus = 0.25f;

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
        yield return Hextile.DrawCircle(RadiusOfArena, Hextile.LastGeneratedTile, Tiles: ArenaTiles);

        Hextile.HexPosition firstGateDirection = (Hextile.HexPosition)1;
        Hextile edgeOne = ArenaTiles[0][0].Edge(firstGateDirection);
        Gates.Add(new GameObject().AddComponent<Landmark_Gate>());
        Gates[0].AssignToTile(edgeOne);
        Gates[0].SetPositionOnTile(firstGateDirection);
        Chambers.Add(edgeOne.Extend(firstGateDirection));
        BloodWell = new GameObject().AddComponent<Landmark_Well>();
        BloodWell.AssignToTile(Chambers[0]);

        Hextile.HexPosition secondGateDirection = Hextile.RotateHexPosition(firstGateDirection, 2);
        Hextile edgeTwo = ArenaTiles[0][0].Edge(secondGateDirection);
        Gates.Add(new GameObject().AddComponent<Landmark_Gate>());
        Gates[1].AssignToTile(edgeTwo);
        Gates[1].SetPositionOnTile(secondGateDirection);  
        Chambers.Add(edgeTwo.Extend(secondGateDirection));

        Hextile.HexPosition thirdGateDirection = Hextile.RotateHexPosition(secondGateDirection, 2);
        Hextile edgeThree = ArenaTiles[0][0].Edge(thirdGateDirection);
        Gates.Add(new GameObject().AddComponent<Landmark_Gate>());
        Gates[2].AssignToTile(edgeThree);
        Gates[2].SetPositionOnTile(thirdGateDirection);
        Chambers.Add(edgeThree.Extend(thirdGateDirection));
        //yield return Hextile.DrawCircle(RadiusOfCrypt, ArenaTiles[0][0].Edge(startingDirection), startingDirection, CryptTiles);

        AllTilesInPlay.AddRange(ArenaTiles.Aggregate(new List<Hextile>(), (x, result) => result.Concat(x).ToList()));
        //AllTilesInPlay.AddRange(CryptTiles.Aggregate(new List<Hextile>(), (x, result) => result.Concat(x).ToList()));
        AllTilesInPlay.AddRange(Chambers);

        yield return buildArenaLandmarks(ArenaTiles);
        //yield return buildCryptLandmarks(CryptTiles);

        yield return new WaitForSeconds(0.5f);

        new GameObject().AddComponent<Player>();
        Hextile randomTile = ArenaTiles[ArenaTiles.Count - 1][UnityEngine.Random.Range(0, ArenaTiles[ArenaTiles.Count - 1].Count)];
        randomTile = ArenaTiles[ArenaTiles.Count - 1][UnityEngine.Random.Range(0, ArenaTiles[ArenaTiles.Count - 1].Count)];
        SPAWN(typeof(Shade), typeof(Janitor), RAND_POS_IN_TILE(randomTile));
        Commissioned = true;

        yield return gameLoop();
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
                            //new GameObject().AddComponent<Landmark_Barrier>().AssignToTile(tile.Extend(position));
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
        Player.INSTANCE.HostEntity.transform.position = RAND_POS_IN_TILE(Chambers[0]);
        Player.INSTANCE.HostEntity.Vitality = 1;
        List<GameObject> spawnedMobs = new List<GameObject>();
        List<GameObject> spawnedElites = new List<GameObject>();
        blurbIndicator = _BlurbService.createBlurb(Requiem_Arena.INSTANCE.Alter.TopStep, "Test", Color.red, sizeScalar: 3);
        blurbIndicator.SetActive(false);
        blurbIndicator.GetComponent<Text>().text = "0:00";
        //MobSpawner.FinishedPeriodicSpawning.AddListener((mobs) => spawnedMobs = mobs);
        //EliteSpawner.FinishedPeriodicSpawning.AddListener((elites) => spawnedElites = elites);
        StateOfGame = GameState.Lobby;
        yield return new WaitUntil(() => BloodWell.Volume == 0);
        while (true)
        {
            StateOfGame = GameState.Liminal;
            Alter.DesiredOffering = Player.INSTANCE.HostEntity.gameObject;
            Ritual++;
            Gates[0].OpenDoor();
            yield return new WaitUntil(() => !Alter.Energized);
            yield return new WaitUntil(() => Alter.Used && Alter.Energized);
            StateOfGame = GameState.Wave;
            Alter.DesiredOffering = Alter.TopStep;
            Gates[0].CloseDoor();
            TotalStrengthOfWave = RitualStrengths[Ritual % 3];
            TimeGateTimeLeft = RitualTimes[Ritual % 3];
            spawnedMobs = spawnMobs(TotalStrengthOfWave);
            blurbIndicator.SetActive(true);
            while (Requiem.INSTANCE.StateOfGame == GameState.Wave && TimeGateTimeLeft > 0)
            {
                if (!Paused)
                {
                    int minutesLeft = (int)TimeGateTimeLeft / 60;
                    int secondsLeft = (int)TimeGateTimeLeft % 60;
                    string timeLeftText = minutesLeft.ToString("0") + ":" + secondsLeft.ToString("00");
                    blurbIndicator.GetComponent<Text>().text = timeLeftText;
                    yield return new WaitForSeconds(1f);
                    spawnedMobs.RemoveAll(x => x == null);
                    int minStrength = TotalStrengthOfWave / 2;
                    if (spawnedMobs.Aggregate(0f, (result, x) => result += x.GetComponent<Entity>().Strength) < minStrength)
                    {
                        int respawnStrength = TotalStrengthOfWave / 2;
                        spawnedMobs.AddRange(spawnMobs(respawnStrength));
                    }
                    TimeGateTimeLeft -= 1;
                }
            }
            while(spawnedMobs.Count(x => x != null) != 0)
            {
                foreach (GameObject mob in spawnedMobs)
                {
                    if (mob)
                    {
                        mob.GetComponent<Entity>().Vitality = 0;
                    }
                }
                yield return null;
            }
            Scoreboard.KillMultiplier += WaveKillMultiplierBonus;
            BloodWell.Volume = 100;
            blurbIndicator.SetActive(false);
        }
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
        { typeof(Bully), 2 },
        //{ typeof(Biter), 2 },
        { typeof(Revanent), 7 },
    };
    public Dictionary<Type, Type> AIEntityPairings = new Dictionary<Type, Type>()
    {
        {typeof(Goon),typeof(Skelly) },
        {typeof(Biter),typeof(Skully) },
        {typeof(Bully),typeof(Nephalim) },
        {typeof(Revanent),typeof(Wraith) },
    };
    public Dictionary<int, int> RitualStrengths = new Dictionary<int, int>()
    {
        {1, 600 },
        {2, 800 },
        {0, 1000 },
    };
    public Dictionary<int, int> RitualTimes = new Dictionary<int, int>()
    {
        {1, 80 },
        {2, 90 },
        {0, 100 },
    };
    public List<Type> UnlockedEntities = new List<Type>();
    public List<Type> UnlockedAIs = new List<Type>();

    public int TotalStrengthOfWave;
    public List<Type> chosenAIsForWave = new List<Type>();
    public List<GameObject> WaveMobs = new List<GameObject>();
    private List<GameObject> spawnMobs(int strengthToSpawn)
    {

        List<Type> viableAIs = new List<Type>();
        viableAIs = AIDifficulties.Where(x => x.Value <= Ritual).Select(x => x.Key).ToList();

        chosenAIsForWave = new List<Type>();
        while(chosenAIsForWave.Count < Mathf.Min(viableAIs.Count, Mathf.FloorToInt(Mathf.Sqrt(Ritual))))
        {
            int randomIndexFromViableAIs = UnityEngine.Random.Range(0, viableAIs.Count);
            chosenAIsForWave.Add(viableAIs[randomIndexFromViableAIs]);
            viableAIs.RemoveAt(randomIndexFromViableAIs);
        }

        //sort by max HP
        chosenAIsForWave = chosenAIsForWave.OrderByDescending(x => EntityStrengths[AIEntityPairings[x]]).ToList();

        WaveMobs = new List<GameObject>();
        int spawnIndex = 0;
        Hextile playerTile = Player.INSTANCE.HostEntity.TileLocation;
        List<Hextile> viableSpawnTiles = ArenaTiles.Aggregate(new List<Hextile>(), (ring, result) => result.Concat(ring.Where(tile => tile != playerTile && !tile.AdjacentTiles.ContainsKey(playerTile)).ToList()).ToList());
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
