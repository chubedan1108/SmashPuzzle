using UnityEngine;

[CreateAssetMenu(fileName = "BoosterInfiniteBallConfig", menuName = "SmashPuzzle/Booster Config/Infinity Ball")]
public class BoosterInfiniteBallConfig : BoosterConfigSO
{
    [Header("Infinite Ball Settings")]
    public float duration = 3.0f;       // Thời gian hiệu lực (ví dụ 3 giây)
    public float autoFireDelay = 0.5f;  // Bắn tự động mỗi 0.5s khi giữ ngón tay

    public override BoosterBase CreateBoosterLogic()
    {
        return new InfiniteBallBooster(this);
    }
}
