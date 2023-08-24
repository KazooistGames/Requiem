using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class _Martial : MonoBehaviour
{
    private Character me;
    private Weapon myWeapon;
    private Weapon FoeWeapon;

    public float ReflexPeriod = 0.1f;

    public UnityEvent CompletedTask = new UnityEvent();
    public UnityEvent EventInRange = new UnityEvent();
    public UnityEvent EventFoeInRange = new UnityEvent();
    public UnityEvent EventOutOfRange = new UnityEvent();
    public UnityEvent EventFoeOutOfRange = new UnityEvent();

    public Task TaskActive { get; private set; }
    public Task TaskQueued { get; private set; }


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
    }

    private float taskTimer = 0;
    private float reflexTimer  = 0;

    private bool inRange = false;
    private bool foeInRange = false;

    private static Task EMPTY_TASK = new Task() { action = Action.Idle, duration = 0};

    void Start()
    {
        TaskActive = EMPTY_TASK;
        TaskQueued = EMPTY_TASK;
        me = GetComponent<Character>();
        if (!me)
        {
            Destroy(this);
        }
    }

    void Update()
    {
        myWeapon = me.MainHand ? me.MainHand.GetComponent<Weapon>() : null;
        if (!myWeapon) { return; } //no weapon means nothing to control
        if ((taskTimer += Time.deltaTime) >= TaskActive.duration)
        {
            taskTimer -= TaskActive.duration;
            myWeapon.PrimaryTrigger = false;
            myWeapon.SecondaryTrigger = false;
            myWeapon.TertiaryTrigger = false;
            TaskActive = TaskQueued;
            TaskQueued = EMPTY_TASK;
            CompletedTask.Invoke();
        }
        else
        {
            switch (TaskActive.action)
            {
                case Action.Idle:
                    myWeapon.PrimaryTrigger = false;
                    myWeapon.SecondaryTrigger = false;
                    myWeapon.TertiaryTrigger = false;
                    break;
                case Action.Primary:
                    myWeapon.PrimaryTrigger = true;
                    break;
                case Action.Secondary:
                    myWeapon.SecondaryTrigger = true;
                    break;
                case Action.Tertiary:
                    myWeapon.TertiaryTrigger = true;
                    break;
            }
        }
        if(!me.Foe) { return; } //assumes we have a foe assigned to duel
        FoeWeapon = me.Foe.MainHand ? me.Foe.MainHand.GetComponent<Weapon>() : null;
        if ((reflexTimer += Time.deltaTime) >= ReflexPeriod)
        {
            reflexTimer -= ReflexPeriod;    
            float rangeNeeded = (me.Foe.transform.position - transform.position).magnitude;
            if (inRange && rangeNeeded > myWeapon.Range)
            {
                EventOutOfRange.Invoke();
            }
            else if (!inRange && rangeNeeded <= myWeapon.Range)
            {
                EventInRange.Invoke();
            }
            if (FoeWeapon)
            {
                float foeRangeNeeded = (transform.position - me.Foe.transform.position).magnitude;
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

    public void OverrideTask(Action action, float duration)
    {
        Task task = new Task() { action = action, duration = duration};
        TaskActive = task;
    }

    public void QueueTask(Action action, float duration)
    {
        Task task = new Task() { action = action, duration = duration};
        TaskQueued = task;
    }

    public void AbortTask()
    {
        taskTimer = TaskActive.duration;
    }


    /***** PROTECTED *****/


    /***** PRIVATE *****/


}
