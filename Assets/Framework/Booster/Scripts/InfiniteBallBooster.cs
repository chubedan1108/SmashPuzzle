using UnityEngine;

public class InfiniteBallBooster : BoosterBase
{
    private readonly BoosterInfiniteBallConfig config;
    public override BoosterConfigSO BaseConfig => config;

    private float remainingTime;

    public InfiniteBallBooster(BoosterInfiniteBallConfig config)
    {
        this.config = config;
    }

    public override void Activate(BoosterContext context)
    {
        IsActive = true;
        remainingTime = config.duration > 0f ? config.duration : 3.0f;

        // Kích hoạt chế độ Infinite Ball trên ná bắn với khoảng cách tự động bắn 0.5s
        if (context.Slingshot != null)
        {
            context.Slingshot.SetInfiniteMode(true, config.autoFireDelay > 0f ? config.autoFireDelay : 0.5f);
        }

        Debug.Log($"[InfiniteBallBooster] Activated! Duration: {remainingTime}s, AutoFireDelay: {config.autoFireDelay}s");
    }

    public override void OnUpdate(float deltaTime, BoosterContext context)
    {
        if (!IsActive) return;

        remainingTime -= deltaTime;
        if (remainingTime <= 0f)
        {
            Deactivate(context);
        }
    }

    public override void OnBulletFired(BoosterContext context)
    {
        // Khi bắn 1 viên trong thời gian Infinite -> Báo giữ đạn liên tục, KHÔNG Deactivate
    }

    public override void Deactivate(BoosterContext context)
    {
        if (!IsActive) return;
        IsActive = false;

        // Tắt chế độ Infinite Ball trên ná bắn, quay về đạn thường & input bình thường
        if (context.Slingshot != null)
        {
            context.Slingshot.SetInfiniteMode(false);
        }

        Debug.Log("[InfiniteBallBooster] Deactivated! Returned to normal input.");
    }
}