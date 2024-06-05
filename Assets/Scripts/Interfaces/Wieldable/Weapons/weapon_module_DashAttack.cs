using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weapon_module_DashAttack : MonoBehaviour
{

    public Cycle ActiveCycle = 0;
    public Cycle NextCycle = 0;
    public Cycle PreviousCycle = 0;


    private Animator animationController;
    private AnimatorStateInfo currentAnimation;
    private Weapon weapon;

    public enum Cycle
    {
        Stab = 0,
        Spear = 1,
        Smash = 2,
    }

    void Start()
    {
        if (!GetComponent<Weapon>()) { Destroy(this); }
        else 
        { 
            animationController = GetComponent<Animator>();
            weapon = GetComponent<Weapon>();
            weapon.Hitting.AddListener(handle_weapon_hit);
        }
    }

    void Update()
    {
        PreviousCycle = ActiveCycle;
        ActiveCycle = NextCycle;
        currentAnimation = animationController.GetCurrentAnimatorStateInfo(0);
        animationController.SetInteger("cycle", (int)ActiveCycle);
        if (wielder_is_dashing())
        {
            NextCycle = Cycle.Smash;
            weapon.modPower["dashAttack"] = weapon.Wielder.Resolve;
        }
        else
        {
            NextCycle = Cycle.Stab;
            weapon.modPower["dashAttack"] = 0;
        }

    }


    private bool wielder_is_dashing()
    {
        Weapon weapon = GetComponent<Weapon>();
        if (!weapon.Wielder)
        {
            return false;
        }
        else if (weapon.Wielder.dashDirection != Vector3.zero)
        {
            return true;
        }
        else
        {
            return false;  
        }
    }

    private void handle_weapon_hit(Weapon weapon, Entity foe)
    {
        if(PreviousCycle == Cycle.Smash)
        {
            foe.Stagger(weapon.Power / foe.Strength);
        }
    }
}
