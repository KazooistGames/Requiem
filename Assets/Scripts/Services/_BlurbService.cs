using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _BlurbService : MonoBehaviour
{
    public static _BlurbService Instance;


    //blurb system
    public Canvas blurbCanvas;
    public Font blurbFont;
    public int blurbFontSize = 30;
    public int blurbLayer = 5;
    public Camera blurbCamera;
    public float blurbScalar = 1.0f;
    public Vector3 blurbOffset = Vector3.zero;
    private List<(GameObject, GameObject)> activeBlurbs = new List<(GameObject, GameObject)>();
    private Dictionary<GameObject, float> blurbDurations = new Dictionary<GameObject, float>();

    private void Start()
    {
        Instance = this;
        Instance.StartCoroutine(blurbUpdate());
        Instance.blurbCamera = Camera.main;
        Instance.blurbCanvas = new GameObject("blurbCanvas").AddComponent<Canvas>();
        Instance.blurbCanvas.gameObject.AddComponent<CanvasRenderer>();
        Instance.blurbCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Instance.blurbCanvas.worldCamera = Instance.blurbCamera;
    }

    /* BLURBS */
    public static GameObject createBlurb(GameObject speaker, string message, Color color, float duration = 0, float sizeScalar = 1.0f)
    {
        if (Instance)
        {
            GameObject newBlurb = new GameObject(message, typeof(RectTransform), typeof(Text));
            newBlurb.transform.SetParent(Instance.blurbCanvas.transform, false);
            Text tex = newBlurb.GetComponent<Text>();
            tex.text = message;
            tex.color = color;
            tex.font = Instance.blurbFont;
            tex.fontStyle = FontStyle.Bold;
            tex.fontSize = (int)(Instance.blurbFontSize * sizeScalar);
            tex.alignment = TextAnchor.MiddleCenter;
            RectTransform rect = newBlurb.GetComponent<RectTransform>();
            int lengthScalar = (message.Length / 10);
            float fontSizeScalar = (tex.fontSize / Instance.blurbFontSize);
            rect.sizeDelta = new Vector2(150 * (1 + lengthScalar) * fontSizeScalar, 50 * (1 + lengthScalar) * fontSizeScalar);
            //float idealTextPositionOffset = speaker.GetComponent<MeshFilter>() ? speaker.GetComponent<MeshFilter>().mesh.bounds.max.y * speaker.transform.lossyScale.y * 1.20f : 0;
            if (Instance.blurbCamera)
            {
                rect.anchoredPosition = Instance.blurbCamera.WorldToScreenPoint(speaker.transform.position);
                rect.anchoredPosition -= new Vector2(Screen.width / 2, Screen.height / 2);
            }
            Instance.activeBlurbs.Add((newBlurb, speaker));
            if (duration > 0)
            {
                Instance.blurbDurations.Add(newBlurb, duration);
            }
            return newBlurb;
        }
        else
        {
            return null;
        }
    }

    private IEnumerator blurbUpdate()
    {
        float frequency = 0.01f;
        while (true)
        {
            yield return new WaitUntil(() => activeBlurbs.Count > 0 && blurbCamera);
            foreach ((GameObject, GameObject) blurb in activeBlurbs)
            {
                if (blurb.Item2 == null)
                {
                    blurbDurations.Remove(blurb.Item1);
                    Destroy(blurb.Item1);
                }
                else if (blurb.Item1)
                {
                    RectTransform rect = blurb.Item1.GetComponent<RectTransform>();
                    GameObject speaker = blurb.Item2;
                    //float idealTextPositionOffset = speaker.GetComponent<MeshFilter>() ? speaker.GetComponent<MeshFilter>().mesh.bounds.max.y * speaker.transform.lossyScale.y * 1.25f : 0;
                    rect.anchoredPosition = Instance.blurbCamera.WorldToScreenPoint(speaker.transform.position);
                    rect.anchoredPosition -= new Vector2(Screen.width / 2, Screen.height / 2);
                    if (blurbDurations.ContainsKey(blurb.Item1))
                    {
                        if ((blurbDurations[blurb.Item1] -= frequency) <= 0)
                        {
                            blurbDurations.Remove(blurb.Item1);
                            Destroy(blurb.Item1);
                        }
                    }
                }
                else if (blurbDurations.ContainsKey(blurb.Item1))
                {
                    blurbDurations.Remove(blurb.Item1);
                }
            }
            activeBlurbs.RemoveAll(x => !x.Item1 || !x.Item2);
            yield return new WaitForSecondsRealtime(frequency);
        }
    }
}
