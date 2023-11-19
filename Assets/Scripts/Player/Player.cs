using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.InputSystem;
using Newtonsoft.Json;

public class Player : MonoBehaviour
{
    public Weapon MyWeapon;
    public static Player INSTANCE { get; private set; }
    public int BonesCollected = 0;

    public PlayerHUD HUD;
    public PlayerCamera Cam;
    public PlayerProgression Progression;
    public Keyboard CurrentKeyboard;
    public Mouse CurrentMouse;

    public bool Dead = false;
    public Entity.Loyalty Faction = Entity.Loyalty.neutral;
    public Entity HostEntity;

    public Vector3 Direction = Vector3.zero;

    public SphereCollider interactBox;
    protected AudioListener listener;
    protected Light moon;

    private Vector3 lastDirection = Vector3.zero;

    private float relocateTimer = 0f;
    private float relocatePeriod = 0.25f;
    private Vector3 relocateOrigin;

    private float weaponSheathTimer = 0f;
    private float weaponSheathPeriod = 0.25f;

    private Vector3 rawInput;
    public float mouseSpeedScalar = 1.0f;

    private Material bloodSplatter;

    void Start()
    {
        gameObject.name = "Player";
        gameObject.tag = "Player";
        InitPlayerInGame();
    }

    void Update()
    {
        //HUD.setIndicatorOnHUD("bones", "Bones: " + BonesCollected.ToString());
        if (HostEntity ? HostEntity.MainHand : false)
        {
            MyWeapon = HostEntity.MainHand.GetComponent<Weapon>();
        }
        if (CurrentKeyboard.escapeKey.wasPressedThisFrame)
        {
            Game.INSTANCE.Paused = !Game.INSTANCE.Paused;
        }
        if (!Game.INSTANCE.Paused)
        {
            if (HostEntity)
            {
                inputCamera();
                inputMovement();
                inputItemControls();
                inputItemManagement();
            }
            else
            {
                Dead = true;
            }
        }
        if (!bloodSplatter)
        {
            bloodSplatter = Resources.Load<Material>("Materials/FX/bloodSplatter");
        }
        Vector4 playerPosition = new Vector4(transform.position.x, transform.position.y, transform.position.z, 1);
        bloodSplatter.SetVector("_PlayerPosition", playerPosition);
    }

    void FixedUpdate()
    {
        if (HostEntity)
        {
            transform.eulerAngles = HostEntity.transform.eulerAngles;
            if (transform.position == HostEntity.transform.position)
            {
                relocateTimer = 0f;
                transform.position = HostEntity.transform.position;
                relocateOrigin = transform.position;
            }
            else
            {
                relocateTimer += Time.fixedDeltaTime;
                float x = 2 * relocateTimer / relocatePeriod;
                x = Mathf.Clamp(x, 0f, 2f);
                float y = (Mathf.Pow(x, 2) - Mathf.Pow(x, 3) / 3);
                float scalar = (y) / 1.33f;
                scalar = Mathf.Clamp(scalar, 0f, 1f);
                transform.position = Vector3.Lerp(relocateOrigin, HostEntity.transform.position, scalar);
            }
        }
    }

    private void inputCamera()
    {

        float differenceCap = 0f;
        Vector2 cameraFlat = new Vector2(Cam.transform.forward.x, Cam.transform.forward.z).normalized;
        Vector2 hostFlat = new Vector2(HostEntity.LookDirection.x, HostEntity.LookDirection.z).normalized;
        float angleDifference = Vector2.SignedAngle(cameraFlat, hostFlat);
        float spinSpeed = Mathf.Abs(angleDifference) >= differenceCap ? 60 : 30;
        float hostScalar = Mathf.Lerp(HostEntity.TurnSpeed / Entity.DefaultTurnSpeed, 1.0f, 0.25f);
        float increment = Time.deltaTime * spinSpeed * hostScalar * mouseSpeedScalar;
        bool aligning = Mathf.Sign(CurrentMouse.delta.ReadValue().x) == Mathf.Sign(angleDifference);
        Cam.VerticalAngle -= (increment / 3 * CurrentMouse.delta.ReadValue().y);
        Cam.Eyes.fieldOfView -= (CurrentMouse.scroll.ReadValue().y / 360) * Cam.ZoomSensitivity;
        if (differenceCap == 0)
        {

            HostEntity.LookDirection = AIBehaviour.angleToDirection(AIBehaviour.getAngle(HostEntity.LookDirection) - increment * CurrentMouse.delta.ReadValue().x);
            Cam.HorizonatalOffsetAngle += angleDifference / 5;
        }
        else
        {
            if (Mathf.Abs(angleDifference) < differenceCap || aligning)
            {
                HostEntity.LookDirection = AIBehaviour.angleToDirection(AIBehaviour.getAngle(HostEntity.LookDirection) - increment * CurrentMouse.delta.ReadValue().x);
            }
            if (Mathf.Abs(angleDifference) >= differenceCap && !aligning)
            {
                Cam.HorizonatalOffsetAngle -= spinSpeed * Time.deltaTime * CurrentMouse.delta.ReadValue().x;
            }
            if (Mathf.Abs(angleDifference) > differenceCap * 2)
            {
                Cam.HorizonatalOffsetAngle += angleDifference / 2;
            }
        }
    }

    private void inputMovement()
    {
        rawInput = new Vector3();
        rawInput.z = (CurrentKeyboard.wKey.isPressed ? 1 : 0) + (CurrentKeyboard.sKey.isPressed ? -1 : 0);
        rawInput.x = (CurrentKeyboard.aKey.isPressed ? -1 : 0) + (CurrentKeyboard.dKey.isPressed ? 1 : 0);
        Direction = rawInput == Vector3.zero ? Vector3.zero : AIBehaviour.angleToDirection(AIBehaviour.getAngle(rawInput) + 90 + Cam.HorizonatalOffsetAngle);
        Direction = Direction.normalized;
        if (Direction != lastDirection)
        {
            HostEntity.WalkDirection = Direction;
            lastDirection = Direction;
        }
        HostEntity.DashCharging = CurrentKeyboard.spaceKey.isPressed && !HostEntity.Dashing;
        HostEntity.dashDirection = HostEntity.WalkDirection;
    }

    private void inputItemManagement()
    {
        if (CurrentKeyboard.rKey.wasPressedThisFrame)
        {

        }
        if (CurrentKeyboard.fKey.wasPressedThisFrame)
        {
            HostEntity.EventAttemptInteraction.Invoke(HostEntity);
        }
        if (CurrentKeyboard.tabKey.isPressed || CurrentKeyboard.tabKey.wasReleasedThisFrame) //cycle equipped item
        {
            bool released = CurrentKeyboard.tabKey.wasReleasedThisFrame;
            switch (HostEntity.wieldMode)
            {
                case Entity.WieldMode.EmptyHanded:
                    if (released && weaponSheathTimer < weaponSheathPeriod)
                    {
                        if (HostEntity.leftStorage || HostEntity.rightStorage)
                        {
                            HostEntity.wieldMode = Entity.WieldMode.OneHanders;
                        }
                        else if (HostEntity.backStorage)
                        {
                            HostEntity.wieldMode = Entity.WieldMode.TwoHanders;
                        }
                    }

                    break;
                case Entity.WieldMode.OneHanders:
                    if (HostEntity.backStorage && released)
                    {
                        HostEntity.wieldMode = Entity.WieldMode.TwoHanders;
                    }
                    else if (weaponSheathTimer >= weaponSheathPeriod)
                    {
                        HostEntity.wieldMode = Entity.WieldMode.EmptyHanded;
                    }
                    break;
                case Entity.WieldMode.TwoHanders:
                    if ((HostEntity.leftStorage || HostEntity.rightStorage) && released)
                    {
                        HostEntity.wieldMode = Entity.WieldMode.OneHanders;
                    }
                    else if (weaponSheathTimer >= weaponSheathPeriod)
                    {
                        HostEntity.wieldMode = Entity.WieldMode.EmptyHanded;
                    }
                    break;
            }
            weaponSheathTimer = released ? 0 : weaponSheathTimer + Time.deltaTime;
        }
        else if (CurrentKeyboard.eKey.wasPressedThisFrame)
        {
            HostEntity.EventAttemptPickup.Invoke(HostEntity);
            HostEntity.EventAttemptPickup.RemoveAllListeners();
        }
        else if (CurrentKeyboard.qKey.wasPressedThisFrame)
        {
            if (HostEntity.MainHand)
            {
                HostEntity.MainHand.DropItem();
            }
        }
    }

    private void inputItemControls()
    {
        if (HostEntity.MainHand || HostEntity.OffHand)
        {
            Wieldable mainWep = null;
            Wieldable offWep = null;
            if (HostEntity.MainHand)
            {
                mainWep = HostEntity.MainHand;
                mainWep.PrimaryTrigger = CurrentMouse.leftButton.isPressed;
                mainWep.SecondaryTrigger = CurrentKeyboard.leftShiftKey.isPressed;
                mainWep.TertiaryTrigger = CurrentMouse.rightButton.isPressed;
            }
            if (HostEntity.OffHand)
            {
                offWep = HostEntity.OffHand;
                offWep.PrimaryTrigger = CurrentMouse.rightButton.isPressed;
                offWep.SecondaryTrigger = CurrentKeyboard.leftShiftKey.isPressed;
            }
            Wieldable throwWep = HostEntity.OffHand ? HostEntity.OffHand : HostEntity.MainHand;
            throwWep.ThrowTrigger = CurrentKeyboard.leftCtrlKey.isPressed;
        }

    }

    private void InitPlayerInGame()
    {
        INSTANCE = this;   
        interactBox = GetComponent<SphereCollider>() ? GetComponent<SphereCollider>() : gameObject.AddComponent<SphereCollider>();
        interactBox.radius = 0.3f;
        interactBox.isTrigger = true;
        moon = new GameObject("moon").AddComponent<Light>();
        moon.transform.SetParent(transform);
        moon.gameObject.transform.localPosition = new Vector3(0, 0.20f, 0.10f);
        moon.intensity = 1f;
        moon.range = 3f;
        moon.color = Color.Lerp(Color.white, Color.blue, 0.25f);
        moon.bounceIntensity = 0;
        HUD = Instantiate(Resources.Load<GameObject>("Prefabs/UX/HUD")).GetComponent<PlayerHUD>();
        Cam = new GameObject().AddComponent<PlayerCamera>();
        Progression = gameObject.AddComponent<PlayerProgression>();
        CurrentMouse = Mouse.current;
        CurrentKeyboard = Keyboard.current;
        listener = GetComponent<AudioListener>() ? GetComponent<AudioListener>() : gameObject.AddComponent<AudioListener>();
        Faction = Entity.Loyalty.hostile;
        HostEntity = new GameObject().AddComponent<Struggler>();
        HostEntity.transform.position = transform.position;
        HostEntity.requiemPlayer = this;
        HostEntity.FinalDashEnabled = true;
        StartCoroutine(FadeCurtains());
    }

    private IEnumerator FadeCurtains()
    {
        yield return null;
        HostEntity.flames.gameObject.SetActive(true);
        HUD.Curtains.enabled = true;
        HUD.Curtains.gameObject.SetActive(true);
        float fadeTimer = 0f;
        float fadePeriod = 5f;
        yield return new WaitUntil(() => Hextile.LastGeneratedTile);
        while (fadeTimer < fadePeriod)
        {
            HUD.Curtains.color = new Color(0, 0, 0, 1 - fadeTimer / fadePeriod);
            fadeTimer += Time.deltaTime;
            yield return null;
        }
        HUD.Curtains.enabled = false;
        yield return new WaitUntil(() => Dead);
        fadeTimer = 0.0f;
        while (fadeTimer < fadePeriod)
        {
            HUD.Curtains.enabled = true;
            HUD.Curtains.color = new Color(0, 0, 0, fadeTimer / fadePeriod);
            fadeTimer += Time.deltaTime;
            yield return null;
        }
        Game.INSTANCE.Restart();
    }

}

