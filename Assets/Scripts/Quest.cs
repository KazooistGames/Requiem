using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest : MonoBehaviour
{
    public delegate bool Evaluation();
    public Evaluation interactionTrigger;
    public Evaluation completionTrigger;

    public delegate void Callback();
    public Callback onDormant;
    public Callback onInteraction;
    public Callback onCompletion;

    public bool Completed = false;
    public bool Interacting = false;

    public float Goal;
    public float GoalProgress;

    protected GameObject MessageBlurb;

    private void Start()
    {
        StartCoroutine(questRuntime());
    }

    private IEnumerator questRuntime()
    {
        yield return new WaitUntil(() => completionTrigger != null && onCompletion != null);
        yield return new WaitUntil(() => interactionTrigger != null && onInteraction != null);
        while (!Completed)
        {
            Completed = completionTrigger();
            if (interactionTrigger())
            {
                onInteraction();
            }
            else
            {
                onDormant();
            }
            yield return null;
        }
        onCompletion();
        yield break;
    }
}
