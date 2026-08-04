using UnityEngine;

public class Slingshot : MonoBehaviour
{
    [SerializeField] private Transform ikPivot;

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
