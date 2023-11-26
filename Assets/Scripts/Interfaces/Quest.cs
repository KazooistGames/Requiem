   using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Quest : MonoBehaviour
{
    public delegate bool Evaluation();
    public Evaluation interactionTrigger;
    public Evaluation completionTrigger;

    public UnityEvent DormantGo;
    public UnityEvent InteractionGo;
    public UnityEvent CompletionGo;

    //public bool Completed = false;
    //public bool Interacting = false;
    public enum States
    {
        Dormant,
        Interaction,
        Completed
    }
    States State;

    public float Goal;
    public float GoalProgress;

    protected GameObject MessageBlurb;

    private void Start()
    {
        StartCoroutine(questRuntime());
    }

    private IEnumerator questRuntime()
    {
        yield return new WaitUntil(() => completionTrigger != null && interactionTrigger != null);
        while (true) 
        { 
            if (completionTrigger())
            {
                State = States.Completed;
                CompletionGo.Invoke();
                yield return new WaitWhile(() => completionTrigger());
            }
            else if (interactionTrigger())
            {
                State = States.Interaction;
                InteractionGo.Invoke();
                yield return new WaitWhile(() => interactionTrigger());
            }
            else
            {
                State = States.Dormant;
                DormantGo.Invoke();
                yield return new WaitUntil(()=> interactionTrigger() || completionTrigger());
            }
            yield return null;
        }
    }
}

