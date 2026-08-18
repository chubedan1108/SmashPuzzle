using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Bullet : GameUnit
{
    [SerializeField] private Renderer transparentMesh;
    [SerializeField] private Material transparentMaterial;
    [SerializeField] private Material originMaterial;
    [SerializeField] private float lifetime = 2.5f;
    [SerializeField] private float fadeDuration = 0.5f;
    [Header("Ground damping")]
    [SerializeField] private float groundLinearDamping = 3f;
    [SerializeField] private float groundAngularDamping = 3f;
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
    private SphereCollider sphereCollider;
    private Coroutine returnCoroutine;
    private Vector3 velocityBeforeImpact;
    private int hitCount;
    private bool hasFirstContact;
    private bool hasLaunched;
    private bool isFading;

    public float CollisionRadius
    {
        get
        {
            Vector3 scale = transform.lossyScale;
            float largestScale = Mathf.Max(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y),
                Mathf.Abs(scale.z)
            );

            return sphereCollider.radius * largestScale;
        }
    }


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void FixedUpdate()
    {
        if (hasLaunched)
        {
            velocityBeforeImpact = rb.linearVelocity;
        }
    }

    private void OnCollisionEnter(Collision collision) //Note sua lai ham nay
    {
        if (!hasLaunched || isFading || collision.contactCount == 0)
        {
            return;
        }

        // Ignore collisions with other Bullets or launcher environment planes (Plane)
        if (collision.gameObject.GetComponent<Bullet>() != null ||
            collision.gameObject.name.Contains("Plane"))
        {
            Physics.IgnoreCollision(sphereCollider, collision.collider, true);
            return;
        }

        ContactPoint contact = collision.GetContact(0);
        Debug.Log($"[Bullet Collision Debug] Bullet collided with target: '{collision.gameObject.name}' (Layer: {LayerMask.LayerToName(collision.gameObject.layer)}) at point: {contact.point}");

        bool isGround = collision.gameObject.TryGetComponent<Ground>(out _) ||
                        collision.gameObject.name.Contains("Ground");

        if (isGround)
        {
            // Tang luc can khi cham dat de bong giam toc do dan, tranh lan mai
            rb.linearDamping = groundLinearDamping;
            rb.angularDamping = groundAngularDamping;
        }

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
        isFading = false;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.linearVelocity = initialVelocity;
        velocityBeforeImpact = initialVelocity;

        //sua lai logic khi cham nen hoac sau 2.5f ma khong xay ra va cham gi thi return to pool (de luc sau thi sua)
        returnCoroutine = StartCoroutine(ReturnToPoolAfterLifetime());
    }

    public void ResetBullet()
    {
        transform.gameObject.SetActive(false);
        transparentMesh.material = originMaterial;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        velocityBeforeImpact = Vector3.zero;
        hitCount = 0;
        hasFirstContact = false;
        hasLaunched = false;
        isFading = false;
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
        yield return new WaitForSeconds(lifetime-fadeDuration);
        TransparentObject();
    }

    private void TransparentObject()
    {
        if (isFading)
        {
            return;
        }
        isFading = true;

        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        transparentMesh.material = transparentMaterial;
        transparentMesh.material.DOFade(0f, "_BaseColor", fadeDuration).SetLink(gameObject)
            .OnComplete(() =>
            {
                ResetBullet();
                SimplePool.Despawn(poolType, this);
            });
    }
}
