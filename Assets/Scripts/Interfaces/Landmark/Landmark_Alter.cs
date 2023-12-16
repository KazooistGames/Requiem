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
    public static float StepHeight = 0.015f;
    public static float AlterToTileRatio = 0.5f;
    public static float StepIngressRatio = 0.85f;
    public static float podiumScalar = 12;
    public float RitualDurationSeconds = 5;

    private List<GameObject> steps = new List<GameObject>();
    private CapsuleCollider offeringDetectionCollider;

    private void Update()
    {
        if (DesiredOffering)
        {
            Energized = offeringDetectionCollider.bounds.Contains(DesiredOffering.transform.position);
        }
        //else if(Player.INSTANCE)
        //{
        //    Energized = offeringDetectionCollider.bounds.Contains(Player.INSTANCE.HostEntity.transform.position) && Player.INSTANCE.HostEntity.Interacting;
        //}
    }


    /******** alter functions ***********/
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

        offeringDetectionCollider = gameObject.AddComponent<CapsuleCollider>();
        offeringDetectionCollider.isTrigger = true;
        offeringDetectionCollider.radius = Hextile.Radius * AlterToTileRatio * Mathf.Pow(StepIngressRatio, StepCount);
        offeringDetectionCollider.height = Hextile.Thickness * 2 + StepHeight*StepCount;

        StartCoroutine(RitualCycler());
    }

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
        _Flames ritualFlame;
        ritualFlame = Instantiate(Requiem.SpiritFlameTemplate).GetComponent<_Flames>();
        ritualFlame.transform.SetParent(Tile.gameObject.transform, false);
        ritualFlame.shapeModule.shapeType = ParticleSystemShapeType.Donut;
        ritualFlame.shapeModule.scale = flameScale;
        ritualFlame.FlameStyle(0);
        ritualFlame.particleLight.intensity = 1f;
        LineRenderer Lines;
        Lines = gameObject.AddComponent<LineRenderer>();
        Lines.enabled = false;
        Lines.useWorldSpace = false;
        Lines.startColor = new Color(1.0f, 0.0f, 0.0f, 0.75f);
        Lines.endColor = new Color(1.0f, 0.0f, 0.0f, 0.75f);
        Lines.material = Resources.Load<Material>("Materials/FX/LineRenderer");
        Lines.widthMultiplier = 0.015f;
        List<Vector3> points = new List<Vector3>()
        {
            AIBehaviour.angleToDirection(54) * 0.4f * pentagramRadius,
            AIBehaviour.angleToDirection(270) * 0.4f * pentagramRadius,
            AIBehaviour.angleToDirection(126) * 0.4f * pentagramRadius,
            AIBehaviour.angleToDirection(342) * 0.4f * pentagramRadius,
            AIBehaviour.angleToDirection(198) * 0.4f * pentagramRadius,
        };
        _Flames.FlameStyles flamePreset = _Flames.FlameStyles.Magic;
        yield return null;
        while (true)
        {
            float drawTimer = 0.0f;
            float timerRatio;
            Used = false;
            ritualFlame.FlameStyle(flamePreset);
            ritualFlame.shapeModule.position = Vector3.zero + Vector3.up * pentagramHeight;
            yield return new WaitUntil(() => Energized);
            ritualFlame.shapeModule.scale = flameScale;
            ritualFlame.shapeModule.position = points[0];
            while (drawTimer < RitualDurationSeconds)
            {
                Lines.enabled = true;
                Lines.startColor = new Color(1.0f, 0.0f, 0.0f, 0.75f);
                Lines.endColor = new Color(1.0f, 0.0f, 0.0f, 0.75f);
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
                Lines.positionCount = positions.Count;
                Lines.SetPositions(positions.ToArray());
                ritualFlame.shapeModule.position = drawTimer == 0 ? Vector3.zero + Vector3.up * pentagramHeight : positions[positions.Count - 1];
                drawTimer += Energized ? Time.deltaTime : -Time.deltaTime;
                drawTimer = Mathf.Clamp(drawTimer, 0, RitualDurationSeconds);
                yield return null;
            }
            ritualFlame.shapeModule.position = Vector3.zero + Vector3.up * pentagramHeight;
            ritualFlame.FlameStyle(flamePreset);
            Used = true;
            ritualFlame.FlameStyle(0);
            yield return new WaitWhile(() => Energized);
            float fadeOutTimer = 0;
            float fadeOutPeriod = 3;
            float ratio;
            while (fadeOutTimer < fadeOutPeriod)
            {
                ratio = fadeOutTimer / fadeOutPeriod;
                Lines.startColor = new Color(1.0f, 0.0f, 0.0f, Mathf.Lerp(0.75f, 0.0f, ratio));
                Lines.endColor = new Color(1.0f, 0.0f, 0.0f, Mathf.Lerp(0.75f, 0.0f, ratio));
                fadeOutTimer += Time.deltaTime;
                yield return null;
            }
        }       
    }

}
