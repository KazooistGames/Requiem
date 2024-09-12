using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Weapon;

public class weapon_module_SpecialAttacks : MonoBehaviour
{

    private Animator animationController;
    private Weapon weapon;

    private const float combo_tempo_increment = 1f;

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
        weapon.Hitting.AddListener(increment_combo_tempo);
        weapon.Clashing.AddListener(increment_combo_tempo);
    }


    void Update()
    {
        animationController.SetBool("Dash", check_wielder_dashing());

        //UPDATE_CHARGE();
        //UPDATE_TRUESTRIKE();
        //UPDATE_PIERCE();
        UPDATE_KNOCKBACK();
        UPDATE_BLEED();
        //UPDATE_CLOBBER();
        UPDATE_DISARM();
        UPDATE_SUNDER();

        //UPDATE_TEMPO();
    }

    private void UPDATE_SUNDER()
    {
        if (check_strong_attack())
        {
            weapon.Specials[SpecialAttacks.Sunder] = true;
        }
        else
        {
            weapon.Specials[SpecialAttacks.Sunder] = false;
        }
    }
    private void UPDATE_DISARM()
    {
        if (weapon.Action == ActionAnim.Parrying)
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



    private bool check_dash_attack()
    {
        bool swings = weapon.currentAnimation.IsName("backhandDashSwing") || weapon.currentAnimation.IsName("DashSwing");
        bool coils = weapon.currentAnimation.IsName("backhandDashCoil") || weapon.currentAnimation.IsName("DashCoil");
        return swings || coils;
    }

    private bool check_strong_attack()
    {
        return weapon.Action == ActionAnim.StrongAttack;
    }

    private bool check_quick_attack()
    {
        return weapon.Action == ActionAnim.QuickAttack && !check_dash_attack();
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
   

    private float tempoCharge = 0;
    private float tempoChargePeriod = 0.75f;
    private float convert_charge_to_tempo(float charge)
    {
        //float frequencyVariable = Mathf.PI * charge;
        //float frequencyConstant = Mathf.PI / 2;
        //float amplitudeScalar = 0.5f;
        //float amplitudeConstant = 0.5f;
        //float function = amplitudeScalar * Mathf.Sin(frequencyVariable + frequencyConstant) + amplitudeConstant;
        //return 1 - function;

        return 1 - Mathf.Pow((1 - charge), 2);
    }


    private IEnumerator tempo_routine()
    {
        yield return null;
        while (true)
        {
            yield return new WaitUntil(() => weapon.Action == ActionAnim.Guarding || weapon.Action == ActionAnim.StrongCoil || check_dash_attack() || check_quick_attack());
            weapon.Tempo = 0;
            tempoCharge = 0;
            if (weapon.Action == ActionAnim.StrongCoil)
            {
                yield return tempo_charge_routine();
            }
            else if(weapon.Action == ActionAnim.Guarding)
            {
                yield return tempo_parry_routine();
            }
            else if (check_dash_attack())
            {
                yield return tempo_dash_routine();
            }
            else if (check_quick_attack())
            {
                yield return tempo_combo_routine();
            }
            weapon.Tempo = 0;
            tempoCharge = 0;
        }
    }

    private IEnumerator tempo_dash_routine()
    {
        Vector3 origin = weapon.MostRecentWielder.transform.position;
        float distance = 0;
        yield return new WaitUntil(() => weapon.MostRecentWielder.Dashing || !check_dash_attack());
        while (check_dash_attack() && weapon.MostRecentWielder.Dashing && weapon.Action != ActionAnim.QuickAttack)
        {
            distance = Mathf.Max(distance, (origin - weapon.MostRecentWielder.transform.position).magnitude);
            weapon.Tempo = Mathf.Clamp(distance, 0, 1);
            yield return null;
        }
        yield return new WaitUntil(() => !check_dash_attack() || weapon.MostRecentWielder.DashCharging);
    }

    private IEnumerator tempo_charge_routine()
    {
        while (weapon.Action == ActionAnim.StrongCoil)
        {
            //if (tempoCharge < 1)
            //{
            float increment = (Time.deltaTime / tempoChargePeriod);
            tempoCharge += increment;
            //}
            weapon.Tempo = Mathf.Clamp(convert_charge_to_tempo(tempoCharge), 0, 1);
            yield return null;
        }
        yield return new WaitUntil(() => !check_strong_attack());
    }

    private IEnumerator tempo_combo_routine()
    {
        while (!check_wielder_dashing() && weapon.Action != ActionAnim.StrongCoil && weapon.Action != ActionAnim.Guarding)
        {
            float decrement = Time.deltaTime / 2;
            weapon.Tempo = Mathf.Clamp(weapon.Tempo -= decrement, 0, 1);
            yield return null;
        }
    }

    private IEnumerator tempo_parry_routine()
    {
        while(weapon.Action == ActionAnim.Guarding)
        {
            float period = 1 / 2f;
            tempoCharge = Mathf.Clamp(tempoCharge + Time.deltaTime / period, 0, 1);
            weapon.Tempo = 1 - convert_charge_to_tempo(tempoCharge);
            yield return null;
        }
    }

    private void increment_combo_tempo(Weapon weapon, Entity entity)
    {
        if (check_quick_attack())
        {
            weapon.Tempo += combo_tempo_increment;
        }
    }
    private void increment_combo_tempo(Weapon weapon, Weapon foe_weapon)
    {
        if (check_quick_attack())
        {
            weapon.Tempo = 0;
        }
    }


}
