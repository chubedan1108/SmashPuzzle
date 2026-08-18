using UnityEngine;

public class SlingshotAnimationEvent : MonoBehaviour
{
    public void AnimEvent_OnPullCompleted()
    {
        GameEvents.OnPullCompleted?.Invoke();
    }

    public void AnimEvent_OnShootCompleted()
    {
        GameEvents.OnShootCompleted?.Invoke();
    }
}
