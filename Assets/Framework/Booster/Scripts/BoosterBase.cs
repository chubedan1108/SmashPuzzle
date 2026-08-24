using UnityEngine;

public abstract class BoosterBase
{
    public abstract BoosterConfigSO BaseConfig { get; }
    public bool IsActive { get; protected set; }

    public bool IsUnlock(int currentLevel)
    {
        return currentLevel >= BaseConfig.unlockAtLevel;
    }
    public virtual bool CanActivate(BoosterContext context)
    {
        return !IsActive;
    }
    public abstract void Activate(BoosterContext context);
    public virtual void OnUpdate(float deltaTime, BoosterContext context) { }
    public virtual void OnBulletFired(BoosterContext context) { }
    public abstract void Deactivate(BoosterContext context);
}
public struct BoosterContext
{
    public SlingshotController Slingshot;
    public BoosterManager Manager;
}