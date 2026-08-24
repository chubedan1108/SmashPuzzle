
using UnityEngine;

[CreateAssetMenu(fileName = "BoosterBigBallConfig", menuName = "SmashPuzzle/Booster Config/Big Ball")]
public class BoosterBigBallConfig : BoosterConfigSO
{
    public PoolType prefabType;
    public float scalePenguin;

    public override BoosterBase CreateBoosterLogic()
    {
        return new BigBallBooster(this);
    }
}
