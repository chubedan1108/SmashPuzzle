using UnityEngine;

public class SlingshotController : MonoBehaviour
{
    [SerializeField] private Transform ikPivot;

    private void Awake()
    {
        GameEvents.OnSlingshotRotate += Rotate;
    }

    void OnDestroy()
    {
        GameEvents.OnSlingshotRotate -= Rotate;
    }
    public void Rotate(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0; // Keep the rotation only on the horizontal plane
        transform.rotation = Quaternion.LookRotation(direction);
        ObjectPool.Instance.GetObject(PoolType.Bullet).transform.parent = ikPivot; 
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
