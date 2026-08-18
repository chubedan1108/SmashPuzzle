using System.Collections.Generic;
using UnityEngine;

public static class SimplePool
{
    private static Dictionary<PoolType, Pool> poolInstance = new();


    //Khoi tao pool moi
    public static void Preload(GameUnit prefab, int amount, Transform parent)
    {
       if (prefab == null)
       {
            Debug.LogError("Prefab is null. Cannot preload pool.");
            return;
       }
       if (!poolInstance.ContainsKey(prefab.poolType) || poolInstance[prefab.poolType] == null)
       {
            Pool p = new();
            p.Preload(prefab, amount, parent);
            poolInstance.Add(prefab.poolType, p);
       }
    }

    //Lay phan tu trong pool
    public static T Spawn<T>(PoolType poolType, Vector3 pos, Quaternion rot) where T : GameUnit
    {
        if (!poolInstance.ContainsKey(poolType))
        {
            Debug.LogError($"Pool of type {poolType} does not exist.");
            return null;
        }
        return poolInstance[poolType].Spawn(pos, rot) as T;
    }

    // Tra lai phan tu vao pool
    public static void Despawn(PoolType poolType, GameUnit obj)
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
    GameUnit prefab;
    Queue<GameUnit> inactives = new();
    List<GameUnit> actives = new();

    //Khoi tao pool 
    public void Preload(GameUnit prefab, int amount, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;
        for(int i = 0; i < amount; i++)
        {
            Despawn(GameObject.Instantiate(prefab,parent));
        }

    }

    //Lay phan tu trong pool
    public GameUnit Spawn(Vector3 pos, Quaternion rot)
    {
        GameUnit unit;
        if (inactives.Count <= 0)
        {
            unit = GameUnit.Instantiate(prefab, parent);
        }
        else
        {
            unit = inactives.Dequeue();
        }
        unit.transform.position = pos;
        unit.transform.rotation = rot;
        actives.Add(unit);
        return unit;
    }

    //Tra lai phan tu vao pool
    public void Despawn(GameUnit unit)
    {
        if (unit != null)
        {
            if (actives.Contains(unit))
            {
                actives.Remove(unit);
            }
            if (!inactives.Contains(unit))
            {
                inactives.Enqueue(unit);
            }
            unit.gameObject.SetActive(false);
            unit.transform.SetParent(parent);
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
            GameUnit unit = inactives.Dequeue();
            if (unit != null)
            {
                GameObject.Destroy(unit.gameObject);
            }
        }
        inactives.Clear();
    }   

}