using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class strong_attack_cycler : MonoBehaviour
{
    public int Variants = 3;

    public int ActiveIndex = 0;
    public int NextIndex = 0;


    private Animator animationController;
    private AnimatorStateInfo currentAnimation;

    void Start()
    {
        if (!GetComponent<Weapon>()) { Destroy(this); }
        else { animationController = GetComponent<Animator>(); }
    }

    public float dashLingerPeriod = 0.0f;
    private float dashLingerTimer = 0.0f;
    void Update()
    {
        currentAnimation = animationController.GetCurrentAnimatorStateInfo(0);
        animationController.SetInteger("cycle", NextIndex);
        if (wielder_is_dashing())
        {
            NextIndex = 2;
        }
        else if(NextIndex == 2 && (dashLingerTimer += Time.deltaTime) < dashLingerPeriod)
        {
            NextIndex = 2;
        }
        else
        {
            dashLingerTimer = 0;
            NextIndex = 1;
        }
        //if (currentAnimation.IsTag("Cycle"))
        //{
        //    animationController.SetInteger("cycle", NextIndex);
        //    ActiveIndex = NextIndex;
        //}
        //else
        //{
        //    animationController.SetInteger("cycle", -1);
        //    NextIndex = get_next_index(ActiveIndex);
        //}
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


    private void get_state_indices()
    {

    }

    private bool wielder_is_dashing()
    {
        Weapon weapon = GetComponent<Weapon>();
        if (!weapon.Wielder)
        {
            return false;
        }
        else if (weapon.Wielder.Dashing  /*|| weapon.Wielder.dashDirection != Vector3.zero || weapon.Wielder.DashCharging */)
        {
            return true;
        }
        else
        {
            return false;  
        }
    }
}
