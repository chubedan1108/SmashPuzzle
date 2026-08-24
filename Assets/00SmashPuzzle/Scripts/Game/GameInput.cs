using UnityEngine;
using UnityEngine.EventSystems;

public class GameInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Camera cam;

    public static bool IsHolding { get; private set; }
    public static Vector3 LastHitPoint { get; private set; }

    private bool isPointerOverUI = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Chỉ coi là bấm vào UI nếu chạm vào một Button thực sự (khác GameInput)
        GameObject currentUI = eventData.pointerCurrentRaycast.gameObject;
        if (currentUI != null && currentUI != gameObject && currentUI.GetComponentInParent<UnityEngine.UI.Button>() != null)
        {
            isPointerOverUI = true;
            return;
        }

        isPointerOverUI = false;
        IsHolding = true;
        ProcessingAim(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPointerOverUI) return;
        ProcessingAim(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPointerOverUI)
        {
            isPointerOverUI = false;
            return;
        }

        IsHolding = false;
        Ray ray = cam.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            LastHitPoint = hit.point;
            GameEvents.OnShoot?.Invoke(hit.point);
        }
    }

    private void ProcessingAim(Vector2 screenPosition)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            LastHitPoint = hit.point;
            GameEvents.OnAim?.Invoke(hit.point);
        }
    }
}
