using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Hexwall : MonoBehaviour
{
    public static float WallHeight = 1.0f;
    public static Mesh WallMesh;
    public Hextile.HexPosition Position = Hextile.HexPosition.error;

    protected MeshFilter filter;
    protected MeshRenderer render;
    protected BoxCollider physicsBox;

    public void Start()
    {
        if(Position != Hextile.HexPosition.error)
        {
            Initialize((int)Position);
        }
    }

    public void Initialize(int i)
    {
        gameObject.layer = transform.parent.gameObject.layer == Game.layerTile ? Game.layerWall : transform.parent.gameObject.layer;
        filter = GetComponent<MeshFilter>() == null ? gameObject.AddComponent<MeshFilter>() : GetComponent<MeshFilter>();
        render = GetComponent<MeshRenderer>() == null ? gameObject.AddComponent<MeshRenderer>() : GetComponent<MeshRenderer>();
        Position = (Hextile.HexPosition)i;
        if (!WallMesh)
        {
            generateWallMesh();
        }
        filter.mesh = WallMesh;
        render.material = Resources.Load<Material>("Materials/Tiles/Wall");
        transform.localPosition = Vector3.zero;
        switch (Position)
        {
            case Hextile.HexPosition.UpRight:
                transform.localEulerAngles = Vector3.up * 60f;
                break;
            case Hextile.HexPosition.Up:
                transform.localEulerAngles = Vector3.up * 0f;
                break;
            case Hextile.HexPosition.UpLeft:
                transform.localEulerAngles = Vector3.up * -60f;
                break;
            case Hextile.HexPosition.DownLeft:
                transform.localEulerAngles = Vector3.up * -120f;
                break;
            case Hextile.HexPosition.Down:
                transform.localEulerAngles = Vector3.up * 180f;
                break;
            case Hextile.HexPosition.DownRight:
                transform.localEulerAngles = Vector3.up * 120f;
                break;
        }
        physicsBox = GetComponent<BoxCollider>() == null ? gameObject.AddComponent<BoxCollider>() : GetComponent<BoxCollider>();
    }


    /***************** wall functions ****************/
    public GameObject CreateGate()
    {
        GameObject newGate = Instantiate(Resources.Load<GameObject>("Prefabs/Structures/Gate"));


        return newGate;
    }

    private void generateWallMesh()
    {
        int i = (int)Position;
        Vector3[] wallVerts = new Vector3[]
        {
                    new Vector3(Mathf.Cos(DegToRad(60)) * Hextile.Radius, Hextile.Thickness/2 + Hextile.Thickness*WallHeight, Mathf.Sin(DegToRad(60)) * Hextile.Radius),
                    new Vector3(Mathf.Cos(DegToRad(120)) * Hextile.Radius, Hextile.Thickness/2 + Hextile.Thickness*WallHeight, Mathf.Sin(DegToRad(120)) * Hextile.Radius),
                    new Vector3(Mathf.Cos(DegToRad(60)) * Hextile.Radius, Hextile.Thickness/2, Mathf.Sin(DegToRad(60)) * Hextile.Radius),
                    new Vector3(Mathf.Cos(DegToRad(120)) * Hextile.Radius, Hextile.Thickness/2, Mathf.Sin(DegToRad(120)) * Hextile.Radius),
        };
        int[] wallTris = new int[]
        {
                    0, 3, 1,
                    0, 2, 3,
        };

        Vector2[] wallUV = new Vector2[]
        {
                    new Vector2(Mathf.Sin(DegToRad(60)), Hextile.Thickness/2f),
                    new Vector2(Mathf.Cos(DegToRad(60)), Hextile.Thickness/2f),
                    new Vector2(Mathf.Sin(DegToRad(60)), -Hextile.Thickness/2f),
                    new Vector2(Mathf.Cos(DegToRad(60)), -Hextile.Thickness/2f),
        };
        WallMesh = new Mesh();
        WallMesh.name = "HexWall";
        WallMesh.vertices = wallVerts;
        WallMesh.triangles = wallTris;
        WallMesh.uv = wallUV;
        WallMesh.RecalculateNormals();
        WallMesh.RecalculateBounds();
        //AssetDatabase.CreateAsset(WallMesh, "Assets/Resources/Prefabs/Hextile/HexWallMesh.asset");
        //AssetDatabase.SaveAssets();
    }

    private float DegToRad(float degrees)
    {
        return degrees * (Mathf.PI / 180);
    }
}
