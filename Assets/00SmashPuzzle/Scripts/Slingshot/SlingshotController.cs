using UnityEngine;

public class SlingshotController : MonoBehaviour
{
    [SerializeField] private Transform ikPivot;
    [SerializeField] private Transform firePoint;
    [Header("Ballistic setting")]
    [SerializeField] private float heightOffset = 10f;
    [SerializeField] private float speed = 20f;

    private Bullet currentBullet;
    private readonly float gravity = Mathf.Abs(Physics.gravity.y);

    private void Awake()
    {
        GameEvents.OnSlingshotRotate += Rotate;
    }

    private void Start()
    {
        LoadNextBullet();
    }

    private void OnDestroy()
    {
        GameEvents.OnSlingshotRotate -= Rotate;
    }

    public void Rotate(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
        FireAt(target);
    }

    private void LoadNextBullet()
    {
        GameObject bulletObject = ObjectPool.Instance.GetObject(PoolType.Bullet);
        if (bulletObject == null || !bulletObject.TryGetComponent(out currentBullet))
        {
            Debug.LogError("Could not load a Bullet from the ObjectPool.", this);
            currentBullet = null;
            return;
        }

        currentBullet.ResetBullet();
        currentBullet.transform.SetParent(firePoint, false);
        currentBullet.transform.position = firePoint.position;
        currentBullet.transform.rotation = firePoint.rotation;
        currentBullet.transform.localScale = firePoint.localScale;
    }

    public void FireAt(Vector3 target)
    {
        if (currentBullet == null)
        {
            LoadNextBullet();
            if (currentBullet == null)
            {
                return;
            }
        }

        Bullet bulletToFire = currentBullet;
        currentBullet = null;

        bulletToFire.transform.SetParent(null, true);
        bulletToFire.transform.localScale = firePoint.localScale;
        Vector3 calculatedVelocity = CalculateLaunchVelocity(firePoint.position, target);
        bulletToFire.Launch(calculatedVelocity);

        LoadNextBullet();
    }

    private Vector3 CalculateLaunchVelocity(Vector3 startPoint, Vector3 targetPoint)
    {
        Vector3 finalVelocity = Vector3.zero;

        // Chênh lệch độ cao (y) và khoảng cách ngang (x)
        float y = targetPoint.y - startPoint.y;
        Vector3 directionXZ = new Vector3(targetPoint.x - startPoint.x, 0, targetPoint.z - startPoint.z);
        float x = directionXZ.magnitude;

        // Tránh chia cho 0 nếu điểm bắn và mục tiêu trùng nhau
        //if (x < 0.01f) return false;

        // Công thức vật lý đạn đạo: Tính phần dưới dấu căn (Discriminant)
        float v2 = speed * speed;
        float v4 = speed * speed * speed * speed;
        float g = gravity;

        // Biệt thức delta trong phương trình góc ném
        float discriminant = v4 - (g * ((g * x * x) + (2 * y * v2)));

        // Nếu discriminant < 0, tốc độ (speed) quá yếu, không thể bắn tới mục tiêu dù ở góc tối ưu 45 độ
        // if (discriminant < 0) return false;

        // Chọn góc bắn thấp (Low Arc) để đạn bay căng và nhanh (Dùng dấu TRỪ trước căn bậc 2)
        // Nếu muốn bắn bổng (súng cối), thay dấu TRỪ thành CỘNG
        float root = Mathf.Sqrt(discriminant);
        float lowAngle = Mathf.Atan((v2 - root) / (g * x));

        // Chuyển đổi góc bắn từ Toán học sang Vector3 của Unity
        Vector3 velocityXZ = directionXZ.normalized * (Mathf.Cos(lowAngle) * speed);
        float velocityY = Mathf.Sin(lowAngle) * speed;

        return finalVelocity = new Vector3(velocityXZ.x, velocityY, velocityXZ.z);
    }

    public void HandleOnMouseDown()
    {
        Debug.Log("Slingshot clicked");
    }

    public void HandleOnMouseDrag()
    {
        Debug.Log("Slingshot dragging");
    }

    public void HandleOnMouseUp()
    {
        Debug.Log("Slingshot released");
    }
}
