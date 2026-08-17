using UnityEngine;
using UnityEngine.EventSystems;

public class GameInput : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Camera cam;
    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
                {
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 2f);
                    GameEvents.OnSlingshotRotate?.Invoke(hit.point);
                }
                else
                {
                  
                }
            }
        }
    }
}
