using UnityEngine.EventSystems;
using UnityEngine;

public class PointerEvents : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Maximium Distance")]
    public float MaxDistance = 100f;

    [Header("Enable Events")]
    public bool Enabled = true;

    [Header("Unity Events : ")]
    [SerializeField] private PointerEventDataEvent OnPointerClickEvent;
    [SerializeField] private PointerEventDataEvent OnPointerEnterEvent;
    [SerializeField] private PointerEventDataEvent OnPointerExitEvent;
    [SerializeField] private PointerEventDataEvent OnPointerDownEvent;
    [SerializeField] private PointerEventDataEvent OnPointerUpEvent;

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (DistanceExceeded(eventData))
        {
            return;
        }

        OnPointerClickEvent?.Invoke(eventData);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (DistanceExceeded(eventData))
        {
            return;
        }

        OnPointerEnterEvent?.Invoke(eventData);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent?.Invoke(eventData);
    }


    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (DistanceExceeded(eventData))
        {
            return;
        }

        OnPointerDownEvent?.Invoke(eventData);
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        OnPointerUpEvent?.Invoke(eventData);
    }

    public virtual bool DistanceExceeded(PointerEventData eventData)
    {
        if (eventData == null)
        {
            return false;
        }

        if (eventData.pointerCurrentRaycast.distance > MaxDistance)
        {
            return true;
        }

        return false;
    }
}