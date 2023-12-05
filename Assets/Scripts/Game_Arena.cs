using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Game_Arena : Game
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
        StartCoroutine(SetupGame());
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
        //yield return Hextile.DrawLine(LengthOfCorridors, centerTile, startingDirection, corridor1); //corridor1
        yield return Hextile.DrawCircle(1, crypt1[0][0].Edge(startingDirection), startingDirection, crypt2);
        StartingChamber = crypt2[0][0];
        well = new GameObject().AddComponent<Landmark_Well>();
        well.AssignToTile(StartingChamber);
        //yield return Hextile.DrawLine(LengthOfCorridors, centerTile, offsetBy3, corridor2); //corridor2
        //yield return Hextile.DrawCircle(RadiusOfCrypts, corridor1.Last(x => x), randomDirection, crypt2); //crypt2
        //yield return Hextile.DrawCircle(RadiusOfCrypts, corridor2.Last(x => x), offsetBy3, crypt3); //crypt3
        //Hextile.HexPosition connectCrypt2and3 = Hextile.VectorToHexPosition(crypt2[0][0].transform.position - crypt3[0][0].transform.position);
        //yield return Hextile.DrawLine(LengthOfCorridors, crypt3[0][0], connectCrypt2and3, corridor3); //corridor3

        //crypt chambers
        List<Hextile> cryptChamberKeys = new List<Hextile>();
        chamberCandidates.AddRange(crypt1[0][0].AdjacentTiles.Where(x => x.Value != startingDirection && x.Value != offsetBy3).ToList());
        //chamberCandidates.AddRange(crypt2[0][0].AdjacentTiles.Where(x => x.Value != Hextile.RotateHexPosition(randomDirection, 3) && x.Value != Hextile.RotateHexPosition(connectCrypt2and3, 3)).ToList());
        //chamberCandidates.AddRange(crypt3[0][0].AdjacentTiles.Where(x => x.Value != connectCrypt2and3 && x.Value != Hextile.RotateHexPosition(offsetBy3, 3)).ToList());
        while (chambers.Count < CountOfChambers && chamberCandidates.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, chamberCandidates.Count);
            Hextile startingTile = chamberCandidates[randomIndex].Key;
            Hextile.HexPosition direction = chamberCandidates[randomIndex].Value;
            Hextile doorTile = startingTile.Edge(direction);
            //yield return Hextile.DrawCircle(1, startingTile, direction);
            Hextile newChamber = startingTile.Edge(direction).Extend(direction);
            //Landmark_Gate newGate = new GameObject().AddComponent<Landmark_Gate>();
            //newGate.AssignToTile(doorTile);
            //newGate.SetPositionOnTile(direction);
            chambers.Add(newChamber);
            chamberCandidates.RemoveAt(randomIndex);
            //set up new quest class
            //newGate.gameObject.AddComponent<Quest_Gate>();
        }

        ////corridor coves
        //List<Hextile> corridorCoveKeys = new List<Hextile>();
        //corridorCoveKeys.AddRange(corridor1.Where(x => x.AdjacentTiles.Count(y=>y.Key.AdjacentTiles.Count > 2) == 0).ToList());
        ////corridorCoveKeys.Add(corridor2.First(x => x.AdjacentTiles.Count == 2));
        ////corridorCoveKeys.Add(corridor3.First(x => x.AdjacentTiles.Count == 2));
        //foreach (Hextile tile in corridorCoveKeys)
        //{
        //    for (int i = 1; i <= 6; i++)
        //    {
        //        Hextile.HexPosition position = (Hextile.HexPosition)i;
        //        if (!tile.AdjacentTiles.ContainsValue(position))
        //        {
        //            coveCandidates.Add( new KeyValuePair<Hextile, Hextile.HexPosition>(tile, position));
        //        }
        //    }
        //}
        //while (coves.Count < CountOfCoves && coveCandidates.Count > 0)
        //{
        //    int randomIndex = UnityEngine.Random.Range(0, coveCandidates.Count);
        //    Hextile startingTile = coveCandidates[randomIndex].Key;
        //    Hextile.HexPosition direction = coveCandidates[randomIndex].Value;
        //    Hextile newCove = startingTile.Edge(direction).Extend(direction);
        //    if (!coves.Contains(newCove))
        //    {
        //        coves.Add(newCove);
        //    }
        //    coveCandidates.RemoveAt(randomIndex);
        //}

        AllTilesInPlay.AddRange(crypt1.Aggregate(new List<Hextile>(), (x, result) => result.Concat(x).ToList()));
        //AllTilesInPlay.AddRange(crypt2.Aggregate(new List<Hextile>(), (x, result) => result.Concat(x).ToList()));
        //AllTilesInPlay.AddRange(crypt3.Aggregate(new List<Hextile>(), (x, result) => result.Concat(x).ToList()));
        //AllTilesInPlay.AddRange(corridor1);
        //AllTilesInPlay.AddRange(corridor2);
        //AllTilesInPlay.AddRange(corridor3);
        AllTilesInPlay.AddRange(chambers);

        //yield return corridorLandmarks(corridor1);
        //yield return corridorLandmarks(corridor2);
        //yield return corridorLandmarks(corridor3);

        yield return cryptLandmarks(crypt1);
        //yield return cryptLandmarks(crypt2);
        //yield return cryptLandmarks(crypt3);

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
        foreach(Hextile tile in tiles)
        {
            new GameObject().AddComponent<Landmark_Barrier>().AssignToTile(tile);
        }
        yield break;
    }

    private IEnumerator cryptLandmarks(List<List<Hextile>>rings)
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
        MobSpawner.FinishedPeriodicSpawning.AddListener((mobs) => spawnedMobs = mobs );
        EliteSpawner.FinishedPeriodicSpawning.AddListener((elites) => spawnedElites = elites );
        StateOfGame = GameState.Liminal;
        yield return new WaitUntil(() => well.Used);
        alter.DesiredOffering = Player.INSTANCE.HostEntity.gameObject;
        while (true)
        {
            yield return null;
            alter.DesiredOffering = Player.INSTANCE.HostEntity.gameObject;
            Difficulty++;
            StateOfGame = GameState.Wave;
            StartingGate.OpenDoor();
            well.Volume = 100;
            yield return new WaitUntil(() => alter.Used && alter.Energized);
            alter.DesiredOffering = alter.TopStep;
            StartingGate.CloseDoor();
            spawnedMobs = spawnMobs();
            yield return new WaitUntil(() => spawnedMobs.Count(x=> x != null) == 0);

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


    private Dictionary<Type, int> EntityStrengths = new Dictionary<Type, int>()
    {
        { typeof(Skelly), 100 },
        { typeof(Nephalim), 200 },
        { typeof(Skully), 25 },
    };
    private Dictionary<Type, int> AIDifficulties = new Dictionary<Type, int>()
    {
        { typeof(Goon), 1 },
        { typeof(Bully), 3 },
        { typeof(Biter), 3 },
    };
    private List<(Type, Type)> EntityAIPairings = new List<(Type, Type)>
    {
        {(typeof(Goon), typeof(Skelly))},
        {(typeof(Bully), typeof(Nephalim))},
        {(typeof(Biter), typeof(Skully))},
    };

    private List<Type> UnlockedEntities = new List<Type>();
    private List<Type> UnlockedAIs = new List<Type>();

    private List<GameObject> spawnMobs()
    {
        //set up containers for this wave
        List<GameObject> mobs = new List<GameObject>();
        List<Type> chosenTypes = new List<Type>();
        List<Type> viableTypes = new List<Type>();

        //establish viable mob types
        viableTypes = AIDifficulties.Where(x => x.Value <= Difficulty).Select(x => x.Key).ToList();

        //pick mob types based on what is available




        //sort by max HP

        //spawn 1 of highest

        //spawn other types in order of highest to lowest HP to match highest HP enemy

        //loop back through until at population


        return mobs;
    }


}
