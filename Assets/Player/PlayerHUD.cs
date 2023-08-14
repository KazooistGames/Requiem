using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

public class PlayerHUD : MonoBehaviour
{
    public RawImage Curtains;
    public GameObject PauseMenu;
    public GameObject Info;
    public GameObject StatBar;
    public GameObject TempoBar;
    public GameObject RageBar;
    public GameObject CenterPopup;
    public GameObject SidePopup;
    public GameObject Message;
    public Button MainMenuButton;
    public Slider MouseSlider;
    public Slider SoundSlider;
    public Dictionary<string, Text> StatusIndicators = new Dictionary<string, Text>();

    private RectTransform[] statBar;
    private RectTransform[] tempoBarTransforms;

    private void Start()
    {
        CenterPopup.SetActive(false);
        SidePopup.SetActive(false);
        Message.SetActive(false);
        //MainMenuButton.onClick.AddListener(Requiem.Instance.gotoTitle);        
        MainMenuButton.onClick.AddListener(Game.INSTANCE.QuitGame);
        statBar = StatBar.GetComponentsInChildren<RectTransform>();
        tempoBarTransforms = TempoBar.GetComponentsInChildren<RectTransform>();
        tempoBarTransforms[3].sizeDelta = new Vector2(10, 25);
        tempoBarTransforms[3].anchoredPosition = new Vector2(0, -10);        
        tempoBarTransforms[2].sizeDelta = new Vector2(40, 25);
        tempoBarTransforms[2].anchoredPosition = new Vector2(0, -10);
    }
    void Update()
    {
        Player.INSTANCE.mouseSpeedScalar = MouseSlider.value;
        Mullet.SOUND_MASTER_VOLUME = SoundSlider.value;
        updateInfo();
        updateStats();
        updateTempo();
        updateIndicators();
        if (Game.INSTANCE.Paused)
        {
            PauseMenu.SetActive(true);
        }
        else
        {
            PauseMenu.SetActive(false);
        }
        if (Player.INSTANCE.Dead)
        {
            Message.SetActive(true);
            Message.GetComponent<Text>().text = "Dead..";
        }
    }

    /**********PRIVATE **********/
    private void updateTempo()
    {
        if(Player.INSTANCE ? Player.INSTANCE.HostEntity : false)
        {
            if (Player.INSTANCE.HostEntity.MainHand)
            {
                TempoBar.SetActive(true);
                tempoBarTransforms[3].anchorMin = new Vector2(Player.INSTANCE.HostEntity.Tempo, 1f);
                tempoBarTransforms[3].anchorMax = new Vector2(Player.INSTANCE.HostEntity.Tempo, 1f);
                tempoBarTransforms[2].anchorMin = new Vector2(Player.INSTANCE.HostEntity.TempoTarget, 1f);
                tempoBarTransforms[2].anchorMax = new Vector2(Player.INSTANCE.HostEntity.TempoTarget, 1f);
            }
            else
            {
                TempoBar.SetActive(false);
            }
        }
    }

    private void updateStats()
    {
        if (Player.INSTANCE ? Player.INSTANCE.HostEntity : false)
        {
            //SpiritBar.GetComponentInChildren<Text>().text = Requiem.Instance.Director.Wave.ToString();
            statBar[1].anchorMax = new Vector2(Player.INSTANCE.HostEntity.Poise / Player.INSTANCE.HostEntity.Strength, 1f);
            //statBar[1].GetComponent<Image>().color = Player.INSTANCE.HostEntity.posture >= Entity.Posture.Flow ? new Color(0.7f, 0.5f, 0) : new Color(0.6f, 0.5f, 0.3333f);
            statBar[1].GetComponent<Image>().color = new Color(0.6f, 0.5f, 0.3333f);
            statBar[3].anchorMax = new Vector2(Player.INSTANCE.HostEntity.Vitality / Player.INSTANCE.HostEntity.Strength, 1f);
            statBar[3].GetComponent<Image>().color = (int)Player.INSTANCE.HostEntity.posture > -1 ? (Player.INSTANCE.HostEntity.posture == Character.Posture.Flow ? new Color(1, 0, 0, 1.0f) : new Color(1, 0, 0, 0.5f)) : new Color(1, 0, 0.75f, 0.5f);
        }
        else
        {
            statBar[1].anchorMax = new Vector2(0, 1f);
            statBar[3].anchorMax = new Vector2(0, 1f);
        }
        
    }

    private void updateInfo()
    {
        Text[] metrics = Info.GetComponentsInChildren<Text>();
        metrics[0].text = "fps:    " + (1 / Time.smoothDeltaTime).ToString("0");
        metrics[1].text = "Kills:  " + Game.KillCount.ToString();
        metrics[2].text = "Time:   " + Game.INSTANCE.GameClock.ToString("0.00");
    }

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

    public Text getSetIndicator(string key)
    {
        Text indicator;
        if (StatusIndicators.ContainsKey(key))
        {
            indicator = StatusIndicators[key];
        }
        else
        {
            indicator = Instantiate(Resources.Load<GameObject>("Prefabs/UX/statusIndicator")).GetComponent<Text>();
            indicator.text = key;
            StatusIndicators[key] = indicator;
        }
        return indicator;
    }
   

}
