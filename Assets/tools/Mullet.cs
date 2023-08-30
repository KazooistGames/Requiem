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

    public static float BellCurve(float mean, float standardDeviation, float min = Mathf.NegativeInfinity, float max = Mathf.Infinity)
    {
        float u1 = 1.0f - UnityEngine.Random.value;
        float u2 = 1.0f - UnityEngine.Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
        Mathf.Sin(2.0f * Mathf.PI * u2);
        float value = mean + standardDeviation * randStdNormal;
        return Mathf.Clamp(value, min, max);
    }

    //public static float DegToRad(float degrees)
    //{
    //    return degrees * (Mathf.PI / 180);
    //}

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
