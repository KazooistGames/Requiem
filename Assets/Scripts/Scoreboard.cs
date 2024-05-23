using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

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
        Landmark_Bloodwell.JustGulped.AddListener(PENALIZE_GULP);
    }

    void Update()
    {
        KillMultiplier = Mathf.Max(1.0f, KillMultiplier);
    }

    private void OnDestroy()
    {
        Entity.EntityVanquished.RemoveListener(Score_Kill);
        Landmark_Bloodwell.JustGulped.RemoveListener(PENALIZE_GULP);
    }

    /***** PUBLIC *****/
    public static void Add_Score(float baseScore)
    {
        Score += baseScore * KillMultiplier;
    }
    public static void Score_Kill(Entity vanquishedEntity)
    {
        if (validEntitiesToScoreFromKilling.Contains(vanquishedEntity) && vanquishedEntity.Vitality < vanquishedEntity.Strength)
        {
            Add_Score(GET_ENTITY_BASE_SCORE_VALUE(vanquishedEntity));
            validEntitiesToScoreFromKilling.Remove(vanquishedEntity);
            SoulPearl pearl = new GameObject().AddComponent<SoulPearl>();
            pearl.transform.position = vanquishedEntity.transform.position;
            pearl.Telecommute(Requiem_Arena.INSTANCE.CenterTile.gameObject, 0.1f, (x) => Destroy(x.gameObject), false, true);
            pearl.Body.useGravity = false;
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

    private static Dictionary<Type, int> DIC_ENTITY_BASE_VALUES = new Dictionary<Type, int>()
    {
        { typeof(Biter), 50 },
        { typeof(Goon), 100 },
        { typeof(Assassin), 200 },
        { typeof(Sentinel), 500 },
        { typeof(Bully), 800 },
        { typeof(Revanent), 1000 },
        { typeof(Nemesis), 2500 },
    };

    private static int GET_ENTITY_BASE_SCORE_VALUE(Entity entity)
    {
        Type foeType = entity.GetComponent<AIBehaviour>().GetType();
        if (!DIC_ENTITY_BASE_VALUES.ContainsKey(foeType))
        {
            return (int)entity.Strength;
        }
        else
        {
            return DIC_ENTITY_BASE_VALUES[foeType];
        }

    }



}
    

