using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] private LevelSpawner levelSpawner;
    [SerializeField] private LevelLoader levelLoader;

    private void Start()
    {
        LoadLevel();   
    }

    private void LoadLevel()
    {
        LevelData data = levelLoader.LoadLevel();
        levelSpawner.SpawnLevel(data);
    }
}
