using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public static class SimplePool
{
    private static Dictionary<PoolType, Pool> poolInstance = new();


    //Khoi tao pool moi
    public static void Preload(PoolType poolType, GameObject prefab, int amount, Transform parent)
    {
       if (prefab == null)
       {
            Debug.LogError("Prefab is null. Cannot preload pool.");
            return;
       }
       if (!poolInstance.ContainsKey(poolType) || poolInstance[poolType] == null)
       {
            Pool p = new();
            p.Preload(prefab, amount, parent);
            poolInstance.Add(poolType, p);
       }
    }

    //Lay phan tu trong pool
    public static T Spawn<T>(PoolType poolType, Vector3 pos, Quaternion rot) where T : Component
    {
        if (!poolInstance.ContainsKey(poolType))
        {
            Debug.LogError($"Pool of type {poolType} does not exist.");
            return null;
        }
        return poolInstance[poolType].Spawn(pos, rot) as T;
    }

    // Tra lai phan tu vao pool
    public static void Despawn(PoolType poolType, GameObject obj)
    {
        if (!poolInstance.ContainsKey(poolType))
        {
            Debug.LogError($"Pool of type {poolType} does not exist.");
            return;
        }
        poolInstance[poolType].Despawn(obj);
    }

    // Tra lai tat ca phan tu vao pool
    public static void Collect(PoolType poolType)
    {
        if (!poolInstance.ContainsKey(poolType))
        {
            Debug.LogError($"Pool of type {poolType} does not exist.");
            return;
        }
        poolInstance[poolType].Collect();
    }

    //Thu thap tat ca phan tu trong tat ca pool
    public static void CollectAll()
    {
        foreach (var pool in poolInstance.Values)
        {
            pool.Collect();
        }
    }

    // Destroy pool
    public static void Release(PoolType poolType)
    {
        if (!poolInstance.ContainsKey(poolType))
        {
            Debug.LogError($"Pool of type {poolType} does not exist.");
            return;
        }
        poolInstance[poolType].Release();
        poolInstance.Remove(poolType);
    }

    // Destroy tat ca pool
    public static void ReleaseAll()
    {
        foreach (var pool in poolInstance.Values)
        {
            pool.Release();
        }
        poolInstance.Clear();
    }
}
public class Pool
{
    Transform parent;
    GameObject prefab;
    Queue<GameObject> inactives = new();
    List<GameObject> actives = new();

    //Khoi tao pool 
    public void Preload(GameObject prefab, int amount, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;
        for(int i = 0; i < amount; i++)
        {
            Despawn(Spawn(Vector3.zero, Quaternion.identity));
        }

    }

    //Lay phan tu trong pool
    public GameObject Spawn(Vector3 pos, Quaternion rot)
    {
        GameObject unit;
        if (inactives.Count <= 0)
        {
            unit = GameObject.Instantiate(prefab, parent);
        }
        else
        {
            unit = inactives.Dequeue();
        }
        unit.transform.position = pos;
        unit.transform.rotation = rot;
        actives.Add(unit);
        unit.SetActive(true);
        return unit;
    }

    //Tra lai phan tu vao pool
    public void Despawn(GameObject unit)
    {
        if (unit != null & unit.gameObject.activeSelf)
        {
            actives.Remove(unit);
            inactives.Enqueue(unit);
            unit.SetActive(false);
        }
    }

    //Tra lai tat ca phan tu vao pool
    public void Collect()
    {
        while(actives.Count > 0)
        {
            Despawn(actives[0]);
        }
    }

    //Destroy pool
    public void Release()
    {
        Collect();
        while(inactives.Count > 0)
        {
            GameObject.Destroy(inactives.Dequeue());
        }
        inactives.Clear();
    }   

}