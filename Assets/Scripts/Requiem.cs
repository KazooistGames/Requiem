using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using System.Threading;

public class Requiem: MonoBehaviour
{
    public static Requiem INSTANCE { get; private set; }

    public List<Hextile> AllTilesInPlay = new List<Hextile>();

    public bool Commissioned = false;
    public float GameClock = 0;
    public bool Paused = false;
    public float TimeScale;
    public static int KillCount = 0;
    public static int Score = 0;

    public static GameObject SpiritFlameTemplate;
    public static Dictionary<string, Mesh> weaponMeshes = new Dictionary<string, Mesh>();
    public static AudioClip[] damageSounds;
    public static AudioClip[] deathSounds;
    public static AudioClip[] boneSounds;
    public static AudioClip[] ambienceSounds;

    public static int layerScript = 31;
    public static int layerAudio = 30;
    public static int layerEntity = 13;
    public static int layerObstacle = 9;
    public static int layerItem = 8;
    public static int layerTile = 7;
    public static int layerWall = 6;
    public static int layerInvisible = 3;

    void Awake()
    {
        UnityEngine.Random.InitState((int)DateTime.UtcNow.Ticks);
        INSTANCE = this;
        gameObject.name = "REQUIEM";
        gameObject.layer = layerScript;
        Paused = false;
        KillCount = 0;
        loadSounds();
        loadMeshes();
        SpiritFlameTemplate = Resources.Load<GameObject>("Prefabs/rageFlame");
    }

    protected virtual void Start()
    {
        
        TimeScale = Time.timeScale;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientSkyColor = Color.black;
        GameClock = 0f;
    }

    protected virtual void Update()
    {
        if (Paused)
        {
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = TimeScale;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    /* CUSTOM METHODS */

    public static AudioClip getSound(string path)
    {
        return Resources.Load<AudioClip>(path);
    }    


    /* SCENE SWITCH */

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    /* LOAD */
    private void loadSounds()
    {
        damageSounds = Resources.LoadAll<AudioClip>("Audio/damage/");
        deathSounds = Resources.LoadAll<AudioClip>("Audio/death/");
        boneSounds = Resources.LoadAll<AudioClip>("Audio/bones/");
        ambienceSounds = Resources.LoadAll<AudioClip>("Audio/ambience");
    }

    private void loadMeshes()
    {
        foreach (GameObject obj in Resources.LoadAll<GameObject>("obj/weapons").ToList())
        {
            weaponMeshes[obj.name] = obj.GetComponentInChildren<MeshFilter>().sharedMesh;
        };
    }

    /* UTILITY */

    public static GameObject SPAWN(Type entity, Type ai, Vector3 position)
    {
        GameObject spawned = new GameObject();
        spawned.transform.position = position;
        spawned.AddComponent(entity);
        spawned.AddComponent(ai);
        return spawned;
    }

    public static Vector3 RAND_POS_IN_TILE(Hextile tile)
    {
        float outerLimit = Hextile.Radius * 0.8f;
        float innerLimit = Hextile.Radius * 0.2f;
        Vector3 location = tile.transform.position + (AIBehaviour.RandomDirection() * Mathf.Clamp(UnityEngine.Random.value, innerLimit, outerLimit)) + Vector3.up * Hextile.Thickness / 2;
        return location;
    }

    public Vector3 RandomPositionInRandomTileInPlay()
    {
        Hextile randomTile = AllTilesInPlay[UnityEngine.Random.Range(0, AllTilesInPlay.Count)];
        return RAND_POS_IN_TILE(randomTile);
    }


}
