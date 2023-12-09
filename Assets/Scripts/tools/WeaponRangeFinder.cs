using System.Collections;
using UnityEngine;

public class WeaponRangeFinder : MonoBehaviour
{
    public bool Debugging = false;
    public float fudgeFactor = 1f;
    private bool isRunning = false;
    private Weapon weapon;
    public float maxDistance;

    private void Start()
    {
        weapon = GetComponent<Weapon>();
        if (weapon == null)
        {
            Debug.LogError("Weapon script not found!");
            Destroy(this);
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
            yield return new WaitUntil(() => checkIfWeShouldEstimateRange());
            while (checkIfWeShouldEstimateRange())
            {
                // Get the mesh filter component
                MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

                if (meshFilter != null && meshFilter.mesh != null)
                {
                    // Calculate the bounds of the mesh
                    Bounds bounds = weapon.HitBox.bounds;

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
                    maxDistance = 0;
                    foreach (Vector3 corner in corners)
                    {
                        Vector3 dispositionFromWielder = weapon.Wielder.transform.position - corner;
                        float instantaneousDistanceFromWielder = dispositionFromWielder.magnitude * fudgeFactor;
                        if (instantaneousDistanceFromWielder > maxDistance)
                        {
                            maxDistance = instantaneousDistanceFromWielder;
                        }
                    }
                }
                yield return null;
            }
            if (Debugging) 
            { 
                Debug.Log("Max Distance: " + maxDistance);
                Debug.DrawLine(weapon.Wielder.transform.position, weapon.Wielder.transform.position + weapon.Wielder.LookDirection.normalized * maxDistance, Color.red, 2);
            }
            weapon.Range = maxDistance;
        }
    }


    private bool checkIfWeShouldEstimateRange()
    {
        if (weapon ? weapon.Wielder : false)
        {
            return weapon.Action == Weapon.ActionAnimation.StrongAttack || weapon.Action == Weapon.ActionAnimation.QuickAttack;
        }
        else
        {
            return false;
        }
    }
}
