using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Game_Arena : Game
{
    int RadiusOfTiles = 2;
    private List<List<Hextile>> TileRings = new List<List<Hextile>>();
    List<Hextile> AllTiles = new List<Hextile>();
    private Hextile centerTile;
    private Landmark_Well well;

    public enum GameState
    {
        Liminal,
        Arena,
        Crypt,
        Finale,
    }
    public GameState State = GameState.Liminal;

    private Spawner MobSpawner;
    private Spawner EliteSpawner;

    public int Difficulty = 0;
    public Stage CryptCurrentlyActive;
    public List<Stage> Crypts;
    public List<Landmark_Gate> CryptGates;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(gameInit());
    }

    protected override void Update()
    {
        base.Update();
        gameObject.name = "ARENA";
    }

    protected IEnumerator gameInit()
    {
        yield return null;
        centerTile = Hextile.GenerateRootTile();
        yield return null;
        yield return Hextile.DrawCircle(RadiusOfTiles, Hextile.LastGeneratedTile, Tiles: TileRings);
        yield return new WaitForSecondsRealtime(0.5f);
        AllTiles = TileRings.Aggregate(new List<Hextile>(), (x, result) => result.Concat(x).ToList());
        yield return gameInitLandmarks();
        yield return gameInitCrypts();
        new GameObject().AddComponent<Player>();
        Idol Idol = Instantiate(Resources.Load<GameObject>("Prefabs/Wieldable/Idol")).GetComponent<Idol>();
        Hextile randomTile = TileRings[TileRings.Count - 1][UnityEngine.Random.Range(0, TileRings[TileRings.Count - 1].Count)];
        Idol.transform.position = RAND_POS_IN_TILE(randomTile);
        randomTile = TileRings[TileRings.Count - 1][UnityEngine.Random.Range(0, TileRings[TileRings.Count - 1].Count)];
        SPAWN(typeof(Shade), typeof(Janitor), RAND_POS_IN_TILE(randomTile));
        configureSpawners();
        Commissioned = true;
        StartCoroutine(gameLoop());
    }
    private IEnumerator gameInitLandmarks()
    {
        foreach (List<Hextile> ring in TileRings)
        {
            int ringNum = TileRings.IndexOf(ring);
            switch (ringNum)
            {
                case 0:
                    well = new GameObject().AddComponent<Landmark_Well>();
                    well.AssignToTile(centerTile);
                    break;
                case 1:
                    foreach (Hextile tile in ring)
                    {
                        if (tile ? (tile.Landmarks.Count == 0) : false)
                        {
                            new GameObject().AddComponent<Landmark_Barrier>().AssignToTile(tile);
                            new GameObject().AddComponent<Landmark_Pillar>().AssignToTile(tile);
                        }
                        yield return new WaitForFixedUpdate();
                        yield return null;
                    }
                    break;
                case 2:
                    foreach (Hextile tile in ring)
                    {
                        if (tile ? (tile.Landmarks.Count == 0) : false)
                        {

                            if (tile.AdjacentTiles.Count >= 4)
                            {

                                new GameObject().AddComponent<Landmark_Pillar>().AssignToTile(tile);
                                new GameObject().AddComponent<Landmark_Barrier>().AssignToTile(tile);
                            }
                            else
                            {
                                new GameObject().AddComponent<Landmark_Barrier>().AssignToTile(tile);
                                Landmark_Gate newGate = new GameObject().AddComponent<Landmark_Gate>();
                                newGate.AssignToTile(tile);
                                newGate.SetPositionOnTile((Hextile.HexPosition)((360 + AI.getAngle(tile.transform.position - centerTile.transform.position) % 360) / 60 + 1));
                                CryptGates.Add(newGate);
                            }
                        }
                        yield return new WaitForFixedUpdate();
                        yield return null;
                    }
                    break;
            }
        }
    }
    private IEnumerator gameInitCrypts()
    {
        yield return null;
        foreach(Landmark_Gate gate in CryptGates)
        {
            Stage newStage = new GameObject().AddComponent<Stage>();
            newStage.GateIn = gate;
            newStage.GateOut = gate;
            Crypts.Add(newStage);
            yield return new WaitUntil(() => newStage.Initialized);           
            //CreateCryptQuest(newStage);
        }  
        yield break;
    }
    protected IEnumerator gameLoop()
    {
        State = GameState.Liminal;
        Difficulty = 0;
        yield return new WaitUntil(() => Commissioned);
        //Player.INSTANCE.HostEntity.transform.position = RAND_POS_IN_TILE(AllTiles[UnityEngine.Random.Range(1, AllTiles.Count)]);
        Player.INSTANCE.HostEntity.transform.position = RAND_POS_IN_TILE(AllTiles[0]);
        while (Difficulty < 6)
        {
            State = GameState.Liminal;
            MobSpawner.PeriodicSpawningEnabled = false;

            yield return new WaitUntil(() => well.Used);
            MobSpawner.maxConcurrentInstances = 6 + (2 * Difficulty);
            MobSpawner.PeriodicSpawningEnabled = true;
            State = GameState.Arena;
            yield return new WaitUntil(() => { CryptCurrentlyActive = Crypts.FirstOrDefault(x => x.PlayerInStage); return CryptCurrentlyActive ? true : false; });
            State = GameState.Crypt;
            CryptCurrentlyActive.GateIn.CloseDoor();
            EliteSpawner.SpawnPosition = CryptCurrentlyActive.CenterTile.transform.position;
            EliteSpawner.SpawnObjects(Difficulty + 1);
            yield return new WaitUntil(() => EliteSpawner.listofSpawnedInstances.Count(x => x != null) == 0);
            CryptCurrentlyActive.GateOut.OpenDoor();
            yield return new WaitUntil(() => !CryptCurrentlyActive.PlayerInStage);
            CryptCurrentlyActive.GateOut.CloseDoor();
            well.ResetLiquid();
            Difficulty++;
            Crypts.Remove(CryptCurrentlyActive);
            Destroy(CryptCurrentlyActive.gameObject);
        }
        State = GameState.Finale;
    }
  

    private void configureSpawners()
    {
        MobSpawner = gameObject.AddComponent<Spawner>();
        MobSpawner.listOfSpawnComponents.Add(typeof(Skelly));
        MobSpawner.listOfSpawnComponents.Add(typeof(Goon));
        MobSpawner.transform.position = centerTile.transform.position + Vector3.up;
        MobSpawner.PeriodicSpawnFrequency = 30f;
        MobSpawner.PeriodicSpawnSize = 3;
        MobSpawner.OnSpawned = (listOfNewInstances) =>
        {
            //foreach(GameObject mob in listOfNewInstances)
            //{
            //    Construct_Character entity = mob.GetComponent<Construct_Character>();
            //    if(UnityEngine.Random.value <= (0.1f * Difficulty))
            //    {
            //        entity.isMutatingOnDeath = true;
            //    }
            //}
            Hextile randomTile = AllTiles[UnityEngine.Random.Range(1, AllTiles.Count)];
            MobSpawner.SpawnPosition = randomTile.transform.position + Vector3.up;
        };
        Hextile randomTile = AllTiles[UnityEngine.Random.Range(1, AllTiles.Count)];
        MobSpawner.SpawnPosition = randomTile.transform.position + Vector3.up;
        EliteSpawner = gameObject.AddComponent<Spawner>();
        EliteSpawner.listOfSpawnComponents.Add(typeof(Wraith));
        EliteSpawner.listOfSpawnComponents.Add(typeof(Revanent));
        EliteSpawner.PeriodicSpawningEnabled = false;
    }

    //private void CreateCryptQuest(Stage stage)
    //{
    //    if(UnityEngine.Random.value > 0.5f)
    //    {
    //        Type weaponType = Weapon.StandardTypes[UnityEngine.Random.Range(0, Weapon.StandardTypes.Count)];
    //        StartCoroutine(weaponQuest(stage, weaponType));
    //    }
    //    else
    //    {
    //        Type entityType = Entity.StandardTypes[UnityEngine.Random.Range(0, Entity.StandardTypes.Count)];
    //        StartCoroutine(foeQuest(stage, entityType));
    //    }

    //}

    private IEnumerator weaponQuest(Stage stage, Type weaponType)
    {
        bool completed = false;
        float progress = 0;
        int goal = UnityEngine.Random.Range(750, 1500);
        void increment(Weapon triggeringWeapon)
        {
            Type match = weaponType;
            Type triggerType = triggeringWeapon.GetType();
            if(triggerType == match)
            {
                progress += triggeringWeapon.Power;
            }
        }
        Weapon.On_Weapon_Hit.AddListener(increment);
        GameObject messageBlurb = Mullet.createBlurb(stage.GateIn.model, weaponType.ToString() + " Damage " + progress.ToString() + "/" + goal.ToString(), Color.yellow);
        messageBlurb.SetActive(false);
        while (!completed)
        {
            completed = progress >= goal;
            if (completed)
            {
                stage.GateIn.OpenDoor();
                stage.Interactable = true;
            }
            else if (stage.GateIn.PlayerAtDoor && !stage.PlayerInStage)
            {
                messageBlurb.GetComponent<Text>().text = weaponType.ToString() + " Damage " + progress.ToString() + "/" + goal.ToString();
                messageBlurb.SetActive(true);
            }
            else
            {
                messageBlurb.SetActive(false);
            }
            yield return null;
        }
        Weapon.On_Weapon_Hit.RemoveListener(increment);
        yield break;
    }

    private IEnumerator foeQuest(Stage stage, Type entityType)
    {
        bool completed = false;
        int progress = 0;
        int goal = UnityEngine.Random.Range(5, 15);
        void increment(Character triggeringEntity)
        {
            Type match = entityType;
            Type triggerType = triggeringEntity.GetType();
            if (triggerType == match)
            {
                progress++;
            }
        }
        Character.EntityVanquished.AddListener(increment);
        GameObject messageBlurb = Mullet.createBlurb(stage.GateIn.model, entityType.ToString() + " kills " + progress.ToString() + "/" + goal.ToString(), Color.yellow);
        messageBlurb.SetActive(false);
        while (!completed)
        {
            completed = progress >= goal;
            if (completed)
            {
                stage.GateIn.OpenDoor();
                stage.Interactable = true;
            }
            else if (stage.GateIn.PlayerAtDoor && !stage.PlayerInStage)
            {
                messageBlurb.GetComponent<Text>().text = entityType.ToString() + " kills " + progress.ToString() + "/" + goal.ToString();
                messageBlurb.SetActive(true);
            }
            else
            {
                messageBlurb.SetActive(false);
            }
            yield return null;
        }
        Character.EntityVanquished.RemoveListener(increment);
        yield break;
    }
}
