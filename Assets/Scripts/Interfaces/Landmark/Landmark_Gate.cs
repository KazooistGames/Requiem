using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Landmark_Gate : Landmark
{

    private GameObject door;

    public bool ToggleDoor = false;

    public BoxCollider BoundingBox;
    public bool PlayerAtDoor;
    public UnityEvent PlayerArrivedAtDoor = new UnityEvent();
    public UnityEvent PlayerLeftDoor = new UnityEvent();

    private static float openPosition = -0.65f;
    private static float closedPosition = 0;
    private static float openSpeed = 0.2f;
    private static float closeSpeed = 0.8f;

    private float doorPositionDesired = 0;
    private float doorSpeedDesired = 0;

    private void Update()
    {
        if (Player.INSTANCE)
        {
            bool playerWasAlreadyAtDoor = PlayerAtDoor;
            PlayerAtDoor = Player.INSTANCE.interactBox.bounds.Intersects(BoundingBox.bounds);
            if(PlayerAtDoor && !playerWasAlreadyAtDoor) 
            {
                PlayerArrivedAtDoor.Invoke();
            }
            else if(!PlayerAtDoor && playerWasAlreadyAtDoor)
            {
                PlayerLeftDoor.Invoke();
            }
        }
        if (ToggleDoor)
        {
            if (Used)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == Player.INSTANCE.gameObject)
        {
            PlayerArrivedAtDoor.Invoke();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject == Player.INSTANCE.gameObject)
        {
            PlayerLeftDoor.Invoke();
        }
    }

    /* GATE FUNCTIONS */
    public override void AssignToTile(Hextile tile)
    {
        base.AssignToTile(tile);
        gameObject.name = "Gate";
        gameObject.layer = Requiem.layerWall;
        if (!model)
        {
            model = Instantiate(Resources.Load<GameObject>("Prefabs/structures/Gate"));
            door = model.GetComponentsInChildren<Transform>()[1].gameObject;
            model.transform.SetParent(transform, false);
            model.layer = Requiem.layerWall;
            door.layer = Requiem.layerWall;
            door.AddComponent<Rigidbody>().isKinematic = true;
            BoundingBox = model.AddComponent<BoxCollider>();
            BoundingBox.isTrigger = true;
            StartCoroutine(doorPositionControl());
        }   
    }



    public void OpenDoor()
    {
        doorPositionDesired = openPosition;
        doorSpeedDesired = openSpeed;
    }

    public void CloseDoor()
    {
        doorPositionDesired = closedPosition;
        doorSpeedDesired = closeSpeed;
    }

    private IEnumerator doorPositionControl()
    {
        while (true)
        {
            yield return new WaitWhile(() => door.transform.localPosition.y == doorPositionDesired);
            playDoorGrindingSound();
            Energized = true;
            float delta = doorPositionDesired - door.transform.localPosition.y;
            float increment;
            while (delta != 0)
            {
                delta = doorPositionDesired - door.transform.localPosition.y;
                increment = Mathf.Sign(delta) * Mathf.Min(Time.deltaTime * doorSpeedDesired, Mathf.Abs(delta));
                door.transform.localPosition += new Vector3(0, increment, 0);
                yield return null;
            }
            Used = door.transform.localPosition.y == openPosition;
            Energized = false;
            playDoorStopSound();
        }
    }

    private void playDoorGrindingSound()
    {
        
    }

    private void playDoorStopSound()
    {

    }
}
