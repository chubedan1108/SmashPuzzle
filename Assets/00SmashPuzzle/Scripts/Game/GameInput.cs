using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameInput : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
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
                Vector3 mouseInput = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mouseInput);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, layerMask))
                {
                    Debug.Log(hit.collider.name);
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 1f);
                    GameEvents.OnSlingshotRotate?.Invoke(hit.point);
                }
                else
                {
                    Debug.Log("No object hit");
                }
            }
        }
    }
}
