using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{

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
        Entity.EntityVanquished.AddListener(Score_Kill);
        //Entity.EntityWounded.AddListener(Score_Hit);
        Landmark_Well.JustGulped.AddListener(PENALIZE_GULP);
    }

    void Update()
    {
        KillMultiplier = Mathf.Max(1.0f, KillMultiplier);
    }


    /***** PUBLIC *****/
    public static void Score_Kill(Entity vanquishedEntity)
    {
        if (validEntitiesToScoreFromKilling.Contains(vanquishedEntity) && vanquishedEntity.Vitality < vanquishedEntity.Strength)
        {
            ADD_SCORE(vanquishedEntity.Strength);
            validEntitiesToScoreFromKilling.Remove(vanquishedEntity);
        }
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

    /***** PROTECTED *****/


    /***** PRIVATE *****/
    private static void PENALIZE_GULP(Entity entity, float amountGulped)
    {
        if(entity == Player.INSTANCE.HostEntity && KillMultiplier > 1)
        {
            KillMultiplier -= amountGulped / 100f;
            KillMultiplier = Mathf.Max(1, KillMultiplier);
        }
    }

    private static void ADD_SCORE(float baseScore)
    {
        Score += baseScore * KillMultiplier;
    }

}
    

