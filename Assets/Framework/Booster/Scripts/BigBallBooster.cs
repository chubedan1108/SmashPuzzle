using System.Collections;
using UnityEngine;

public class BigBallBooster : BoosterBase
{
    private readonly BoosterBigBallConfig config;
    public override BoosterConfigSO BaseConfig => config;


    public BigBallBooster(BoosterBigBallConfig config)
    {
        this.config = config;
    }
    public override void Activate(BoosterContext context)
    {
        IsActive = true;
        context.Slingshot.ChangeNextBullet(config.prefabType);
        Debug.Log($"[Booster] Actived BigBall");
    }
    public override void OnBulletFired(BoosterContext context)
    {
        Deactivate(context);
    }

    public override void Deactivate(BoosterContext context)
    {
        if (!IsActive) return;
        IsActive = false;
        Debug.Log($"[Booster] Deactivated BigBall.");
    }
}