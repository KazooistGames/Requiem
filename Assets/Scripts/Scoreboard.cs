using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Scoreboard : MonoBehaviour
{
    public float Charge = 0;

    public static Scoreboard INSTANCE;
    public static float Score = 0;

    public static float KillMultiplier = 1;
    public static float SpeedBonus = 0;

    private static List<Entity> validEntitiesToScoreFromKilling = new List<Entity>();
    private static float WAVE_MULTIPLIER_BONUS = 0.25f;

    void Start()
    {
        INSTANCE = this;
        Score = 0;
        KillMultiplier = 1;
        SpeedBonus = 0; 
        validEntitiesToScoreFromKilling = new List<Entity>();

    }

    void Update()
    {
        KillMultiplier = Mathf.Max(1.0f, KillMultiplier);
    }

    private void OnDestroy()
    {

    }



    public static void Score_Hit(Entity entity, float magnitude)
    {
        validEntitiesToScoreFromKilling.Add(entity);
    }

    public static void Wave_Completed_Rewards()
    {
        if(KillMultiplier < 2)
        {
            float availableBonus = Mathf.Min(2 - KillMultiplier, WAVE_MULTIPLIER_BONUS);
            KillMultiplier += availableBonus;
        }
    }




}
    

