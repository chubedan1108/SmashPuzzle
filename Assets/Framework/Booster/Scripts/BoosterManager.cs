using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class BoosterManager : SingletonBase<BoosterManager>
{
    [SerializeField] private List<BoosterConfigSO> boosterConfig;
    [SerializeField] private SlingshotController slingshotController;

    private Dictionary<BoosterType, BoosterBase> activeLogics = new();
    private BoosterBase currentBooster;
    private BoosterContext context;
    protected override void Awake()
    {
        base.Awake();
        InitializeBooster();
    }

    private void InitializeBooster()
    {
        foreach(var config in boosterConfig)
        {
            activeLogics[config.boosterType] = config.CreateBoosterLogic();
            Debug.Log(activeLogics[config.boosterType]);
        }
    }

    private void Start()
    {
        context = new BoosterContext
        {
            Slingshot = slingshotController,
            Manager = this,
        };

    }

    private void Update()
    {
        if (currentBooster != null && currentBooster.IsActive)
        {
            currentBooster.OnUpdate(Time.deltaTime, context);
        }
    }

    public bool TryActiveBooster(BoosterType type, int currentLevel)
    {
        if (!activeLogics.TryGetValue(type, out BoosterBase booster)) return false;
        if (booster.IsUnlock(currentLevel))
        {
            if (currentBooster!= null && currentBooster.IsActive)
            {
                currentBooster.Deactivate(context);
            }
            currentBooster = booster;
            currentBooster.Activate(context);
        }
        return true;
    }

    public void NotifyBulletFired()
    {
        if (currentBooster != null && currentBooster.IsActive)
        {
            currentBooster.OnBulletFired(context);
        }
    }

}
