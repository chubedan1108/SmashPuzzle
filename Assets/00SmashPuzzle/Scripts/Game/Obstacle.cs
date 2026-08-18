using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Obstacle : Entity
{
    [Header("Physical Attributes")]
    [SerializeField] protected Vector3Int m_size = Vector3Int.one;
    [SerializeField] protected float m_density = 0.9f;
    [SerializeField] protected float m_additiveMass = 0.15f;
    [SerializeField] protected Transform m_centerOfMass;
    [SerializeField] protected bool m_useAnalyticInertia = true;
    [Header("Friction & Bounciness")]
    [SerializeField] protected float m_dynamicFriction = 0.45f;
    [SerializeField] protected float m_staticFriction = 0.45f;
    [SerializeField] protected float m_bounciness = 0.1f;
    [SerializeField] protected PhysicsMaterialCombine m_frictionCombine = PhysicsMaterialCombine.Average;
    [SerializeField] protected PhysicsMaterialCombine m_bounceCombine = PhysicsMaterialCombine.Average;
    [Header("Destruction & Explosion")]
    [SerializeField] protected bool m_explodes = false;
    [SerializeField] protected float m_explodeMinSpeed = 10f;
    [SerializeField] protected float m_explodeNonBallSpeedMultiplier = 1.5f;
    [SerializeField] protected GameObject m_impactEffect;
    [SerializeField] protected GameObject m_explosionEffect;
    [Header("References")]
    [SerializeField] private ObstacleCommon m_common;
    // Static Tracking
    private static readonly List<Obstacle> s_alive = new List<Obstacle>();
    public static IReadOnlyList<Obstacle> Alive => s_alive;
    public static bool MaintainContactGraph { get; set; }
    // State variables
    private Rigidbody m_rb;
    private bool m_completed;
    private HashSet<Obstacle> m_contacts = new HashSet<Obstacle>();
    public Rigidbody Body => m_rb;
    public ObstacleCommon Common => m_common;
    public abstract ObstacleType Type { get; }
    public Vector3Int Size => m_size;
    public bool Completed => m_completed;
    public bool Explodes => m_explodes;
    protected virtual void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        if (m_common == null)
        {
            m_common = GetComponent<ObstacleCommon>();
        }
        // 1. Tính toán khối lượng chuẩn xác
        m_rb.mass = ComputeMass();
        // 2. Thiết lập trọng tâm (Center of Mass)
        if (m_centerOfMass != null)
        {
            m_rb.centerOfMass = m_centerOfMass.localPosition;
        }
        // 3. Khởi tạo vật liệu vật lý (PhysicMaterial)
        PhysicsMaterial mat = new PhysicsMaterial($"{name}_Mat")
        {
            dynamicFriction = m_dynamicFriction,
            staticFriction = m_staticFriction,
            bounciness = m_bounciness,
            frictionCombine = m_frictionCombine,
            bounceCombine = m_bounceCombine
        };
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.sharedMaterial = mat;
        }
    }
    protected virtual void OnEnable()
    {
        s_alive.Add(this);
    }
    protected virtual void OnDisable()
    {
        s_alive.Remove(this);
    }
    protected virtual void Start()
    {
        if (m_useAnalyticInertia)
        {
            ApplyAnalyticInertia();
        }
    }
    /// <summary>
    /// Tính toán khối lượng = Thể tích (X * Y * Z) * Mật độ (Density) + Khối lượng phụ (AdditiveMass).
    /// </summary>
    public float ComputeMass()
    {
        float volume = m_size.x * m_size.y * m_size.z;
        return (volume * m_density) + m_additiveMass;
    }
    /// <summary>
    /// Thiết lập mô-men quán tính giải tích giúp vật thể lật/đổ thực tế hơn dạng khối hộp/hình trụ.
    /// </summary>
    private void ApplyAnalyticInertia()
    {
        if (m_rb == null) return;
        float mass = m_rb.mass;

        // Nếu có CylinderCollider -> tính quán tính khối trụ (Cylinder)
        var cylinderCol = GetComponentInChildren<CylinderCollider>();
        if (cylinderCol != null)
        {
            m_rb.inertiaTensor = cylinderCol.ComputeInertiaTensor(mass);
            return;
        }

        // Mặc định tính quán tính khối hộp chữ nhật (Box): I = 1/12 * M * (a^2 + b^2)
        float x2 = m_size.x * m_size.x;
        float y2 = m_size.y * m_size.y;
        float z2 = m_size.z * m_size.z;
        Vector3 inertia = new Vector3(
            (1f / 12f) * mass * (y2 + z2),
            (1f / 12f) * mass * (x2 + z2),
            (1f / 12f) * mass * (x2 + y2)
        );
        m_rb.inertiaTensor = inertia;
    }
    protected virtual void OnCollisionEnter(Collision collision)
    {
        float relativeSpeed = collision.relativeVelocity.magnitude;
        // 1. Kiểm tra va chạm với bóng hoặc vật thể khác
        bool isBall = collision.gameObject.GetComponent<Bullet>() != null;

        // 2. Kích hoạt hiệu ứng va chạm (Impact VFX)
        if (m_impactEffect != null && collision.contactCount > 0)
        {
            Instantiate(m_impactEffect, collision.contacts[0].point, Quaternion.identity);
        }

        // 3. Xử lý nổ / vỡ vụn do tốc độ va chạm vượt ngưỡng
        float thresholdSpeed = isBall ? m_explodeMinSpeed : (m_explodeMinSpeed * m_explodeNonBallSpeedMultiplier);
        if (m_explodes && relativeSpeed >= thresholdSpeed)
        {
            Explode(1f);
        }
    }
    /// <summary>
    /// Cho biết bóng có thể đâm xuyên qua chướng ngại vật hay không.
    /// </summary>
    public virtual bool BallPunchesThrough(float impactSpeed)
    {
        return false; // Mặc định không cho bóng đâm xuyên trừ khi có booster hoặc đối tượng dễ vỡ
    }
    /// <summary>
    /// Kích hoạt trạng thái nổ / vỡ vụn hoàn toàn.
    /// </summary>
    public virtual void Explode(float strength = 1f)
    {
        if (m_completed) return;
        Complete();
        // Kích hoạt vỡ mảnh trong ObstacleCommon
        if (m_common != null)
        {
            m_common.SpawnBrokenPieces(transform.position, strength);
          
        }
        // Kích hoạt hiệu ứng nổ VFX
        if (m_explosionEffect != null)
        {
            Instantiate(m_explosionEffect, transform.position, Quaternion.identity);
        }
        // Tự hủy sau khi vỡ
        StartCoroutine(WaitAndDestroyCoroutine());
    }
    private IEnumerator WaitAndDestroyCoroutine()
    {
        yield return new WaitForSeconds(0.05f);
        Destroy(gameObject);
    }
    /// <summary>
    /// Khi lon rơi khỏi bàn hoặc bị tiêu diệt hoàn toàn.
    /// </summary>
    public void FallOff()
    {
        if (m_completed) return;
        Complete();
    }
    protected void Complete()
    {
        if (m_completed) return;
        m_completed = true;
        // Thông báo cho hệ thống Gameplay/LevelManager (đếm số lon đã bị hạ)
        // Ví dụ: GameplayManager.Instance.OnObstacleCompleted(this);
    }
}
public enum ObstacleType
{
    None = 0,
    Can = 1,          // Lon thiếc
    JamJar = 2,       // Lọ mứt
    Stone = 3,        // Đá
    Box = 4,          // Hộp
    Ice = 5,          // Khối băng
    Wormhole = 6,     // Hố sâu / cổng dịch chuyển
    Tnt = 7,          // Thùng thuốc nổ
    Wood = 8,         // Thanh gỗ
    ColorBox = 9,     // Hộp màu
    Column = 10,      // Cột trụ
    Bouncer = 11      // Đệm nảy
}