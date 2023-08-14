using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Drawing;
using UnityEditor;

public class Hextile : MonoBehaviour
{
    [Tooltip("parent gameobject to keep unity editor looking nice")]
    public static GameObject HEXBOARD;
    [Tooltip("Every instantiation")]
    public static List<Hextile> Tiles = new List<Hextile>();
    [Tooltip("The most recently generated tile in the game")]
    public static Hextile LastGeneratedTile;
    [Tooltip("The most recently extended tile in the game")]
    public static Hextile LastExtendedTile;
    public static Mesh HexagonMesh;
    public static float Radius = 1.0f;
    public static float Thickness = 0.75f;


    public enum HexPosition
    {
        error = 0,
        UpRight = 1,
        Up = 2,
        UpLeft = 3,
        DownLeft = 4,
        Down = 5,
        DownRight = 6,
    }
    public Dictionary<Hextile, HexPosition> AdjacentTiles = new Dictionary<Hextile, HexPosition>();
    public List<Hexwall> Walls = new List<Hexwall>();

    public List<Landmark> Landmarks = new List<Landmark>();

    protected MeshFilter filter;
    protected MeshRenderer render;
    protected MeshCollider physics;
    protected SphereCollider positional;
    protected Rigidbody body;

    public List<GameObject> ContainedObjects = new List<GameObject>();
    private int objectsDestroyMask;

    void Awake()
    {
        filter = GetComponent<MeshFilter>() ? GetComponent<MeshFilter>() : gameObject.AddComponent<MeshFilter>();
        render = GetComponent<MeshRenderer>() ? GetComponent<MeshRenderer>() : gameObject.AddComponent<MeshRenderer>();
        render.material = Resources.Load<Material>("Materials/Tiles/Dungeon");
        physics = GetComponent<MeshCollider>() ? GetComponent<MeshCollider>() : gameObject.AddComponent<MeshCollider>();
        positional = GetComponent<SphereCollider>() ? GetComponent<SphereCollider>() : gameObject.AddComponent<SphereCollider>();
        body = GetComponent<Rigidbody>() ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();
        physics.convex = true;
        physics.sharedMesh = HexagonMesh;
        filter.mesh = HexagonMesh;
    }

    void Start()
    {
        LastGeneratedTile = this;
        Tiles.Add(this);
        objectsDestroyMask = ~((1 << Game.layerTile) + (1 << Game.layerWall) + (1 << Game.layerScript));
        gameObject.name = "HexTile";
        transform.SetParent(HEXBOARD.transform, false);
        transform.localEulerAngles = Vector3.zero;
        transform.localPosition = new Vector3(transform.position.x, 0, transform.position.z);
        gameObject.layer = Game.layerTile;
        body.useGravity = false;
        body.isKinematic = true;
        positional.isTrigger = true;
        positional.center = Vector3.zero;
        positional.radius = Radius * 1f;
        refreshWalls();
    }

    void OnTriggerEnter(Collider other)
    {
        Hextile tile = other.gameObject.GetComponent<Hextile>();
        if (tile)
        {
            alterTiles(tile);
            tile.alterTiles(this);
        }
        int objectMask = (1 << Game.layerTile) + (1 << Game.layerWall);
        if (!ContainedObjects.Contains(other.gameObject) && (other.gameObject.layer & objectMask) == 0)
        {
            ContainedObjects.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Hextile tile = other.gameObject.GetComponent<Hextile>();
        if (tile)
        {
            alterTiles(tile, true);
            tile.alterTiles(this, true);
        }
        if (ContainedObjects.Contains(other.gameObject))
        {
            ContainedObjects.Remove(other.gameObject);
        }
    }

    void OnDestroy()
    {
        if(LastGeneratedTile == this)
        {
            LastGeneratedTile = AdjacentTiles.Keys.FirstOrDefault(x=>x);
        }
        if(LastExtendedTile == this)
        {
            LastExtendedTile = AdjacentTiles.Keys.FirstOrDefault(x => x);
        }
        Tiles.Remove(this);
        Dictionary<Hextile, HexPosition> adjCopy = new Dictionary<Hextile, HexPosition>();
        foreach (KeyValuePair<Hextile, HexPosition> tile in AdjacentTiles)
        {
            adjCopy[tile.Key] = tile.Value;
        }
        foreach (KeyValuePair<Hextile, HexPosition> tile in adjCopy)
        {
            if (tile.Key)
            {
                tile.Key.GetComponent<Hextile>().alterTiles(this, remove: true);
            }
        }
        RaycastHit[] hits = Physics.SphereCastAll(new Ray(transform.position + (Vector3.up * Radius * 2), Vector3.down), Radius, Radius * 2, objectsDestroyMask, QueryTriggerInteraction.Collide);
        foreach(RaycastHit hit in hits)
        {
            GameObject obj = hit.collider.gameObject;
            if (!obj)
            {

            }
            else if (obj.GetComponent<Hexwall>())
            {

            }
            else if (obj.GetComponent<Player>())
            {

            }
            else if (!Player.INSTANCE)
            {
                Destroy(obj);
            }
            else if (Player.INSTANCE.HostEntity ? Player.INSTANCE.HostEntity.gameObject == obj : false)
            {

            }
            else
            {
                Destroy(obj);
            }
        }
        foreach(Landmark landmark in Landmarks)
        {
            Destroy(landmark);
        }
        Destroy(gameObject);
    }

    /*******************HEXTILE FUNCTIONS***********************/
    public void alterTiles(Hextile otherTile, bool remove = false)
    {
        if (remove)
        {
            if (AdjacentTiles.ContainsKey(otherTile))
            {
                AdjacentTiles.Remove(otherTile);
                otherTile.alterTiles(this, true);
            }
        }
        else
        {
            if (!AdjacentTiles.ContainsKey(otherTile))
            {
                int angle = (Mathf.RoundToInt(AI.getAngle(otherTile.transform.position - transform.position)) + 360) % 360;
                HexPosition position = (HexPosition)Mathf.RoundToInt((angle + 30) / 60);
                AdjacentTiles.Add(otherTile, position);
                otherTile.alterTiles(this);
            }
        }
        refreshWalls();
    }

    public Hextile Extend(HexPosition direction)
    {

        Hextile newExtendedTile;
        if (AdjacentTiles.ContainsValue(direction))
        {
            newExtendedTile = AdjacentTiles.First(item => item.Value == direction).Key;
        }
        else
        {
            newExtendedTile = new GameObject().AddComponent<Hextile>();
            newExtendedTile.transform.SetParent(transform.parent);
            float rads = Mullet.DegToRad(60f * (int)direction - 30);
            float scaledRadius = 2 * Radius * Mathf.Sin(Mathf.PI / 3);
            Vector3 increment = new Vector3(Mathf.Cos(rads) * scaledRadius, 0, Mathf.Sin(rads) * scaledRadius);
            newExtendedTile.transform.position = transform.position + increment;
            newExtendedTile.transform.position = new Vector3(newExtendedTile.transform.position.x, 0, newExtendedTile.transform.position.z);
        }
        LastExtendedTile = newExtendedTile;
        return newExtendedTile;
    }

    public void refreshWalls()
    {
        for (int i = 1; i <= 6; i++)
        {
            if (!Walls.FirstOrDefault(wall => wall.Position == (HexPosition)i))
            {
                GameObject wall = new GameObject("Wall");
                wall.transform.parent = transform;
                wall.AddComponent<Hexwall>().Initialize(i);
                Walls.Add(wall.GetComponent<Hexwall>());
            }
            Walls.Find(wall => wall.Position == (HexPosition)i).gameObject.SetActive(!AdjacentTiles.ContainsValue((HexPosition)i));
        }
    }


    /* STRUCTURE CREATION */

    public static IEnumerator CreateLine(int length, Hextile startingTile, HexPosition direction, List<Hextile> Tiles = null)
    {
        if (direction == HexPosition.error || length <= 0) { yield break; };
        yield return null;
        startingTile.Extend(direction);
        yield return null;
        for (int i = 0; i < length-1; i++)
        {
            yield return null;
            yield return null;
            yield return null;
            Hextile newTile = LastExtendedTile.Extend(direction);
            if (Tiles != null)
            {
                Tiles.Add(newTile);
            }
            yield return null;
        }

        yield break;
    }

    public static IEnumerator DrawCircle(int circleRadius, Hextile startingTile, HexPosition direction = HexPosition.error, List<List<Hextile>> Tiles = null)
    {
        yield return null;
        startingTile.Extend(direction);
        yield return null;
        for (int i = 0; i <= circleRadius; i++)
        {
            yield return null;
            yield return null;
            yield return null;
            if(direction != HexPosition.error && i != circleRadius)
            {
                LastExtendedTile.Extend(direction);
            }
            if (Tiles != null)
            {
                Tiles.Add(new List<Hextile>());
            }
        }
        yield return null;
        Hextile CenterTile = direction == HexPosition.error ? startingTile : LastGeneratedTile;
        Tiles[0].Add(CenterTile);
        for (int i = 1; i <= 6; i++)
        {
            yield return null;
            Hextile incrementalTile = CenterTile;
            for (int j = 1; j <= circleRadius; j++)
            {
                incrementalTile = incrementalTile.Extend((HexPosition)i);
                Tiles[j].Add(incrementalTile);
                yield return null;
                Hextile leg = incrementalTile;
                HexPosition fill = (HexPosition)(i + 2 > 6 ? i + 2 - 6 : i + 2);
                for (int k = 0; k < j - 1; k++)
                {
                    incrementalTile = incrementalTile.Extend(fill);
                    Tiles[j].Add(incrementalTile);
                    yield return null;
                }
                incrementalTile = leg;
            }
        }
        yield return null;
        yield break;
    }

    public static Hextile GenerateRootTile()
    {
        if (Tiles.Count(x => x) > 0) return null;
        if (!HexagonMesh) GenerateHexMesh();
        HEXBOARD = new GameObject("HexBoard");
        LastGeneratedTile = new GameObject().AddComponent<Hextile>();
        LastGeneratedTile.gameObject.layer = Game.layerTile;
        LastGeneratedTile.transform.position = Vector3.zero;
        return LastGeneratedTile;
    }


    /* AUTO GENERATION OF MESH */
    public static void GenerateHexMesh()
    {
        HexagonMesh = new Mesh();
        HexagonMesh.name = "HexMesh";
        HexagonMesh.vertices = getHexMeshVerts();
        HexagonMesh.triangles = getHexMeshTris();
        HexagonMesh.uv = getHexMeshUV();
        HexagonMesh.RecalculateNormals();
        HexagonMesh.RecalculateBounds();
        AssetDatabase.CreateAsset(HexagonMesh, "Assets/Resources/Prefabs/Hextile/HextileMesh.asset");
        AssetDatabase.SaveAssets();
    }

    private static Vector3[] getHexMeshVerts()
    {
        float temp = Mathf.Round(1000 * Thickness) / 2000;
        Vector3[] allFaces = new Vector3[]
        {
            //topcenter 0
            new Vector3 (0, temp, 0),
            //top 1-6
            new Vector3(Mathf.Cos(DegToRad(0))*Radius, temp, Mathf.Sin(DegToRad(0))*Radius),
            new Vector3(Mathf.Cos(DegToRad(60))*Radius, temp, Mathf.Sin(DegToRad(60))*Radius),
            new Vector3(Mathf.Cos(DegToRad(120))*Radius, temp, Mathf.Sin(DegToRad(120))*Radius),
            new Vector3(Mathf.Cos(DegToRad(180))*Radius, temp, Mathf.Sin(DegToRad(180))*Radius),
            new Vector3(Mathf.Cos(DegToRad(240))*Radius, temp, Mathf.Sin(DegToRad(240))*Radius),
            new Vector3(Mathf.Cos(DegToRad(300))*Radius, temp, Mathf.Sin(DegToRad(300))*Radius),
            //bottom 7-12
            new Vector3(Mathf.Cos(DegToRad(0))*Radius, -temp, Mathf.Sin(DegToRad(0))*Radius),
            new Vector3(Mathf.Cos(DegToRad(60))*Radius, -temp, Mathf.Sin(DegToRad(60))*Radius),
            new Vector3(Mathf.Cos(DegToRad(120))*Radius, -temp, Mathf.Sin(DegToRad(120))*Radius),
            new Vector3(Mathf.Cos(DegToRad(180))*Radius, -temp, Mathf.Sin(DegToRad(180))*Radius),
            new Vector3(Mathf.Cos(DegToRad(240))*Radius, -temp, Mathf.Sin(DegToRad(240))*Radius),
            new Vector3(Mathf.Cos(DegToRad(300))*Radius, -temp, Mathf.Sin(DegToRad(300))*Radius),
            //frontRight 13-16
            new Vector3(Mathf.Cos(DegToRad(0))*Radius, temp, Mathf.Sin(DegToRad(0))*Radius),
            new Vector3(Mathf.Cos(DegToRad(60))*Radius, temp, Mathf.Sin(DegToRad(60))*Radius),
            new Vector3(Mathf.Cos(DegToRad(0))*Radius, -temp, Mathf.Sin(DegToRad(0))*Radius),
            new Vector3(Mathf.Cos(DegToRad(60))*Radius, -temp, Mathf.Sin(DegToRad(60))*Radius),
            //front 17-20
            new Vector3(Mathf.Cos(DegToRad(60))*Radius, temp, Mathf.Sin(DegToRad(60))*Radius),
            new Vector3(Mathf.Cos(DegToRad(120))*Radius, temp, Mathf.Sin(DegToRad(120))*Radius),
            new Vector3(Mathf.Cos(DegToRad(60))*Radius, -temp, Mathf.Sin(DegToRad(60))*Radius),
            new Vector3(Mathf.Cos(DegToRad(120))*Radius, -temp, Mathf.Sin(DegToRad(120))*Radius),
            //frontLeft 21-24
            new Vector3(Mathf.Cos(DegToRad(120))*Radius, temp, Mathf.Sin(DegToRad(120))*Radius),
            new Vector3(Mathf.Cos(DegToRad(180))*Radius, temp, Mathf.Sin(DegToRad(180))*Radius),
            new Vector3(Mathf.Cos(DegToRad(120))*Radius, -temp, Mathf.Sin(DegToRad(120))*Radius),
            new Vector3(Mathf.Cos(DegToRad(180))*Radius, -temp, Mathf.Sin(DegToRad(180))*Radius),
            //backLeft 25-28
            new Vector3(Mathf.Cos(DegToRad(180))*Radius, temp, Mathf.Sin(DegToRad(180))*Radius),
            new Vector3(Mathf.Cos(DegToRad(240))*Radius, temp, Mathf.Sin(DegToRad(240))*Radius),
            new Vector3(Mathf.Cos(DegToRad(180))*Radius, -temp, Mathf.Sin(DegToRad(180))*Radius),
            new Vector3(Mathf.Cos(DegToRad(240))*Radius, -temp, Mathf.Sin(DegToRad(240))*Radius),
            //back 29-32
            new Vector3(Mathf.Cos(DegToRad(240))*Radius, temp, Mathf.Sin(DegToRad(240))*Radius),
            new Vector3(Mathf.Cos(DegToRad(300))*Radius, temp, Mathf.Sin(DegToRad(300))*Radius),
            new Vector3(Mathf.Cos(DegToRad(240))*Radius, -temp, Mathf.Sin(DegToRad(240))*Radius),
            new Vector3(Mathf.Cos(DegToRad(300))*Radius, -temp, Mathf.Sin(DegToRad(300))*Radius),
            //backRight 33-36
            new Vector3(Mathf.Cos(DegToRad(300))*Radius, temp, Mathf.Sin(DegToRad(300))*Radius),
            new Vector3(Mathf.Cos(DegToRad(0))*Radius, temp, Mathf.Sin(DegToRad(0))*Radius),
            new Vector3(Mathf.Cos(DegToRad(300))*Radius, -temp, Mathf.Sin(DegToRad(300))*Radius),
            new Vector3(Mathf.Cos(DegToRad(0))*Radius, -temp, Mathf.Sin(DegToRad(0))*Radius),
            //bottomcenter 37
            new Vector3(0, -temp, 0)
        };
        return allFaces;
    }

    private static int[] getHexMeshTris()
    {
        return new int[]
        {
            //top
            0, 2, 1,
            0, 3, 2,
            0, 4, 3,
            0, 5, 4,
            0, 6, 5,
            0, 1, 6,
            //bottom
            37, 7, 8,
            37, 8, 9,
            37, 9, 10,
            37, 10, 11,
            37, 11, 12,
            37, 12, 7,
            //frontRight
            13, 14, 16,
            13, 16, 15,
            //front
            17, 18, 20,
            17, 20, 19,
            //frontLeft
            21, 22, 24,
            21, 24, 23,
            //backLeft
            25, 26, 28,
            25, 28, 27,
            //back
            29, 30, 32,
            29, 32, 31,
            //backRight
            33, 34, 36,
            33, 36, 35,
        };
    }
 
    private static Vector2[] getHexMeshUV()
    {
        Vector2[] uvmap;
        uvmap = new Vector2[]
        {
            //topcenter 0
            Vector2.zero,
            //top 1-6
            new Vector2(Mathf.Cos(DegToRad(0))*Radius, Mathf.Sin(DegToRad(0))*Radius),
            new Vector2(Mathf.Cos(DegToRad(60))*Radius, Mathf.Sin(DegToRad(60))*Radius),
            new Vector2(Mathf.Cos(DegToRad(120))*Radius, Mathf.Sin(DegToRad(120))*Radius),
            new Vector2(Mathf.Cos(DegToRad(180))*Radius, Mathf.Sin(DegToRad(180))*Radius),
            new Vector2(Mathf.Cos(DegToRad(240))*Radius, Mathf.Sin(DegToRad(240))*Radius),
            new Vector2(Mathf.Cos(DegToRad(300))*Radius, Mathf.Sin(DegToRad(300))*Radius),
            //bottom 7-12
            new Vector2(Mathf.Cos(DegToRad(0))*Radius, Mathf.Sin(DegToRad(0))*Radius),
            new Vector2(Mathf.Cos(DegToRad(60))*Radius, Mathf.Sin(DegToRad(60))*Radius),
            new Vector2(Mathf.Cos(DegToRad(120))*Radius, Mathf.Sin(DegToRad(120))*Radius),
            new Vector2(Mathf.Cos(DegToRad(180))*Radius, Mathf.Sin(DegToRad(180))*Radius),
            new Vector2(Mathf.Cos(DegToRad(240))*Radius, Mathf.Sin(DegToRad(240))*Radius),
            new Vector2(Mathf.Cos(DegToRad(300))*Radius, Mathf.Sin(DegToRad(300))*Radius),
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
        };
        return uvmap;
    }

    /************************** QOL Functions **********************************/
    private static float DegToRad(float degrees)
    {
        return degrees * (Mathf.PI/180);
    }

    public static HexPosition Rotate(HexPosition startPosition, int turns)
    {
        int finshPosition = (int)startPosition;
        int sign = (int)Mathf.Sign(turns);
        for(int i = 0; i != turns; i += sign)
        {
            finshPosition += sign;
            if(finshPosition > 6)
            {
                finshPosition -= 6;
            }
            else if(finshPosition < 1)
            {
                finshPosition += 6;
            }
        }
        return (HexPosition)finshPosition;
    }
 

}
