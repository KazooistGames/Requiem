using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Swarm_Waver : Waver
{


    public override void Start()
    {
        base.Start();
        population_range = new Vector2(0, 20);
        group_size_range = new Vector2(3, 8);
        spawn_period_range = new Vector2(8, 20);
        entity_type = typeof(Skully);
        behaviour_type = typeof(Biter);

    }


    public override void Update()
    {
        base.Update();
        population_range = calculate_population_range();

    }

    /***** PUBLIC *****/



    /***** PRIVATE *****/

    private Vector2 calculate_population_range()
    {
        Vector2 range = new Vector2();
        float period = 150;
        int amplitude = 10;
        range.x = (amplitude - amplitude * Mathf.Cos(Requiem.GameClock / period) ) / 2;
        range.y = 20 + range.x;
        return range;
    }


}
