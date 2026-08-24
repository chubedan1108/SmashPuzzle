using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class FrozenBallBooster : BoosterBase
{
    private readonly BoosterFrozenBallConfig config;
    public override BoosterConfigSO BaseConfig => config;


    public FrozenBallBooster(BoosterFrozenBallConfig config)
    {
        this.config = config;
    }
    public override void Activate(BoosterContext context)
    {
        IsActive = true;
        context.Slingshot.ChangeNextBullet(config.prefabType);
        Debug.Log($"[Booster] actived frozen ball");
    }

    public override void OnBulletFired(BoosterContext context)
    {
        Deactivate(context);
    }
    public override void Deactivate(BoosterContext context)
    {
        if (!IsActive) return;
        IsActive = false;
        Debug.Log($"[Booster] Deactived frozen ball");
    }
}