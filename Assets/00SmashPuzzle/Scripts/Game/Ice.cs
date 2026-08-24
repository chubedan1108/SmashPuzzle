using UnityEngine;

public class Ice : Obstacle
{
    [Header("Ice Specific")]
    [SerializeField] private bool m_applyRandomRotation = true;

    public override ObstacleType Type => ObstacleType.Ice;
    public bool ApplyRandomRotation => m_applyRandomRotation;

    protected override void Awake()
    {
        base.Awake();

        // Cấu hình mặc định cho Ice theo game gốc nếu chưa gán
        if (m_density <= 0f) m_density = 1.5f;
        if (m_additiveMass <= 0f) m_additiveMass = 1.5f;
        if (m_dynamicFriction <= 0f) m_dynamicFriction = 0.135f;
        if (m_staticFriction <= 0f) m_staticFriction = 0.18f;

        if (m_applyRandomRotation)
        {
            ApplyRandomVisualRotation();
        }
    }

    /// <summary>
    /// Xoay ngẫu nhiên 90 hoặc 180 độ theo các trục đối xứng để tạo sự đa dạng diện mạo/texture cho khối băng.
    /// </summary>
    private void ApplyRandomVisualRotation()
    {
        if (m_size.x == m_size.z && Random.value > 0.5f)
        {
            transform.Rotate(0f, 90f * Random.Range(1, 4), 0f, Space.Self);
        }
        else if (Random.value > 0.5f)
        {
            transform.Rotate(0f, 180f, 0f, Space.Self);
        }
    }
}
