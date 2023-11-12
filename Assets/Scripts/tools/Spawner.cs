using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System.Drawing;

public class Spawner : MonoBehaviour
{
    public UnityEvent<List<GameObject>> JustSpawned = new UnityEvent<List<GameObject>>();
    public UnityEvent<List<GameObject>> FinishedPeriodicSpawning = new UnityEvent<List<GameObject>>();
    public List<Type> ListOfComponentsAddedToSpawnedObjects = new List<Type>();
    public List<GameObject> AllSpawnedObjects = new List<GameObject>();

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
        if (JustSpawned != null)
        {
            JustSpawned.Invoke(listOfNewInstances);
        }
        return listOfNewInstances;
    }

    public void PeriodicallySpawn(float periodSeconds, int groupSize = 1, int minConcurrentInstances = -1, int maxConcurrentInstances = -1)
    {
        if(groupSize <= 0) { return; }
        StopAllCoroutines();
        StartCoroutine(routinePeriodicSpawning(periodSeconds, groupSize, minConcurrentInstances, maxConcurrentInstances));
    }

    public void StopPeriodicSpawning()
    {
        StopAllCoroutines();
    }

    /***** PROTECTED *****/


    /***** PRIVATE *****/


    private IEnumerator routinePeriodicSpawning(float periodSecondsBetweenSpawns, int groupSize = 1, int minConcurrentInstances = int.MinValue, int maxConcurrentInstances = int.MaxValue)
    {
        if(groupSize <= 0) { yield break; }
        List<GameObject> currentInstances = new List<GameObject>();
        float timer;
        while (true)
        {
            timer = 0;
            currentInstances = AllSpawnedObjects.Where(x => x != null).ToList();
            if(currentInstances.Count < maxConcurrentInstances)
            {
                int delta = maxConcurrentInstances - currentInstances.Count;
                int permissableGroupSize = Mathf.Min(groupSize, delta);
                AllSpawnedObjects.AddRange(InstantSpawn(permissableGroupSize));
            }
            yield return new WaitUntil( () => (timer += Time.deltaTime) > periodSecondsBetweenSpawns);
            timer -= periodSecondsBetweenSpawns;
        }
    }




}
