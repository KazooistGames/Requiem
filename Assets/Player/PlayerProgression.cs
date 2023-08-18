using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Events;

public class PlayerProgression : MonoBehaviour
{
    public bool AwaitingSelection = false;

    public delegate void Powerup();
    public Dictionary<string, (string, UnityAction)> PowerupList = new Dictionary<string, (string, UnityAction)>();

    void Start()
    {
        PowerupList = new Dictionary<string, (string, UnityAction)>()
        {
            ["Second Wind"] = (
            "Restore 100% Health",
            () => {
                Player.INSTANCE.HostEntity.Vitality = Player.INSTANCE.HostEntity.Strength;
            }),
            ["Strength"] = (
            BuffDescriptionBuilder("Strength Boost", Player.INSTANCE.HostEntity.Strength-100, 10, "%"),
            () => {
                Player.INSTANCE.HostEntity.Strength *= 1.1f; Player.INSTANCE.HostEntity.Vitality *= 1.1f; Player.INSTANCE.HostEntity.Special *= 1.1f; ;
                PowerupList["Strength"] = (
                BuffDescriptionBuilder("Strength Boost", Player.INSTANCE.HostEntity.Strength-100, 10, "%"),
                PowerupList["Strength"].Item2);
                }
            ),
            ["Resolve"] = (
            BuffDescriptionBuilder("Resolve", Player.INSTANCE.HostEntity.Resolve, 10, "%"),
            () => {
                Player.INSTANCE.HostEntity.Resolve *= 1.1f;
                PowerupList["Resolve"] = (
                BuffDescriptionBuilder("Resolve", Player.INSTANCE.HostEntity.Resolve, 10, "%"),
                PowerupList["Resolve"].Item2);
                }
            ),
            ["Haste"] = (
            BuffDescriptionBuilder("Buff", Player.INSTANCE.HostEntity.Haste, 0.5f, "") +
            "Improves movement speed\nImproves dash charge speed\nImproves energy recovery",
            () => { 
                Player.INSTANCE.HostEntity.Haste += 0.5f;
                PowerupList["Haste"] = (
                BuffDescriptionBuilder("Buff", Player.INSTANCE.HostEntity.Haste, 0.5f, "") +
                "Improves movement speed\nImproves dash charge speed\nImproves energy recovery",
                PowerupList["Haste"].Item2);              
                }
            ),

        };
        StartCoroutine(progressionRoutine());
    }

    void Update()
    {
        if (AwaitingSelection)
        {
            Game.INSTANCE.Paused = true;
        }
        Player.INSTANCE.HUD.CenterPopup.SetActive(AwaitingSelection);
    }

    /************** talent functions *************/
    private IEnumerator progressionRoutine()
    {
        yield return null;
        int levelsAccountedFor = Player.INSTANCE.HostEntity.Lvl;
        while (Player.INSTANCE ? Player.INSTANCE.HostEntity : false)
        {
            yield return new WaitUntil(() => Player.INSTANCE.HostEntity ? Player.INSTANCE.HostEntity.Lvl != levelsAccountedFor : true);
            if (Player.INSTANCE.HostEntity)
            {
                AwaitingSelection = true;
                seedChoices();
                yield return new WaitWhile(() => AwaitingSelection);
                levelsAccountedFor++;
            }
            yield return null;
        }
        yield break;
    }

    private string BuffDescriptionBuilder(string nameOfAttribute, float oldValue, float deltaValue, string units = "")
    {
        string result = nameOfAttribute + " " + oldValue.ToString() + units + " >>> " + (oldValue + deltaValue).ToString() + units + "\n";
        return result;
    }


    private void seedChoices()
    {
        List<Text> texts = new List<Text>();
        Player.INSTANCE.HUD.CenterPopup.GetComponentsInChildren(true, texts);
        List<Button> buttons = new List<Button>();
        Player.INSTANCE.HUD.CenterPopup.GetComponentsInChildren(true, buttons);
        List<string> alreadySeeded = new List<string>();
        for(int i = 0; i < buttons.Count; i++)
        {
            Button button = buttons[i];
            button.onClick.RemoveAllListeners();

            string randomPowerup;
            do
            {
                randomPowerup = PowerupList.Keys.ToList()[UnityEngine.Random.Range(0, PowerupList.Keys.Count)];
            }while (alreadySeeded.Contains(randomPowerup));
            alreadySeeded.Add(randomPowerup);
            button.onClick.AddListener(PowerupList[randomPowerup].Item2);
            button.onClick.AddListener(getPowerup);
            texts[i * 2].text = randomPowerup;
            texts[(i * 2) + 1].text = PowerupList[randomPowerup].Item1;
        }
    }

    private void getPowerup()
    {
        AwaitingSelection = false;
        Game.INSTANCE.Paused = false;
    }

}
