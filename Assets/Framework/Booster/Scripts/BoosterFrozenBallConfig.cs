using UnityEngine;

[CreateAssetMenu(fileName = "BoosterFrozenBallConfig", menuName = "SmashPuzzle/Booster Config/Frozen Ball")]
public class BoosterFrozenBallConfig : BoosterConfigSO
{
    public PoolType prefabType;

    public override BoosterBase CreateBoosterLogic()
    {
        return new FrozenBallBooster(this);
    }
}
