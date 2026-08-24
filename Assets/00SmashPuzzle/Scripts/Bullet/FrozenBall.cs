using System.Collections.Generic;
using UnityEngine;

public class FrozenBall : Bullet
{
    [Header("Frozen Ball BoxCast Settings")]
    [Tooltip("Kích thước nửa bán kính khung BoxCast. (1.5, 1.5, 0.25) tương đương khung bức tường 3x3x0.5")]
    [SerializeField] private Vector3 boxHalfExtents = new Vector3(1.5f, 1.5f, 0.25f);
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private GameObject iceImpactVfxPrefab;

    private bool hasTriggered = false;

    private void Awake()
    {
        poolType = PoolType.FrozenBall;
        if (obstacleLayer == 0)
        {
            obstacleLayer = LayerMask.GetMask("Obstacle", "Default", "Block");
        }
    }

    public override void ResetBullet()
    {
        base.ResetBullet();
        hasTriggered = false;
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (hasTriggered) return;

        // Chỉ kích hoạt khi chạm vào Obstacle hoặc các khối block
        Obstacle hitObstacle = collision.gameObject.GetComponentInParent<Obstacle>();
        bool isObstacle = hitObstacle != null || collision.gameObject.CompareTag("Obstacle");

        if (isObstacle)
        {
            hasTriggered = true;
            Vector3 impactPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;

            // 1. Quét tìm tất cả khối Obstacle trong khung bức tường 3x3x0.5 thẳng đứng độc lập
            ExecuteFrozenBoxCast(impactPoint);

            // 2. Spawn hiệu ứng hạt đóng băng
            SpawnIceImpactEffect(impactPoint);

            // 3. Biến mất và thu hồi bóng về SimplePool ngay lập tức
            DespawnFrozenBall();
        }
    }

    /// <summary>
    /// Bật BoxCast quét thẳng đứng độc lập hoàn toàn với góc xoay/nghiêng của viên đạn
    /// </summary>
    private void ExecuteFrozenBoxCast(Vector3 impactPoint)
    {
        // Khung BoxCast luôn đứng thẳng 100% theo hệ tọa độ thế giới (Quaternion.identity), 
        // hoàn toàn không bị ảnh hưởng bởi transform.rotation của viên đạn
        Quaternion uprightRotation = Quaternion.identity;
        Collider[] hitColliders = Physics.OverlapBox(transform.position, boxHalfExtents, uprightRotation, obstacleLayer);

        HashSet<Obstacle> uniqueObstacles = new HashSet<Obstacle>();

        foreach (Collider col in hitColliders)
        {
            if (col == null) continue;
            Obstacle obstacle = col.GetComponentInParent<Obstacle>();
            if (obstacle != null)
            {
                uniqueObstacles.Add(obstacle);
            }
        }

        Debug.Log($"[FrozenBall] Tìm thấy {uniqueObstacles.Count} khối trong bức tường 3x3x0.5 thẳng đứng!");

        // Tiến hành chuyển đổi từng khối sang Ice
        foreach (Obstacle obstacle in uniqueObstacles)
        {
            ObstacleConverter.ConvertObstacleToIce(obstacle);
        }
    }

    private void SpawnIceImpactEffect(Vector3 position)
    {
        if (iceImpactVfxPrefab != null)
        {
            GameObject vfx = Instantiate(iceImpactVfxPrefab, position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }

    private void DespawnFrozenBall()
    {
        SimplePool.Despawn(PoolType.FrozenBall, this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Matrix4x4 oldMatrix = Gizmos.matrix;

        // Khung Gizmos màu đỏ 3x3x0.5 luôn luôn đứng thẳng (Quaternion.identity)
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);

        Gizmos.matrix = oldMatrix;
    }
}
