using System.Collections.Generic;
using UnityEngine;
using static _MartialController;
using UnityEngine.Events;

public class _MartialController : MonoBehaviour
{
    public static _MartialController INSTANCE;

    public UnityEvent<Weapon> ClearedQueue = new UnityEvent<Weapon>();

    public delegate bool Requisite();

    public struct MartialJob
    {
        public Weapon.ActionAnimation Action;
        public float Debounce;
        public Requisite Prerequisite;
    }

    public static Dictionary<Weapon, UnityEvent> Weapon_Completed_Action { get; private set; } = new Dictionary<Weapon, UnityEvent>();
    public static Dictionary<Weapon, MartialJob> Weapon_Actions { get; private set; } = new Dictionary<Weapon, MartialJob>();
    public static Dictionary<Weapon, Queue<MartialJob>> Weapon_Queues { get; private set; } = new Dictionary<Weapon, Queue<MartialJob>>();
    public static Dictionary<Weapon, float> Debounce_Timers { get; private set; } = new Dictionary<Weapon, float>();

    private static List<Weapon> KEYS_TO_DEQUEUE_THIS_FRAME = new List<Weapon>();


    void Start()
    {
        if (INSTANCE)
        {
            Destroy(INSTANCE);
        }
        INSTANCE = this;
        Weapon_Actions = new Dictionary<Weapon, MartialJob>();
        Weapon_Queues = new Dictionary<Weapon, Queue<MartialJob>>();
        Debounce_Timers = new Dictionary<Weapon, float>();
        KEYS_TO_DEQUEUE_THIS_FRAME = new List<Weapon>();
    }

    void Update()
    {
        KEYS_TO_DEQUEUE_THIS_FRAME.Clear();
        foreach (KeyValuePair<Weapon, MartialJob> kvp in Weapon_Actions)
        {
            Weapon weapon = kvp.Key;
            Weapon.ActionAnimation desiredAction = kvp.Value.Action;
            float debounce = kvp.Value.Debounce;
            if (weapon ? !weapon.Wielder : true)
            {
                KEYS_TO_DEQUEUE_THIS_FRAME.Add(weapon);
            }
            else if (Debounce_Timers[weapon] > 0)
            {
                bool completedRequisite = kvp.Value.Prerequisite == null ? true : kvp.Value.Prerequisite();
                if (Debounce_Timers[weapon] > debounce && completedRequisite)
                {
                    Debounce_Timers[weapon] = 0;
                    KEYS_TO_DEQUEUE_THIS_FRAME.Add(weapon);
                }
                else
                {
                    Debounce_Timers[weapon] += Time.deltaTime;
                }
            }
            else if (attemptToExecuteDesiredActionWithWeapon(weapon, desiredAction))
            {
                Debounce_Timers[weapon] += Time.deltaTime;
            }
        }
        foreach (Weapon weapon in KEYS_TO_DEQUEUE_THIS_FRAME)
        {
            if (!weapon)
            {
                Cancel_Actions(weapon);
            }
            else if (!Weapon_Queues.ContainsKey(weapon))
            {

            }
            else if (Weapon_Queues[weapon].Count > 0)
            {
                Weapon_Actions[weapon] = Weapon_Queues[weapon].Dequeue();
                if(Weapon_Queues[weapon].Count == 0)
                {
                    ClearedQueue.Invoke(weapon);
                    Debug.Log("Cleared queue for: " + weapon.name);
                }
            }

        }

    }

    /***** PUBLIC *****/
    public static void Queue_Action(Weapon weapon, Weapon.ActionAnimation action, float debounce = 0, Requisite requisite = null)
    {
        MartialJob newJob = new MartialJob() { Action = action, Debounce = debounce, Prerequisite = requisite};
        if (!Weapon_Actions.ContainsKey(weapon))
        {
            createNewWeaponKey(weapon, newJob);
        }
        else
        {
            if (!Weapon_Queues.ContainsKey(weapon))
            {
                Weapon_Queues[weapon] = new Queue<MartialJob>();
            }
            Weapon_Queues[weapon].Enqueue(newJob);
        }
    }

    public static void Override_Queue(Weapon weapon, Weapon.ActionAnimation action, float debounce = 0, Requisite requisite = null)
    {
        if (Weapon_Queues.ContainsKey(weapon))
        {
            Weapon_Queues[weapon].Clear();
        }
        Queue_Action(weapon, action, debounce, requisite);
    }

    public static void Override_Action(Weapon weapon, Weapon.ActionAnimation action, float debounce = 0, Requisite requisite = null)
    {
        MartialJob newJob = new MartialJob() { Action = action, Debounce = debounce, Prerequisite = requisite };
        Weapon_Actions[weapon] = newJob;
        Debounce_Timers[weapon] = 0;
    }

    public static void Cancel_Actions(Weapon weapon)
    {
        if (Weapon_Actions.ContainsKey(weapon))
        {
            Weapon_Actions.Remove(weapon);
        }
        if (Weapon_Queues.ContainsKey(weapon))
        {
            Weapon_Queues.Remove(weapon);
        }
        if (Debounce_Timers.ContainsKey(weapon))
        {
            Debounce_Timers.Remove(weapon);
        }
    }

    /***** PROTECTED *****/



    /***** PRIVATE *****/
    private static void createNewWeaponKey(Weapon weapon, MartialJob newJob)
    {
        if (Weapon_Actions.ContainsKey(weapon)) { return; }
        Weapon_Actions[weapon] = newJob;
        Weapon_Queues[weapon] = new Queue<MartialJob>();
        Weapon_Completed_Action[weapon] = new UnityEvent();
        Debounce_Timers[weapon] = 0;
    }


    private static bool attemptToExecuteDesiredActionWithWeapon(Weapon weapon, Weapon.ActionAnimation desiredAction)
    {
        (bool, bool, bool) triggerControlValues;
        weapon.ThrowTrigger = false;
        switch (desiredAction)
        {
            case Weapon.ActionAnimation.Idle:
                triggerControlValues = (false, false, false);
                break;
            case Weapon.ActionAnimation.QuickCoil:
                triggerControlValues = (true, false, false);
                break;
            case Weapon.ActionAnimation.QuickAttack:
                if (weapon.CurrentAction == Weapon.ActionAnimation.QuickCoil)
                {
                    triggerControlValues = (false, false, false);
                }
                else
                {
                    triggerControlValues = (true, false, false);
                }
                break;
            case Weapon.ActionAnimation.StrongAttack:
                triggerControlValues = (false, false, true);
                break;
            case Weapon.ActionAnimation.Guarding:
                triggerControlValues = (weapon.PrimaryTrigger, true, false);
                break;
            case Weapon.ActionAnimation.Parrying:
                if (weapon.CurrentAction == Weapon.ActionAnimation.Guarding) 
                {
                    triggerControlValues = (false, false, false);
                }
                else
                {
                    triggerControlValues = (false, true, false);
                }
                break;
            case Weapon.ActionAnimation.Aiming:
                triggerControlValues = (false, false, false);
                weapon.ThrowTrigger = true;
                break;
            case Weapon.ActionAnimation.Throwing:
                triggerControlValues = (false, false, false);
                weapon.ThrowTrigger = weapon.CurrentAction != Weapon.ActionAnimation.Aiming;
                break;
            default:
                triggerControlValues = (false, false, false);
                break;
        }
        weapon.PrimaryTrigger = triggerControlValues.Item1;
        weapon.SecondaryTrigger = triggerControlValues.Item2;
        weapon.TertiaryTrigger = triggerControlValues.Item3;
        if(weapon.CurrentAction == desiredAction)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
