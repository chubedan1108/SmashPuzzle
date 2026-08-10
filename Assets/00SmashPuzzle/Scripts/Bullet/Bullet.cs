using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 2.5f;
    [Header("First contact explosion")]
    [SerializeField] private float firstContactExplosionForce = 3f;
    [SerializeField] private float firstContactExplosionRadius = 2f;
    [SerializeField] private float firstContactExplosionUpwards = 0.5f;
    [SerializeField] private float firstContactExploderMultiplier = 1.2f;
    [Header("Hit velocity retention")]
    [SerializeField] private List<float> hitVelocityRetention = new()
    {
        0.5f,
        0.8f,
        0.8f,
        0.8f
    };

    private Rigidbody rb;
    private Coroutine returnCoroutine;
    private Vector3 velocityBeforeImpact;
    private int hitCount;
    private bool hasFirstContact;
    private bool hasLaunched;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (hasLaunched)
        {
            velocityBeforeImpact = rb.linearVelocity;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasLaunched || collision.contactCount == 0)
        {
            return;
        }

        ContactPoint contact = collision.GetContact(0);

        if (!hasFirstContact)
        {
            hasFirstContact = true;
            ApplyFirstContactExplosion(contact.point);
        }

        ApplyHitVelocityRetention(contact.normal);
    }

    public void Launch(Vector3 initialVelocity)
    {
        transform.SetParent(null, true);
        hasLaunched = true;
        rb.isKinematic = false;
        rb.linearVelocity = initialVelocity;
        velocityBeforeImpact = initialVelocity;

        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }

        returnCoroutine = StartCoroutine(ReturnToPoolAfterLifetime());
    }

    public void ResetBullet()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        velocityBeforeImpact = Vector3.zero;
        hitCount = 0;
        hasFirstContact = false;
        hasLaunched = false;
    }

    private void ApplyFirstContactExplosion(Vector3 explosionPosition)
    {
        Collider[] hitColliders = Physics.OverlapSphere(
            explosionPosition,
            firstContactExplosionRadius
        );

        HashSet<Rigidbody> affectedRigidbodies = new();
        float finalExplosionForce =
            firstContactExplosionForce * firstContactExploderMultiplier;

        foreach (Collider hitCollider in hitColliders)
        {
            Rigidbody targetRigidbody = hitCollider.attachedRigidbody;

            if (targetRigidbody == null ||
                targetRigidbody == rb ||
                !affectedRigidbodies.Add(targetRigidbody))
            {
                continue;
            }

            targetRigidbody.AddExplosionForce(
                finalExplosionForce,
                explosionPosition,
                firstContactExplosionRadius,
                firstContactExplosionUpwards,
                ForceMode.Impulse
            );
        }
    }

    private void ApplyHitVelocityRetention(Vector3 contactNormal)
    {
        if (hitVelocityRetention == null || hitVelocityRetention.Count == 0)
        {
            return;
        }

        int retentionIndex = Mathf.Min(
            hitCount,
            hitVelocityRetention.Count - 1
        );

        float retention = Mathf.Clamp01(hitVelocityRetention[retentionIndex]);
        Vector3 reflectedVelocity = Vector3.Reflect(
            velocityBeforeImpact,
            contactNormal
        );

        rb.linearVelocity = reflectedVelocity * retention;
        hitCount++;
    }

    private IEnumerator ReturnToPoolAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);

        returnCoroutine = null;
        ResetBullet();
        ObjectPool.Instance.ReturnObject(PoolType.Bullet, gameObject);
    }
}
