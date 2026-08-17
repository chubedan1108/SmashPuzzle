using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class SlingshotController : MonoBehaviour
{
    [SerializeField] private Transform slingshotRoot;
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(0f)] private float rotationDuration = 1f;
    [Header("Ballistic setting")]
    [FormerlySerializedAs("speed")]
    [SerializeField, Min(0.01f)] private float desiredHorizontalSpeed = 20f;

    private Bullet currentBullet;
    private Quaternion initialRootLocalRotation;
    private Coroutine rotationCoroutine;

    private void Awake()
    {
        initialRootLocalRotation = slingshotRoot.localRotation;
        GameEvents.OnSlingshotRotate += Rotate;
    }

    private void Start()
    {
        LoadNextBullet();
    }

    private void OnDestroy()
    {
        GameEvents.OnSlingshotRotate -= Rotate;
    }

    private Vector3 debugLastTarget;
    private Vector3 debugLastVelocity;

    //Rotate weapon
    public void Rotate(Vector3 target)
    {
        Vector3 direction = target - firePoint.position;
        direction.y = 0;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }
        // World yaw angle towards target
        float targetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        targetYaw = Mathf.Clamp(targetYaw, -60f, 60f);
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
        rotationCoroutine = StartCoroutine(
            RotateAndFire(target, targetYaw)
        );
    }

    private IEnumerator RotateAndFire(Vector3 target,float targetYaw)
    {
        Quaternion startRotation = slingshotRoot.localRotation;
        Quaternion yawRotation = Quaternion.AngleAxis(targetYaw, Vector3.up);
        Quaternion targetRotation = yawRotation * initialRootLocalRotation;

        if (rotationDuration <= 0f)
        {
            slingshotRoot.localRotation = targetRotation;
            rotationCoroutine = null;
            FireAt(target);
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / rotationDuration);

            yield return null;
            slingshotRoot.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                progress
            );
        }

        slingshotRoot.localRotation = targetRotation;
        rotationCoroutine = null;
        FireAt(target);
    }

    private void LoadNextBullet()
    {
        currentBullet = SimplePool.Spawn<Bullet>(
            PoolType.Bullet,
            firePoint.position,
            Quaternion.identity
        );

        if (currentBullet == null)
        {
            Debug.LogError("Could not spawn a Bullet from SimplePool.", this);
            return;
        }

        currentBullet.ResetBullet();
        currentBullet.transform.SetParent(firePoint, false);
        currentBullet.transform.localPosition = Vector3.zero;
        currentBullet.transform.localRotation = Quaternion.identity;
        currentBullet.transform.localScale = Vector3.one;
    }

    public void FireAt(Vector3 target)
    {
        if (currentBullet == null)
        {
            LoadNextBullet();
            if (currentBullet == null)
            {
                return;
            }
        }

        Bullet bulletToFire = currentBullet;
        Vector3 startPoint = bulletToFire.transform.position;

        // Offset collision target along the trajectory vector instead of target normal
        Vector3 fireDirection = (target - startPoint).normalized;
        Vector3 collisionTarget = target - fireDirection * bulletToFire.CollisionRadius;

        if (!TryCalculateLaunchVelocity(
                startPoint,
                collisionTarget,
                out Vector3 calculatedVelocity))
        {
            Debug.LogWarning(
                "Cannot calculate a launch velocity for this target.",
                this
            );
            return;
        }

        // Ignore collisions between the bullet and the slingshot structure so it doesn't bounce on launch
        Collider bulletCollider = bulletToFire.GetComponent<Collider>();
        if (bulletCollider != null)
        {
            Collider[] slingshotColliders = GetComponentsInChildren<Collider>();
            foreach (Collider col in slingshotColliders)
            {
                if (col != null && col != bulletCollider)
                {
                    Physics.IgnoreCollision(bulletCollider, col, true);
                }
            }
        }

        debugLastTarget = target;
        debugLastVelocity = calculatedVelocity;

        currentBullet = null;
        bulletToFire.transform.SetParent(null, true);
        bulletToFire.Launch(calculatedVelocity);

        // Delay loading the next bullet so it doesn't overlap with the fired bullet at launch
        StartCoroutine(DelayLoadNextBullet(0.5f));
    }

    private IEnumerator DelayLoadNextBullet(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentBullet == null)
        {
            LoadNextBullet();
        }
    }

    private bool TryCalculateLaunchVelocity(
        Vector3 startPoint,
        Vector3 targetPoint,
        out Vector3 launchVelocity)
    {
        launchVelocity = Vector3.zero;

        // Horizontal distance is measured only on the XZ plane.
        Vector3 displacement = targetPoint - startPoint;
        Vector3 horizontalDisplacement = new Vector3(
            displacement.x,
            0f,
            displacement.z
        );
        float horizontalDistance = horizontalDisplacement.magnitude;

        // A vertical-only shot cannot use a horizontal-speed model.
        if (horizontalDistance <= Mathf.Epsilon ||
            desiredHorizontalSpeed <= Mathf.Epsilon)
        {
            return false;
        }

        float flightTime = horizontalDistance / desiredHorizontalSpeed;

        // Compensate for gravity over the calculated flight time.
        launchVelocity =
            displacement / flightTime -
            0.5f * Physics.gravity * flightTime;

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
