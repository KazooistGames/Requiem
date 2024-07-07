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

    public static float GameClock = 0;
    public static bool Paused = false;
    public static float TimeScale;
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

    public static float EnvironmentLightStrobePeriod = 10;
    private Light environmentLight;

    public enum GameState
    {
        Liminal,
        Wave,
        Boss,
        Final,
        Lobby,
    }
    public static GameState StateOfGame = GameState.Liminal;

    void Awake()
    {
        if (INSTANCE)
        {
            Destroy(this);
        }
        else
        {
            INSTANCE = this;
        }
        UnityEngine.Random.InitState((int)DateTime.UtcNow.Ticks);
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
        environmentLight = new GameObject("Directional Light").AddComponent<Light>();
        environmentLight.type = LightType.Directional;
        environmentLight.intensity = 0.4f;
        environmentLight.shadows = LightShadows.None;
        StartCoroutine(GAME_SCRIPT());
    }

    protected virtual void Update()
    {
        if (Player.INSTANCE ? Player.INSTANCE.Dead : false)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Paused)
        {
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            GameClock += Time.deltaTime;
            Time.timeScale = TimeScale;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
        }
        update_environment_light();
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }



    private IEnumerator GAME_SCRIPT()
    {
        yield return null;
        yield return new WaitUntil(() => Map.Commissioned);
        yield return new WaitUntil(() => Rituals.Nemesis);
        Waver.StartWave(10, 5, 2);
    }

    /***** PUBLIC *****/
    public static AudioClip getSound(string path)
    {
        return Resources.Load<AudioClip>(path);
    }

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
        Hextile randomTile = Map.Tiles[UnityEngine.Random.Range(0, Map.Tiles.Count)];
        return RAND_POS_IN_TILE(randomTile);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    /***** PRIVATE *****/

    private void update_environment_light()
    {
        if (!environmentLight) { return; }
        float time = Time.unscaledTime / EnvironmentLightStrobePeriod;
        float red_channel = Mathf.Max(Mathf.Sin(time), Mathf.Cos(time + Mathf.PI / 2));
        float green_channel = Mathf.Max(Mathf.Sin(2*time + Mathf.PI / 2), Mathf.Cos(2 * time + Mathf.PI));
        float blue_channel = Mathf.Max(Mathf.Sin(time + Mathf.PI / 2), Mathf.Cos(time + Mathf.PI));
        environmentLight.color = new Color(red_channel, green_channel, blue_channel);
        float intensity = Mathf.Lerp(0.25f, 0.5f, (Mathf.Sin(time / 4.5f) + 1) / 2);
        environmentLight.intensity = intensity;
        float x_angle = Mathf.Lerp(0f, -60f, (Mathf.Cos(time * 4.5f) + 1) / 2);
        float z_angle = Mathf.Lerp(30f, -30f, (Mathf.Cos(time * 1.5f) + 1) / 2);
        environmentLight.transform.eulerAngles = new Vector3(x_angle, 0, z_angle);
    }


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




}
