
using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : SingletonBase<ObjectPool>
{
    [SerializeField] private List<PoolTemp> pools;
    private Dictionary<PoolType, Queue<GameObject>> poolDictionary;
    private Dictionary<PoolType, Transform> poolParentDictionary;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Initialize()
    {
        poolDictionary = new Dictionary<PoolType, Queue<GameObject>>();
        poolParentDictionary = new Dictionary<PoolType, Transform>();

        foreach (PoolTemp pool in pools)
        {
            if (pool.prefab == null)
            {
                Debug.LogError($"Prefab is missing for pool type: {pool.type}", this);
                continue;
            }

            if (poolDictionary.ContainsKey(pool.type))
            {
                Debug.LogError($"Pool type is duplicated: {pool.type}", this);
                continue;
            }

            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, pool.parent);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.type, objectPool);
            poolParentDictionary.Add(pool.type, pool.parent);
        }
    }

    public GameObject GetObject(PoolType type)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogWarning($"Pool of type {type} does not exist.");
            return null;
        }
        Queue<GameObject> objectPool = poolDictionary[type];
        if (objectPool.Count == 0)
        {
            Debug.LogWarning($"Pool of type {type} is empty. Consider increasing the pool size.");
            return null;
        }
        GameObject obj = objectPool.Dequeue();
        obj.SetActive(true);
        return obj;
    }
    public void ReturnObject(PoolType type, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogWarning($"Pool of type {type} does not exist.");
            Destroy(obj);
            return;
        }
        obj.transform.SetParent(poolParentDictionary[type], false);
        obj.SetActive(false);
        poolDictionary[type].Enqueue(obj);
    }
}

public enum PoolType
{
    None,
    Bullet,
}
[Serializable]
public class PoolTemp
{
    public PoolType type;
    public GameObject prefab;
    public Transform parent;
    public int size;
}
