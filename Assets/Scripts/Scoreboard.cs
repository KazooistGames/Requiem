using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{

    public static Scoreboard INSTANCE;
    public int Score = 0;
    public int KillMultiplier = 1;

    public int SpeedBonus = 0;
    public float SpeedTimeGate = 0;
    
    void Start()
    {
        INSTANCE = this;
        Entity.EntityVanquished.AddListener(ADD_POINTS_FOR_KILL);
        StartCoroutine(WaveTimer());
    }

    void Update()
    {

    }


    /***** PUBLIC *****/


    /***** PROTECTED *****/


    /***** PRIVATE *****/
    private IEnumerator WaveTimer()
    {
        while (true)
        {
            yield return new WaitUntil(() => Requiem.INSTANCE.StateOfGame == Requiem.GameState.Wave);
            yield return null;
            Requiem_Arena arena = Requiem.INSTANCE.GetComponent<Requiem_Arena>();
            int totalStrength = arena.WaveStrengths[arena.Wave];
            SpeedTimeGate = totalStrength / 5;
            SpeedBonus = (int)Mathf.Pow(totalStrength, 2) / 1000;
            while (Requiem.INSTANCE.StateOfGame == Requiem.GameState.Wave && SpeedTimeGate > 0)
            {
                SpeedTimeGate -= Time.deltaTime;
                yield return null;
            }
            if(SpeedTimeGate > 0)
            {
                INSTANCE.Score += SpeedBonus;
            }
        }
        
    }

    private static void ADD_POINTS_FOR_KILL(Entity vanquishedEntity)
    {
        if(vanquishedEntity.Allegiance != Player.INSTANCE.HostEntity.Allegiance)
        {
            INSTANCE.Score += (int)vanquishedEntity.Strength;
        }
    }

}
