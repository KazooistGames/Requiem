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

    void Start()
    {
        INSTANCE = this;
        Score = 0;
        KillMultiplier = 1;
        SpeedBonus = 0; 
        validEntitiesToScoreFromKilling = new List<Entity>();
        Entity.EntityVanquished.AddListener(Score_Entity_Kill);
        Entity.EntityWounded.AddListener(Score_Weapon_Hit);
        Landmark_Well.JustGulped.AddListener(PENALIZE_GULP);
    }

    void Update()
    {
        KillMultiplier = Mathf.Max(1.0f, KillMultiplier);
    }


    /***** PUBLIC *****/
    public static void Score_Weapon_Hit(Entity entity, float magnitude)
    {
        validEntitiesToScoreFromKilling.Add(entity);
        if(magnitude > entity.Strength) //overkill gives bonus points
        {
            ADD_SCORE(magnitude - entity.Vitality);
        }
    }

    /***** PROTECTED *****/


    /***** PRIVATE *****/
    private static void PENALIZE_GULP(Entity entity, float amountGulped)
    {
        if(entity == Player.INSTANCE.HostEntity)
        {
            KillMultiplier -= amountGulped / 100f;
        }
    }

    private static void ADD_SCORE(float baseScore)
    {
        Score += baseScore * KillMultiplier;
    }


    private static void Score_Entity_Kill(Entity vanquishedEntity)
    {
        if(validEntitiesToScoreFromKilling.Contains(vanquishedEntity))
        {
            ADD_SCORE(vanquishedEntity.Strength);
            validEntitiesToScoreFromKilling.Remove(vanquishedEntity);
        }
    }

}
