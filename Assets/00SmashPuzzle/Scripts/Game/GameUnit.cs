using UnityEngine;

public class GameUnit : MonoBehaviour
{
    public PoolType poolType;
    private Transform tf;
    public Transform TF => tf ??= transform;

}

public enum PoolType
{
    None,
    Bullet,
}
