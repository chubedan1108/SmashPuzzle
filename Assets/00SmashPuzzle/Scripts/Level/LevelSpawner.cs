using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    [SerializeField] private EntityDatabase entityDatabase;
    [SerializeField] private Transform root;
    private LevelData currentLevelData;
    private List<GameObject> objSpawned = new();

    public void SpawnLevel(LevelData levelData)
    {
        currentLevelData = levelData;
        ClearLevel();
        List<EntityData> entityList = levelData.Entities;
        for (int i = 0; i < entityList.Count; i++)
        {
            SpawnEntity(entityList[i]);
        }
    }


    private void SpawnEntity(EntityData entityData)
    {
        GameObject prefab = entityDatabase.GetPrefab(entityData.Id);
        if (prefab != null)
        {
            GameObject spawned = Instantiate(prefab, root);
            spawned.transform.localPosition = entityData.Position;
            spawned.transform.localRotation = entityData.Rotation;
            Debug.Log($"name: {spawned.transform.name} \n LocalPosition: {spawned.transform.localPosition}");
            if (entityData.Custom != null)
            {
                Entity entity = spawned.GetComponent<Entity>();
                string customJson = JsonUtility.ToJson(entityData.Custom);
                entity.ReadCustomData(customJson);
            }
            objSpawned.Add(spawned);
        }
    }

    private void ClearLevel()
    {
        for (int i = 0; i< objSpawned.Count; i++)
        {
            if (objSpawned[i] != null)
            {
                Destroy(objSpawned[i]);
            }
        }
        objSpawned.Clear();
    }
}
