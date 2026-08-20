using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private TextAsset textAsset;

    public LevelData LoadLevel()
    {
        LevelData data = JsonUtility.FromJson<LevelData>(textAsset.text);
        return data;
    }
}
