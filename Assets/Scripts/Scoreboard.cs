using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{

    public static Scoreboard INSTANCE;
    public int Score = 0;
    public int KillMultiplier = 1;

    public int SpeedBonus = 0;
    public float SpeedTimeGateTimeLeft = 0;

    private GameObject blurbIndicator;
    
    void Start()
    {
        INSTANCE = this;
        Entity.EntityVanquished.AddListener(ADD_POINTS_FOR_KILL);
        StartCoroutine(WaveTimer());

    }

    void Update()
    {
        if (!Requiem_Arena.INSTANCE.Commissioned)
        {
            return;
        }
        else if(!blurbIndicator)
        {
            blurbIndicator = _BlurbService.createBlurb(Requiem_Arena.INSTANCE.Alter.TopStep, "Test", Color.red, sizeScalar: 2);
        }
        else if(Requiem.INSTANCE.StateOfGame == Requiem.GameState.Wave)
        {
            int minutesLeft = (int)SpeedTimeGateTimeLeft / 60;
            int secondsLeft = (int)SpeedTimeGateTimeLeft % 60;
            string timeLeftText = minutesLeft.ToString() + ":" + secondsLeft.ToString();
            blurbIndicator.SetActive(true);
            blurbIndicator.GetComponent<Text>().text = timeLeftText;
        }
        else
        {
            blurbIndicator.SetActive(false);
        }

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
            int totalStrength = arena.WaveStrengths[arena.Wave%3];
            SpeedTimeGateTimeLeft = totalStrength / 10;
            SpeedBonus = (int)Mathf.Pow(totalStrength, 2) / 1000;
            while (Requiem.INSTANCE.StateOfGame == Requiem.GameState.Wave && SpeedTimeGateTimeLeft > 0)
            {
                SpeedTimeGateTimeLeft -= Time.deltaTime;
                yield return null;
            }
            if(SpeedTimeGateTimeLeft > 0)
            {
                INSTANCE.Score += SpeedBonus;
            }
            yield return new WaitUntil(()=> Requiem.INSTANCE.StateOfGame != Requiem.GameState.Wave);
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
