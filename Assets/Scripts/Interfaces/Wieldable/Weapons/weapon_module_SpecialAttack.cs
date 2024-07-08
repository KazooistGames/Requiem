using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Weapon;

public class weapon_module_SpecialAttacks : MonoBehaviour
{

    private Animator animationController;
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
        StartCoroutine(tempo_routine());
    }

    void Update()
    {
        animationController.SetBool("Dash", check_wielder_dashing());

        UPDATE_CHARGE();
        //UPDATE_TRUESTRIKE();
        //UPDATE_PIERCE();
        UPDATE_KNOCKBACK();
        UPDATE_BLEED();
        UPDATE_CLOBBER();
        UPDATE_DISARM();

        //UPDATE_TEMPO();
    }

    private void UPDATE_DISARM()
    {
        if (weapon.Action == ActionAnim.Guarding)
        {
            weapon.Specials[SpecialAttacks.Disarm] = true;
        }
        else
        {
            weapon.Specials[SpecialAttacks.Disarm] = false;
        }
    }

    private void UPDATE_CLOBBER()
    {
        if (weapon.Action == ActionAnim.StrongAttack)
        {
            weapon.Specials[SpecialAttacks.Clobber] = true;
        }
        else
        {
            weapon.Specials[SpecialAttacks.Clobber] = false;
        }
    }

    private void UPDATE_BLEED()
    {
        if (weapon.currentAnimation.IsName("Slash") || weapon.currentAnimation.IsName("backhandSlash"))
        {
            weapon.Specials[SpecialAttacks.Bleed] = true;
        }
        else
        {
            weapon.Specials[SpecialAttacks.Bleed] = false;
        }
    }

    private void UPDATE_PIERCE()
    {
        if (check_dash_attack())
        {
            weapon.Specials[SpecialAttacks.Pierce] = true;
        }
        else
        {
            weapon.Specials[SpecialAttacks.Pierce] = false;
        }
    }
    private void UPDATE_KNOCKBACK()
    {
        if (check_dash_attack())
        {
            weapon.Specials[SpecialAttacks.Knockback] = true;
        }
        else
        {
            weapon.Specials[SpecialAttacks.Knockback] = false;
        }
    }

    private void UPDATE_TRUESTRIKE()
    {
        if(check_dash_attack())
        {
            weapon.Specials[SpecialAttacks.Truestrike] = true;
        }
        else
        {
            weapon.Specials[SpecialAttacks.Truestrike] = false;
        }
    }

    private void UPDATE_CHARGE()
    {
        if (weapon.Action == ActionAnim.StrongCoil)
        {
            weapon.Specials[SpecialAttacks.Charge] = true;
        }
        else
        {
            weapon.Specials[SpecialAttacks.Charge] = false;
        }
    }

    private void UPDATE_TEMPO()
    {
        weapon.Tempo = Mathf.Clamp(convert_charge_to_tempo(tempoCharge), 0, 1);
        if (weapon.Specials[SpecialAttacks.Charge])
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
        else if (weapon.Thrown || weapon.Action == ActionAnim.StrongAttack || weapon.Action == ActionAnim.QuickAttack)
        {

        }
        else
        {
            tempoCharge = 0;
            tempoChargeONS = true;
        }
    }

    private bool check_dash_attack()
    {
        return weapon.currentAnimation.IsName("backhandDashSwing") || weapon.currentAnimation.IsName("DashSwing"); ;
    }


    private bool check_wielder_dashing()
    {
        if (!weapon.Wielder)
        {
            return false;
        }
        else if (weapon.Wielder.dashDirection != Vector3.zero || weapon.Wielder.Dashing || weapon.Wielder.DashCharging)
        {
            return true;
        }
        else
        {
            return false;  
        }
    }
   

    private bool tempoChargeONS = true;
    private float tempoCharge = 0;
    private float tempoChargePeriod = 0.75f;
    private float convert_charge_to_tempo(float charge)
    {
        float frequencyVariable = Mathf.PI * charge;
        float frequencyConstant = Mathf.PI / 2;
        float amplitudeScalar = 0.5f;
        float amplitudeConstant = 0.5f;
        float function = amplitudeScalar * Mathf.Sin(frequencyVariable + frequencyConstant) + amplitudeConstant;
        return 1 - function;
    }


    private IEnumerator tempo_routine()
    {
        yield return null;
        while (true)
        {
            yield return new WaitUntil(() => weapon.Specials[SpecialAttacks.Charge]);
            if(weapon.Specials[SpecialAttacks.Charge])
            {
                while (weapon.Specials[SpecialAttacks.Charge])
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
                    weapon.Tempo = Mathf.Clamp(convert_charge_to_tempo(tempoCharge), 0, 1);
                    yield return null;
                }
                yield return new WaitUntil(() => weapon.Action == ActionAnim.Recovering || weapon.Action == ActionAnim.Recoiling);
                tempoCharge = 0;
                tempoChargeONS = true;
                weapon.Tempo = Mathf.Clamp(convert_charge_to_tempo(tempoCharge), 0, 1);
            }
            //else if (check_wielder_dashing())
            //{
            //    float scalar = 0.5f;
            //    while (check_wielder_dashing())
            //    {
            //        if (weapon.Wielder.Dashing)
            //        {
            //            tempoCharge += Time.deltaTime / scalar;
            //        }
            //        weapon.Tempo = Mathf.Clamp(convert_charge_to_tempo(tempoCharge), 0, 1);
            //        yield return null;
            //    }
            //    yield return new WaitUntil(() => !check_dash_attack());
            //    weapon.Tempo = 0;
            //    tempoCharge = 0;
            //}      
        }
    }


}
