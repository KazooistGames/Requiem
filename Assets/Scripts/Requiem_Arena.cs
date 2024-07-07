using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

public class Requiem_Arena : Requiem
{
    public static new Requiem_Arena INSTANCE;

    //public Shade QuestGiver;

    public float TimeGateTimeLeft = 0;

    public int Ritual = 0;

    private Idol idol;

    protected override void Start()
    {
        base.Start();
        INSTANCE = this;
        StartCoroutine(gameLoop());
    }

    protected override void Update()
    {
        base.Update();

        if(!Map.Commissioned) { return; } //logic meant for runtime after map is generated

        determineAlterOffering();
        foreach (Hextile chamber in Map.Chambers)
        {
            Landmark_Gate gate = Map.Gates[Map.Chambers.IndexOf(chamber)];
            if (!gate)
            {

            }
            else if(chamber.DetectContainedObjects().Count(x=>x.GetComponent<Entity>()) > 0)
            {
                gate.OpenDoor();
            }
            else
            {
                gate.CloseDoor();
            }
        }
    }

    /***** PUBLIC *****/

    /***** PROTECTED *****/
     protected IEnumerator gameLoop()
    {
        //yield return new WaitUntil(() => Commissioned);
        Torch.Toggle(false);
        Haunt.INSTANCE.TargetTile = Map.CenterTile;
        Player.INSTANCE.HostEntity.transform.position = RAND_POS_IN_TILE(Map.CenterTile);
        //Player.INSTANCE.HostEntity.Vitality = 1;
        blurbIndicator = _BlurbService.createBlurb(Map.Alter.TopStep, "Test", Color.red, sizeScalar: 3);
        blurbIndicator.SetActive(false);
        blurbIndicator.GetComponent<Text>().text = "0:00";
        StateOfGame = GameState.Lobby;
        yield return null;

        while (Player.INSTANCE.HostEntity)
        {
            StateOfGame = GameState.Liminal;
            Ritual++;
            Torch.Toggle(false);
            //collect_everything(Haunt.INSTANCE.gameObject);
            if (!idol)
            {
                idol = spawnIdol();
            }
            yield return new WaitUntil(() => !Map.Alter.Energized);
            yield return new WaitUntil(() => Map.Alter.Used && Map.Alter.Energized);
            if (Ritual == 10)
            {
                StateOfGame = GameState.Final;
                yield return finalBoss();
            }
            else
            {
                StateOfGame = GameState.Wave;
                //yield return waveRoutine();
                //GetComponent<Waver>().StartWave(10, 5, 3, 3);
            }
        }
    }


    protected IEnumerator finalBoss()
    {
        idol.BecomeMob();
        while (idol.mobEntity)
        {
            yield return null;
        }
        Torch.Toggle(false);
        yield return new WaitForSeconds(5);
        Player.INSTANCE.RequiemAchieved = true;
        yield return new WaitForSeconds(5);
    }


    /***** PRIVATE *****/

    private void determineAlterOffering()
    {
        Landmark_Alter alter = Map.Alter;
        if (!alter || !Player.INSTANCE)
        {

        }
        else if (StateOfGame == GameState.Wave || StateOfGame == GameState.Boss)
        {
            alter.DesiredOffering = alter.TopStep;
        }
        else if(Ritual == 10)
        {
            alter.DesiredOffering = idol.gameObject;
            alter.PentagramLineColor = new Color(1, 0, 0.75f);
            alter.PentagramFlameStyle = _Flames.FlameStyles.Soulless;
            idol.flames.FlamePresentationStyle = _Flames.FlameStyles.Soulless;
        }
        else
        {
            alter.DesiredOffering = idol.gameObject;
            alter.PentagramLineColor = new Color(1, 0, 0);
            alter.PentagramFlameStyle = _Flames.FlameStyles.Soulless;
            idol.flames.FlamePresentationStyle = _Flames.FlameStyles.Inferno;
            idol.flames.emissionModule.enabled = true;
        }
    }

    private GameObject blurbIndicator;

    private Idol spawnIdol()
    {
        Idol newIdol = Instantiate(Resources.Load<GameObject>("Prefabs/Wieldable/Idol")).GetComponent<Idol>();
        List<Hextile> spawnCandidates = Map.ArenaTiles[Map.ArenaTiles.Count-1].Where(x => x.Landmarks.FirstOrDefault(x => x.GetComponent<Landmark_Barrier>())).ToList();
        Hextile spawnTile = spawnCandidates[UnityEngine.Random.Range(0, spawnCandidates.Count)];
        newIdol.transform.position = RAND_POS_IN_TILE(spawnTile);
        return newIdol;
    }

    private void materializeSoulPearls()
    {
        List<SoulPearl> pearls = FindObjectsOfType<SoulPearl>().ToList();
        foreach (SoulPearl soulPearl in pearls)
        {
            soulPearl.FlyToPhylactery();
        }
    }
    private void dematerializeGhosts()
    {
        List<Ghosty> ghosts = FindObjectsOfType<Ghosty>().ToList();
        foreach (Ghosty ghosty in ghosts)
        {
            ghosty.Die();
        }
    }

}
