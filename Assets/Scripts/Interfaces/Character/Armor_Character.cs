using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Armor_Character : Character
{
    public bool isMutatingOnDeath = false;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    /***** PUBLIC *****/
    public override void Damage(float magnitude)
    {
        float armorDamage = Mathf.Min(Special, magnitude);
        Special -= armorDamage;   
        float vitalityDamage = posture == Posture.Stiff ? magnitude : magnitude - armorDamage;
        if (vitalityDamage > 0)
        {
            Vitality -= vitalityDamage;
            EventWounded.Invoke(vitalityDamage);
        }
    }



    /***** PROTECTED *****/
    protected override void Die()
    {
        if (isMutatingOnDeath)
        {
            Mutate();
        }
        else
        {
            base.Die();
        }

    }

    protected virtual void Mutate()
    {
        isMutatingOnDeath = false;
        Vitality = Strength;
        Special = Strength;
    }

    protected override void updatePosture()
    {
        if (Staggered)
        {
            posture = Posture.Stiff;
        }
        else if (Special >= Strength)
        {
            posture = Posture.Flow;
        }
        else if (Special <= 0)
        {
            posture = Posture.Stiff;
        }
        else
        {
            posture = Posture.Warm;
        }
    }

    /***** PRIVATE *****/


}
