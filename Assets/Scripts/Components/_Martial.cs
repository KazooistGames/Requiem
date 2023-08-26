using System.Collections.Generic;
using UnityEngine;

public class _Martial : MonoBehaviour
{
    public static _Martial Instance { get; private set; }

    public static Dictionary<Weapon, Queue<Weapon.Action>> WeaponActionQueues { get; private set; }
    private static List<Weapon> keysDequeuedThisFrame = new List<Weapon>();

    void Start()
    {
        if (Instance)
        {
            Destroy(Instance);
        }
        Instance = this;
        WeaponActionQueues = new Dictionary<Weapon, Queue<Weapon.Action>>();
    }

    void Update()
    {
        keysDequeuedThisFrame.Clear();
        foreach (KeyValuePair<Weapon, Queue<Weapon.Action>> kvp in WeaponActionQueues)
        {
            Weapon thisWeapon = kvp.Key;
            Weapon.Action desiredActionForThisWeapon = kvp.Value.Peek();
            if (applyWeaponControlsNeededForDesiredAction(thisWeapon, desiredActionForThisWeapon))
            {
                Weapon.Action actionCompletedThisFrame = WeaponActionQueues[thisWeapon].Dequeue();
                keysDequeuedThisFrame.Add(thisWeapon);
            }
        }
        foreach(Weapon weaponKey in keysDequeuedThisFrame)
        {
            if(WeaponActionQueues[weaponKey].Count == 0)
            {
                WeaponActionQueues.Remove(weaponKey);
            }
        }
    }

    /***** PUBLIC *****/
    public static void QueueActionForWeapon(Weapon weapon, Weapon.Action actionToQueue)
    {
        if (!WeaponActionQueues.ContainsKey(weapon))
        {
            WeaponActionQueues[weapon] = new Queue<Weapon.Action>();
        }
        WeaponActionQueues[weapon].Enqueue(actionToQueue);
    }

    public static void OverrideActionQueueForWeapon(Weapon weapon, Weapon.Action actionToQueue)
    {
        WeaponActionQueues.Remove(weapon);
        QueueActionForWeapon(weapon, actionToQueue);
    }


    /***** PROTECTED *****/



    /***** PRIVATE *****/
    private static bool applyWeaponControlsNeededForDesiredAction(Weapon weapon, Weapon.Action desiredAction)
    {
        (bool, bool, bool) triggerControlValues;
        switch (desiredAction)
        {
            case Weapon.Action.Idle:
                triggerControlValues = (false, false, false);
                break;
            case Weapon.Action.QuickCoil:
                triggerControlValues = (true, false, false);
                break;
            case Weapon.Action.QuickAttack:
                if (weapon.ActionCurrentlyAnimated == Weapon.Action.QuickCoil)
                {
                    triggerControlValues = (true, false, true);
                }
                else
                {
                    triggerControlValues = (true, false, false);
                }
                break;
            case Weapon.Action.StrongAttack:
                triggerControlValues = (false, false, true);
                break;
            case Weapon.Action.Guarding:
                triggerControlValues = (false, true, false);
                break;
            case Weapon.Action.Parrying:
                if (weapon.ActionCurrentlyAnimated == Weapon.Action.Guarding) 
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
        if(weapon.ActionCurrentlyAnimated == desiredAction)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
