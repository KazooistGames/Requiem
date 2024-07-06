using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ritual : MonoBehaviour
{
    static Ritual INSTANCE;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {    
        determineAlterOffering();
    }


    /***** PUBLIC *****/
    public static void determineAlterOffering()
    {
        Landmark_Alter alter = Map.Alter;
        if (!alter || !Player.INSTANCE)
        {

        }
        else if (alter.Used)
        {
            alter.DesiredOffering = alter.TopStep;
        }
        else
        {
            alter.DesiredOffering = Player.INSTANCE.gameObject;
            alter.PentagramLineColor = new Color(1, 0, 0);
            alter.PentagramFlameStyle = _Flames.FlameStyles.Soulless;
        }
    }

    //public static Idol spawnIdol()
    //{
    //    Idol newIdol = Instantiate(Resources.Load<GameObject>("Prefabs/Wieldable/Idol")).GetComponent<Idol>();
    //    List<Hextile> spawnCandidates = Map.INSTANCE.ArenaTiles[Map.INSTANCE.ArenaTiles.Count - 1].Where(x => x.Landmarks.FirstOrDefault(x => x.GetComponent<Landmark_Barrier>())).ToList();
    //    Hextile spawnTile = spawnCandidates[UnityEngine.Random.Range(0, spawnCandidates.Count)];
    //    newIdol.transform.position = RAND_POS_IN_TILE(spawnTile);
    //    return newIdol;
    //}

    /***** PRIVATE *****/


}
