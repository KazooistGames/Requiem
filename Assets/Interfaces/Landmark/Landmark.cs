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

    /******** custom funcs ***********/
    public virtual void Impacted()
    {
        return;
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


}
