using UnityEngine;

public enum BoosterType
{
    BigBall,
    InfiniteBall,
    FrozenBall
}

public abstract class BoosterConfigSO : ScriptableObject
{
    public BoosterType boosterType;
    public int unlockAtLevel = 1;            
    public int defaultAmount = 3;

    public abstract BoosterBase CreateBoosterLogic();
}
