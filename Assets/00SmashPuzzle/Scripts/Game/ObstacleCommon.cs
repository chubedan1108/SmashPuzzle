using System.Collections.Generic;
using UnityEngine;

public class ObstacleCommon : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject m_renderer;
    [SerializeField] private GameObject m_brokenPiecesRoot;
    [SerializeField] private List<GameObject> m_brokenPieces = new List<GameObject>();

    [Header("Break on Ground")]
    [SerializeField] private bool m_breakOnGround = true;

    [Header("Break Physics Parameters")]
    public float BreakForce = 8f;        // Lực nổ hất văng mảnh vỡ
    public float BreakRadius = 2f;       // Bán kính nổ mảnh vỡ
    public float BreakUpwards = 0.1f;    // Lực hất bổng mảnh vỡ lên trời
    public float BreakMaxSpeed = 3f;     // Tốc độ tối đa của mảnh vỡ
    public float BreakSpin = 450f;       // Độ xoáy góc ngẫu nhiên cho mảnh vỡ

    public GameObject Renderer => m_renderer;
    public GameObject BrokenPiecesRoot => m_brokenPiecesRoot;
    public List<GameObject> BrokenPieces => m_brokenPieces;

    private bool m_isBroken;

    private void Awake()
    {
        // Mặc định ẩn cây mảnh vỡ, hiển thị mô hình nguyên vẹn
        if (m_brokenPiecesRoot != null)
        {
            m_brokenPiecesRoot.SetActive(false);
        }
        if (m_renderer != null)
        {
            m_renderer.SetActive(true);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 contactPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
        CheckGroundAndBreak(collision.gameObject, collision.collider, contactPoint);
    }

    private void OnCollisionStay(Collision collision)
    {
        Vector3 contactPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
        CheckGroundAndBreak(collision.gameObject, collision.collider, contactPoint);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckGroundAndBreak(other.gameObject, other, transform.position);
    }

    private void CheckGroundAndBreak(GameObject hitGO, Collider hitCollider, Vector3 contactPoint)
    {
        if (m_isBroken || !m_breakOnGround)
        {
            return;
        }

        bool isGround = hitGO.GetComponentInParent<Ground>() != null ||
                        (hitCollider != null && hitCollider.GetComponentInParent<Ground>() != null) ||
                        hitGO.CompareTag("Ground") ||
                        hitGO.layer == LayerMask.NameToLayer("Ground") ||
                        hitGO.name.IndexOf("Ground", System.StringComparison.OrdinalIgnoreCase) >= 0;

        if (isGround)
        {
            Debug.Log($"[ObstacleCommon] Va chạm với Ground qua: '{hitGO.name}' -> Kích hoạt Break!");
            Break(contactPoint);
        }
    }

    /// <summary>
    /// Kích hoạt vỡ vụn và hoàn tất vòng đời đối tượng.
    /// </summary>
    public void Break(Vector3 explosionCenter, float strengthMultiplier = 1f)
    {
        if (m_isBroken) return;
        m_isBroken = true;

        Debug.Log($"[ObstacleCommon] Break() được gọi tại tâm: {explosionCenter}");

        if (TryGetComponent<Obstacle>(out var obstacle))
        {
            obstacle.FallOff();
        }

        SpawnBrokenPieces(explosionCenter, strengthMultiplier);

        // Vô hiệu hóa các Collider của đối tượng gốc để không cản đường vật lý
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // Tự hủy GameObject chính sau khi đã tách các mảnh vỡ
        Destroy(gameObject, 0.05f);
    }

    /// <summary>
    /// Kích hoạt trạng thái vỡ vụn thành từng mảnh.
    /// </summary>
    public void SpawnBrokenPieces(Vector3 explosionCenter, float strengthMultiplier = 1f)
    {
        m_isBroken = true;
        Debug.Log($"[ObstacleCommon] SpawnBrokenPieces() bắt đầu thực thi trên '{gameObject.name}'!");

        // 1. Tách cụm mảnh vỡ ra khỏi cha TRƯỚC TIÊN để không bị ảnh hưởng khi m_renderer bị tắt
        if (m_brokenPiecesRoot != null)
        {
            m_brokenPiecesRoot.transform.SetParent(null);
            m_brokenPiecesRoot.SetActive(true);
            Destroy(m_brokenPiecesRoot, 5f); // Tự dọn dẹp các mảnh vỡ sau 5 giây
        }
        else
        {
            Debug.LogWarning($"[ObstacleCommon] m_brokenPiecesRoot đang null trên '{gameObject.name}'!");
        }

        // 2. Tắt hiển thị mô hình nguyên vẹn
        if (m_renderer != null)
        {
            m_renderer.SetActive(false);
        }

        // Tự động tìm mảnh con nếu danh sách m_brokenPieces đang trống
        if ((m_brokenPieces == null || m_brokenPieces.Count == 0) && m_brokenPiecesRoot != null)
        {
            m_brokenPieces = new List<GameObject>();
            foreach (Transform child in m_brokenPiecesRoot.transform)
            {
                m_brokenPieces.Add(child.gameObject);
            }
        }

        // 3. Truyền xung lực nổ và độ xoáy cho từng mảnh vỡ
        float appliedForce = BreakForce * strengthMultiplier;
        if (m_brokenPieces != null)
        {
            foreach (var piece in m_brokenPieces)
            {
                if (piece == null) continue;
                piece.SetActive(true);

                if (!piece.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb = piece.AddComponent<Rigidbody>();
                }
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.maxAngularVelocity = 50f;
                // Áp dụng lực nổ tỏa tròn
                rb.AddExplosionForce(appliedForce, explosionCenter, BreakRadius, BreakUpwards, ForceMode.Impulse);
                // Giới hạn tốc độ văng tối đa
                if (rb.linearVelocity.magnitude > BreakMaxSpeed)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * BreakMaxSpeed;
                }
                // Thêm độ xoáy ngẫu nhiên
                Vector3 randomTorque = Random.insideUnitSphere * (BreakSpin * Mathf.Deg2Rad);
                rb.AddTorque(randomTorque, ForceMode.Impulse);
            }
        }
    }
}
