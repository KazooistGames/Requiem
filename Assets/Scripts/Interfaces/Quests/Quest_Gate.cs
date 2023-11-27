using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest_Gate : MonoBehaviour
{
    public Landmark_Gate Gate;
    private string progressMessage;
    private int bonesRequiredToUnlockDoor;

    protected GameObject MessageBlurb;


    protected void Start()
    {
        bonesRequiredToUnlockDoor = GENERATE_BONE_REQUIREMENTS();
        progressMessage = bonesRequiredToUnlockDoor.ToString() + " Bones";
        MessageBlurb = _BlurbService.createBlurb(gameObject, progressMessage, Color.yellow);
        MessageBlurb.SetActive(false);
    }

    protected void Update()
    {
        if (Gate)
        {
            if (Gate.PlayerAtDoor && !Gate.Used && !Gate.Energized)
            {
                MessageBlurb.SetActive(true);
            }
            else
            {
                MessageBlurb.SetActive(false);
            }
        }
        else if (gameObject.GetComponent<Landmark_Gate>())
        {
            Gate = gameObject.GetComponent<Landmark_Gate>();
            Gate.PlayerArrivedAtDoor.AddListener(addInteractionTrigger);
            Gate.PlayerLeftDoor.AddListener(removeInteractionTrigger);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    /***** PUBLIC *****/



    /***** PROTECTED *****/



    /***** PRIVATE *****/
    private void addInteractionTrigger()
    {
        Player.INSTANCE.HostEntity.Interact.AddListener(Interact);
    }

    private void removeInteractionTrigger()
    {
        Player.INSTANCE.HostEntity.Interact.RemoveListener(Interact);
    }
    private void Interact(Entity entity)
    {
        if(Player.INSTANCE.BonesCollected >= bonesRequiredToUnlockDoor)
        {
            Player.INSTANCE.BonesCollected -= bonesRequiredToUnlockDoor;
            Gate.OpenDoor();
            Gate.PlayerArrivedAtDoor.RemoveListener(addInteractionTrigger);
            Gate.PlayerLeftDoor.RemoveListener(removeInteractionTrigger);
            Player.INSTANCE.HostEntity.Interact.RemoveListener(Interact);
        }
        else
        {

        }
    }

    private static int GENERATE_BONE_REQUIREMENTS()
    {
        return Mathf.RoundToInt(Mullet.BellCurve(1200, 200, 400, 2000));
    }
}
