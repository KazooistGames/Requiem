using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basic_Waver : Waver
{

    public override void Start()
    {
        base.Start();
        population_range = new Vector2(0, 9);
        group_size_range = new Vector2(1, 3);
        spawn_period_range = new Vector2(30, 45);
        entity_type = typeof(Skelly);
        behaviour_type = typeof(Goon);

    }

    public override void Update()
    {
        base.Update();
        spawn_period_range = calculate_spawn_period_range();

    }

    /***** PUBLIC *****/



    /***** PRIVATE *****/

    private Vector2 calculate_spawn_period_range()
    {
        Vector2 range = new Vector2();
        float minutes = Mathf.RoundToInt(Requiem.GameClock / 60f);
        float max_duration = 30;
        float ratio = minutes / max_duration;
        range.x = Mathf.Lerp(30, 0, ratio);
        range.y = Mathf.Lerp(45, 15, ratio);
        return range;
    }


}
