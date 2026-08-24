using System.Collections.Generic;
using UnityEngine;

public static class ObstacleConverter
{
    /// <summary>
    /// Chuyển đổi một khối Obstacle sang khối băng tương ứng (IceSquareBt_X hoặc IceCylinderBt_X)
    /// </summary>
    public static Obstacle ConvertObstacleToIce(Obstacle originalObstacle)
    {
        if (originalObstacle == null || originalObstacle.gameObject == null) return null;

        // Nếu đối tượng đã là băng rồi thì không cần chuyển đổi
        if (originalObstacle is Ice || originalObstacle.Type == ObstacleType.Ice)
        {
            return originalObstacle;
        }

        // 1. Phân loại hình dạng (Cylinder hay Square)
        bool isCylinder = originalObstacle.GetComponentInChildren<CylinderCollider>() != null ||
                           originalObstacle.Type == ObstacleType.Can ||
                           originalObstacle.gameObject.name.Contains("Cylinder");

        // 2. Lấy kích thước chiều dài (1..5)
        int sizeX = Mathf.Clamp(Mathf.RoundToInt(originalObstacle.Size.x), 1, 5);

        // 3. Xác định tên Prefab Băng tương ứng
        string prefixBt = isCylinder ? "IceCylinderBt_" : "IceSquareBt_";
        string prefix = isCylinder ? "IceCylinder_" : "IceSquare_";
        string prefabNameBt = prefixBt + sizeX;
        string prefabName = prefix + sizeX;

        // 4. Lưu lại Transform của đối tượng cũ
        Transform parentTransform = originalObstacle.transform.parent;
        Vector3 position = originalObstacle.transform.position;
        Quaternion rotation = originalObstacle.transform.rotation;
        Vector3 scale = originalObstacle.transform.localScale;

        // 5. Tải Prefab từ Resources với các đường dẫn phổ biến
        GameObject icePrefab = Resources.Load<GameObject>($"Blocks/{prefabNameBt}") ?? 
                               Resources.Load<GameObject>($"{prefabNameBt}") ??
                               Resources.Load<GameObject>($"Blocks/{prefabName}") ??
                               Resources.Load<GameObject>($"{prefabName}");

        if (icePrefab != null)
        {
            GameObject newIceObject = Object.Instantiate(icePrefab, position, rotation, parentTransform);
            newIceObject.transform.localScale = scale;
            newIceObject.name = prefabNameBt;

            Debug.Log($"[ObstacleConverter] Chuyển đổi thành công '{originalObstacle.gameObject.name}' -> '{newIceObject.name}'");

            // 6. Xóa đối tượng cũ
            Object.Destroy(originalObstacle.gameObject);
            return newIceObject.GetComponent<Obstacle>();
        }
        else
        {
            Debug.LogError($"[ObstacleConverter] Không thể load Prefab '{prefabNameBt}' trong Resources! Đảm bảo Prefab nằm trong folder Resources (ví dụ: Resources/Blocks/{prefabNameBt}.prefab).");
            return null;
        }
    }
}
