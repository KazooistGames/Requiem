using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attack_cycler : MonoBehaviour
{
    public int Variants = 3;

    public int ActiveIndex = 0;
    public int NextIndex = 0;
    public int PreviousIndex = 0;


    private Animator animationController;
    private AnimatorStateInfo currentAnimation;
    private Weapon weapon;

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
        PreviousIndex = ActiveIndex;
        ActiveIndex = NextIndex;
        currentAnimation = animationController.GetCurrentAnimatorStateInfo(0);
        animationController.SetInteger("cycle", ActiveIndex);
        if (wielder_is_dashing())
        {
            NextIndex = 2;
            weapon.modPower["dashAttack"] = weapon.Wielder.Resolve;
        }
        else
        {
            NextIndex = 1;
            weapon.modPower["dashAttack"] = 0;
        }

    }

    private int get_next_index(int current_index)
    {
        int next_index = current_index + 1;
        if(next_index > Variants)
        {
            next_index = 1;
        }
        return next_index;
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
        if(PreviousIndex == 2)
        {
            foe.Stagger(weapon.Power / foe.Strength);
        }
    }
}
