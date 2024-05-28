using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public bool RequiemAchieved = false;

    public Weapon HostWeapon;
    public static Player INSTANCE { get; private set; }
    public int BonesCollected = 0;

    public PlayerHUD HUD;
    public PlayerCamera Cam;
    public PlayerProgression Progression;
    public Keyboard CurrentKeyboard;
    public Mouse CurrentMouse;

    public GameObject CursorIndicator;

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

    public float ChainlinkLength = 0.05f;
    public float ChainlinkWidth = 0.025f;
    private LineRenderer chainRenderer;

    void Start()
    {
        gameObject.name = "Player";
        gameObject.tag = "Player";
        InitPlayerInGame();
        CursorIndicator = Instantiate(Resources.Load<GameObject>("Prefabs/UX/Indicator"));
    }

    void Update()
    {
        //HUD.setIndicatorOnHUD("bones", "Bones: " + BonesCollected.ToString());
        if (HostWeapon)
        {
            updateChainlink();
            HostWeapon.TrueStrikeEnabled = true;
        }
        else if (HostEntity ? HostEntity.MainHand : false)
        {
            HostWeapon = HostEntity.MainHand.GetComponent<Weapon>();
            HostWeapon.playShing();
        }
        if (CurrentKeyboard.pKey.wasPressedThisFrame)
        {
            Requiem.INSTANCE.Paused = !Requiem.INSTANCE.Paused;
        }
        if (!Requiem.INSTANCE.Paused)
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


    /***** PUBLIC *****/

    /***** PROTECTED *****/

    /***** PRIVATE *****/

    private void inputCamera()
    {
        Vector2 mouse_screen_position = CurrentMouse.position.ReadValue();
        Ray mouse_ray = Cam.Eyes.ScreenPointToRay(mouse_screen_position);
        RaycastHit floor_ray_hit;
        if (Physics.Raycast(mouse_ray, out floor_ray_hit, 100, 1 << Requiem.layerTile, QueryTriggerInteraction.Ignore))
        {
            Vector3 mouse_world_position = floor_ray_hit.point;
            mouse_world_position.y = Hextile.Thickness / 2;
            //Debug.Log(mouse_world_position);
            Debug.DrawLine(transform.position, mouse_world_position, Color.red, Time.deltaTime);
            CursorIndicator.transform.position = mouse_world_position;
            Vector3 newLookPosition = (mouse_world_position - transform.position);
            newLookPosition.Scale(new Vector3(1, 0, 1));
            newLookPosition.Normalize();
            HostEntity.LookDirection = newLookPosition;
        }
    }

    private void inputMovement()
    {
        rawInput = new Vector3();
        rawInput.z = (CurrentKeyboard.wKey.isPressed ? 1 : 0) + (CurrentKeyboard.sKey.isPressed ? -1 : 0);
        rawInput.x = (CurrentKeyboard.aKey.isPressed ? -1 : 0) + (CurrentKeyboard.dKey.isPressed ? 1 : 0);
        Direction = rawInput == Vector3.zero ? Vector3.zero : AIBehaviour.angleToVector(AIBehaviour.getAngle(rawInput));
        Direction = Direction.normalized;
        if (Direction != lastDirection)
        {
            HostEntity.WalkDirection = Direction;
            lastDirection = Direction;
        }
        HostEntity.DashCharging = CurrentKeyboard.spaceKey.isPressed && !HostEntity.Dashing;
        if (!HostEntity.Dashing)
        {
            HostEntity.dashDirection = HostEntity.WalkDirection;
        }
    }

    private void inputItemManagement()
    {
        if (CurrentKeyboard.rKey.wasPressedThisFrame)
        {

        }
        if (CurrentKeyboard.fKey.wasPressedThisFrame)
        {
            HostEntity.Interact.Invoke(HostEntity);
        }
        HostEntity.Interacting = CurrentKeyboard.fKey.isPressed;
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
            if (HostWeapon)
            {
                yankWeapon();
            }
            else
            {
                HostEntity.EventAttemptPickup.Invoke(HostEntity);
            }

        }
        else if (CurrentKeyboard.qKey.wasPressedThisFrame)
        {
            if (HostWeapon.Wielder != HostEntity)
            {
                recallWeapon();
            }
        }
        else if (CurrentKeyboard.gKey.wasPressedThisFrame)
        {
            if (HostEntity.MainHand)
            {
                HostEntity.MainHand.DropItem();
            }
        }
        else if (!HostWeapon)
        {

        }
        else if (HostWeapon.Wielder != HostEntity && !HostWeapon.Thrown && !HostWeapon.Telecommuting && !HostEntity.Staggered)
        {
            if((yank_timer += Time.deltaTime) >= yank_delay)
            {
                yank_timer -= yank_delay;
                yankWeapon();
                recallWeapon();
            }

        }
    }
    private float yank_timer = 0;
    private float yank_delay = 0.25f;

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
            throwWep.ThrowTrigger = CurrentKeyboard.qKey.isPressed;
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
        moon.gameObject.transform.localPosition = new Vector3(0, 0.25f, 0f);
        moon.intensity = 2f;
        moon.range = 2f;
        moon.color = Color.Lerp(Color.white, Color.blue, 0.25f);
        moon.bounceIntensity = 0;
        //moon.shadowStrength = 0.5f;
        //moon.shadows = LightShadows.Soft;
        //moon.shadowNearPlane = 0.03f;
        HUD = Instantiate(Resources.Load<GameObject>("Prefabs/UX/HUD")).GetComponent<PlayerHUD>();
        Cam = new GameObject().AddComponent<PlayerCamera>();
        Progression = gameObject.AddComponent<PlayerProgression>();
        CurrentMouse = Mouse.current;
        CurrentKeyboard = Keyboard.current;
        listener = GetComponent<AudioListener>() ? GetComponent<AudioListener>() : gameObject.AddComponent<AudioListener>();
        Faction = Entity.Loyalty.neutral;
        HostEntity = new GameObject().AddComponent<Struggler>();
        HostEntity.transform.position = transform.position;
        HostEntity.requiemPlayer = this;
        HostEntity.FinalDashEnabled = true;
        HostEntity.JustLandedHit.AddListener(Scoreboard.Score_Hit);
        //HostEntity.mortality = Entity.Mortality.vulnerable;
        chainRenderer = Instantiate(Resources.Load<GameObject>("Prefabs/chainRenderer")).GetComponent<LineRenderer>();
        chainRenderer.transform.SetParent(transform);
        chainRenderer.transform.localEulerAngles = Vector3.right * 90;
        chainRenderer.startWidth = ChainlinkWidth;
        chainRenderer.endWidth = ChainlinkWidth;
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
        HUD.Curtains.gameObject.SetActive(false);
        foreach(Transform child in HUD.Curtains.transform)
        {
            child.gameObject.SetActive(false);
        }
        yield return new WaitUntil(() => Dead || RequiemAchieved);
        HUD.Curtains.gameObject.SetActive(true);
        fadeTimer = 0.0f;
        while (fadeTimer < fadePeriod)
        {
            HUD.Curtains.enabled = true;
            HUD.Curtains.color = new Color(0, 0, 0, fadeTimer / fadePeriod);
            HUD.scoreTransforms[1].GetComponent<Text>().fontSize = (int)Mathf.Lerp(54, 162, fadeTimer / fadePeriod);
            fadeTimer += Time.deltaTime;
            yield return null;
        }
        HUD.Curtains.color = new Color(0, 0, 0, 1);
        HUD.scoreTransforms[1].GetComponent<Text>().fontSize = 162;
        if (RequiemAchieved)
        {
            foreach (Transform child in HUD.Curtains.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
        _BlurbService.Toggle_Blurbs(false);

        //Requiem.INSTANCE.Restart();
    }

    private void recallWeapon()
    {
        if (HostWeapon ? HostWeapon.Telecommuting : true)
        {
            return;
        }
        if (HostWeapon.ImpaledObject)
        {
            HostWeapon.ImpaleRelease();
        }
        else
        {
            HostWeapon.playShing();
        }
        if (HostWeapon.Wielder != HostEntity)
        {
            HostWeapon.DropItem();
            Vector3 disposition = HostWeapon.transform.position - transform.position; 
            HostWeapon.transform.LookAt(HostEntity.transform);
            HostWeapon.transform.Rotate(Vector3.Cross(disposition.normalized, Vector3.up), -90, Space.World);
            float recallPeriod = Mathf.Min(disposition.magnitude/4f, 0.4f);
            HostWeapon.Telecommute(HostEntity.gameObject, recallPeriod, x => x.PickupItem(HostEntity));
        }
    }

    private void yankWeapon()
    {
        if (!HostWeapon)
        {

        }
        else if(HostWeapon.Wielder == HostEntity)
        {

        }
        else if (HostEntity.personalBox.bounds.Contains(HostWeapon.transform.position))
        {
            HostWeapon.PickupItem(HostEntity);
        }
        else if (HostWeapon.ImpaledObject)
        {
            Entity impaledFoe = HostWeapon.ImpaledObject.GetComponent<Entity>();
            Wieldable impaledObject = HostWeapon.ImpaledObject.GetComponent<Wieldable>();
            float yankStrength = Mathf.Lerp(Entity.Min_Velocity_Of_Dash, Entity.Max_Velocity_Of_Dash, 1f);
            if (impaledFoe)
            {
                Vector3 disposition = HostWeapon.ImpaledObject.transform.position - transform.position;
                impaledFoe.Shove(-disposition.normalized * Entity.Strength_Ratio(HostEntity, impaledFoe) * yankStrength);
                //impaledFoe.Stagger(Entity.Strength_Ratio(HostEntity, impaledFoe));
            }
            else if (impaledObject)
            {
                Vector3 disposition = HostWeapon.ImpaledObject.transform.position - transform.position;
                impaledObject.DropItem(yeet: true, (Vector3.up / 2) - disposition.normalized, yankStrength);
            }
            else
            {
                Vector3 disposition = HostWeapon.transform.position - transform.position;
                HostEntity.Shove(disposition.normalized * Entity.Strength_Ratio(HostEntity, impaledFoe) * yankStrength);
            }
        }
        else
        {
            Vector3 disposition = transform.position - HostWeapon.transform.position;
            HostWeapon.transform.LookAt(HostEntity.transform);
            HostWeapon.transform.Rotate(Vector3.Cross(-disposition.normalized, Vector3.up), -90, Space.World);
            HostWeapon.DropItem(yeet: true, disposition.normalized + Vector3.up/2, Entity.Min_Velocity_Of_Dash);
        }
    }


    private void updateChainlink()
    {

        if(ChainlinkLength == 0 || HostEntity == null || HostWeapon == null) 
        {
            chainRenderer.gameObject.SetActive(false);
        }
        else
        {
            chainRenderer.gameObject.SetActive(true);
            Vector3 disposition = HostWeapon.transform.position - HostEntity.transform.position;
            Vector3 additiveLinkDisposition = disposition;
            List<Vector3> chainlinkPositions = new List<Vector3>();
            Vector3 newChainlinkPosition = transform.position;
            while (Mathf.Min(additiveLinkDisposition.magnitude, disposition.magnitude) > ChainlinkLength)
            {
                newChainlinkPosition = newChainlinkPosition + disposition.normalized * ChainlinkLength;
                chainlinkPositions.Add(newChainlinkPosition);
                additiveLinkDisposition = HostWeapon.transform.position - newChainlinkPosition;
                if (additiveLinkDisposition.magnitude > disposition.magnitude)
                {
                    break;
                }
            }
            chainRenderer.positionCount = chainlinkPositions.Count;
            chainRenderer.SetPositions(chainlinkPositions.ToArray());
        }
        
    }

}

