using System;
using UnityEngine;

public class PoolControl : MonoBehaviour
{
    [SerializeField] private PoolAmount[] poolAmounts;
    private void Awake()
    {
        for (int i = 0; i < poolAmounts.Length; i++)
        {
            SimplePool.Preload(poolAmounts[i].poolType,poolAmounts[i].prefab, (int)poolAmounts[i].amount, poolAmounts[i].parent);
        }
    }  
    [Serializable]
    public class PoolAmount
    {
        public PoolType poolType;
        public GameObject prefab;
        public Transform parent;
        public float amount;
    }
}
