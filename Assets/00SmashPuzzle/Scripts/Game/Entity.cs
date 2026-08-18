using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    /// <summary>
    /// Đọc các tham số tùy biến (Custom Params) từ dữ liệu của Level.
    /// </summary>
    public virtual void ReadCustomData(string data)
    {
        // Các lớp con sẽ override để đọc thêm dữ liệu riêng
    }

    /// <summary>
    /// Ghi các tham số tùy biến khi lưu màn chơi.
    /// </summary>
    public virtual void WriteCustomData(string data)
    {
        // Các lớp con sẽ override để ghi dữ liệu riêng
    }
}