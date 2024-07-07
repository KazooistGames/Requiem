using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Rituals : MonoBehaviour
{
    public static Rituals INSTANCE;
    public static Idol Idol;
    public static Entity Nemesis;


    void Start()
    {
        if (INSTANCE)
        {
            Destroy(this);
        }
        else
        {
            INSTANCE = this;
        }
        StartCoroutine(ritual_cycler());
    }


    void Update()
    {    

    }


    /***** PUBLIC *****/



    /***** PRIVATE *****/
    private static IEnumerator ritual_cycler()
    {
        yield return null;
        yield return new WaitUntil(() => Map.Commissioned);
        Idol = spawn_idol();
        yield return null;
        Map.Alter.DesiredOffering = Idol.gameObject;
        Map.Alter.PentagramLineColor = new Color(1, 0, 0);
        Map.Alter.PentagramFlameStyle = _Flames.FlameStyles.Inferno;
        yield return new WaitUntil(() => { Map.Alter.PentagramFlameStyle = Idol.flames.FlamePresentationStyle; return Map.Alter.Used; });
        Nemesis = Idol.BecomeMob();
        Map.Alter.DesiredOffering = Map.Alter.TopStep;
    }

    private static Idol spawn_idol()
    {
        Idol newIdol = Instantiate(Resources.Load<GameObject>("Prefabs/Wieldable/Idol")).GetComponent<Idol>();
        List<Hextile> spawnCandidates = Map.ArenaTiles[Map.ArenaTiles.Count - 1].Where(x => x.Landmarks.FirstOrDefault(x => x.GetComponent<Landmark_Barrier>())).ToList();
        Hextile spawnTile = spawnCandidates[Random.Range(0, spawnCandidates.Count)];
        newIdol.transform.position = Requiem.RAND_POS_IN_TILE(spawnTile);
        return newIdol;
    }


}
