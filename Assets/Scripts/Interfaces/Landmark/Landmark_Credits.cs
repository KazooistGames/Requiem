using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Hextile;

public class Landmark_Credits : Landmark
{
    void Start()
    {
        model = Instantiate(Resources.Load<GameObject>("Prefabs/UX/Credits"));
        model.transform.SetParent(transform);
    }

    void Update()
    {
        float rads = Mathf.Deg2Rad * (60f * (int)PositionOnTile - 30);
        float scaledRadius = Radius * Mathf.Sin(Mathf.PI / 3.1f);
        model.transform.localPosition = new Vector3(Mathf.Cos(rads) * scaledRadius, Thickness, Mathf.Sin(rads) * scaledRadius);
        model.transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 30 - 60 * (int)PositionOnTile, transform.localEulerAngles.z);
    }


}
