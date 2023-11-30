using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Landmark : MonoBehaviour
{
    public bool Initialized = false;
    public Hextile Tile;
    public bool Used = false;
    public bool Energized = false;
    public GameObject model;

    public Hextile.HexPosition PositionOnTile = Hextile.HexPosition.error;

    protected List<Torch> torches = new List<Torch>();

    private void Start()
    {
        GetComponentsInChildren(torches);
        foreach (Torch torch in torches)
        {
            Vector3 position = torch.transform.position - transform.position;
            torch.Mount(gameObject, position);
        }
    }
    /***** PUBLIC *****/
    public virtual void Impacted()
    {
        foreach (Torch torch in torches)
        {
            if (torch ? torch.MountTarget : false)
            {
                torch.MountTarget = null;
                torch.DropItem(yeet: true);
            }
        }
    }

    public virtual void AssignToTile(Hextile Tile)
    {
        if (this.Tile)
        {
            this.Tile.Landmarks.Remove(this);
        }
        transform.SetParent(Tile.transform, false);
        transform.localPosition = Vector3.zero;
        this.Tile = Tile;
        this.Tile.Landmarks.Add(this);
        Initialized = true;
    }

    public void SetPositionOnTile(Hextile.HexPosition newPosition)
    {
        if (newPosition == Hextile.HexPosition.error || !Initialized) return;
        float rads = Mathf.Deg2Rad * (60f * (int)newPosition - 30);
        float scaledRadius = Hextile.Radius * Mathf.Sin(Mathf.PI / 3);
        model.transform.localPosition = new Vector3(Mathf.Cos(rads) * scaledRadius, Hextile.Thickness / 2, Mathf.Sin(rads) * scaledRadius);
        model.transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 30 - 60 * (int)newPosition, transform.localEulerAngles.z);
        PositionOnTile = newPosition;
    }

}
