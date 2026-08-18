using UnityEngine;
using UnityEngine.EventSystems;

public class GameInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Camera cam;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("on pointer down");
        ProcessingAim(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("on drag");   
        ProcessingAim(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("on pointer up");
        Ray ray = cam.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            GameEvents.OnShoot?.Invoke(hit.point);
        }
    }

    private void ProcessingAim(Vector2 screenPosition)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Debug.Log(hit.collider.name);
            GameEvents.OnAim?.Invoke(hit.point);
        }
    }
}
