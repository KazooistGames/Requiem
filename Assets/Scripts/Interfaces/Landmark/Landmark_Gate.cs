using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class Landmark_Gate : Landmark
{
    public Hextile.HexPosition PositionOnTile = Hextile.HexPosition.error;
    private GameObject door;

    public BoxCollider BoundingBox;
    public bool PlayerAtDoor;

    private static float openPosition = -0.65f;
    private static float closedPosition = 0;
    private static float openSpeed = 0.15f;
    private static float closeSpeed = 0.6f;

    private float doorPositionDesired = 0;
    private float doorSpeedDesired = 0;

    private void Update()
    {
        if (Player.INSTANCE)
        {
            PlayerAtDoor = Player.INSTANCE.interactBox.bounds.Intersects(BoundingBox.bounds);
        }
    }

    /* GATE FUNCTIONS */
    public override void AssignToTile(Hextile tile)
    {
        base.AssignToTile(tile);
        gameObject.name = "Gate";
        gameObject.layer = Game.layerWall;
        if (!model)
        {
            model = Instantiate(Resources.Load<GameObject>("Prefabs/structures/Gate"));
            door = model.GetComponentsInChildren<Transform>()[1].gameObject;
            model.transform.SetParent(transform, false);
            model.layer = Game.layerWall;
            door.layer = Game.layerWall;
            door.AddComponent<Rigidbody>().isKinematic = true;
            BoundingBox = model.AddComponent<BoxCollider>();
            BoundingBox.isTrigger = true;
            StartCoroutine(doorPositionControl());
        }   
    }

    public void SetPositionOnTile(Hextile.HexPosition newPosition)
    {
        if (newPosition == Hextile.HexPosition.error || !Initialized) return;
        float rads = Mathf.Deg2Rad * (60f * (int)newPosition - 30);
        float scaledRadius = Hextile.Radius * Mathf.Sin(Mathf.PI/3);
        model.transform.localPosition = new Vector3(Mathf.Cos(rads) * scaledRadius, Hextile.Thickness/2, Mathf.Sin(rads) * scaledRadius);
        model.transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 30 - 60 * (int)newPosition, transform.localEulerAngles.z);
        PositionOnTile = newPosition;

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
