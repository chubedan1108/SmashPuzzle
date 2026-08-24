using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class SlingshotController : MonoBehaviour
{
    [SerializeField] private Transform slingshotRoot;
    [SerializeField] private Transform firePoint;
    [Header("Effect cho lung linh")]
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem featherSlingshot;  
    [Header("Ballistic setting")]
    [Tooltip("Toc do ngang cua dan")]
    [SerializeField, Min(0.01f)] private float desiredHorizontalSpeed = 20f;

    private Bullet currentBullet;
    private PoolType currentPoolType;
    private Quaternion initialRootLocalRotation;
    private const string ANIM_TRIGGER_SHOOT="Shoot";
    private Vector3 currentTarget;

    private void Awake()
    {
        initialRootLocalRotation = slingshotRoot.localRotation;
        currentPoolType = PoolType.Bullet;
        GameEvents.OnShoot += OnShoot;
        GameEvents.OnAim += Rotate;
        GameEvents.OnPullCompleted += AnimPullCompleted;
        GameEvents.OnShootCompleted += AnimOnShootCompleted;
    }

    private void Start()
    {
        LoadNextBullet();
    }

    private void OnDestroy()
    {
        GameEvents.OnShoot -= OnShoot;
        GameEvents.OnAim -= Rotate;
        GameEvents.OnPullCompleted -= AnimPullCompleted;
        GameEvents.OnShootCompleted -= AnimOnShootCompleted;
    }

    private Vector3 debugLastTarget;
    private Vector3 debugLastVelocity;

    //Dien anim ban
    public void OnShoot(Vector3 target)
    {
        currentTarget = target;
        if (animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_SHOOT);
        }
        else
        {
            FireAt(target);
        }
        if (featherSlingshot != null)
        {
            featherSlingshot.Play();
        }
    }

    //Xoay ve huong muc tieu
    private void Rotate(Vector3 target)
    {
        currentTarget = target;

        Vector3 direction = target - firePoint.position;
        direction.y = 0;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }
        float targetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        targetYaw = Mathf.Clamp(targetYaw, -60f, 60f);
        Quaternion yawRotation = Quaternion.AngleAxis(targetYaw, Vector3.up);
        Quaternion targetRotation = yawRotation * initialRootLocalRotation;
        slingshotRoot.localRotation = targetRotation;
    }

    //Khi anim dien doan keo xong thi ban
    private void AnimPullCompleted()
    {
        FireAt(currentTarget);
    }

    private void AnimOnShootCompleted()
    {

    }
    //Load bullet tiep theo
    private void LoadNextBullet()
    {
        currentBullet = SimplePool.Spawn<Bullet>(currentPoolType,firePoint.position,Quaternion.identity);
       // Debug.Log(currentBullet.transform.position + " " + currentBullet.GetPos());
        currentBullet.ResetBullet();
        currentBullet.transform.SetParent(firePoint, false);
        currentBullet.transform.localPosition = Vector3.zero;
        currentBullet.transform.localRotation = Quaternion.identity;
        currentBullet.transform.localScale = Vector3.one;
    }

    public void ChangeNextBullet(PoolType newType)
    {
        if (currentBullet != null)
        {
            SimplePool.Despawn(currentPoolType, currentBullet);
            currentBullet = null;
        }
        currentPoolType = newType;
        LoadNextBullet();
    }
    

    private bool isInfiniteMode = false;
    private float autoFireDelay = 0.5f;
    private float autoFireTimer = 0f;

    public void SetInfiniteMode(bool enable, float delay = 0.5f)
    {
        isInfiniteMode = enable;
        autoFireDelay = delay;
        autoFireTimer = 0f;
    }

    private void Update()
    {
        if (isInfiniteMode && GameInput.IsHolding)
        {
            autoFireTimer -= Time.deltaTime;
            if (autoFireTimer <= 0f)
            {
                autoFireTimer = autoFireDelay;
                Vector3 target = GameInput.LastHitPoint != Vector3.zero ? GameInput.LastHitPoint : currentTarget;
                OnShoot(target);
            }
        }
    }

    //Ban tai muc tieu
    public void FireAt(Vector3 target)
    {
        if (currentBullet == null)
        {
            LoadNextBullet();
        }
        Bullet bulletToFire = currentBullet;
        Vector3 startPoint = bulletToFire.transform.position;
        Vector3 fireDirection = (target - startPoint).normalized;
        Vector3 collisionTarget = target - fireDirection * bulletToFire.CollisionRadius;

        if (!TryCalculateLaunchVelocity(startPoint,collisionTarget,out Vector3 calculatedVelocity))
        {
            Debug.LogWarning(
                "Cannot calculate a launch velocity for this target.",
                this
            );
            return;
        }

        debugLastTarget = target;
        debugLastVelocity = calculatedVelocity;

        currentBullet = null;
        bulletToFire.transform.SetParent(null, true);
        bulletToFire.gameObject.SetActive(true);
        bulletToFire.Launch(calculatedVelocity);

        if (BoosterManager.Instance != null)
        {
            BoosterManager.Instance.NotifyBulletFired();
        }

        if (!isInfiniteMode)
        {
            currentPoolType = PoolType.Bullet;
        }

        LoadNextBullet();
    }

    //Tinh van toc cuoi cung dua tren van toc ngang va phan bu trong luc
    private bool TryCalculateLaunchVelocity(Vector3 startPoint,Vector3 targetPoint,out Vector3 launchVelocity)
    {
        launchVelocity = Vector3.zero;

        Vector3 displacement = targetPoint - startPoint;
        Vector3 horizontalDisplacement = new Vector3(displacement.x, 0f,displacement.z);
        float horizontalDistance = horizontalDisplacement.magnitude;

        if (horizontalDistance <= Mathf.Epsilon || desiredHorizontalSpeed <= Mathf.Epsilon)
        {
            return false;
        }

        float flightTime = horizontalDistance / desiredHorizontalSpeed;
        launchVelocity = displacement / flightTime - 0.5f * Physics.gravity * flightTime;
        return true;
    }

   
    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            // Green ray: Forward direction of the fire point
            Gizmos.color = Color.green;
            Gizmos.DrawRay(firePoint.position, firePoint.forward * 3f);

            // Red sphere & line: Target point hit by raycast
            if (debugLastTarget != Vector3.zero)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(debugLastTarget, 0.3f);
                Gizmos.DrawLine(firePoint.position, debugLastTarget);
            }

            // Blue line: Calculated launch velocity vector direction
            if (debugLastVelocity != Vector3.zero)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(firePoint.position, debugLastVelocity.normalized * 5f);
            }
        }
    }
}
