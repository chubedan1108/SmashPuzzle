using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;

public class EntityDatabase : MonoBehaviour
{
   
    [System.Serializable]
    public class EntityEntry
    {
        [Tooltip("Id khớp với trường Id trong JSON level, ví dụ: CanCylinder_1")]
        public string id;
        [Tooltip("Prefab GameObject sẽ được Instantiate")]
        public GameObject prefab;
    }
    [SerializeField] private List<EntityEntry> entries = new();
    public GameObject GetPrefab(string id)
    {
        foreach(var entry in entries)
        {
            if(entry.id == id)
                return entry.prefab;
        }
        return null;
    }
}
