using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

[ExecuteInEditMode]
public class PlayerCamera : MonoBehaviour
{
    public Camera Eyes;
    public bool LockPosition = true;

    public Vector2 FOVSpan = new Vector2(15, 45);

    public int RenderResolutionPixelWidth = 480;
    private int pixelsHigh = 240;


    void Start()
    {
        gameObject.name = "PlayerCamera";
        Eyes = GetComponent<Camera>() ? GetComponent<Camera>() : gameObject.AddComponent<Camera>();
        Eyes.cullingMask = ~(1 << Requiem.layerInvisible); 
        Eyes.clearFlags = CameraClearFlags.Color;
        Eyes.backgroundColor = Color.black;
        Eyes.nearClipPlane = 0.01f;
        Eyes.farClipPlane = 100f;
        Eyes.fieldOfView = 0;
        _BlurbService.INSTANCE.blurbCamera = Eyes;
        transform.position = new Vector3(0, 10.1f, -1.8f);
        transform.eulerAngles = new Vector3(80f, 0, 0);
        Eyes.fieldOfView = 30;
    }

 
    void Update()
    {
        float ratio = (float)Eyes.pixelHeight / (float)Eyes.pixelWidth;
        RenderResolutionPixelWidth = Mathf.Max(RenderResolutionPixelWidth, 1);
        pixelsHigh = Mathf.RoundToInt(RenderResolutionPixelWidth * ratio);
        Eyes.tag = "MainCamera";
        Eyes.fieldOfView = Mathf.Clamp(Eyes.fieldOfView, FOVSpan.x, FOVSpan.y);
    }

    public float SpeedScalar = 2;
    void FixedUpdate()
    {
        Vector3 disposition = triangulateOptimalPosition() - transform.position;
        transform.position = Vector3.MoveTowards(transform.position, triangulateOptimalPosition(), disposition.magnitude * SpeedScalar * Time.fixedDeltaTime);

    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        source.filterMode = FilterMode.Point;
        RenderTexture buffer = RenderTexture.GetTemporary(RenderResolutionPixelWidth, pixelsHigh, -1);
        buffer.filterMode = FilterMode.Point;
        Graphics.Blit(source, buffer);
        Graphics.Blit(buffer, destination);
        RenderTexture.ReleaseTemporary(buffer);
    }

    public float MaxRange = 5;
    private Vector3 triangulateOptimalPosition()
    {
        Vector3 transversal_offset = calc_transversal_offset();
        Vector3 vertical_offset = Vector3.up * calc_vertical_offset();

        return transversal_offset + vertical_offset;
    }


    public Vector2 YSpan = new Vector2(5, 15);
    public float MaxDistance = 5;
    private float calc_vertical_offset()
    {
        float return_value = Mathf.Lerp(YSpan.x, YSpan.y, get_cursor_disposition() / MaxDistance);
        return Mathf.Clamp(return_value, YSpan.x, YSpan.y);
    }

    public Vector2 ZSpan = new Vector2(-0.5f, -1.5f);
    public Vector2 homingSpan = new Vector2(0.20f, 0.5f);
    private Vector3 calc_transversal_offset()
    {
        float player_position_weight = 2;
        float cursor_position_weight = 1;
        float total_weight = player_position_weight + cursor_position_weight;

        Vector3 weighted_player_position = Player.INSTANCE.transform.position * player_position_weight;
        Vector3 weighted_cursor_position = Player.INSTANCE.CursorIndicator.transform.position * cursor_position_weight;
        Vector3 unhomed_position = (weighted_player_position + weighted_cursor_position) / total_weight;

        float homing_weight = Mathf.Lerp(homingSpan.x, homingSpan.y, get_cursor_disposition() / MaxRange);
        Vector3 pullback_offset = Vector3.forward * Mathf.Lerp(ZSpan.x, ZSpan.y, get_cursor_disposition() / MaxRange);

        return Vector3.Lerp(unhomed_position, Vector3.zero, homing_weight) + pullback_offset;
    }

    private float get_cursor_disposition()
    {
        if (!Player.INSTANCE) { return 0; }
        return (Player.INSTANCE.transform.position - Player.INSTANCE.CursorIndicator.transform.position).magnitude;
    }
}
