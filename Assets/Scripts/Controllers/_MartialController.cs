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
        public Weapon.ActionAnim Action;
        public float Debounce;
        public Requisite Prerequisite;
    }

    public static Dictionary<Weapon, UnityEvent> Weapon_Completed_Action { get; private set; } = new Dictionary<Weapon, UnityEvent>();
    public static Dictionary<Weapon, MartialJob> Weapon_Actions { get; private set; } = new Dictionary<Weapon, MartialJob>();
    public static Dictionary<Weapon, Queue<MartialJob>> Weapon_Queues { get; private set; } = new Dictionary<Weapon, Queue<MartialJob>>();
    public static Dictionary<Weapon, float> Debounce_Timers { get; private set; } = new Dictionary<Weapon, float>();

    private static List<Weapon> KEYS_TO_DEQUEUE_THIS_FRAME = new List<Weapon>();


    //TODO: OPTIMIZE!! cannot call events every frame any more

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
            Weapon.ActionAnim desiredAction = kvp.Value.Action;
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
            else if (weapon ? Weapon_Queues[weapon].Count == 0 : false)
            {
                ClearedQueue.Invoke(weapon);
                Debug.Log("Cleared queue for: " + weapon.name);
            }
            else if (Weapon_Queues[weapon].Count > 0)
            {
                Weapon_Actions[weapon] = Weapon_Queues[weapon].Dequeue();

            }
        }

    }

    /***** PUBLIC *****/
    public static void Queue_Action(Weapon weapon, Weapon.ActionAnim action, float debounce = 0, Requisite requisite = null)
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

    public static void Override_Queue(Weapon weapon, Weapon.ActionAnim action, float debounce = 0, Requisite requisite = null)
    {
        if(weapon == null) { return; }
        if (Weapon_Queues.ContainsKey(weapon))
        {
            Weapon_Queues[weapon].Clear();
        }
        Queue_Action(weapon, action, debounce, requisite);
    }

    public static void Override_Action(Weapon weapon, Weapon.ActionAnim action, float debounce = 0, Requisite requisite = null)
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

    public static Weapon.ActionAnim Get_Next_Action(Weapon weapon)
    {
        if (Weapon_Queues[weapon].Count > 0)
        {
            return Weapon_Queues[weapon].Peek().Action; 
        }
        else
        {
            return Weapon.ActionAnim.error;
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


    private static bool attemptToExecuteDesiredActionWithWeapon(Weapon weapon, Weapon.ActionAnim desiredAction)
    {
        (bool, bool, bool) triggerControlValues;
        weapon.ThrowTrigger = false;
        switch (desiredAction)
        {
            case Weapon.ActionAnim.Idle:
                triggerControlValues = (false, false, false);
                break;
            case Weapon.ActionAnim.QuickCoil:
                triggerControlValues = (true, false, false);
                break;
            case Weapon.ActionAnim.QuickAttack:
                if (weapon.Action == Weapon.ActionAnim.QuickCoil)
                {
                    triggerControlValues = (false, false, false);
                }
                else
                {
                    triggerControlValues = (true, false, false);
                }
                break;
            case Weapon.ActionAnim.StrongCoil:
                triggerControlValues = (false, false, true);
                break;
            case Weapon.ActionAnim.StrongAttack:
                if(weapon.Action == Weapon.ActionAnim.StrongCoil)
                {
                    triggerControlValues = (false, false, false);
                }
                else
                {
                    triggerControlValues = (false, false, true);
                }
                break;
            case Weapon.ActionAnim.Guarding:
                triggerControlValues = (weapon.PrimaryTrigger, true, false);
                break;
            case Weapon.ActionAnim.Parrying:
                if (weapon.Action == Weapon.ActionAnim.Guarding) 
                {
                    triggerControlValues = (false, false, false);
                }
                else
                {
                    triggerControlValues = (false, true, false);
                }
                break;
            case Weapon.ActionAnim.Aiming:
                triggerControlValues = (false, false, false);
                weapon.ThrowTrigger = true;
                break;
            case Weapon.ActionAnim.Throwing:
                triggerControlValues = (false, false, false);
                weapon.ThrowTrigger = weapon.Action != Weapon.ActionAnim.Aiming;
                break;
            default:
                triggerControlValues = (false, false, false);
                break;
        }
        weapon.PrimaryTrigger = triggerControlValues.Item1;
        weapon.SecondaryTrigger = triggerControlValues.Item2;
        weapon.TertiaryTrigger = triggerControlValues.Item3;
        if(weapon.Action == desiredAction)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
