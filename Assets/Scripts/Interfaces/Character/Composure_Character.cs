using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Composure_Character : Character
{
    private static float COMPOSURE_REGEN_PERIOD = 3f;
    private static float COMPOSURE_DEBOUNCE_PERIOD = 3f;
    private static float COMPOSURE_RESTING_PERCENTAGE = 1f;
    private float composureDebounceTimer = 0.0f;

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
        float poiseDamage = Mathf.Min(Special, magnitude);
        alterComposure(-poiseDamage);
        float vitalityDamage = posture == Posture.Stiff ? magnitude : magnitude - poiseDamage;
        if (vitalityDamage > 0)
        {
            Vitality -= vitalityDamage;
            EventWounded.Invoke(vitalityDamage);
        }
    }


    public void alterComposure(float value)
    {
        bool reduction = value <= 0;
        if (reduction && posture == Posture.Stiff)
        {
            Rebuke(0.25f + 1.5f * (-value / Strength));
            return;
        }
        float existingDelta = Special - COMPOSURE_RESTING_PERCENTAGE * Strength;
        Special += value;
        Special = Mathf.Clamp(Special, -1, Strength);
        if (value * existingDelta >= 0)
        {
            composureDebounceTimer = 0.0f;
        }
        else
        {
            composureDebounceTimer = COMPOSURE_DEBOUNCE_PERIOD;
        }
        updatePosture();
        if (reduction && posture == Posture.Stiff)
        {
            Rebuke(0.25f + 1.5f * (-value / Strength));
        }
    }


    /***** PROTECTED *****/
    protected override void updateStats()
    {
        base.updateStats();
        if (Staggered)
        {
            Special = 0;
        }
        else
        {
            if ((composureDebounceTimer += Time.deltaTime) >= (COMPOSURE_DEBOUNCE_PERIOD / Resolve))
            {
                float increment = Resolve * Time.deltaTime * Strength / COMPOSURE_REGEN_PERIOD;
                float restingValue = COMPOSURE_RESTING_PERCENTAGE * Strength;
                float delta = Special - restingValue;
                if (Mathf.Abs(delta) <= increment)
                {
                    Special = COMPOSURE_RESTING_PERCENTAGE * Strength;
                }
                else if (Special > restingValue)
                {
                    Special -= increment;
                }
                else if (Special < restingValue)
                {
                    Special += increment;
                }
            }
        }
        Special = Mathf.Clamp(Special, 0, Strength);
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
        else if (posture != Posture.Stiff)
        {
            posture = Posture.Warm;
        }
    }

    /***** PRIVATE *****/



}
