
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class ObjectPool : SingletonBase<ObjectPool>
{
    [SerializeField] private List<Pool> pools;
    private Dictionary<PoolType, Queue<GameObject>> poolDictionary;
    private Dictionary<PoolType, AsyncOperationHandle<GameObject>> poolHandleDictionary;
    protected override void Awake()
    {
        base.Awake();
        Initialized();
    }
    private void Initialized()
    {
        poolDictionary = new Dictionary<PoolType, Queue<GameObject>>();
        poolHandleDictionary = new Dictionary<PoolType, AsyncOperationHandle<GameObject>>();
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(pool.prefab);
            handle.Completed += _ =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject prefab = handle.Result;
                    for (int i = 0; i < pool.size; i++)
                    {
                        GameObject obj = Instantiate(prefab);
                        obj.transform.SetParent(pool.parent);
                        obj.SetActive(false);
                        objectPool.Enqueue(obj);
                    }
                }
                else
                {
                    Debug.LogError($"Failed to load prefab for pool type: {pool.type}");
                }
                
            };
            poolDictionary.Add(pool.type, objectPool);
            poolHandleDictionary.Add(pool.type, handle);
        }
    }

    private void OnDestroy()
    {
        foreach (var handle in poolHandleDictionary.Values)
        {
            Addressables.Release(handle);
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
public class Pool
{
    public PoolType type;
    public AssetReference prefab;
    public Transform parent;
    public int size;
}