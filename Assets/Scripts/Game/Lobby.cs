using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby : MonoBehaviour
{
    public static Lobby INSTANCE;
    public Landmark_Gate GateOut;

    public bool ReadyToPlay = false;
    public bool PlayerPresent = false;
    public Hextile Tile;

    public Weapon StartingWeapon;

    void Start()
    {
        name = "LOBBY";
        INSTANCE = this;
        StartCoroutine(initializeLobby());
    }

    private void Update()
    {
        if(Player.INSTANCE ? Player.INSTANCE.HostEntity : false)
        {
            PlayerPresent = Player.INSTANCE ? Tile.ContainedObjects.Contains(Player.INSTANCE.HostEntity.gameObject) : false;
            if (!ReadyToPlay)
            {
                ReadyToPlay = Player.INSTANCE.HostEntity.MainHand;
            }
            else if(PlayerPresent)
            {
                GateOut.OpenDoor();
            }
        }  


    }

    private IEnumerator initializeLobby()
    {
        Tile = Hextile.GenerateRootTile();
        yield return null;
        GateOut = new GameObject().AddComponent<Landmark_Gate>();
        GateOut.AssignToTile(Hextile.LastGeneratedTile);
        GateOut.SetPositionOnTile(Hextile.HexPosition.Up);
        yield return null;
        new GameObject().AddComponent<Player>();
        yield return null;
        yield return null;
        Player.INSTANCE.HostEntity.transform.position = Game.RAND_POS_IN_TILE(Tile);
        //StartingWeapon = (Weapon)new GameObject().AddComponent(Weapon.StandardTypes[UnityEngine.Random.Range(0, Weapon.StandardTypes.Count)]);
        StartingWeapon = new GameObject().AddComponent<Spear>();
        StartingWeapon.transform.position = Tile.transform.position + Vector3.up;
        new GameObject().AddComponent<Level>();
        yield break;
    }

    public void Decommission()
    {
        Destroy(Tile);
    }

}
