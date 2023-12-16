using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{

    public static Scoreboard INSTANCE;
    public int Score = 0;
    
    void Start()
    {
        INSTANCE = this;
        Entity.EntityVanquished.AddListener(ADD_POINTS_FOR_KILL);
    }

    void Update()
    {
        
    }


    /***** PUBLIC *****/


    /***** PROTECTED *****/


    /***** PRIVATE *****/
    private static void ADD_POINTS_FOR_KILL(Entity vanquishedEntity)
    {
        if(vanquishedEntity.Allegiance != Player.INSTANCE.HostEntity.Allegiance)
        {
            INSTANCE.Score += (int)vanquishedEntity.Strength;
        }
    }

}
