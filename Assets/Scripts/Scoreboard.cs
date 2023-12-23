using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{

    public static Scoreboard INSTANCE;
    public static int Score = 0;

    public static float KillMultiplier = 1;
    public static float SpeedBonus = 0;

    
    void Start()
    {
        INSTANCE = this;
        Entity.EntityVanquished.AddListener(ADD_POINTS_FOR_KILL);
    }

    void Update()
    {
        KillMultiplier = Mathf.Max(1.0f, KillMultiplier);
    }


    /***** PUBLIC *****/


    /***** PROTECTED *****/


    /***** PRIVATE *****/


    private static void ADD_POINTS_FOR_KILL(Entity vanquishedEntity)
    {
        if(vanquishedEntity.Allegiance != Player.INSTANCE.HostEntity.Allegiance)
        {
            Score += Mathf.RoundToInt(vanquishedEntity.Strength * KillMultiplier);
        }
    }

}
