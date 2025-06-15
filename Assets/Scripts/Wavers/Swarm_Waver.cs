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
        group_size_range = new Vector2(1, 5);
        spawn_period_range = new Vector2(8, 20);
        entity_type = typeof(Skully);
        behaviour_type = typeof(Biter);

    }


    public override void Update()
    {
        base.Update();
    }

    /***** PUBLIC *****/



    /***** PRIVATE *****/




}
