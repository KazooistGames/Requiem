using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Haunt : AIBehaviour
{
    public static Haunt INSTANCE;
    public Hextile TargetTile;


    protected override void Start()
    {
        base.Start();
        INSTANCE = this;
        Destroy(entity.indicator);
    
    }


    protected override void Update()
    {
        base.Update();

    }

   


}
