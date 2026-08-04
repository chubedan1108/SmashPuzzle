using UnityEditor.Rendering;
using UnityEngine;

public class InputSlingShot : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask layermask;
    private bool isDragging = false;
    private Slingshot slingshot;
    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
           Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, layermask))
            {
                slingshot = hit.collider.GetComponent<Slingshot>();
                slingshot?.HandleOnMouseDown();
                isDragging = true;
            }
        }
        if (Input.GetMouseButton(0) && isDragging)
        {
           slingshot?.HandleOnMouseDrag();
        }
        if(Input.GetMouseButtonUp(0) && isDragging)
        {
            slingshot?.HandleOnMouseUp();
            isDragging = false;
        }
    }
}
