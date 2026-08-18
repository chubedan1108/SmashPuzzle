using UnityEngine;

public class Box : Obstacle
{
    [Header("Box Specific")]
    [SerializeField] private bool m_applyRandomRotation = true;

    public override ObstacleType Type => ObstacleType.Box;
    public bool ApplyRandomRotation => m_applyRandomRotation;

    protected override void Awake()
    {
        base.Awake();

        if (m_applyRandomRotation)
        {
            ApplyRandomVisualRotation();
        }
    }

    /// <summary>
    /// Xoay ngẫu nhiên 180 độ theo các trục đối xứng để tạo sự đa dạng về hình ảnh/texture mà không làm thay đổi hitbox.
    /// </summary>
    private void ApplyRandomVisualRotation()
    {
        // Với các khối hộp đối xứng, xoay ngẫu nhiên 180 độ quanh trục Y hoặc Z để thay đổi góc nhìn texture
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
