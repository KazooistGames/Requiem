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
    public Landmark_Credits Credits;
    public Shade QuestGiver;

    public float TimeGateTimeLeft = 0;

    public int Ritual = 0;

    private Idol idol;

    protected override void Start()
    {
        base.Start();
        INSTANCE = this;
        StartCoroutine(SetupGame());
    }

    protected override void Update()
    {
        base.Update();
        determineAlterOffering();
    }

    /***** PUBLIC *****/

    /***** PROTECTED *****/
    protected IEnumerator SetupGame()
    {
        yield return null;
        Goon.Alternative_Weapon = null;

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
        Credits = new GameObject().AddComponent<Landmark_Credits>();
        Credits.AssignToTile(Chambers[0]);
        yield return null;
        Credits.SetPositionOnTile(firstGateDirection);

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

    protected IEnumerator gameLoop()
    {
        yield return new WaitUntil(() => Commissioned);
        Torch.Toggle(true);
        Player.INSTANCE.HostEntity.transform.position = RAND_POS_IN_TILE(Chambers[0]);
        Player.INSTANCE.HostEntity.Vitality = 1;

        blurbIndicator = _BlurbService.createBlurb(Alter.TopStep, "Test", Color.red, sizeScalar: 3);
        blurbIndicator.SetActive(false);
        blurbIndicator.GetComponent<Text>().text = "0:00";
        StateOfGame = GameState.Lobby;
        yield return new WaitUntil(() => BloodWell.Volume == 0);
        while (Player.INSTANCE.HostEntity)
        {
            StateOfGame = GameState.Liminal;
            Ritual++;
            if(Ritual == 10)
            {
                foreach (Torch torch in FindObjectsOfType<Torch>())
                {
                    torch.Lit = false;
                }
            }
            Gates[0].OpenDoor();
            yield return new WaitUntil(() => !Alter.Energized);
            yield return new WaitUntil(() => Alter.Used && Alter.Energized);
            if (!idol && Ritual >= 3)
            {
                idol = spawnIdol();
            }
            Gates[0].CloseDoor();
            if (Alter.DesiredOffering == Player.INSTANCE.HostEntity.gameObject)
            {
                StateOfGame = GameState.Wave;
                yield return waveRoutine();
            }
            else if(idol ? Alter.DesiredOffering == idol.gameObject : false)
            {
                StateOfGame = GameState.Boss;
                yield return bossRoutine();
            }
        }
    }

    protected IEnumerator waveRoutine()
    {
        List<GameObject> spawnedMobs = new List<GameObject>();
        TotalStrengthOfWave = RitualStrengths[Ritual % 3];
        TimeGateTimeLeft = RitualTimes[Ritual % 3];
        spawnedMobs = spawnMobs(TotalStrengthOfWave);
        blurbIndicator.SetActive(true);
        while (StateOfGame == GameState.Wave && TimeGateTimeLeft > 0)
        {
            if (!Paused)
            {
                if (idol ? Alter.OfferingBox.bounds.Contains(idol.transform.position) : false)
                {
                    Alter.Consume(idol.gameObject);
                }
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
        blurbIndicator.SetActive(false);
        while (spawnedMobs.Count(x => x != null) != 0)
        {
            foreach (GameObject mob in spawnedMobs)
            {
                if (mob)
                {
                    mob.GetComponent<Entity>().Vitality = 0;
                }
                yield return null;
            }
            yield return null;
        }
        if(Ritual % 3 == 0)
        {
            StateOfGame = GameState.Boss;
            GameObject Boss = null;
            switch (Ritual)
            {
                case 3:
                    Boss = SPAWN(typeof(Imp), typeof(Sentinel), CenterTile.transform.position);
                    Goon.Alternative_Weapon = typeof(Spear);
                    break;
                case 6:
                    Boss = SPAWN(typeof(Nephalim), typeof(Bully), CenterTile.transform.position);
                    Goon.Alternative_Weapon = typeof(Greataxe);
                    break;
                case 9:
                    Boss = SPAWN(typeof(Wraith), typeof(Revanent), CenterTile.transform.position);
                    Goon.Alternative_Weapon = typeof(Greatsword);
                    break;
                default:
                    break;
            }
            yield return new WaitWhile(() => Boss != null);
        }
        yield return null;
        Scoreboard.Wave_Completed_Rewards();
        BloodWell.UnGulp();

    }

    protected IEnumerator bossRoutine()
    {
        List<GameObject> spawnedSkulls = new List<GameObject>();
        idol.BecomeMob();
        while (idol.mobEntity)
        {
            yield return null;
        }
    }


    /***** PRIVATE *****/
    private void determineAlterOffering()
    {
        if (!Alter || !Player.INSTANCE)
        {

        }
        else if (StateOfGame == GameState.Wave || StateOfGame == GameState.Boss)
        {
            Alter.DesiredOffering = Alter.TopStep;
        }
        else if(Ritual % 10 == 0 && Ritual > 0)
        {
            Alter.DesiredOffering = idol.gameObject;
            Alter.PentagramLineColor = new Color(1, 0, 0.75f);
            Alter.PentagramFlameStyle = _Flames.FlameStyles.Inferno;
            idol.flames.FlamePresentationStyle = _Flames.FlameStyles.Inferno;
            idol.flames.emissionModule.enabled = true;
        }
        else if(Player.INSTANCE.HostEntity)
        {
            Alter.DesiredOffering = Player.INSTANCE.HostEntity.gameObject;
            Alter.PentagramLineColor = Color.red;
            Alter.PentagramFlameStyle = _Flames.FlameStyles.Soulless;
        }
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
                            new GameObject().AddComponent<Landmark_Barrier>().AssignToTile(tile);
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
                        else
                        {
                            Landmark_Barrier newBarrier = new GameObject().AddComponent<Landmark_Barrier>();
                            newBarrier.OuterBarriersOnWallsOnly = true;
                            newBarrier.AssignToTile(tile);
                        }
                        yield return new WaitForFixedUpdate();
                        yield return null;
                    }
                    break;
            }
        }
    }

    private GameObject blurbIndicator;

    public Dictionary<Type, int> EntityStrengths = new Dictionary<Type, int>()
    {
        { typeof(Skelly), 100 },
        { typeof(Nephalim), 200 },
        { typeof(Skully), 25 },
        { typeof(Wraith), 250 },
        { typeof(Imp), 125 },
    };
    public Dictionary<Type, int> AIDifficulties = new Dictionary<Type, int>()
    {
        { typeof(Goon), 1 },
        //{ typeof(Bully), 3 },
        //{ typeof(Sentinel), 6 },
        //{ typeof(Revanent), 9 },
    };
    public Dictionary<Type, Type> AIEntityPairings = new Dictionary<Type, Type>()
    {
        {typeof(Goon),typeof(Skelly) },
        {typeof(Biter),typeof(Skully) },
        {typeof(Bully),typeof(Nephalim) },
        {typeof(Revanent),typeof(Wraith) },
        {typeof(Sentinel),typeof(Imp) },
    };
    public Dictionary<int, int> RitualStrengths = new Dictionary<int, int>()
    {
        {1, 600 },
        {2, 900 },
        {0, 1200 },
    };
    public Dictionary<int, int> RitualTimes = new Dictionary<int, int>()
    {
        {1, 100 },
        {2, 80 },
        {0, 60 },
    };
    public List<Type> UnlockedEntities = new List<Type>();
    public List<Type> UnlockedAIs = new List<Type>();

    public int TotalStrengthOfWave;
    public List<Type> chosenAIsForWave = new List<Type>();
    public List<GameObject> WaveMobs = new List<GameObject>();
    private List<GameObject> spawnMobs(int strengthToSpawn)
    {
        int numberOfUniqueTypesToSpawn = 1 + Mathf.FloorToInt(Mathf.Sqrt(Ritual));
        List<Type> viableAIs = AIDifficulties.Where(x => x.Value <= Ritual).Select(x => x.Key).ToList();
        chosenAIsForWave = new List<Type>();
        while(chosenAIsForWave.Count <= Mathf.Min(viableAIs.Count, numberOfUniqueTypesToSpawn))
        {
            int randomIndexFromViableAIs = UnityEngine.Random.Range(0, viableAIs.Count);
            chosenAIsForWave.Add(viableAIs[randomIndexFromViableAIs]);
            viableAIs.RemoveAt(randomIndexFromViableAIs);
        }
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

    private Idol spawnIdol()
    {
        Idol newIdol = Instantiate(Resources.Load<GameObject>("Prefabs/Wieldable/Idol")).GetComponent<Idol>();
        List<Hextile> spawnCandidates = ArenaTiles[2].Where(x => x.Landmarks.FirstOrDefault(x => x.GetComponent<Landmark_Barrier>())).ToList();
        Hextile spawnTile = spawnCandidates[UnityEngine.Random.Range(0, spawnCandidates.Count)];
        newIdol.transform.position = RAND_POS_IN_TILE(spawnTile);
        return newIdol;
    }

}
