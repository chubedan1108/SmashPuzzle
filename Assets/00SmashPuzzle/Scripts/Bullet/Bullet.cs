using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Bullet : GameUnit
{
    [SerializeField] protected LayerMask ignoreImpact;
    [SerializeField] protected BoxRaycast raycast;
    [SerializeField] protected Renderer transparentMesh;
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
        0.5f,0.8f, 0.8f,0.8f
    };

    private Rigidbody rb;
    private SphereCollider sphereCollider;
    private Coroutine returnCoroutine;
    private Vector3 velocityBeforeImpact;
    private Material originalMaterial;
    private int hitCount;
    private bool hasFirstContact;
    private bool hasLaunched;
    private bool isFading;
    private bool check = false;
    public float CollisionRadius
    {
        get
        {
            if (sphereCollider == null) sphereCollider = GetComponent<SphereCollider>();
            if (sphereCollider == null) return 0.5f;

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
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (sphereCollider == null) sphereCollider = GetComponent<SphereCollider>();
        if (transparentMesh != null && originalMaterial == null)
        {
            originalMaterial = transparentMesh.material;
        }
    }

    private void FixedUpdate()
    {
        if (hasLaunched && rb != null)
        {
            velocityBeforeImpact = rb.linearVelocity;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (check) return;
       // raycast.DetectAllBlocksInPath();
        check = true;
        if (!hasLaunched || isFading || collision.contactCount == 0)
        {
            return;
        }
        if (((1 << collision.gameObject.layer) & ignoreImpact.value) != 0) return;

        ContactPoint contact = collision.GetContact(0);
        if (collision.gameObject.GetComponent<Ground>())
        {
            HandleGroundImpact();
        }
        
        if (collision.gameObject.GetComponent<Obstacle>())
        {
            HandleObstacleImpact(contact);

        }
       
    }

    private void HandleGroundImpact()
    {
        //Tang luc can cua vat lieu de tranh bong lan mai
        rb.linearDamping = groundLinearDamping;
        rb.angularDamping = groundAngularDamping;
        return;
    }

    private void HandleObstacleImpact(ContactPoint contact)
    {
        if (!hasFirstContact)
        {
            hasFirstContact = true;
            ApplyFirstContactExplosion(contact.point);
        }

        ApplyHitVelocityRetention(contact.normal);
    }

    public virtual void Launch(Vector3 initialVelocity)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        transform.SetParent(null, true);
        hasLaunched = true;
        isFading = false;
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
            rb.linearVelocity = initialVelocity;
        }
        velocityBeforeImpact = initialVelocity;

        //sua lai logic khi cham nen hoac sau 2.5f ma khong xay ra va cham gi thi return to pool (de luc sau thi sua)
        returnCoroutine = StartCoroutine(ReturnToPoolAfterLifetime());
    }

    public virtual void ResetBullet()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (sphereCollider == null) sphereCollider = GetComponent<SphereCollider>();

        transform.gameObject.SetActive(false);
       
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
        }

        velocityBeforeImpact = Vector3.zero;
        hitCount = 0;
        hasFirstContact = false;
        hasLaunched = false;
        isFading = false;
    }

    public Vector3 GetPos()
    {
        return rb.transform.position;
    }

    //Hieu ung o lan tuong tac dau tien
    private void ApplyFirstContactExplosion(Vector3 explosionPosition)
    {
        Collider[] hitColliders = Physics.OverlapSphere( explosionPosition,firstContactExplosionRadius
        );

        HashSet<Rigidbody> affectedRigidbodies = new();
        float finalExplosionForce = firstContactExplosionForce * firstContactExploderMultiplier;

        foreach (Collider hitCollider in hitColliders)
        {
            Rigidbody targetRigidbody = hitCollider.attachedRigidbody;

            if (targetRigidbody == null || targetRigidbody == rb || !affectedRigidbodies.Add(targetRigidbody))
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

    // Hieu ung van toc khi hit trung
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
        originalMaterial.DOFloat(0f, "_Alpha", fadeDuration)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                ResetBullet();
                originalMaterial.SetFloat("_Alpha", 1f);
                SimplePool.Despawn(PoolType.Bullet, this);
            });
    }
}
