using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Mullet : MonoBehaviour
{
    public static Mullet Instance;

    //sound system
    public int soundLayer = 30;
    public float DefaultAudioRange = 5;
    public static float SOUND_MASTER_VOLUME = 1.0f;
    public static int SOUND_DUPLICATES_ALLOWED = 5;
    private static Dictionary<string, AudioClip> SOUND_CLIPS = new Dictionary<string, AudioClip>();
    private Dictionary<AudioClip, List<AudioSource>> soundSources = new Dictionary<AudioClip, List<AudioSource>>();
    public delegate void soundSpawnDelegate(GameObject newSound);

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
        Instance.StartCoroutine(audioUpdate());
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

    /* SOUND */
    public static GameObject PlayAmbientSound(string path, Vector3 position, float pitch, float volume, float range = -1, soundSpawnDelegate onSoundSpawn = null)
    {
        if (Instance && FindObjectOfType(typeof(AudioListener)))
        {
            if (!SOUND_CLIPS.ContainsKey(path))
            {
                SOUND_CLIPS[path] = Resources.Load<AudioClip>(path);

            }
            AudioClip clip = SOUND_CLIPS[path];
            if (!Instance.soundSources.ContainsKey(clip))
            {
                Instance.soundSources[clip] = new List<AudioSource>();
            }
            AudioSource audioSource = createNewAudioSource(clip, position, pitch, volume, range);
            audioSource.transform.position = position;
            audioSource.Play();
            Instance.soundSources[clip].Add(audioSource);
            if (Instance.soundSources[clip].Count > SOUND_DUPLICATES_ALLOWED)
            {
                Destroy(Instance.soundSources[clip].First(x => x).gameObject);
            }
            if (onSoundSpawn != null)
            {
                onSoundSpawn(audioSource.gameObject);
            }
            return audioSource.gameObject;
        }
        else
        {
            return null;
        }
    }
    public static GameObject PlayAmbientSound(AudioClip clip, Vector3 position, float pitch, float volume, float range = -1, soundSpawnDelegate onSoundSpawn = null)
    {
        if (Instance && FindObjectOfType(typeof(AudioListener)))
        {
            if (!SOUND_CLIPS.ContainsKey(clip.name))
            {
                SOUND_CLIPS[clip.name] = clip;
            }
            if (!Instance.soundSources.ContainsKey(clip))
            {
                Instance.soundSources[clip] = new List<AudioSource>();
            }
            AudioSource audioSource = createNewAudioSource(clip, position, pitch, volume, range);
            audioSource.transform.position = position;
            audioSource.Play();
            Instance.soundSources[clip].Add(audioSource);
            if (Instance.soundSources[clip].Count > SOUND_DUPLICATES_ALLOWED)
            {
                Destroy(Instance.soundSources[clip].First(x => x).gameObject);
            }
            if (onSoundSpawn != null)
            {
                onSoundSpawn(audioSource.gameObject);
            }
            return audioSource.gameObject;
        }
        else
        {
            return null;
        }
    }    //overloaded constructor of method above

    private static AudioSource createNewAudioSource(AudioClip clip, Vector3 position, float pitch, float volume, float range = -1)
    {
        AudioSource audioSource = new GameObject(clip.name).AddComponent<AudioSource>();
        audioSource.gameObject.layer = Instance.soundLayer;
        audioSource.playOnAwake = false;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 0;
        audioSource.maxDistance = range >= 0 ? range : Instance.DefaultAudioRange;
        audioSource.spatialBlend = 1.0f;
        audioSource.spread = 60f;
        audioSource.clip = clip;
        audioSource.volume = volume * SOUND_MASTER_VOLUME;
        audioSource.pitch = pitch;
        return audioSource;
    }

    private IEnumerator audioUpdate()
    {
        yield return null;
        while (Instance)
        {
            yield return new WaitUntil(() => soundSources.Values.Count(x => x.Count > 0) > 0);
            for (int i = 0; i < soundSources.Count; i++)
            {
                yield return null;
                foreach (AudioSource source in soundSources.ElementAt(i).Value.Where(x => x ? !x.isPlaying || !x.clip || x.pitch <= 0 : false))
                {
                    Destroy(source.gameObject);
                }
                if(soundSources.ElementAt(i).Value.Count(x => x) > SOUND_DUPLICATES_ALLOWED)
                {
                    for(int j = 0; j < soundSources.ElementAt(i).Value.Count(x => x) - SOUND_DUPLICATES_ALLOWED; j++)
                    {
                        Destroy(soundSources.ElementAt(i).Value.First(x => x).gameObject);
                    }
                }
                soundSources.ElementAt(i).Value.RemoveAll(x => !x);
            }
        }
    }

    /* TOOLS */
    public static Vector3 Vec3Mult(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
    public static float BellCurve(float mean, float standardDeviation, float min = Mathf.NegativeInfinity, float max = Mathf.Infinity)
    {
        float u1 = 1.0f - UnityEngine.Random.value;
        float u2 = 1.0f - UnityEngine.Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
        Mathf.Sin(2.0f * Mathf.PI * u2);
        float value = mean + standardDeviation * randStdNormal;
        return Mathf.Clamp(value, min, max);
    }
    public static float DegToRad(float degrees)
    {
        return degrees * (Mathf.PI / 180);
    }

    static List<Mesh> CutMesh(Mesh mesh, Plane plane)
    {
        List<Mesh> result = new List<Mesh>();

        // Create two empty lists to store the vertices on each side of the plane
        List<Vector3> verticesA = new List<Vector3>();
        List<Vector3> verticesB = new List<Vector3>();

        // Get the mesh vertices and triangles
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Loop through each triangle in the mesh
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // Get the triangle vertices
            Vector3 vertexA = vertices[triangles[i]];
            Vector3 vertexB = vertices[triangles[i + 1]];
            Vector3 vertexC = vertices[triangles[i + 2]];

            // Check which side of the plane each vertex is on
            float distanceA = plane.GetDistanceToPoint(vertexA);
            float distanceB = plane.GetDistanceToPoint(vertexB);
            float distanceC = plane.GetDistanceToPoint(vertexC);

            if (distanceA >= 0 && distanceB >= 0 && distanceC >= 0)
            {
                // All vertices are on the positive side of the plane
                verticesA.Add(vertexA);
                verticesA.Add(vertexB);
                verticesA.Add(vertexC);
            }
            else if (distanceA < 0 && distanceB < 0 && distanceC < 0)
            {
                // All vertices are on the negative side of the plane
                verticesB.Add(vertexA);
                verticesB.Add(vertexB);
                verticesB.Add(vertexC);
            }
            else
            {
                // The triangle intersects the plane
                Vector3[] verticesOnPlane = new Vector3[2];
                int count = 0;

                if (distanceA * distanceB < 0)
                {
                    // Vertex A and B are on opposite sides of the plane
                    verticesOnPlane[count++] = GetIntersectionPoint(vertexA, vertexB, plane);
                }

                if (distanceB * distanceC < 0)
                {
                    // Vertex B and C are on opposite sides of the plane
                    verticesOnPlane[count++] = GetIntersectionPoint(vertexB, vertexC, plane);
                }

                if (distanceC * distanceA < 0)
                {
                    // Vertex C and A are on opposite sides of the plane
                    verticesOnPlane[count++] = GetIntersectionPoint(vertexC, vertexA, plane);
                }

                if (count != 2)
                {
                    // Error: couldn't find 2 intersection points
                    continue;
                }

                // Add the two intersection points to both sides of the plane
                verticesA.Add(verticesOnPlane[0]);
                verticesA.Add(verticesOnPlane[1]);
                verticesA.Add(vertexA);
                verticesA.Add(vertexB);
                verticesA.Add(vertexC);

                verticesB.Add(verticesOnPlane[0]);
                verticesB.Add(verticesOnPlane[1]);
                verticesB.Add(vertexA);
                verticesB.Add(vertexB);
                verticesB.Add(vertexC);
            }
        }

        // Create two new meshes from the split vertices
        Mesh meshA = new Mesh();
        Mesh meshB = new Mesh();

        meshA.vertices = verticesA.ToArray();
        meshB.vertices = verticesB.ToArray();

        meshA.triangles = TriangulateVertices(verticesA).ToArray();
        meshB.triangles = TriangulateVertices(verticesB).ToArray();

        // Add the new meshes to the result list
        result.Add(meshA);
        result.Add(meshB);

        return result;
    }

    static Vector3 GetIntersectionPoint(Vector3 vertex1, Vector3 vertex2, Plane plane)
    {
        Vector3 direction = vertex2 - vertex1;
        float dot = Vector3.Dot(direction, plane.normal);
        float distance = -(Vector3.Dot(vertex1, plane.normal) + plane.distance) / dot;
        return vertex1 + direction * distance;
    }

    static List<int> TriangulateVertices(List<Vector3> vertices)
    {
        List<int> triangles = new List<int>();

        // Triangulate the vertices
        // (This is a very simple example that assumes a convex polygon)
        for (int i = 2; i < vertices.Count; i++)
        {
            triangles.Add(0);
            triangles.Add(i - 1);
            triangles.Add(i);
        }

        return triangles;
    }

}
