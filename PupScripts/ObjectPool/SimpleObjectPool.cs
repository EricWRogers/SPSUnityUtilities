using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperPupSystems.Helper
{
public class SimpleObjectPool : MonoBehaviour
{
    // instance reference
    public static SimpleObjectPool Instance { get; private set; }
    // pool config
    public List<Pool> pools;
    // pool
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    public void Awake()
    {
        // Make an instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        // set the pool to an emply Dictionary
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            // make an empty game object to hold our items
            GameObject p = new GameObject();
            p.name = pool.code;
            p.transform.parent = this.gameObject.transform;

            // 
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                // load objects into pool
                GameObject obj = Instantiate(pool.prefab) as GameObject;
                obj.SetActive(false);
                obj.transform.parent = p.transform;
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.code, objectPool);
        }

    }

    
    public GameObject SpawnFromPool (string code, Vector3 position, Quaternion rotation)
    {
        // check for error
        if (!poolDictionary.ContainsKey(code))
        {
            Debug.LogWarning("Pool with code " + code + " doesn't excist.");
            return null;
        }

        // remove object from beginning of queue
        GameObject objectToSpawn = poolDictionary[code].Dequeue();

        // set position and rotation
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // add object to the end of the queue
        poolDictionary[code].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}

[System.Serializable]
public class Pool
{
    public string code;
    public GameObject prefab;
    public int size;
}
    
} // end of namespace
