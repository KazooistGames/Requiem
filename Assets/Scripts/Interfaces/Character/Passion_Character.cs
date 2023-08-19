using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public abstract class Passion_Character : Character
{
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
        if (magnitude > 0)
        {
            Vitality -= magnitude;
            EventWounded.Invoke(magnitude);
        }
    }


    /***** PROTECTED *****/
    protected override void updatePosture()
    {
        if (Staggered)
        {
            posture = Posture.Stiff;
        }
        else if (Poise >= Strength)
        {
            posture = Posture.Flow;
        }
        else if (Poise <= 0)
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
