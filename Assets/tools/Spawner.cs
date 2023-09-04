using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System.Drawing;

public class Spawner : MonoBehaviour
{
    public UnityEvent<List<GameObject>> Spawned = new UnityEvent<List<GameObject>>();
    public UnityEvent<List<GameObject>> FinishedPeriodicSpawning = new UnityEvent<List<GameObject>>();
    public List<Type> ListOfComponentsAddedToSpawnedObjects = new List<Type>();


    void Start()
    {

    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }


    /***** PUBLIC *****/
    public List<GameObject> InstantSpawn(int groupSize)
    {
        if(groupSize <= 0)
        {
            return new List<GameObject>();
        }
        List<GameObject> listOfNewInstances = new List<GameObject>();
        for (int i = 0; i < groupSize; i++)
        {
            GameObject spawned = new GameObject();
            spawned.transform.position = transform.position;
            foreach (Type componentType in ListOfComponentsAddedToSpawnedObjects)
            {
                spawned.AddComponent(componentType);
            }
            listOfNewInstances.Add(spawned);
        }
        if (Spawned != null)
        {
            Spawned.Invoke(listOfNewInstances);
        }
        return listOfNewInstances;
    }

    public void PeriodicallySpawn(float frequency, int groupSize = 1, int maxConcurrentInstances = -1, int totalInstancesToSpawn = -1)
    {
        if(groupSize <= 0) { return; }
        StopAllCoroutines();
        StartCoroutine(routinePeriodicSpawning(frequency, groupSize, maxConcurrentInstances, totalInstancesToSpawn));
    }

    public void StopPeriodicSpawning()
    {
        StopAllCoroutines();
    }

    /***** PROTECTED *****/


    /***** PRIVATE *****/


    private IEnumerator routinePeriodicSpawning(float frequency, int groupSize = 1, int maxConcurrentInstances = -1, int totalInstancesToSpawn = -1)
    {
        if(groupSize <= 0) { yield break; }
 
        List<GameObject> instancesSpawnedThisCycle = new List<GameObject>();
        while (instancesSpawnedThisCycle.Count < totalInstancesToSpawn && totalInstancesToSpawn > 0)
        {
            if(maxConcurrentInstances > 0)
            {
                int delta = 0;
                yield return new WaitUntil(() => { delta = maxConcurrentInstances - instancesSpawnedThisCycle.Count(x => x != null); return delta > 0; });
                instancesSpawnedThisCycle.AddRange(InstantSpawn(Mathf.Min(delta, groupSize)));
            }       
            else
            {
                int permissableGroupSize = Mathf.Min(groupSize, totalInstancesToSpawn - instancesSpawnedThisCycle.Count);
                instancesSpawnedThisCycle.AddRange(InstantSpawn(permissableGroupSize));
            }
            yield return new WaitForSeconds(frequency);
        }
        FinishedPeriodicSpawning.Invoke(instancesSpawnedThisCycle);
        yield break;
    }




}
