using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Spawner : MonoBehaviour
{
    public float PeriodicSpawnFrequency = 20.0f;
    public int PeriodicSpawnSize = 2;
    public bool PeriodicSpawningEnabled = false;
    public Vector3 SpawnPosition = Vector3.zero;
    public List<Type> listOfSpawnComponents = new List<Type>();
    public List<GameObject> listofSpawnedInstances { get; internal set; } = new List<GameObject>();
    public int maxConcurrentInstances = 10;

    public delegate void SpawnedDelegate(List<GameObject> listOfNewInstances);
    public SpawnedDelegate OnSpawned;

    void Start()
    {
        StartCoroutine(routinePeriodicSpawning());
    }

    void OnDestroy()
    {
        StopCoroutine(routinePeriodicSpawning());
    }

    private IEnumerator routinePeriodicSpawning()
    {
        while (true)
        {
            if (PeriodicSpawningEnabled)
            {
                SpawnObjects(PeriodicSpawnSize);
                yield return new WaitForSeconds(PeriodicSpawnFrequency);
            }
            else
            {
                yield return null;
            }
        }
    }

    public void SpawnObjects(int count)
    {
        List<GameObject> listOfNewInstances = new List<GameObject>();
        for (int i = 0; i < count && listofSpawnedInstances.Count(x => x != null) < maxConcurrentInstances; i++)
        {
            GameObject spawned = new GameObject();
            spawned.transform.position = SpawnPosition;
            listofSpawnedInstances.Add(spawned);
            foreach (Type componentType in listOfSpawnComponents)
            {
                spawned.AddComponent(componentType);
            }
            listOfNewInstances.Add(spawned);
        }
        if (OnSpawned != null)
        {
            OnSpawned.Invoke(listOfNewInstances);
        }
    }


}
