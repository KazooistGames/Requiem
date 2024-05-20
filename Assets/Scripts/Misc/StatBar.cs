using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    public Entity Host;
    private Canvas canvas;
    private RectTransform[] transforms;

    void Start()
    {
        canvas = GetComponent<Canvas>();
        transforms = GetComponentsInChildren<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        canvas.enabled = Host ? Host.Foe && !Host.requiemPlayer : false;
        if (Host && Player.INSTANCE)
        {
            transform.localScale = (Vector3.one + Vector3.one * Host.Strength/100f)/2;
            canvas.worldCamera = Player.INSTANCE.Cam.Eyes;
            updateFill();
            updateBorder();
            GetComponent<RectTransform>().sizeDelta = new Vector2(1.0f, 0.1f) * Mathf.Pow(Player.INSTANCE.Cam.Eyes.orthographicSize + 0.25f, 1.5f);
            //transform.eulerAngles = new Vector3(-45f, -Player.INSTANCE.Cam.HorizonatalOffsetAngle, 0f);
            transform.localPosition = Vector3.up * Entity.Height * Host.heightScalar;       
        }
        else
        {
            Host = transform.parent.GetComponent<Entity>();
        }

    }



    /***** PRIVATE *****/

    private void updateFill()
    {
        float hp = Host.Vitality / Host.Strength;
        RectTransform fill = transforms[3];
        fill.anchoredPosition = new Vector2((1 - hp) / 2, fill.anchoredPosition.y);
        fill.sizeDelta = new Vector2(hp, fill.sizeDelta.y);
        float alphaValue = Host.BleedingWounds.Count > 0 ? 0.25f : 0.5f;
        fill.GetComponent<Image>().color = (int)Host.Posture > -1 ? new Color(1, 0, 0, alphaValue) : new Color(1, 0, 0.75f, alphaValue);
    }

    private void updateBorder()
    {
        RectTransform border = transforms[1];
        RectTransform backdrop = transforms[2];
        float poiseMeter = Host.Poise / Host.Strength;
        border.anchoredPosition = new Vector2((1 - poiseMeter) / 2, border.anchoredPosition.y);
        border.sizeDelta = new Vector2(poiseMeter, border.sizeDelta.y);
        //border.GetComponent<Image>().color = Host.posture >= Entity.Posture.Flow ? new Color(0.7f, 0.5f, 0) : new Color(0.6f, 0.5f, 0.3333f);
        border.GetComponent<Image>().color = new Color(0.6f, 0.5f, 0.3333f);
    }
}
