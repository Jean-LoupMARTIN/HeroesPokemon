using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerHandlerUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector]
    public UnityEvent enterEvent, exitEvent, downEvent, upEvent, overEvent;

    
    public void OnPointerEnter(PointerEventData eventData) => enterEvent.Invoke();
    public void OnPointerExit (PointerEventData eventData) => exitEvent .Invoke();
    public void OnPointerDown (PointerEventData eventData) => downEvent .Invoke();
    public void OnPointerUp   (PointerEventData eventData) => upEvent   .Invoke();
}
