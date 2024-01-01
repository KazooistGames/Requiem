using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

public class PlayerHUD : MonoBehaviour
{
    public RawImage Curtains;
    public GameObject Score;
    public GameObject PauseMenu;
    public GameObject Info;
    public GameObject StatBar;
    public GameObject TempoBar;
    public GameObject RageBar;
    public GameObject CenterPopup;
    public GameObject SidePopup;
    public Button QuitButton;
    public Slider MouseSlider;
    public Slider SoundSlider;
    public Dictionary<string, Text> StatusIndicators = new Dictionary<string, Text>();

    public RectTransform[] statBarTransforms;
    public RectTransform[] tempoBarTransforms;
    public RectTransform[] scoreTransforms;

    private void Start()
    {
        CenterPopup.SetActive(false);
        SidePopup.SetActive(false);
        //MainMenuButton.onClick.AddListener(Requiem.Instance.gotoTitle);        
        QuitButton.onClick.AddListener(Requiem.INSTANCE.Restart);
        statBarTransforms = StatBar.GetComponentsInChildren<RectTransform>();
        tempoBarTransforms = TempoBar.GetComponentsInChildren<RectTransform>();
        scoreTransforms = Score.GetComponentsInChildren<RectTransform>();
        tempoBarTransforms[3].sizeDelta = new Vector2(10, 25);
        tempoBarTransforms[3].anchoredPosition = new Vector2(0, -11);        
        tempoBarTransforms[1].sizeDelta = new Vector2(40, 25);
        tempoBarTransforms[1].anchoredPosition = new Vector2(0, -11);
    }
    void Update()
    {
        Player.INSTANCE.mouseSpeedScalar = MouseSlider.value;
        _SoundService.SOUND_MASTER_VOLUME = SoundSlider.value;
        Text[] metrics = Info.GetComponentsInChildren<Text>();
        metrics[0].text = "fps:    " + (1 / Time.smoothDeltaTime).ToString("0");
        //metrics[2].text = "Kills:  " + Player.INSTANCE.BonesCollected.ToString();
        metrics[1].text = "Time:   " + Requiem.INSTANCE.GameClock.ToString("0.00");
        metrics[2].enabled = false;
        updateStatBars();
        updateScore();
        updateIndicators();
        if (Player.INSTANCE.Dead)
        {
            PauseMenu.GetComponent<Text>().text = "Dead..";
            PauseMenu.SetActive(true);
            QuitButton.gameObject.SetActive(Curtains.color.a == 1);
            TempoBar.SetActive(false);
            StatBar.SetActive(false);
            MouseSlider.gameObject.SetActive(false);
            SoundSlider.gameObject.SetActive(false);
            scoreTransforms[2].gameObject.SetActive(false);
            scoreTransforms[3].gameObject.SetActive(false);
        }
        else if (Requiem.INSTANCE.Paused)
        {
            PauseMenu.SetActive(true);
        }
        else
        {
            PauseMenu.SetActive(false);
        }

    }
    /***** PUBLIC *****/
    public Text setIndicatorOnHUD(string key, string text)
    {
        Text indicator;
        if (StatusIndicators.ContainsKey(key))
        {
            indicator = StatusIndicators[key];
        }
        else
        {
            indicator = Instantiate(Resources.Load<GameObject>("Prefabs/UX/statusIndicator")).GetComponent<Text>();
            indicator.text = text;
            StatusIndicators[key] = indicator;
        }
        return indicator;
    }

    /***** PROTECTED *****/
   
    /***** PRIVATE *****/
    private void updateIndicators()
    {
        int i = 0;
        foreach (Text indicator in StatusIndicators.Values)
        {
            indicator.transform.SetParent(transform, false);
            indicator.transform.localScale = Vector3.one;
            RectTransform position = indicator.GetComponent<RectTransform>();
            position.anchorMin = new Vector2(0, 1);
            position.anchorMax = new Vector2(0, 1);
            position.sizeDelta = new Vector2(420, 30);
            position.anchoredPosition = new Vector2(position.sizeDelta.x / 2 + 15, -130 - 30 * i);
            indicator.fontSize = 20;
            indicator.fontStyle = FontStyle.Bold;
            indicator.color = new Color(1, 0.5f, 1 / 3f);
            if (indicator.enabled)
            {
                i++;
            }
        }
    }

    private void updateStatBars()
    {

        if (Player.INSTANCE ? Player.INSTANCE.HostEntity : false)
        {
            statBarTransforms[1].anchorMax = new Vector2(Player.INSTANCE.HostEntity.Poise / Player.INSTANCE.HostEntity.Strength, 1f);
            statBarTransforms[1].GetComponent<Image>().color = new Color(0.6f, 0.5f, 0.3333f);
            statBarTransforms[3].anchorMax = new Vector2(Player.INSTANCE.HostEntity.Vitality / Player.INSTANCE.HostEntity.Strength, 1f);
            statBarTransforms[3].GetComponent<Image>().color = (int)Player.INSTANCE.HostEntity.Posture > -1 ? (Player.INSTANCE.HostEntity.Posture == Entity.PostureStrength.Strong ? new Color(1, 0, 0, 1.0f) : new Color(1, 0, 0, 0.5f)) : new Color(1, 0, 0.75f, 0.5f);
            if (Player.INSTANCE.HostWeapon)
            {
                updateTempoBar();
            }
        }
        else
        {
            statBarTransforms[1].anchorMax = new Vector2(0, 1f);
            statBarTransforms[3].anchorMax = new Vector2(0, 1f);
        }
    }

    private void updateScore()
    {
        scoreTransforms[1].GetComponent<Text>().text = Scoreboard.Score.ToString("0");
        scoreTransforms[2].GetComponent<Text>().text = "x " + Scoreboard.KillMultiplier.ToString("0.00");
        scoreTransforms[3].GetComponent<Text>().text = "Ritual " + Requiem_Arena.INSTANCE.Ritual.ToString();
    }

    private void updateTempoBar()
    {
        TempoBar.SetActive(true);
        if (!Player.INSTANCE.HostWeapon)
        {

        }
        else if (Player.INSTANCE.HostWeapon.Action == Weapon.ActionAnim.StrongWindup || Player.INSTANCE.HostWeapon.Action == Weapon.ActionAnim.StrongCoil)
        {
            fadeTransforms(tempoBarTransforms, 0.75f, 0.1f);
        }
        else if(Player.INSTANCE.HostWeapon.Action != Weapon.ActionAnim.StrongAttack)
        {
            fadeTransforms(tempoBarTransforms, 0, 0.5f);
        }
        tempoBarTransforms[3].anchorMin = new Vector2(Player.INSTANCE.HostWeapon.Tempo, 1f);
        tempoBarTransforms[3].anchorMax = new Vector2(Player.INSTANCE.HostWeapon.Tempo, 1f);
        tempoBarTransforms[1].anchorMin = new Vector2(Player.INSTANCE.HostWeapon.TempoTargetCenter, 1f);
        tempoBarTransforms[1].anchorMax = new Vector2(Player.INSTANCE.HostWeapon.TempoTargetCenter, 1f);
        tempoBarTransforms[1].sizeDelta = new Vector2(Player.INSTANCE.HostWeapon.TempoTargetWidth * tempoBarTransforms[0].sizeDelta.x, 25);
    }

    private void fadeTransforms(Transform[] transforms, float alphaValue, float periodSeconds)
    {
        foreach (Transform transform in transforms)
        {
            Image image = transform.GetComponent<Image>();
            if (image)
            {
                Color color = image.color;
                image.color = new Color(color.r, color.g, color.b, Mathf.MoveTowards(color.a, alphaValue, Time.deltaTime / periodSeconds));
            }
        }
    }


}
