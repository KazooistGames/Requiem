using System.Collections.Generic;
using UnityEngine;
using static _MartialController;

public class _MartialController : MonoBehaviour
{
    private static _MartialController Instance;

    public struct MartialJob
    {
        public Weapon.ActionAnimation Action;
        public float Debounce;
    }

    public static Dictionary<Weapon, MartialJob> Weapon_Actions { get; private set; } = new Dictionary<Weapon, MartialJob>();
    public static Dictionary<Weapon, Queue<MartialJob>> Action_Queues { get; private set; } = new Dictionary<Weapon, Queue<MartialJob>>();
    public static Dictionary<Weapon, float> Debounce_Timers { get; private set; } = new Dictionary<Weapon, float>();

    private static List<Weapon> KEYS_TO_DEQUEUE_THIS_FRAME = new List<Weapon>();


    void Start()
    {
        if (Instance)
        {
            Destroy(Instance);
        }
        Instance = this;
        Weapon_Actions = new Dictionary<Weapon, MartialJob>();
        Action_Queues = new Dictionary<Weapon, Queue<MartialJob>>();
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
            if (!weapon)
            {
                Cancel_Actions(weapon);
            }
            else if (Debounce_Timers[weapon] > 0)
            {
                if (Debounce_Timers[weapon] > debounce)
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
            if (Action_Queues[weapon].Count > 0)
            {
                Weapon_Actions[weapon] = Action_Queues[weapon].Dequeue();
            }
        }

    }

    /***** PUBLIC *****/
    public static void Queue_Action(Weapon weapon, Weapon.ActionAnimation action, float debounce = 0)
    {
        MartialJob newJob = new MartialJob() { Action = action, Debounce = debounce };
        if (!Weapon_Actions.ContainsKey(weapon))
        {
            Weapon_Actions[weapon] = newJob;
            Action_Queues[weapon] = new Queue<MartialJob>();
            Debounce_Timers[weapon] = 0;
        }
        else
        {
            Action_Queues[weapon].Enqueue(newJob);
        }
    }

    public static void Override_Queue(Weapon weapon, Weapon.ActionAnimation action)
    {
        if (Action_Queues.ContainsKey(weapon))
        {
            Action_Queues[weapon].Clear();
        }
        Queue_Action(weapon, action);
    }

    public static void Override_Action(Weapon weapon, Weapon.ActionAnimation action, float debounce = 0)
    {
        MartialJob newJob = new MartialJob() { Action = action, Debounce = debounce };
        Weapon_Actions[weapon] = newJob;
        Debounce_Timers[weapon] = 0;
    }

    public static void Cancel_Actions(Weapon weapon)
    {
        if (Weapon_Actions.ContainsKey(weapon))
        {
            Weapon_Actions.Remove(weapon);
        }
        if (Action_Queues.ContainsKey(weapon))
        {
            Action_Queues.Remove(weapon);
        }
        if (Debounce_Timers.ContainsKey(weapon))
        {
            Debounce_Timers.Remove(weapon);
        }
    }

    /***** PROTECTED *****/



    /***** PRIVATE *****/



    private static bool attemptToExecuteDesiredActionWithWeapon(Weapon weapon, Weapon.ActionAnimation desiredAction)
    {
        (bool, bool, bool) triggerControlValues;
        switch (desiredAction)
        {
            case Weapon.ActionAnimation.Idle:
                triggerControlValues = (false, false, false);
                break;
            case Weapon.ActionAnimation.QuickCoil:
                triggerControlValues = (true, false, false);
                break;
            case Weapon.ActionAnimation.QuickAttack:
                if (weapon.ActionAnimated == Weapon.ActionAnimation.QuickCoil)
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
                triggerControlValues = (false, true, false);
                break;
            case Weapon.ActionAnimation.Parrying:
                if (weapon.ActionAnimated == Weapon.ActionAnimation.Guarding) 
                {
                    triggerControlValues = (false, false, false);
                }
                else
                {
                    triggerControlValues = (false, true, false);
                }
                break;
            default:
                triggerControlValues = (false, false, false);
                break;
        }
        weapon.PrimaryTrigger = triggerControlValues.Item1;
        weapon.SecondaryTrigger = triggerControlValues.Item2;
        weapon.TertiaryTrigger = triggerControlValues.Item3;
        if(weapon.ActionAnimated == desiredAction)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
