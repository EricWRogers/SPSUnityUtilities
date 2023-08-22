using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperPupSystems.Helper
{
public class SimpleObjectPool : MonoBehaviour
{
    // instance reference
    public static SimpleObjectPool instance { get; private set; }
    // pool config
    public List<Pool> pools;
    // pool
    private Dictionary<string, Queue<GameObject>> m_poolDictionary;
    public void Awake()
    {
        // Make an instance
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        // set the pool to an empty Dictionary
        m_poolDictionary = new Dictionary<string, Queue<GameObject>>();

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

            m_poolDictionary.Add(pool.code, objectPool);
        }

    }

    
    public GameObject SpawnFromPool (string _code, Vector3 _position, Quaternion _rotation)
    {
        // check for error
        if (!m_poolDictionary.ContainsKey(_code))
        {
            Debug.LogWarning("Pool with code " + _code + " doesn't excist.");
            return null;
        }

        // remove object from beginning of queue
        GameObject objectToSpawn = m_poolDictionary[_code].Dequeue();

        // set position and rotation
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = _position;
        objectToSpawn.transform.rotation = _rotation;

        // add object to the end of the queue
        m_poolDictionary[_code].Enqueue(objectToSpawn);

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
