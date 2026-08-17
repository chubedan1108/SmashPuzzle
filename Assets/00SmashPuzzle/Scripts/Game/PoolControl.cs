using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PoolControl : MonoBehaviour
{
    [SerializeField] private PoolAmount[] poolAmounts;
    private void Awake()
    {
        for (int i = 0; i < poolAmounts.Length; i++)
        {
            SimplePool.Preload(poolAmounts[i].prefab, (int)poolAmounts[i].amount, poolAmounts[i].parent);
            //Dung addressable reference
        }
    }
    [Serializable]
    public class PoolAmount
    {
        public PoolType poolType;
        public GameUnit prefab;
        public Transform parent;
        public float amount;
    }
}
