using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;

public class WeaponQuest : Quest
{
    public Type WeaponType;

    void Start()
    {
        Weapon.Weapon_Hit.AddListener(incrementProgress);
        string progressMessage = WeaponType.ToString() + " Damage " + GoalProgress.ToString() + "/" + Goal.ToString();
        MessageBlurb = _BlurbService.createBlurb(gameObject, progressMessage, Color.yellow);
        MessageBlurb.SetActive(false);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        Weapon.Weapon_Hit.RemoveListener(incrementProgress);
    }

    void incrementProgress(Weapon weapon)
    {
        if (WeaponType == null)
        {
            return;
        }
        else if (weapon.GetType() == WeaponType)
        {
            GoalProgress += (int)weapon.Power;
        }
    }

    private IEnumerator weaponQuest(Stage stage, Type weaponType)
    {
        bool completed = false;
        float progress = 0;
        int goal = UnityEngine.Random.Range(750, 1500);

        Weapon.Weapon_Hit.AddListener(incrementProgress);
        string progressMessage = weaponType.ToString() + " Damage " + progress.ToString() + "/" + goal.ToString();
        GameObject messageBlurb = _BlurbService.createBlurb(stage.GateIn.model, progressMessage, Color.yellow);
        messageBlurb.SetActive(false);
        while (!completed)
        {
            completed = progress >= goal;
            if (completed)
            {
                stage.GateIn.OpenDoor();
            }
            else if (stage.GateIn.PlayerAtDoor && !stage.PlayerInStage)
            {
                messageBlurb.SetActive(true);
            }
            else
            {
                messageBlurb.SetActive(false);
            }
            yield return null;
        }
        Weapon.Weapon_Hit.RemoveListener(incrementProgress);
        yield break;
    }


}
