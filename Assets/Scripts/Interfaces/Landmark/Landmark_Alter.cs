using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;

public class Landmark_Alter : Landmark
{
    public GameObject DesiredOffering;
    public GameObject TopStep;
    public GameObject Podium;

    public static int StepCount = 3;
    public static float StepHeight = 0.0175f;
    public static float AlterToTileRatio = 0.5f;
    public static float StepIngressRatio = 0.8f;
    public static float podiumScalar = 12;
    public float RitualDurationSeconds = 5;

    private List<GameObject> steps = new List<GameObject>();
    public CapsuleCollider OfferingBox;

    LineRenderer PentagramLines;
    public Color PentagramLineColor = Color.red;
    _Flames PentagramFlames;
    public _Flames.FlameStyles PentagramFlameStyle = _Flames.FlameStyles.Soulless;

    private void Update()
    {
        if (DesiredOffering)
        {
            Energized = OfferingBox.bounds.Contains(DesiredOffering.transform.position);
        }
        PentagramFlames.FlamePresentationStyle = PentagramFlameStyle;
        //else if(Player.INSTANCE)
        //{
        //    Energized = offeringDetectionCollider.bounds.Contains(Player.INSTANCE.HostEntity.transform.position) && Player.INSTANCE.HostEntity.Interacting;
        //}
    }


    /***** PUBLIC *****/
    public override void AssignToTile(Hextile tile)
    {
        base.AssignToTile(tile);
        gameObject.name = "Alter";
        gameObject.layer = Requiem.layerTile;
        GameObject lastStep = gameObject;
        for (int i = 0; i < StepCount; i++)
        {
            float ingress = i == 0 ? AlterToTileRatio : StepIngressRatio;
            GameObject step = new GameObject("alterStep");
            step.transform.SetParent(lastStep.transform);
            step.AddComponent<MeshFilter>().mesh = Hextile.HexagonMesh;
            step.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Tiles/Marble");
            step.AddComponent<MeshCollider>().convex = true;
            step.GetComponent<MeshCollider>().sharedMesh = Hextile.HexagonMesh;
            step.transform.localScale = new Vector3(ingress, 1.0f, ingress);
            step.transform.position = lastStep.transform.position + new Vector3(0.0f, StepHeight, 0.0f);
            step.layer = Requiem.layerTile;
            lastStep = step;
            steps.Add(step);
        }
        TopStep = lastStep;
        //create podium for idol
        //createCenterPodium();
        OfferingBox = gameObject.AddComponent<CapsuleCollider>();
        OfferingBox.isTrigger = true;
        OfferingBox.radius = Hextile.Radius * AlterToTileRatio * Mathf.Pow(StepIngressRatio, StepCount);
        OfferingBox.height = Hextile.Thickness * 2 + StepHeight*StepCount;
        setupRitual();
        StartCoroutine(RitualCycler());
    }

    public void Consume(GameObject offering)
    {
        if (OfferingBox.bounds.Contains(offering.transform.position))
        {
            Destroy(offering);
        }
    }

    /***** PROTECTED *****/

    /***** PRIVATE *****/

    private void createCenterPodium()
    {
        Podium = new GameObject("alterPodium");
        Podium.transform.SetParent(TopStep.transform);
        Podium.AddComponent<MeshFilter>().mesh = Hextile.HexagonMesh;
        Podium.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Tiles/Marble");
        Podium.AddComponent<MeshCollider>().convex = true;
        Podium.GetComponent<MeshCollider>().sharedMesh = Hextile.HexagonMesh;
        Podium.transform.localScale = new Vector3(StepIngressRatio / podiumScalar, 1.0f, StepIngressRatio / podiumScalar);
        Podium.transform.position = TopStep.transform.position + new Vector3(0.0f, StepHeight * podiumScalar, 0.0f);
        Podium.layer = Requiem.layerObstacle;
    }

    private IEnumerator RitualCycler()
    {
        float pentagramRadius = (Hextile.Radius * 2) * AlterToTileRatio * Mathf.Pow(StepIngressRatio, StepCount-1);
        float pentagramHeight = (Hextile.Thickness / 2) + (StepCount + 1) * StepHeight;
        Vector3 flameScale = new Vector3(0.05f, 0.1f, 0.05f);    
        List<Vector3> points = new List<Vector3>()
        {
            AIBehaviour.angleToVector(54) * 0.4f * pentagramRadius,
            AIBehaviour.angleToVector(270) * 0.4f * pentagramRadius,
            AIBehaviour.angleToVector(126) * 0.4f * pentagramRadius,
            AIBehaviour.angleToVector(342) * 0.4f * pentagramRadius,
            AIBehaviour.angleToVector(198) * 0.4f * pentagramRadius,
        };
        yield return null;
        while (true)
        {
            float drawTimer = 0.0f;
            float timerRatio;
            Used = false;
            PentagramFlames.shapeModule.position = Vector3.zero + Vector3.up * pentagramHeight;
            PentagramFlames.emissionModule.enabled = true;
            yield return new WaitUntil(() => Energized);
            PentagramFlames.emissionModule.enabled = true;
            PentagramFlames.shapeModule.position = points[0];
            while (drawTimer < RitualDurationSeconds)
            {
                PentagramLines.enabled = true;
                if (Energized)
                {
                    PentagramLines.startColor = PentagramLineColor;
                    PentagramLines.endColor = PentagramLineColor;
                }
                timerRatio = drawTimer / RitualDurationSeconds;
                List<Vector3> positions = new List<Vector3>();
                positions.Add(points[0] + Vector3.up * pentagramHeight);
                Vector3 lastPoint = points[0];
                for (float i = 0.0f; i < timerRatio; i += 0.2f)
                {
                    Vector3 newPoint = i < 0.8f ? points[1 + (int)(i / 0.2f)] : points[0];
                    float temp = timerRatio - 0.2f >= i ? 1 : (timerRatio % 0.2f) / 0.2f;
                    positions.Add(Vector3.Lerp(lastPoint, newPoint, temp) + Vector3.up * pentagramHeight);
                    lastPoint = newPoint;
                }
                PentagramLines.positionCount = positions.Count;
                PentagramLines.SetPositions(positions.ToArray());
                PentagramFlames.shapeModule.position = drawTimer == 0 ? Vector3.zero + Vector3.up * pentagramHeight : positions[positions.Count - 1];
                drawTimer += Energized ? Time.deltaTime : -Time.deltaTime;
                drawTimer = Mathf.Clamp(drawTimer, 0, RitualDurationSeconds);
                yield return null;
            }
            PentagramFlames.shapeModule.position = Vector3.zero + Vector3.up * pentagramHeight;
            PentagramFlames.shapeModule.radius = 0.75f;
            Used = true;
            Color latchedColor = PentagramLineColor;
            yield return new WaitWhile(() => Energized);
            PentagramFlames.emissionModule.enabled = false;
            PentagramFlames.shapeModule.radius = 0.0f;
            Used = false;
            float fadeOutTimer = 0;
            float fadeOutPeriod = 3;
            float ratio;
            Color fadedColor = latchedColor;
            fadedColor.a = 0;
            while (fadeOutTimer < fadeOutPeriod)
            {
                ratio = fadeOutTimer / fadeOutPeriod;
                //PentagramLines.startColor = new Color(1.0f, 0.0f, 0.0f, Mathf.Lerp(0.75f, 0.0f, ratio));
                //PentagramLines.endColor = new Color(1.0f, 0.0f, 0.0f, Mathf.Lerp(0.75f, 0.0f, ratio));
                PentagramLines.startColor = Color.Lerp(PentagramLineColor, fadedColor, ratio);
                PentagramLines.endColor = Color.Lerp(PentagramLineColor, fadedColor, ratio);
                fadeOutTimer += Time.deltaTime;
                yield return null;
            }
        }       
    }

    private void setupRitual()
    {
        PentagramFlames = Instantiate(Requiem.SpiritFlameTemplate).GetComponent<_Flames>();
        PentagramFlames.transform.SetParent(Tile.gameObject.transform, false);
        PentagramFlames.shapeModule.shapeType = ParticleSystemShapeType.Donut;
        PentagramFlames.shapeModule.scale = new Vector3(0.1f, 0.1f, 0.1f);
        PentagramFlames.shapeModule.radius = 0.0f;
        PentagramFlames.shapeModule.donutRadius = 0.1f;
        PentagramFlames.SetFlameStyle(_Flames.FlameStyles.Soulless);
        PentagramFlames.particleLight.intensity = 0.75f;
        PentagramFlames.shapeModule.rotation = Vector3.right * 90;
        PentagramLines = gameObject.AddComponent<LineRenderer>();
        PentagramLines.enabled = false;
        PentagramLines.useWorldSpace = false;
        PentagramLines.material = Resources.Load<Material>("Materials/FX/LineRenderer");
        PentagramLines.widthMultiplier = 0.015f;
    }
}
