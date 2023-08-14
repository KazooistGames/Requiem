using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static List<Level> INSTANCES = new List<Level>();
    public static Level CurrentlyEngaged;
    public static Level Next;

    public bool Commissioned = false;
    public bool Engaged = false;
    public bool Beaten = false;

    public Stage Stage;
    public Encounter Encounter;

    public Level PreviousLevel;

    protected virtual void Start()
    {
        Next = this;
        INSTANCES.Add(this);
        name = "LEVEL_" + INSTANCES.Count.ToString();
        PreviousLevel = INSTANCES.Count > 1 ? INSTANCES[INSTANCES.Count - 2] : null;
        setDifficulty();
        StartCoroutine(routineInstantiateLevel());
    }

    private void Update()
    {

        if (!Stage || !Encounter)
        {
            Commissioned = false;
        }
        else if (!Commissioned)
        {
            Commissioned = Stage.Initialized && Encounter.Initialized;
        }
        else if (!Engaged)
        {
            Stage.Interactable = PreviousLevel ? PreviousLevel.Beaten : true;
            Engaged = Stage.PlayerInStage && Stage.Interactable;
        }
        else //level is commissioned and engaged by player
        {
            CurrentlyEngaged = this;
            Encounter.Initialized = true;
            Stage.GateIn.CloseDoor();
            if(Stage.GateIn.Energized || Stage.GateIn.Used)
            {

            }
            else if (!PreviousLevel)
            {
                if (Lobby.INSTANCE.Tile)
                {
                    Lobby.INSTANCE.Decommission();
                }
                else if (Next == this)
                {
                    Commission();
                }
            }
            else if(PreviousLevel.Commissioned)
            { 
                PreviousLevel.Decommission();
            }
            else if(Next == this)
            {
                Commission();
            }         
            if (Beaten)
            {
                Stage.GateOut.OpenDoor();
            }
            else
            {
                Beaten = Encounter.Cleared;
                Stage.GateOut.CloseDoor();
            }
        }

    }

    public void Decommission()
    {
        if (Stage) { Destroy(Stage); }
        if (Encounter) { Destroy(Encounter); }
    }

    public static Level Commission()
    {
        return new GameObject().AddComponent<Level>();
    }

    private void createStage()
    {
        Stage = new GameObject().AddComponent<Stage>();
        Stage.gameObject.transform.SetParent(transform);
        Stage.GateIn = PreviousLevel ? PreviousLevel.Stage.GateIn : Lobby.INSTANCE.GateOut;
    }

    private void createEncounter()
    {
        Encounter = new GameObject().AddComponent<Encounter>();
        Encounter.gameObject.transform.SetParent(transform);
        //Encounter.Difficulty = setDifficulty();
        //Encounter.SpawnTile = Stage.CenterTile;
    }

    private int setDifficulty()
    {
        float mean = INSTANCES.Count;
        float standardDeviation = 1.0f;
        return Mathf.Max(Mathf.RoundToInt(Mullet.BellCurve(mean, standardDeviation)), 1);
    }

    private IEnumerator routineInstantiateLevel()
    {
        createStage();
        yield return new WaitUntil(() => Stage.Initialized);
        createEncounter();
        yield break;
    }

}
