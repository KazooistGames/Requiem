using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

[ExecuteInEditMode]
public class PlayerCamera : MonoBehaviour
{
    public Camera Eyes;
    public bool LockPosition = true;

    public float CamRepoSpeed = 0.1f;
    public Vector3 camOffsetActual = new Vector3(3, 3, 0);
    public float VerticalAngle = 30f;
    public float VerticalAngleMin = 30f;
    public float VerticalAngleMax = 45f;
    public float HorizonatalOffsetAngle = 0;

    public float camOffsetMax = 1.0f;
    public float camOffsetMin = 0.75f;
    public float CamOffsetMag = 0.75f;
    public float CamHeight = 0.5f;
    public float CamHeightMin = 1f;
    public float CamHeightMax = 1.0f;
    public float minFOV = 50f;
    public float maxFOV = 70f;
    public float minOrthoSize = 0.25f;
    public float maxOrthoSize = 1.25f;
    public float ZoomSensitivity = 3;

    public bool Orthographic = false;

    public int RenderResolutionPixelWidth = 420;
    private int pixelsHigh = 240;


    void Awake()
    {
        camOffsetActual = new Vector3(CamOffsetMag * Mathf.Cos(DegToRad(VerticalAngle)), CamOffsetMag * Mathf.Sin(DegToRad(VerticalAngle)), 0);
    }

    void Start()
    {
        gameObject.name = "PlayerCamera";
        Eyes = GetComponent<Camera>() ? GetComponent<Camera>() : gameObject.AddComponent<Camera>();
        Eyes.cullingMask = ~(1 << Requiem.layerInvisible); 
        Eyes.clearFlags = CameraClearFlags.Color;
        Eyes.backgroundColor = Color.black;
        Eyes.nearClipPlane = 0.01f;
        Eyes.farClipPlane = 10f;
        Eyes.fieldOfView = 0;
        _BlurbService.Instance.blurbCamera = Eyes;
    }

 
    void Update()
    {
        float ratio = (float)Eyes.pixelHeight / (float)Eyes.pixelWidth;
        RenderResolutionPixelWidth = Mathf.Max(RenderResolutionPixelWidth, 1);
        pixelsHigh = Mathf.RoundToInt(RenderResolutionPixelWidth * ratio);
        Eyes.tag = "MainCamera";
        Eyes.fieldOfView = Mathf.Clamp(Eyes.fieldOfView, minFOV, maxFOV);
    }

    void FixedUpdate()
    {
        if (Player.INSTANCE ? Player.INSTANCE.HostEntity : false)
        {
            VerticalAngle = Mathf.Clamp(VerticalAngle, VerticalAngleMin, VerticalAngleMax);
            float scale = (VerticalAngle - VerticalAngleMin) / (VerticalAngleMax - VerticalAngleMin);
            CamOffsetMag = Mathf.Lerp(camOffsetMin, camOffsetMax, scale);
            CamHeight = Mathf.Lerp(CamHeightMin, CamHeightMax, scale);
            Vector3 CameraFocus = Player.INSTANCE.HostEntity.transform.position + (Vector3.up * Entity.Scale * CamHeight);
            float leg = CamOffsetMag * Mathf.Cos(DegToRad(VerticalAngle));
            camOffsetActual = new Vector3(Mathf.Cos(DegToRad(HorizonatalOffsetAngle)) * leg, CamOffsetMag * Mathf.Sin(DegToRad(VerticalAngle)), Mathf.Sin(DegToRad(HorizonatalOffsetAngle)) * leg);
            transform.position = Vector3.MoveTowards(transform.position, CameraFocus + camOffsetActual, CamRepoSpeed);
            transform.LookAt(CameraFocus);
            Vector3 temp = transform.localEulerAngles;
            temp.x = temp.x * 0.95f;
            transform.localEulerAngles = temp;
        }
        _BlurbService.Instance.blurbScalar = 2 - Mathf.Exp(-CamOffsetMag / camOffsetMin);
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

    private float DegToRad(float degrees)
    {
        return degrees * (Mathf.PI / 180);
    }

}
