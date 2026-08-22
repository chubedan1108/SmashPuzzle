using UnityEngine;

public class BoxRaycast : MonoBehaviour
{
    [Header("Cấu hình BoxRaycast")]
    public Vector3 boxHalfExtents = new Vector3(1f, 1f, 1f);
    public float maxDistance = 10f;
    public LayerMask blockLayer;
    public Vector3 direction = Vector3.forward;

    public void DetectAllBlocksInPath()
    {
        RaycastHit[] hits = Physics.BoxCastAll(
            transform.position, 
            boxHalfExtents, 
            direction, 
            Quaternion.identity, 
            maxDistance, 
            blockLayer
        );
        
        Debug.Log($"Tìm thấy {hits.Length} khối block trên đường quét!");
        foreach (RaycastHit hit in hits)
        {
            Debug.Log($"Block trúng: {hit.collider.gameObject.name}");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Quét tìm điểm chạm thực tế
        bool isHit = Physics.BoxCast(
            transform.position, 
            boxHalfExtents, 
            direction, 
            out RaycastHit hit, 
            Quaternion.identity, 
            maxDistance, 
            blockLayer
        );

        float distance = isHit ? hit.distance : maxDistance;
        Vector3 targetPosition = transform.position + direction.normalized * distance;

        // 1. Vẽ tia định hướng từ nguồn tới đích
        Gizmos.DrawLine(transform.position, targetPosition);

        // 2. Vẽ hình hộp đại diện vùng va chạm ở đích
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(targetPosition, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);
        Gizmos.matrix = oldMatrix;
    }
}
