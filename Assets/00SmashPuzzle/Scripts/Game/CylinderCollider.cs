using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CylinderCollider : MonoBehaviour
{
    [SerializeField] private int NumBoxes = 8;
    [SerializeField] private float Radius = 0.5f;
    [SerializeField] private float Height = 1f;
    [SerializeField] private float Phase = 0f;
    [SerializeField] private PhysicsMaterial PhysicsMaterial;

    [Tooltip("Scales the rotational inertia about the ROLL axis (local Y — the axis a cylinder rolls around). 1 = the physically-correct solid cylinder; <1 = EASIER to roll (less inertia, e.g. 0.5), >1 = HARDER. Only takes effect when the obstacle overrides its inertia (Obstacle.m_useAnalyticInertia).")]
    [SerializeField] private Vector3 InertiaMultiplier = new Vector3(1f, 0.5f, 1f);

    public int BoxCount => NumBoxes;
    public float CylinderRadius => Radius;
    public float CylinderHeight => Height;
    public float CylinderPhase => Phase;
    public Vector3 RollInertiaMultiplier => InertiaMultiplier;

    private void Awake()
    {
        ApplyPhysicsMaterial();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (NumBoxes < 3) NumBoxes = 3;
        if (Radius <= 0f) Radius = 0.01f;
        if (Height <= 0f) Height = 0.01f;
        ApplyPhysicsMaterial();
    }
#endif

    /// <summary>
    /// Tính toán mô-men quán tính giải tích (Analytical Inertia Tensor) cho hình trụ đặc xoay quanh trục Y.
    /// </summary>
    /// <param name="mass">Khối lượng của Rigidbody</param>
    /// <returns>Vector3 Inertia Tensor cho X, Y, Z</returns>
    public Vector3 ComputeInertiaTensor(float mass)
    {
        float r2 = Radius * Radius;
        float h2 = Height * Height;

        // Quán tính xoay quanh trục lăn (trục Y cục bộ của hình trụ): I_y = 1/2 * M * R^2
        float inertiaY = 0.5f * mass * r2 * InertiaMultiplier.y;

        // Quán tính xoay quanh trục vuông góc (trục X và Z): I_x = I_z = 1/12 * M * (3*R^2 + H^2)
        float inertiaX = (1f / 12f) * mass * (3f * r2 + h2) * InertiaMultiplier.x;
        float inertiaZ = (1f / 12f) * mass * (3f * r2 + h2) * InertiaMultiplier.z;

        return new Vector3(inertiaX, inertiaY, inertiaZ);
    }

    /// <summary>
    /// Gán PhysicMaterial cho tất cả các BoxCollider con (các mặt facet).
    /// </summary>
    public void ApplyPhysicsMaterial(PhysicsMaterial mat = null)
    {
        PhysicsMaterial materialToApply = mat != null ? mat : PhysicsMaterial;
        if (materialToApply == null) return;

        foreach (var col in GetComponentsInChildren<BoxCollider>())
        {
            col.sharedMaterial = materialToApply;
        }
    }

    /// <summary>
    /// Tự động sinh / cập nhật các BoxCollider con (Facet) để tạo hình trụ chuẩn xác.
    /// </summary>
    [ContextMenu("Rebuild Facet Colliders")]
    public void RebuildColliders()
    {
        // 1. Tìm hoặc tạo các facet con
        List<BoxCollider> existingBoxes = new List<BoxCollider>(GetComponentsInChildren<BoxCollider>());
        
        // Độ dày của từng hộp facet tiếp tuyến
        float angleStep = 180f / NumBoxes;
        float halfAngleRad = (angleStep * 0.5f) * Mathf.Deg2Rad;
        float thickness = 2f * Radius * Mathf.Tan(halfAngleRad);
        float width = 2f * Radius;

        for (int i = 0; i < NumBoxes; i++)
        {
            GameObject facetGO;
            BoxCollider box;

            if (i < existingBoxes.Count)
            {
                box = existingBoxes[i];
                facetGO = box.gameObject;
            }
            else
            {
                facetGO = new GameObject($"Facet_{i}");
                facetGO.transform.SetParent(transform, false);
                box = facetGO.AddComponent<BoxCollider>();
                facetGO.layer = gameObject.layer;
                facetGO.tag = gameObject.tag;
            }

            facetGO.name = $"Facet_{i}";
            float angle = Phase + (i * angleStep);
            facetGO.transform.localRotation = Quaternion.Euler(0f, -angle, 0f);
            facetGO.transform.localPosition = Vector3.zero;
            facetGO.transform.localScale = Vector3.one;

            box.size = new Vector3(width, Height, thickness);
            box.center = Vector3.zero;
            if (PhysicsMaterial != null)
            {
                box.sharedMaterial = PhysicsMaterial;
            }
        }

        // Xóa các facet thừa nếu có
        for (int i = NumBoxes; i < existingBoxes.Count; i++)
        {
            if (existingBoxes[i] != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(existingBoxes[i].gameObject);
                }
                else
                {
                    DestroyImmediate(existingBoxes[i].gameObject);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        // Vẽ các đường vòng tròn biểu diễn hình trụ
        int segments = Mathf.Max(NumBoxes * 2, 16);
        float angleStep = 360f / segments;
        Vector3 topCenter = Vector3.up * (Height * 0.5f);
        Vector3 bottomCenter = Vector3.down * (Height * 0.5f);

        for (int i = 0; i < segments; i++)
        {
            float a1 = i * angleStep * Mathf.Deg2Rad;
            float a2 = (i + 1) * angleStep * Mathf.Deg2Rad;

            Vector3 p1Top = topCenter + new Vector3(Mathf.Cos(a1) * Radius, 0f, Mathf.Sin(a1) * Radius);
            Vector3 p2Top = topCenter + new Vector3(Mathf.Cos(a2) * Radius, 0f, Mathf.Sin(a2) * Radius);
            Vector3 p1Bottom = bottomCenter + new Vector3(Mathf.Cos(a1) * Radius, 0f, Mathf.Sin(a1) * Radius);
            Vector3 p2Bottom = bottomCenter + new Vector3(Mathf.Cos(a2) * Radius, 0f, Mathf.Sin(a2) * Radius);

            Gizmos.DrawLine(p1Top, p2Top);
            Gizmos.DrawLine(p1Bottom, p2Bottom);

            if (i % (segments / NumBoxes) == 0)
            {
                Gizmos.DrawLine(p1Top, p1Bottom);
            }
        }

        Gizmos.matrix = oldMatrix;
    }
}
