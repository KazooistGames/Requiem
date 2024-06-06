using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Weapon;

public class weapon_module_SpecialAttacks : MonoBehaviour
{

    private Animator animationController;
    private AnimatorStateInfo currentAnimation;
    private Weapon weapon;

    void Start()
    {
        if (!GetComponent<Weapon>()) 
        { 
            Destroy(this); 
        }
        else 
        { 
            animationController = GetComponent<Animator>();
            weapon = GetComponent<Weapon>();
        }
    }

    void Update()
    {
        currentAnimation = animationController.GetCurrentAnimatorStateInfo(0);
        animationController.SetBool("Dash", wielder_is_dashing());
        UPDATE_TRUESTRIKE();
        UPDATE_TEMPO();
        UPDATE_RIPPER();
    }

    private void UPDATE_RIPPER()
    {
        if (weapon.currentAnimation.IsName("Slash") || weapon.currentAnimation.IsName("backhandSlash"))
        {
            weapon.Ripper = true;
        }
        else
        {
            weapon.Ripper = false;
        }
    }

    private void UPDATE_TRUESTRIKE()
    {
        if(weapon.currentAnimation.IsName("Thrust Coil") || weapon.currentAnimation.IsName("Thrust"))
        {
            weapon.TrueStrike = true;
        }
        else
        {
            weapon.TrueStrike = false;
        }
    }

    private void UPDATE_TEMPO()
    {
        weapon.Tempo = Mathf.Clamp(convertChargeToTempo(tempoCharge), 0, 1);

        if (weapon.Action == ActionAnim.StrongCoil || weapon.Action == ActionAnim.Aiming)
        {
            if (tempoChargeONS)
            {
                tempoChargeONS = false;
            }
            else if (tempoCharge < 1)
            {
                float increment = (Time.deltaTime / tempoChargePeriod);
                tempoCharge += increment;
            }
        }
        else if (weapon.Thrown || weapon.Action == ActionAnim.StrongAttack)
        {

        }
        else
        {
            tempoCharge = 0;
            tempoChargeONS = true;
        }
    }

    private bool wielder_is_dashing()
    {
        if (!weapon.Wielder)
        {
            return false;
        }
        else if (weapon.Wielder.dashDirection != Vector3.zero || weapon.Wielder.DashCharging)
        {
            return true;
        }
        else
        {
            return false;  
        }
    }


    private float tempoCharge = 0;
    private float tempoChargePeriod = 1f;
    private float tempoChargeExponent = 1 / 2f;
    private bool tempoChargeONS = true;
    private float tempoChargeMin = 0.25f;
    private float convertChargeToTempo(float charge)
    {
        float frequencyVariable = Mathf.PI * charge;
        float frequencyConstant = Mathf.PI / 2;
        float amplitudeScalar = 0.5f;
        float amplitudeConstant = 0.5f;
        float function = amplitudeScalar * Mathf.Sin(frequencyVariable + frequencyConstant) + amplitudeConstant;
        return 1 - function;
    }
}
