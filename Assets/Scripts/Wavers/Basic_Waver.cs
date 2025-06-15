using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basic_Waver : Waver
{

    public override void Start()
    {
        base.Start();
        population_range = new Vector2(1, 5);
        group_size_range = new Vector2(1, 2);
        spawn_period_range = new Vector2(12, 15);
        entity_type = typeof(Skelly);
        behaviour_type = typeof(Goon);

    }


    public override void Update()
    {
        base.Update();
    }

    /***** PUBLIC *****/



    /***** PRIVATE *****/

}
