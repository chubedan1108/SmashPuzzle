using System;
using UnityEngine;

public class PoolControl : MonoBehaviour
{
    [SerializeField] private PoolAmount[] poolAmounts;
    private void Awake()
    {
        //Khuc nay dung addressable de clone ca folder pool

        
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
