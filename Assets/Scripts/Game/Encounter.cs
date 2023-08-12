using System.Collections.Generic;
using UnityEngine;

public class Encounter : MonoBehaviour
{
    public static List<Encounter> INSTANCES = new List<Encounter>();

    public bool Initialized = false;
    public bool Cleared = false;

    public delegate bool Evaluation();
    public Evaluation ObjectiveEvaluation;

    protected virtual void Start()
    {
        name = "ENCOUNTER";
        INSTANCES.Add(this);
    }

    void Update()
    {
        if (!Initialized)
        {
            Cleared = false;
        }
        else if (!Cleared)
        {
            Cleared = ObjectiveEvaluation();
        }
    }

}

