using System.Collections;
using UnityEngine;

public class WeaponRangeFinder : MonoBehaviour
{
    public static bool Debugging = false;
    private bool isRunning = false;
    private Weapon weapon;

    private void Start()
    {
        weapon = GetComponent<Weapon>();
        if (weapon == null)
        {
            Debug.LogError("Weapon script not found!");
            return;
        }
        StartDistanceCheck();
    }

    private void StartDistanceCheck()
    {
        if (!isRunning)
        {
            StartCoroutine(CalculateRange());
        }
    }

    private IEnumerator CalculateRange()
    {
        isRunning = true;
        while (true)
        {
            yield return new WaitUntil(() => transform.parent);
            yield return new WaitUntil(() => weapon.ActionCurrentlyAnimated == Weapon.Action.StrongAttack);
            float maxDistance = 0;
            while (weapon.ActionCurrentlyAnimated == Weapon.Action.StrongAttack)
            {
                // Get the mesh filter component
                MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

                if (meshFilter != null && meshFilter.mesh != null)
                {
                    // Calculate the bounds of the mesh
                    Bounds bounds = meshFilter.mesh.bounds;

                    // Calculate the distance from the center of the bounds to each corner
                    Vector3[] corners = new Vector3[8];
                    corners[0] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
                    corners[1] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z);
                    corners[2] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z);
                    corners[3] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
                    corners[4] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z);
                    corners[5] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z);
                    corners[6] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z);
                    corners[7] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z);

                    // Calculate the distance from each corner to the transform position
                    foreach (Vector3 corner in corners)
                    {
                        float distance = Vector3.Distance(Mullet.Vec3Mult(corner + transform.localPosition, transform.lossyScale), Vector3.zero);
                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                        }
                    }
                }
                yield return null;
            }
            if (Debugging) 
            { 
                Debug.Log("Max Distance: " + maxDistance); 
            }
            weapon.Range = maxDistance * 0.75f;
        }
    }
}
