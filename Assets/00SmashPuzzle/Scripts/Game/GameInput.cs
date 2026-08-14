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
                    Debug.Log($"[GameInput Raycast] ScreenPos: {mouseInput} | RayOrigin: {ray.origin} | RayDir: {ray.direction} | HitObject: '{hit.collider.name}' | HitPoint: {hit.point} | CameraPos: {Camera.main.transform.position} | CameraRot: {Camera.main.transform.eulerAngles}");
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 2f);
                    GameEvents.OnSlingshotRotate?.Invoke(hit.point, hit.normal);
                }
                else
                {
                    Debug.Log($"[GameInput Raycast Missed] ScreenPos: {mouseInput} | RayOrigin: {ray.origin} | RayDir: {ray.direction}");
                }
            }
        }
    }
}
