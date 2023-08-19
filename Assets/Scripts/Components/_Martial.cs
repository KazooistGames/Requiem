using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class _Martial : MonoBehaviour
{

    public Weapon MyWeapon;
    public Character Foe;
    public Weapon FoeWeapon;
    public float ReflexPeriod = 0.1f;

    public UnityEvent EventInRange = new UnityEvent();
    public UnityEvent EventFoeInRange = new UnityEvent();
    public UnityEvent EventOutOfRange = new UnityEvent();
    public UnityEvent EventFoeOutOfRange = new UnityEvent();

    public Task ActiveTask { get; private set; }
    public Task QueuedTask { get; private set; }

    public enum Action
    {
        Idle,
        Primary,
        Secondary,
        Tertiary,
    }

    public struct Task
    {
        public Action action;
        public float duration;
        public TaskAbortion abortion;
    }

    public delegate bool TaskAbortion();

    private float taskTimer = 0;
    private float reflexTimer  = 0;

    private bool inRange = false;
    private bool foeInRange = false;

    private static Task EMPTY_TASK = new Task() { action = Action.Idle, duration = 0};

    void Start()
    {
        ActiveTask = EMPTY_TASK;
        QueuedTask = EMPTY_TASK;
    }

    void Update()
    {
        if (!MyWeapon) { return; } //no weapon means nothing to control
        if ((taskTimer += Time.deltaTime) >= ActiveTask.duration)
        {
            taskTimer -= ActiveTask.duration;
            MyWeapon.PrimaryTrigger = false;
            MyWeapon.SecondaryTrigger = false;
            MyWeapon.TertiaryTrigger = false;
            ActiveTask = QueuedTask;
            QueuedTask = EMPTY_TASK;
        }
        else
        {
            switch (ActiveTask.action)
            {
                case Action.Idle:
                    MyWeapon.PrimaryTrigger = false;
                    MyWeapon.SecondaryTrigger = false;
                    MyWeapon.TertiaryTrigger = false;
                    break;
                case Action.Primary:
                    MyWeapon.PrimaryTrigger = true;
                    break;
                case Action.Secondary:
                    MyWeapon.SecondaryTrigger = true;
                    break;
                case Action.Tertiary:
                    MyWeapon.TertiaryTrigger = true;
                    break;
            }
        }
        if(!Foe) { return; } //assumes we have a foe assigned to duel
        FoeWeapon = Foe.MainHand ? Foe.MainHand.GetComponent<Weapon>() : null;
        if ((reflexTimer += Time.deltaTime) >= ReflexPeriod)
        {
            reflexTimer -= ReflexPeriod;    
            float rangeNeeded = (Foe.transform.position - transform.position).magnitude;
            if (inRange && rangeNeeded > MyWeapon.Range)
            {
                EventOutOfRange.Invoke();
            }
            else if (!inRange && rangeNeeded <= MyWeapon.Range)
            {
                EventInRange.Invoke();
            }
            if (FoeWeapon)
            {
                float foeRangeNeeded = (transform.position - Foe.transform.position).magnitude;
                if (foeInRange && foeRangeNeeded > FoeWeapon.Range)
                {
                    EventFoeOutOfRange.Invoke();
                }
                else if (!foeInRange && foeRangeNeeded <= FoeWeapon.Range)
                {
                    EventFoeInRange.Invoke();
                }
            }
        }
    }




    /***** PUBLIC *****/

    public void OverrideTask(Task task)
    {
        ActiveTask = task;
    }

    public void QueueTask(Task task)
    {
        QueuedTask = task;
    }


}
